using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.AssetHost;

internal static class Ra2GenerationValidation
{
    internal static Ra2GenerationFailureKind ValidateConfiguration(
        Ra2GenerationProviderConfiguration? configuration,
        bool requireWorkspace,
        out string message)
    {
        if (configuration is null)
        {
            message = "Provider configuration is required.";
            return Ra2GenerationFailureKind.ProviderNotConfigured;
        }

        if (string.IsNullOrWhiteSpace(configuration.ExecutablePath) ||
            !Path.IsPathFullyQualified(configuration.ExecutablePath) ||
            !File.Exists(configuration.ExecutablePath))
        {
            message = "The configured provider executable is unavailable.";
            return Ra2GenerationFailureKind.ProviderNotConfigured;
        }

        if (!IsUpperSha256(configuration.ExpectedExecutableSha256) ||
            !IsIdentity(configuration.ExpectedProviderId) ||
            !IsBoundedText(configuration.ExpectedProviderVersion, 128) ||
            !IsBoundedText(configuration.ExpectedModelId, 128) ||
            !IsBoundedText(configuration.ExpectedModelRevision, 128))
        {
            message = "The trusted provider identity configuration is invalid.";
            return Ra2GenerationFailureKind.ProviderNotConfigured;
        }

        if (configuration.RequiredCapability != Ra2GenerationCapability.ReferenceImageToMesh)
        {
            message = "The requested provider capability is unsupported.";
            return Ra2GenerationFailureKind.CapabilityUnsupported;
        }

        if (configuration.ProbeTimeout < Ra2GenerationLimits.MinimumProbeTimeout ||
            configuration.ProbeTimeout > Ra2GenerationLimits.MaximumProbeTimeout ||
            configuration.OrphanTtl < Ra2GenerationLimits.MinimumOrphanTtl ||
            configuration.OrphanTtl > Ra2GenerationLimits.MaximumOrphanTtl ||
            configuration.MaximumWorkspaceRootBytes < Ra2GenerationLimits.MinimumWorkspaceRootBytes ||
            configuration.MaximumWorkspaceRootBytes > Ra2GenerationLimits.MaximumWorkspaceRootBytes)
        {
            message = "The configured provider limits are outside the supported range.";
            return Ra2GenerationFailureKind.ProviderNotConfigured;
        }

        if (requireWorkspace &&
            (string.IsNullOrWhiteSpace(configuration.WorkspaceRoot) ||
             !Path.IsPathFullyQualified(configuration.WorkspaceRoot)))
        {
            message = "The generation workspace is not configured.";
            return Ra2GenerationFailureKind.WorkspaceRejected;
        }

        message = string.Empty;
        return Ra2GenerationFailureKind.None;
    }

    internal static Ra2GenerationFailureKind ValidateRequest(
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest? request,
        out string message)
    {
        if (request is null || request.RunId == Guid.Empty)
        {
            message = "A non-empty generation request is required.";
            return Ra2GenerationFailureKind.InvalidRequest;
        }

        if (Encoding.UTF8.GetByteCount(request.Prompt) is <= 0 or > Ra2GenerationLimits.MaximumPromptBytes ||
            Encoding.UTF8.GetByteCount(request.NegativeConstraints) > Ra2GenerationLimits.MaximumNegativeConstraintBytes ||
            request.References.Count is < 1 or > Ra2GenerationLimits.MaximumReferenceCount ||
            request.CandidateCount is < 1 or > Ra2GenerationLimits.MaximumCandidateCount ||
            request.Seed <= 0 ||
            request.Timeout < Ra2GenerationLimits.MinimumRunTimeout ||
            request.Timeout > Ra2GenerationLimits.MaximumRunTimeout)
        {
            message = "The generation request exceeds the supported bounds.";
            return Ra2GenerationFailureKind.InvalidRequest;
        }

        if (!string.Equals(request.ExpectedProviderId, configuration.ExpectedProviderId, StringComparison.Ordinal) ||
            !string.Equals(request.ExpectedModelRevision, configuration.ExpectedModelRevision, StringComparison.Ordinal))
        {
            message = "The request provider identity does not match the trusted configuration.";
            return Ra2GenerationFailureKind.ProviderIdentityMismatch;
        }

        long aggregate = 0;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (Ra2GenerationReferenceImage reference in request.References)
        {
            if (!IsSafeFileName(reference.Name) ||
                reference.Length is <= 0 or > Ra2GenerationLimits.MaximumReferenceBytes ||
                !names.Add(reference.Name) ||
                !HasExpectedMediaSignature(reference))
            {
                message = "A reference image name, media declaration, or payload is invalid.";
                return Ra2GenerationFailureKind.InvalidRequest;
            }

            aggregate += reference.Length;
            if (aggregate > Ra2GenerationLimits.MaximumInputBytes)
            {
                message = "Reference images exceed the aggregate input limit.";
                return Ra2GenerationFailureKind.ResourceLimitExceeded;
            }
        }

        message = string.Empty;
        return Ra2GenerationFailureKind.None;
    }

    internal static async ValueTask<string> ComputeFileSha256Async(string path, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }

    internal static bool IsIdentity(string value) =>
        IsBoundedText(value, 128) && value.All(character =>
            character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.');

    internal static bool IsBoundedText(string value, int maximumUtf8Bytes) =>
        !string.IsNullOrWhiteSpace(value) && Encoding.UTF8.GetByteCount(value) <= maximumUtf8Bytes &&
        !value.Any(char.IsControl);

    internal static bool IsUpperSha256(string value) =>
        value.Length == 64 && value.All(character => character is >= '0' and <= '9' or >= 'A' and <= 'F');

    internal static bool IsSafeFileName(string value) =>
        IsBoundedText(value, 256) &&
        string.Equals(value, Path.GetFileName(value), StringComparison.Ordinal) &&
        value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
        !value.Contains(':', StringComparison.Ordinal);

    private static bool HasExpectedMediaSignature(Ra2GenerationReferenceImage reference)
    {
        ReadOnlySpan<byte> content = reference.Content.Span;
        return reference.MediaKind switch
        {
            Ra2GenerationMediaKind.Png =>
                content.Length >= 8 && content[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            Ra2GenerationMediaKind.Jpeg =>
                content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF,
            Ra2GenerationMediaKind.Webp =>
                content.Length >= 12 && content[..4].SequenceEqual("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
