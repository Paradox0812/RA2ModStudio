namespace RA2IniEditor.AssetHost;

internal sealed partial class Ra2VoxelGenerationHost
{
    private async ValueTask<Ra2GenerationRunResult> RunCoreAsync(
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest request,
        IProgress<Ra2GenerationProgress>? progress,
        CancellationToken cancellationToken)
    {
        DateTimeOffset startedUtc = DateTimeOffset.UtcNow;
        bool gateEntered = false;
        Ra2GenerationWorkspace? workspace = null;
        IReadOnlyList<Ra2GenerationProgress> progressSummary = Array.Empty<Ra2GenerationProgress>();
        try
        {
            try
            {
                await _runGate.WaitAsync(cancellationToken).ConfigureAwait(false);
                gateEntered = true;
            }
            catch (OperationCanceledException)
            {
                return Failure(
                    Ra2GenerationState.Canceled,
                    Ra2GenerationFailureKind.Canceled,
                    "The generation request was canceled.",
                    progressSummary,
                    startedUtc);
            }

            Ra2GenerationFailureKind configurationFailure = Ra2GenerationValidation.ValidateConfiguration(
                configuration,
                requireWorkspace: true,
                out string configurationMessage);
            if (configurationFailure != Ra2GenerationFailureKind.None)
            {
                return Failure(Ra2GenerationState.Failed, configurationFailure, configurationMessage, progressSummary, startedUtc);
            }

            Ra2GenerationFailureKind requestFailure = Ra2GenerationValidation.ValidateRequest(configuration, request, out string requestMessage);
            if (requestFailure != Ra2GenerationFailureKind.None)
            {
                return Failure(Ra2GenerationState.Failed, requestFailure, requestMessage, progressSummary, startedUtc);
            }

            Ra2GenerationProbeResult probe = await ProbeAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (!probe.Succeeded)
            {
                return Failure(probe.State, probe.FailureKind, probe.Message, progressSummary, startedUtc);
            }

            Ra2GenerationProviderDescriptor descriptor = probe.Descriptor!;
            long inputBytes = request.References.Sum(reference => (long)reference.Length);
            if (request.References.Count > descriptor.MaximumReferenceCount ||
                request.CandidateCount > descriptor.MaximumCandidateCount ||
                inputBytes > descriptor.MaximumInputBytes)
            {
                return Failure(
                    Ra2GenerationState.Failed,
                    Ra2GenerationFailureKind.CapabilityUnsupported,
                    "The request exceeds the configured provider capability declaration.",
                    progressSummary,
                    startedUtc);
            }

            Ra2WorkspacePreparationResult preparation = await Ra2GenerationWorkspace.PrepareAsync(
                configuration,
                request,
                cancellationToken).ConfigureAwait(false);
            if (!preparation.Succeeded)
            {
                return Failure(
                    preparation.FailureKind == Ra2GenerationFailureKind.Canceled ? Ra2GenerationState.Canceled : Ra2GenerationState.Failed,
                    preparation.FailureKind,
                    preparation.Message,
                    progressSummary,
                    startedUtc);
            }

            workspace = preparation.Workspace!;
            string immediateExecutableHash;
            try
            {
                immediateExecutableHash = await Ra2GenerationValidation.ComputeFileSha256Async(
                    configuration.ExecutablePath,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return await FailureWithCleanupAsync(
                    workspace,
                    Ra2GenerationState.Canceled,
                    Ra2GenerationFailureKind.Canceled,
                    "The generation request was canceled.",
                    progressSummary,
                    startedUtc).ConfigureAwait(false);
            }

            if (!string.Equals(immediateExecutableHash, configuration.ExpectedExecutableSha256, StringComparison.Ordinal) ||
                !string.Equals(immediateExecutableHash, descriptor.ExecutableSha256, StringComparison.Ordinal))
            {
                return await FailureWithCleanupAsync(
                    workspace,
                    Ra2GenerationState.Failed,
                    Ra2GenerationFailureKind.ExecutableHashMismatch,
                    "The provider executable changed after readiness validation.",
                    progressSummary,
                    startedUtc).ConfigureAwait(false);
            }

            await using var progressDispatcher = new Ra2GenerationProgressDispatcher(progress);
            var protocol = new Ra2GenerationProtocolSession(
                Ra2ProviderOperation.Generate,
                configuration,
                request,
                progressDispatcher.Publish);
            Ra2ProviderProcessResult processResult = await Ra2ProviderProcessRunner.RunAsync(
                configuration,
                Ra2ProviderOperation.Generate,
                workspace.RunRoot,
                request.Timeout,
                protocol,
                cancellationToken).ConfigureAwait(false);
            progressSummary = protocol.ProgressSummary;
            if (!processResult.Succeeded)
            {
                return await FailureWithCleanupAsync(
                    workspace,
                    StateFor(processResult.FailureKind),
                    processResult.FailureKind,
                    processResult.Message,
                    progressSummary,
                    startedUtc).ConfigureAwait(false);
            }

            if (protocol.ProviderFailureKind != Ra2GenerationFailureKind.None)
            {
                return await FailureWithCleanupAsync(
                    workspace,
                    Ra2GenerationState.Failed,
                    protocol.ProviderFailureKind,
                    string.IsNullOrWhiteSpace(protocol.ProviderFailureMessage)
                        ? "The provider reported a generation failure."
                        : protocol.ProviderFailureMessage,
                    progressSummary,
                    startedUtc).ConfigureAwait(false);
            }

            try
            {
                protocol.EnsureGenerationCompleted();
            }
            catch (Ra2GenerationProtocolException)
            {
                return await FailureWithCleanupAsync(
                    workspace,
                    Ra2GenerationState.Failed,
                    Ra2GenerationFailureKind.ProtocolViolation,
                    "The provider generation response was incomplete.",
                    progressSummary,
                    startedUtc).ConfigureAwait(false);
            }

            Ra2WorkspacePromotionResult promotion = await workspace.ValidateAndPromoteAsync(
                descriptor,
                protocol.Candidates,
                protocol.CompletedCandidateIds,
                cancellationToken).ConfigureAwait(false);
            if (!promotion.Succeeded)
            {
                return await FailureWithCleanupAsync(
                    workspace,
                    StateFor(promotion.FailureKind),
                    promotion.FailureKind,
                    promotion.Message,
                    progressSummary,
                    startedUtc).ConfigureAwait(false);
            }

            workspace = null;
            return Ra2GenerationRunResult.Success(
                promotion.Lease!,
                progressSummary,
                startedUtc,
                DateTimeOffset.UtcNow);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.Cryptography.CryptographicException)
        {
            if (workspace is not null)
            {
                return await FailureWithCleanupAsync(
                    workspace,
                    Ra2GenerationState.Failed,
                    Ra2GenerationFailureKind.UnexpectedFailure,
                    "The generation host encountered a bounded local failure.",
                    progressSummary,
                    startedUtc).ConfigureAwait(false);
            }

            return Failure(
                Ra2GenerationState.Failed,
                Ra2GenerationFailureKind.UnexpectedFailure,
                "The generation host encountered a bounded local failure.",
                progressSummary,
                startedUtc);
        }
        finally
        {
            if (gateEntered)
            {
                _runGate.Release();
            }
        }
    }

    private static async ValueTask<Ra2GenerationRunResult> FailureWithCleanupAsync(
        Ra2GenerationWorkspace workspace,
        Ra2GenerationState state,
        Ra2GenerationFailureKind failureKind,
        string message,
        IReadOnlyList<Ra2GenerationProgress> progress,
        DateTimeOffset startedUtc)
    {
        bool cleaned = await workspace.CleanupFailedRunAsync().ConfigureAwait(false);
        return cleaned
            ? Failure(state, failureKind, message, progress, startedUtc)
            : Failure(
                Ra2GenerationState.Failed,
                Ra2GenerationFailureKind.CleanupFailed,
                "The failed generation workspace could not be cleaned and was quarantined.",
                progress,
                startedUtc);
    }

    private static Ra2GenerationRunResult Failure(
        Ra2GenerationState state,
        Ra2GenerationFailureKind failureKind,
        string message,
        IReadOnlyList<Ra2GenerationProgress> progress,
        DateTimeOffset startedUtc) =>
        Ra2GenerationRunResult.Failure(state, failureKind, message, progress, startedUtc, DateTimeOffset.UtcNow);

    private static Ra2GenerationState StateFor(Ra2GenerationFailureKind failureKind) => failureKind switch
    {
        Ra2GenerationFailureKind.Canceled => Ra2GenerationState.Canceled,
        Ra2GenerationFailureKind.TimedOut => Ra2GenerationState.TimedOut,
        _ => Ra2GenerationState.Failed
    };
}
