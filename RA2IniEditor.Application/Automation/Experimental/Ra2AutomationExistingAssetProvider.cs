namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationExistingAssetProvider : IRa2AutomationAssetProvider
{
    public const int MaximumAggregateContentBytes = 64 * 1024 * 1024;

    private static readonly Ra2AutomationAssetProviderDescriptor Descriptor = new(
        "existing-asset-passthrough",
        1,
        [
            Ra2AutomationAssetKind.ShpAnimation,
            Ra2AutomationAssetKind.Cameo,
            Ra2AutomationAssetKind.VxlModel,
            Ra2AutomationAssetKind.HvaAnimation
        ]);

    public Ra2AutomationAssetProviderDescriptor GetDescriptor() => Descriptor;

    public Ra2AutomationAssetProviderResult Resolve(
        Ra2AutomationAssetManifest manifest,
        IReadOnlyList<Ra2AutomationAssetSource> sources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(sources);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string[] pendingSchema = manifest.Requirements
                .Where(requirement => requirement.Bindings.Any(binding => binding.State != Ra2AutomationAssetBindingState.Proposed))
                .Select(requirement => requirement.RequirementId)
                .ToArray();
            if (pendingSchema.Length != 0)
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.InvalidManifest, "The asset manifest contains unresolved INI bindings.", pendingSchema);

            string[] invalidManifest = manifest.Requirements
                .Where(requirement =>
                    !Descriptor.SupportedKinds.Contains(requirement.Kind) ||
                    !Ra2AutomationAssetContractValidation.HasExpectedExtension(requirement.FileName, requirement.Kind))
                .Select(requirement => requirement.RequirementId)
                .ToArray();
            if (invalidManifest.Length != 0)
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.InvalidManifest, "The asset manifest contains an unsupported kind or file extension.", invalidManifest);

            if (sources.Any(source => source is null))
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.SourceMismatch, "The asset source list contains an invalid item.");

            string[] duplicateSources = sources
                .GroupBy(source => source.RequirementId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
            if (duplicateSources.Length != 0)
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.SourceMismatch, "Asset sources must be unique by requirement identity.", duplicateSources);

            Dictionary<string, Ra2AutomationAssetSource> sourceByRequirement = sources.ToDictionary(source => source.RequirementId, StringComparer.Ordinal);
            string[] expectedIds = manifest.Requirements.Select(requirement => requirement.RequirementId).ToArray();
            string[] missing = expectedIds.Where(id => !sourceByRequirement.ContainsKey(id)).ToArray();
            if (missing.Length != 0)
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.MissingSource, "One or more required asset sources are missing.", missing);

            HashSet<string> expectedSet = new(expectedIds, StringComparer.Ordinal);
            string[] unexpected = sources
                .Where(source => !expectedSet.Contains(source.RequirementId))
                .Select(source => source.RequirementId)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
            if (unexpected.Length != 0)
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.UnexpectedSource, "The request contains sources not declared by the manifest.", unexpected);

            string[] mismatched = manifest.Requirements
                .Where(requirement =>
                {
                    Ra2AutomationAssetSource source = sourceByRequirement[requirement.RequirementId];
                    return source.Kind != requirement.Kind ||
                           !string.Equals(source.FileName, requirement.FileName, StringComparison.OrdinalIgnoreCase) ||
                           !Ra2AutomationAssetContractValidation.HasExpectedExtension(source.FileName, source.Kind);
                })
                .Select(requirement => requirement.RequirementId)
                .ToArray();
            if (mismatched.Length != 0)
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.SourceMismatch, "One or more asset sources do not match their manifest requirements.", mismatched);

            long aggregateBytes = sources.Sum(source => (long)source.ContentLength);
            if (aggregateBytes > MaximumAggregateContentBytes)
                return Failure(manifest, Ra2AutomationAssetProviderFailureKind.AggregateContentLimitExceeded, "The aggregate asset content exceeds the provider limit.", expectedIds);

            List<Ra2AutomationAssetArtifact> artifacts = new(manifest.Requirements.Count);
            foreach (Ra2AutomationAssetRequirement requirement in manifest.Requirements)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2AutomationAssetSource source = sourceByRequirement[requirement.RequirementId];
                byte[] content = source.CopyContent();
                artifacts.Add(new Ra2AutomationAssetArtifact(
                    requirement.RequirementId,
                    requirement.FileName,
                    requirement.Kind,
                    content,
                    Ra2AutomationAssetVerificationLevel.IdentityExtensionAndHash));
            }

            cancellationToken.ThrowIfCancellationRequested();
            return Ra2AutomationAssetProviderResult.CreateSuccess(
                manifest,
                Descriptor,
                "All manifest requirements were resolved to bounded existing-asset artifacts.",
                artifacts);
        }
        catch (OperationCanceledException)
        {
            return Failure(manifest, Ra2AutomationAssetProviderFailureKind.Canceled, "Asset resolution was canceled.");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            return Failure(manifest, Ra2AutomationAssetProviderFailureKind.ProviderFailed, "Asset resolution failed unexpectedly.");
        }
    }

    private static Ra2AutomationAssetProviderResult Failure(
        Ra2AutomationAssetManifest manifest,
        Ra2AutomationAssetProviderFailureKind failureKind,
        string message,
        IEnumerable<string>? relatedRequirementIds = null)
        => Ra2AutomationAssetProviderResult.CreateFailure(
            manifest,
            Descriptor,
            failureKind,
            message,
            relatedRequirementIds);
}
