using System.Diagnostics;

namespace RA2IniEditor.AssetHost.Tests;

public sealed class Ra2GenerationProcessLifecycleTests
{
    [Theory]
    [InlineData("[fixture:malformed]", (int)Ra2GenerationFailureKind.ProtocolViolation)]
    [InlineData("[fixture:duplicate-root]", (int)Ra2GenerationFailureKind.ProtocolViolation)]
    [InlineData("[fixture:oversized-line]", (int)Ra2GenerationFailureKind.ProtocolViolation)]
    [InlineData("[fixture:progress-flood]", (int)Ra2GenerationFailureKind.ProtocolViolation)]
    [InlineData("[fixture:post-terminal]", (int)Ra2GenerationFailureKind.ProtocolViolation)]
    [InlineData("[fixture:crash]", (int)Ra2GenerationFailureKind.ProcessCrashed)]
    [InlineData("[fixture:nonzero-after-completed]", (int)Ra2GenerationFailureKind.ProcessCrashed)]
    [InlineData("[fixture:failed]", (int)Ra2GenerationFailureKind.ProviderReportedFailure)]
    [InlineData("[fixture:path-traversal]", (int)Ra2GenerationFailureKind.OutputRejected)]
    [InlineData("[fixture:hash-mismatch]", (int)Ra2GenerationFailureKind.OutputRejected)]
    public async Task Provider_faults_return_typed_zero_lease_failure_and_clean_run(
        string behavior,
        int expectedFailureValue)
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot),
                AssetHostTestFixture.CreateRequest(prompt: behavior));

            Assert.False(result.Succeeded);
            Assert.Equal((Ra2GenerationFailureKind)expectedFailureValue, result.FailureKind);
            Assert.Null(result.Lease);
            AssertNoRunDirectories(workspaceRoot);
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Concurrent_stderr_and_progress_backpressure_cannot_deadlock_or_block_success()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var observer = new SlowProgressObserver();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot),
                AssetHostTestFixture.CreateRequest(prompt: "[fixture:backpressure]"),
                observer);

            stopwatch.Stop();
            Assert.True(result.Succeeded, result.Message);
            Assert.InRange(observer.Deliveries, 1, 5);
            Assert.InRange(result.Progress.Count, 1, 64);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(8));
            await result.Lease!.DisposeAsync();
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Theory]
    [InlineData("[fixture:hang]")]
    [InlineData("[fixture:cancel-after-candidate]")]
    public async Task Cancellation_before_commit_wins_and_returns_no_candidate(string behavior)
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot),
                AssetHostTestFixture.CreateRequest(prompt: behavior),
                cancellationToken: cancellation.Token);

            Assert.Equal(Ra2GenerationState.Canceled, result.State);
            Assert.Equal(Ra2GenerationFailureKind.Canceled, result.FailureKind);
            Assert.Null(result.Lease);
            AssertNoRunDirectories(workspaceRoot);
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Timeout_is_distinct_from_cancellation_and_cleans_run()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot),
                AssetHostTestFixture.CreateRequest(prompt: "[fixture:hang]", timeout: TimeSpan.FromSeconds(10)));

            stopwatch.Stop();
            Assert.Equal(Ra2GenerationState.TimedOut, result.State);
            Assert.Equal(Ra2GenerationFailureKind.TimedOut, result.FailureKind);
            Assert.Null(result.Lease);
            Assert.InRange(stopwatch.Elapsed, TimeSpan.FromSeconds(9), TimeSpan.FromSeconds(16));
            AssertNoRunDirectories(workspaceRoot);
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
        }
    }

    [Fact]
    public async Task Cancellation_terminates_descendant_process_tree()
    {
        string workspaceRoot = AssetHostTestFixture.CreateUnusedWorkspacePath();
        Guid runId = Guid.NewGuid();
        string heartbeat = Path.Combine(AppContext.BaseDirectory, $"ra2-asset-host-child-{runId:N}.heartbeat");
        using var cancellation = new CancellationTokenSource();
        try
        {
            Task<Ra2GenerationRunResult> run = new Ra2VoxelGenerationHost().RunAsync(
                AssetHostTestFixture.CreateConfiguration(workspaceRoot),
                AssetHostTestFixture.CreateRequest(runId, prompt: "[fixture:spawn-child-hang]"),
                cancellationToken: cancellation.Token).AsTask();

            await WaitForFileAsync(heartbeat, TimeSpan.FromSeconds(5));
            cancellation.Cancel();
            Ra2GenerationRunResult result = await run;
            Assert.Equal(Ra2GenerationFailureKind.Canceled, result.FailureKind);

            await Task.Delay(300);
            string stoppedValue = await File.ReadAllTextAsync(heartbeat);
            await Task.Delay(300);
            Assert.Equal(stoppedValue, await File.ReadAllTextAsync(heartbeat));
            AssertNoRunDirectories(workspaceRoot);
        }
        finally
        {
            DeleteDirectory(workspaceRoot);
            try
            {
                File.Delete(heartbeat);
            }
            catch
            {
            }
        }
    }

    private static async Task WaitForFileAsync(string path, TimeSpan timeout)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow + timeout;
        while (!File.Exists(path))
        {
            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException("The fixture descendant did not start.");
            }

            await Task.Delay(50);
        }
    }

    private static void AssertNoRunDirectories(string workspaceRoot)
    {
        if (!Directory.Exists(workspaceRoot))
        {
            return;
        }

        Assert.Empty(Directory.EnumerateDirectories(workspaceRoot));
    }

    private static void DeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
        }
    }

    private sealed class SlowProgressObserver : IProgress<Ra2GenerationProgress>
    {
        private int _deliveries;
        internal int Deliveries => Volatile.Read(ref _deliveries);

        public void Report(Ra2GenerationProgress value)
        {
            Interlocked.Increment(ref _deliveries);
            Thread.Sleep(250);
        }
    }
}
