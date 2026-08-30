namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelAssemblyProbeFailureKind
{
    None = 0,
    MissingArtifact,
    UnexpectedArtifact,
    AmbiguousArtifactIdentity,
    InvalidVxl,
    InvalidHva,
    SectionMismatch
}

internal sealed record Ra2VoxelAssemblyPartFacts(
    string PartId,
    Ra2VoxelAssemblyPartRole Role,
    string? ParentPartId,
    Ra2VxlFileFacts Vxl,
    Ra2HvaFileFacts? Hva);

internal sealed class Ra2VoxelAssemblyProbeResult
{
    private Ra2VoxelAssemblyProbeResult(
        Ra2VoxelAssemblyProbeFailureKind failureKind,
        string message,
        IEnumerable<Ra2VoxelAssemblyPartFacts>? parts = null,
        string? failedPartId = null)
    {
        Ra2VoxelAssemblyPartFacts[] partArray = (parts ?? []).ToArray();
        bool succeeded = failureKind == Ra2VoxelAssemblyProbeFailureKind.None;
        if (succeeded != (partArray.Length > 0 && failedPartId is null) || (!succeeded && partArray.Length != 0))
            throw new ArgumentException("Voxel assembly probe payload does not match its failure state.");

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = string.IsNullOrWhiteSpace(message) ? "Voxel assembly probe failed." : message.Trim();
        Parts = Array.AsReadOnly(partArray);
        FailedPartId = failedPartId;
    }

    internal bool Succeeded { get; }
    internal Ra2VoxelAssemblyProbeFailureKind FailureKind { get; }
    internal string Message { get; }
    internal IReadOnlyList<Ra2VoxelAssemblyPartFacts> Parts { get; }
    internal string? FailedPartId { get; }

    internal static Ra2VoxelAssemblyProbeResult Success(IEnumerable<Ra2VoxelAssemblyPartFacts> parts)
        => new(Ra2VoxelAssemblyProbeFailureKind.None, "Voxel assembly probe completed.", parts);

    internal static Ra2VoxelAssemblyProbeResult Failure(
        Ra2VoxelAssemblyProbeFailureKind failureKind,
        string message,
        string? failedPartId)
    {
        if (failureKind == Ra2VoxelAssemblyProbeFailureKind.None)
            throw new ArgumentException("A failed assembly probe requires a failure kind.", nameof(failureKind));
        return new(failureKind, message, failedPartId: failedPartId);
    }
}

internal static class Ra2VoxelAssemblyProbe
{
    internal static Ra2VoxelAssemblyProbeResult Probe(
        Ra2VoxelAssetAssemblySpec assembly,
        IReadOnlyDictionary<string, byte[]> artifacts)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(artifacts);

        string[] expectedFileNames = assembly.Parts
            .SelectMany(part => part.HvaFileName is null
                ? [part.VxlFileName]
                : new[] { part.VxlFileName, part.HvaFileName })
            .ToArray();
        string? ambiguousFileName = artifacts.Keys
            .GroupBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (ambiguousFileName is not null)
        {
            return Ra2VoxelAssemblyProbeResult.Failure(
                Ra2VoxelAssemblyProbeFailureKind.AmbiguousArtifactIdentity,
                $"Artifact identity '{ambiguousFileName}' is ambiguous under case-insensitive file matching.",
                failedPartId: null);
        }

        string? unexpectedFileName = artifacts.Keys.FirstOrDefault(fileName =>
            !expectedFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase));
        if (unexpectedFileName is not null)
        {
            return Ra2VoxelAssemblyProbeResult.Failure(
                Ra2VoxelAssemblyProbeFailureKind.UnexpectedArtifact,
                $"Artifact '{unexpectedFileName}' is not declared by the voxel assembly.",
                failedPartId: null);
        }

        List<Ra2VoxelAssemblyPartFacts> parts = new(assembly.Parts.Count);
        foreach (Ra2VoxelAssemblyPartSpec part in assembly.Parts)
        {
            if (!TryGetArtifact(artifacts, part.VxlFileName, out byte[] vxlBytes))
            {
                return Ra2VoxelAssemblyProbeResult.Failure(
                    Ra2VoxelAssemblyProbeFailureKind.MissingArtifact,
                    $"Required VXL '{part.VxlFileName}' is missing.",
                    part.PartId);
            }

            using MemoryStream vxlStream = new(vxlBytes, writable: false);
            Ra2VoxelBinaryProbeResult<Ra2VxlFileFacts> vxlResult = Ra2VoxelBinaryProbe.ProbeVxl(vxlStream);
            if (!vxlResult.Succeeded)
            {
                return Ra2VoxelAssemblyProbeResult.Failure(
                    Ra2VoxelAssemblyProbeFailureKind.InvalidVxl,
                    $"VXL '{part.VxlFileName}' is invalid: {vxlResult.Message}",
                    part.PartId);
            }
            if (!vxlResult.Facts!.Sections.Any(section =>
                    string.Equals(section.Name, part.ExpectedSectionName, StringComparison.OrdinalIgnoreCase)))
            {
                return Ra2VoxelAssemblyProbeResult.Failure(
                    Ra2VoxelAssemblyProbeFailureKind.SectionMismatch,
                    $"VXL '{part.VxlFileName}' does not contain expected Section '{part.ExpectedSectionName}'.",
                    part.PartId);
            }

            Ra2HvaFileFacts? hvaFacts = null;
            if (part.HvaFileName is not null)
            {
                if (!TryGetArtifact(artifacts, part.HvaFileName, out byte[] hvaBytes))
                {
                    return Ra2VoxelAssemblyProbeResult.Failure(
                        Ra2VoxelAssemblyProbeFailureKind.MissingArtifact,
                        $"Required HVA '{part.HvaFileName}' is missing.",
                        part.PartId);
                }

                using MemoryStream hvaStream = new(hvaBytes, writable: false);
                Ra2VoxelBinaryProbeResult<Ra2HvaFileFacts> hvaResult = Ra2VoxelBinaryProbe.ProbeHva(hvaStream);
                if (!hvaResult.Succeeded)
                {
                    return Ra2VoxelAssemblyProbeResult.Failure(
                        Ra2VoxelAssemblyProbeFailureKind.InvalidHva,
                        $"HVA '{part.HvaFileName}' is invalid: {hvaResult.Message}",
                        part.PartId);
                }
                bool matchesExpectedSection = hvaResult.Facts!.SectionNames.Contains(
                    part.ExpectedSectionName,
                    StringComparer.OrdinalIgnoreCase);
                bool hasUnambiguousLegacyUnnamedSection =
                    hvaResult.Facts.SectionNames.Count == 1 &&
                    string.IsNullOrWhiteSpace(hvaResult.Facts.SectionNames[0]) &&
                    vxlResult.Facts.Sections.Count == 1;
                if (!matchesExpectedSection && !hasUnambiguousLegacyUnnamedSection)
                {
                    return Ra2VoxelAssemblyProbeResult.Failure(
                        Ra2VoxelAssemblyProbeFailureKind.SectionMismatch,
                        $"HVA '{part.HvaFileName}' does not contain expected Section '{part.ExpectedSectionName}'.",
                        part.PartId);
                }
                hvaFacts = hvaResult.Facts;
            }

            parts.Add(new Ra2VoxelAssemblyPartFacts(
                part.PartId,
                part.Role,
                part.ParentPartId,
                vxlResult.Facts,
                hvaFacts));
        }

        return Ra2VoxelAssemblyProbeResult.Success(parts);
    }

    private static bool TryGetArtifact(
        IReadOnlyDictionary<string, byte[]> artifacts,
        string fileName,
        out byte[] content)
    {
        foreach ((string candidateName, byte[] candidateContent) in artifacts)
        {
            if (string.Equals(candidateName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                content = candidateContent;
                return content is not null;
            }
        }

        content = null!;
        return false;
    }
}
