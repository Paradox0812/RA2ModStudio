using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace RA2IniEditor.AssetHost;

internal sealed class Ra2ProviderProcessResult
{
    internal Ra2ProviderProcessResult(
        Ra2GenerationFailureKind failureKind,
        string message,
        int? exitCode,
        bool terminationFailed)
    {
        FailureKind = failureKind;
        Message = message;
        ExitCode = exitCode;
        TerminationFailed = terminationFailed;
    }

    internal Ra2GenerationFailureKind FailureKind { get; }
    internal string Message { get; }
    internal int? ExitCode { get; }
    internal bool TerminationFailed { get; }
    internal bool Succeeded => FailureKind == Ra2GenerationFailureKind.None;
}

internal static class Ra2ProviderProcessRunner
{
    private delegate void ProtocolLineHandler(ReadOnlySpan<byte> line);

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly TimeSpan TerminationGrace = TimeSpan.FromSeconds(5);

    internal static async ValueTask<Ra2ProviderProcessResult> RunAsync(
        Ra2GenerationProviderConfiguration configuration,
        Ra2ProviderOperation operation,
        string? runDirectory,
        TimeSpan timeout,
        Ra2GenerationProtocolSession protocol,
        CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = CreateStartInfo(configuration, operation, runDirectory),
            EnableRaisingEvents = true
        };

        try
        {
            if (!process.Start())
            {
                return Failure(Ra2GenerationFailureKind.ProcessStartFailed, "The provider process could not be started.");
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
            return Failure(Ra2GenerationFailureKind.ProcessStartFailed, "The provider process could not be started.");
        }

        var protocolFault = new TaskCompletionSource<Ra2ProviderProcessResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task stdoutTask = DrainStdoutAsync(process.StandardOutput.BaseStream, protocol, protocolFault);
        Task stderrTask = DrainStderrAsync(process.StandardError.BaseStream);
        Task exitTask = process.WaitForExitAsync(CancellationToken.None);
        Task timeoutTask = Task.Delay(timeout, CancellationToken.None);
        Task cancellationTask = cancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ContinueWith(
                static _ => { },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default)
            : Task.Delay(Timeout.InfiniteTimeSpan);
        using var resourceMonitorCancellation = new CancellationTokenSource();
        Task<bool> resourceTask = operation == Ra2ProviderOperation.Generate
            ? MonitorRunSizeAsync(runDirectory!, resourceMonitorCancellation.Token)
            : new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously).Task;

        Task winner = await Task.WhenAny(exitTask, protocolFault.Task, timeoutTask, cancellationTask, resourceTask).ConfigureAwait(false);
        Ra2GenerationFailureKind latchedFailure = Ra2GenerationFailureKind.None;
        string latchedMessage = string.Empty;

        if (cancellationToken.IsCancellationRequested)
        {
            latchedFailure = Ra2GenerationFailureKind.Canceled;
            latchedMessage = "The provider operation was canceled.";
        }
        else if (winner == timeoutTask)
        {
            latchedFailure = Ra2GenerationFailureKind.TimedOut;
            latchedMessage = "The provider operation timed out.";
        }
        else if (winner == protocolFault.Task)
        {
            Ra2ProviderProcessResult fault = await protocolFault.Task.ConfigureAwait(false);
            latchedFailure = fault.FailureKind;
            latchedMessage = fault.Message;
        }
        else if (winner == resourceTask && await resourceTask.ConfigureAwait(false))
        {
            latchedFailure = Ra2GenerationFailureKind.ResourceLimitExceeded;
            latchedMessage = "The provider exceeded the generation workspace limit.";
        }

        resourceMonitorCancellation.Cancel();

        bool terminationFailed = false;
        if (latchedFailure != Ra2GenerationFailureKind.None && !process.HasExited)
        {
            terminationFailed = !TryTerminateProcessTree(process);
        }

        if (!process.HasExited)
        {
            Task grace = Task.Delay(TerminationGrace);
            if (await Task.WhenAny(exitTask, grace).ConfigureAwait(false) != exitTask)
            {
                terminationFailed = true;
                TryTerminateProcessTree(process);
            }
        }

        Task pumps = Task.WhenAll(stdoutTask, stderrTask);
        if (await Task.WhenAny(pumps, Task.Delay(TerminationGrace)).ConfigureAwait(false) != pumps)
        {
            terminationFailed = true;
        }

        if (terminationFailed)
        {
            return new Ra2ProviderProcessResult(
                Ra2GenerationFailureKind.TerminationFailed,
                "The provider process did not terminate cleanly.",
                process.HasExited ? process.ExitCode : null,
                terminationFailed: true);
        }

        if (latchedFailure != Ra2GenerationFailureKind.None)
        {
            return new Ra2ProviderProcessResult(latchedFailure, latchedMessage, process.ExitCode, terminationFailed: false);
        }

        if (protocolFault.Task.IsCompleted)
        {
            return await protocolFault.Task.ConfigureAwait(false);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Failure(Ra2GenerationFailureKind.Canceled, "The provider operation was canceled.", process.ExitCode);
        }

        if (process.ExitCode != 0)
        {
            return Failure(Ra2GenerationFailureKind.ProcessCrashed, "The provider process exited unsuccessfully.", process.ExitCode);
        }

        if (!protocol.HasTerminal)
        {
            return Failure(Ra2GenerationFailureKind.ProcessCrashed, "The provider process exited without a terminal protocol message.", process.ExitCode);
        }

        return new Ra2ProviderProcessResult(Ra2GenerationFailureKind.None, string.Empty, process.ExitCode, terminationFailed: false);
    }

    internal static ProcessStartInfo CreateStartInfo(
        Ra2GenerationProviderConfiguration configuration,
        Ra2ProviderOperation operation,
        string? runDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = configuration.ExecutablePath,
            WorkingDirectory = Path.GetDirectoryName(configuration.ExecutablePath)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = StrictUtf8,
            StandardErrorEncoding = Encoding.UTF8
        };
        startInfo.ArgumentList.Add("--ra2-asset-host-protocol");
        startInfo.ArgumentList.Add(Ra2GenerationLimits.ProtocolIdentity);
        startInfo.ArgumentList.Add("--operation");
        startInfo.ArgumentList.Add(operation == Ra2ProviderOperation.Probe ? "probe" : "generate");
        if (operation == Ra2ProviderOperation.Generate)
        {
            startInfo.ArgumentList.Add("--run-directory");
            startInfo.ArgumentList.Add(runDirectory!);
        }

        startInfo.Environment.Clear();
        foreach (string variableName in new[] { "SystemRoot", "WINDIR", "TEMP", "TMP" })
        {
            string? value = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                startInfo.Environment[variableName] = value;
            }
        }

        startInfo.Environment["DOTNET_NOLOGO"] = "1";
        startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        return startInfo;
    }

    private static async Task DrainStdoutAsync(
        Stream stream,
        Ra2GenerationProtocolSession protocol,
        TaskCompletionSource<Ra2ProviderProcessResult> protocolFault)
    {
        try
        {
            await ReadBoundedLinesAsync(stream, protocol.AcceptLine).ConfigureAwait(false);
        }
        catch (Ra2GenerationIdentityException)
        {
            protocolFault.TrySetResult(Failure(
                Ra2GenerationFailureKind.ProviderIdentityMismatch,
                "The provider identity did not match the trusted configuration."));
        }
        catch (Exception exception) when (exception is Ra2GenerationProtocolException or JsonException or DecoderFallbackException)
        {
            protocolFault.TrySetResult(Failure(Ra2GenerationFailureKind.ProtocolViolation, "The provider protocol output was invalid."));
        }
        catch (IOException)
        {
            protocolFault.TrySetResult(Failure(Ra2GenerationFailureKind.ProtocolViolation, "The provider protocol stream failed."));
        }
    }

    private static async Task ReadBoundedLinesAsync(Stream stream, ProtocolLineHandler acceptLine)
    {
        byte[] readBuffer = new byte[8192];
        byte[] lineBuffer = new byte[Ra2GenerationLimits.MaximumProtocolLineBytes];
        int lineLength = 0;
        int lineCount = 0;
        while (true)
        {
            int read = await stream.ReadAsync(readBuffer).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            for (int index = 0; index < read; index++)
            {
                byte value = readBuffer[index];
                if (value == (byte)'\n')
                {
                    int contentLength = lineLength > 0 && lineBuffer[lineLength - 1] == (byte)'\r' ? lineLength - 1 : lineLength;
                    if (contentLength > 0)
                    {
                        StrictUtf8.GetCharCount(lineBuffer.AsSpan(0, contentLength));
                        acceptLine(lineBuffer.AsSpan(0, contentLength));
                        lineCount++;
                        if (lineCount > Ra2GenerationLimits.MaximumProtocolLines)
                        {
                            throw new Ra2GenerationProtocolException("The provider emitted too many protocol messages.");
                        }
                    }

                    lineLength = 0;
                    continue;
                }

                if (lineLength >= lineBuffer.Length)
                {
                    throw new Ra2GenerationProtocolException("A provider protocol line exceeded the size limit.");
                }

                lineBuffer[lineLength++] = value;
            }
        }

        if (lineLength > 0)
        {
            int contentLength = lineBuffer[lineLength - 1] == (byte)'\r' ? lineLength - 1 : lineLength;
            StrictUtf8.GetCharCount(lineBuffer.AsSpan(0, contentLength));
            acceptLine(lineBuffer.AsSpan(0, contentLength));
            lineCount++;
            if (lineCount > Ra2GenerationLimits.MaximumProtocolLines)
            {
                throw new Ra2GenerationProtocolException("The provider emitted too many protocol messages.");
            }
        }
    }

    private static async Task DrainStderrAsync(Stream stream)
    {
        byte[] readBuffer = new byte[8192];
        byte[] ring = new byte[Ra2GenerationLimits.MaximumStandardErrorBytes];
        int writeOffset = 0;
        while (true)
        {
            int read = await stream.ReadAsync(readBuffer).ConfigureAwait(false);
            if (read == 0)
            {
                return;
            }

            for (int index = 0; index < read; index++)
            {
                ring[writeOffset] = readBuffer[index];
                writeOffset = (writeOffset + 1) % ring.Length;
            }
        }
    }

    private static bool TryTerminateProcessTree(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
            return true;
        }
        catch (InvalidOperationException)
        {
            return process.HasExited;
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or NotSupportedException)
        {
            return false;
        }
    }

    private static async Task<bool> MonitorRunSizeAsync(string runDirectory, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                if (Ra2GenerationWorkspace.MeasureRunBytes(runDirectory) > Ra2GenerationLimits.MaximumRunBytes)
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or OverflowException)
        {
            return true;
        }
    }

    private static Ra2ProviderProcessResult Failure(
        Ra2GenerationFailureKind failureKind,
        string message,
        int? exitCode = null) =>
        new(failureKind, message, exitCode, terminationFailed: false);
}
