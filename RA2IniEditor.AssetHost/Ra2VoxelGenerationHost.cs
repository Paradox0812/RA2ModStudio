namespace RA2IniEditor.AssetHost;

internal sealed partial class Ra2VoxelGenerationHost : IRa2VoxelGenerationHost
{
    private readonly SemaphoreSlim _runGate = new(1, 1);

    public async ValueTask<Ra2GenerationProbeResult> ProbeAsync(
        Ra2GenerationProviderConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset startedUtc = DateTimeOffset.UtcNow;
        Ra2GenerationFailureKind validation = Ra2GenerationValidation.ValidateConfiguration(
            configuration,
            requireWorkspace: false,
            out string validationMessage);
        if (validation != Ra2GenerationFailureKind.None)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Failed,
                validation,
                validationMessage,
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Canceled,
                Ra2GenerationFailureKind.Canceled,
                "The provider probe was canceled.",
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        string executableHash;
        try
        {
            executableHash = await Ra2GenerationValidation.ComputeFileSha256Async(
                configuration.ExecutablePath,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Canceled,
                Ra2GenerationFailureKind.Canceled,
                "The provider probe was canceled.",
                startedUtc,
                DateTimeOffset.UtcNow);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.Cryptography.CryptographicException)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Failed,
                Ra2GenerationFailureKind.ProviderNotConfigured,
                "The provider executable could not be read.",
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        if (!string.Equals(executableHash, configuration.ExpectedExecutableSha256, StringComparison.Ordinal))
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Failed,
                Ra2GenerationFailureKind.ExecutableHashMismatch,
                "The provider executable hash did not match the trusted configuration.",
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        var protocol = new Ra2GenerationProtocolSession(Ra2ProviderOperation.Probe, configuration, request: null);
        Ra2ProviderProcessResult processResult = await Ra2ProviderProcessRunner.RunAsync(
            configuration,
            Ra2ProviderOperation.Probe,
            runDirectory: null,
            configuration.ProbeTimeout,
            protocol,
            cancellationToken).ConfigureAwait(false);
        if (!processResult.Succeeded)
        {
            return Ra2GenerationProbeResult.Failure(
                processResult.FailureKind == Ra2GenerationFailureKind.Canceled
                    ? Ra2GenerationState.Canceled
                    : processResult.FailureKind == Ra2GenerationFailureKind.TimedOut
                        ? Ra2GenerationState.TimedOut
                        : Ra2GenerationState.Failed,
                processResult.FailureKind,
                processResult.Message,
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        if (protocol.ProviderFailureKind != Ra2GenerationFailureKind.None)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Failed,
                protocol.ProviderFailureKind,
                string.IsNullOrWhiteSpace(protocol.ProviderFailureMessage)
                    ? "The provider reported a readiness failure."
                    : protocol.ProviderFailureMessage,
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        try
        {
            protocol.EnsureProbeCompleted();
        }
        catch (Ra2GenerationProtocolException)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Failed,
                Ra2GenerationFailureKind.ProtocolViolation,
                "The provider readiness response was incomplete.",
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        Ra2GenerationProviderDescriptor descriptor = protocol.Descriptor!;
        Ra2GenerationFailureKind descriptorFailure = ValidateDescriptor(configuration, executableHash, descriptor);
        if (descriptorFailure != Ra2GenerationFailureKind.None)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Failed,
                descriptorFailure,
                DescriptorFailureMessage(descriptorFailure),
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        if (!protocol.ModelReady)
        {
            return Ra2GenerationProbeResult.Failure(
                Ra2GenerationState.Failed,
                Ra2GenerationFailureKind.ProviderNotReady,
                "The configured provider model is not ready.",
                startedUtc,
                DateTimeOffset.UtcNow);
        }

        return Ra2GenerationProbeResult.Ready(descriptor, startedUtc, DateTimeOffset.UtcNow);
    }

    public ValueTask<Ra2GenerationRunResult> RunAsync(
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest request,
        IProgress<Ra2GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        RunCoreAsync(configuration, request, progress, cancellationToken);

    private static Ra2GenerationFailureKind ValidateDescriptor(
        Ra2GenerationProviderConfiguration configuration,
        string executableHash,
        Ra2GenerationProviderDescriptor descriptor)
    {
        if (descriptor.ProtocolVersion != Ra2GenerationLimits.ProtocolVersion ||
            !string.Equals(descriptor.ProviderId, configuration.ExpectedProviderId, StringComparison.Ordinal) ||
            !string.Equals(descriptor.ProviderVersion, configuration.ExpectedProviderVersion, StringComparison.Ordinal) ||
            !string.Equals(descriptor.ModelId, configuration.ExpectedModelId, StringComparison.Ordinal) ||
            !string.Equals(descriptor.ModelRevision, configuration.ExpectedModelRevision, StringComparison.Ordinal))
        {
            return Ra2GenerationFailureKind.ProviderIdentityMismatch;
        }

        if (!string.Equals(descriptor.ExecutableSha256, executableHash, StringComparison.Ordinal))
        {
            return Ra2GenerationFailureKind.ExecutableHashMismatch;
        }

        if ((descriptor.Capabilities & configuration.RequiredCapability) != configuration.RequiredCapability ||
            descriptor.MaximumReferenceCount is < 1 or > Ra2GenerationLimits.MaximumReferenceCount ||
            descriptor.MaximumCandidateCount is < 1 or > Ra2GenerationLimits.MaximumCandidateCount ||
            descriptor.MaximumInputBytes is <= 0 or > Ra2GenerationLimits.MaximumInputBytes ||
            descriptor.MaximumOutputBytes is <= 0 or > Ra2GenerationLimits.MaximumRunBytes)
        {
            return Ra2GenerationFailureKind.CapabilityUnsupported;
        }

        if (descriptor.RequiresUserAcceptance && !configuration.LicenseAccepted)
        {
            return Ra2GenerationFailureKind.LicenseNotAccepted;
        }

        return Ra2GenerationFailureKind.None;
    }

    private static string DescriptorFailureMessage(Ra2GenerationFailureKind failureKind) => failureKind switch
    {
        Ra2GenerationFailureKind.ExecutableHashMismatch => "The provider-reported executable hash did not match.",
        Ra2GenerationFailureKind.CapabilityUnsupported => "The provider capability declaration is unsupported.",
        Ra2GenerationFailureKind.LicenseNotAccepted => "The provider license requires explicit acceptance.",
        _ => "The provider identity did not match the trusted configuration."
    };
}
