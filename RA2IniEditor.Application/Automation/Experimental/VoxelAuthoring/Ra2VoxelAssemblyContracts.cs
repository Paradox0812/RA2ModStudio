namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelAssemblyPartRole
{
    Body = 0,
    Turret,
    Barrel,
    Other
}

internal sealed class Ra2VoxelAssemblyPartSpec
{
    internal const int MaximumIdentityLength = 64;
    internal const int MaximumSectionNameLength = 16;

    internal Ra2VoxelAssemblyPartSpec(
        string partId,
        Ra2VoxelAssemblyPartRole role,
        string fileStem,
        string expectedSectionName,
        string? parentPartId,
        bool requiresHva)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));

        PartId = ValidateIdentity(partId, MaximumIdentityLength, nameof(partId));
        FileStem = ValidateFileStem(fileStem, nameof(fileStem));
        ExpectedSectionName = ValidateIdentity(
            expectedSectionName,
            MaximumSectionNameLength,
            nameof(expectedSectionName));
        ParentPartId = parentPartId is null
            ? null
            : ValidateIdentity(parentPartId, MaximumIdentityLength, nameof(parentPartId));
        Role = role;
        RequiresHva = requiresHva;
    }

    internal string PartId { get; }
    internal Ra2VoxelAssemblyPartRole Role { get; }
    internal string FileStem { get; }
    internal string ExpectedSectionName { get; }
    internal string? ParentPartId { get; }
    internal bool RequiresHva { get; }
    internal string VxlFileName => $"{FileStem}.vxl";
    internal string? HvaFileName => RequiresHva ? $"{FileStem}.hva" : null;

    private static string ValidateIdentity(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Voxel assembly identity cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength ||
            normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
        {
            throw new ArgumentException("Voxel assembly identity is invalid or exceeds its limit.", parameterName);
        }

        return normalized;
    }

    private static string ValidateFileStem(string value, string parameterName)
    {
        string normalized = ValidateIdentity(value, MaximumIdentityLength, parameterName);
        if (normalized is "." or ".." ||
            normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            normalized.Contains('.'))
        {
            throw new ArgumentException("Voxel assembly file stem must be a simple extension-free file name.", parameterName);
        }

        return normalized;
    }
}

internal sealed class Ra2VoxelAssetAssemblySpec
{
    internal const int MaximumPartCount = 16;

    internal Ra2VoxelAssetAssemblySpec(
        string assemblyId,
        IEnumerable<Ra2VoxelAssemblyPartSpec> parts)
    {
        if (string.IsNullOrWhiteSpace(assemblyId))
            throw new ArgumentException("Voxel assembly identity cannot be empty.", nameof(assemblyId));

        string normalizedAssemblyId = assemblyId.Trim();
        if (normalizedAssemblyId.Length > Ra2VoxelAssemblyPartSpec.MaximumIdentityLength ||
            normalizedAssemblyId.IndexOfAny(['\r', '\n', '\0']) >= 0)
        {
            throw new ArgumentException("Voxel assembly identity is invalid or exceeds its limit.", nameof(assemblyId));
        }

        ArgumentNullException.ThrowIfNull(parts);
        Ra2VoxelAssemblyPartSpec[] partArray = parts.ToArray();
        if (partArray.Length is < 1 or > MaximumPartCount || partArray.Any(part => part is null))
            throw new ArgumentOutOfRangeException(nameof(parts));
        if (partArray.GroupBy(part => part.PartId, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new ArgumentException("Voxel assembly part identities must be unique.", nameof(parts));
        if (partArray.GroupBy(part => part.FileStem, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new ArgumentException("Voxel assembly file stems must be unique.", nameof(parts));

        Ra2VoxelAssemblyPartSpec[] bodyParts = partArray
            .Where(part => part.Role == Ra2VoxelAssemblyPartRole.Body)
            .ToArray();
        if (bodyParts.Length != 1 || bodyParts[0].ParentPartId is not null)
            throw new ArgumentException("A voxel assembly requires exactly one root Body part.", nameof(parts));

        Dictionary<string, Ra2VoxelAssemblyPartSpec> byId = partArray.ToDictionary(
            part => part.PartId,
            StringComparer.OrdinalIgnoreCase);
        foreach (Ra2VoxelAssemblyPartSpec part in partArray)
        {
            if (part.Role != Ra2VoxelAssemblyPartRole.Body && part.ParentPartId is null)
                throw new ArgumentException($"Non-body part '{part.PartId}' requires a parent.", nameof(parts));
            if (part.ParentPartId is not null && !byId.ContainsKey(part.ParentPartId))
                throw new ArgumentException($"Part '{part.PartId}' references an unknown parent.", nameof(parts));

            HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
            Ra2VoxelAssemblyPartSpec current = part;
            while (current.ParentPartId is not null)
            {
                if (!visited.Add(current.PartId))
                    throw new ArgumentException("Voxel assembly parent relationships cannot contain a cycle.", nameof(parts));
                current = byId[current.ParentPartId];
            }

            if (current.Role != Ra2VoxelAssemblyPartRole.Body)
                throw new ArgumentException($"Part '{part.PartId}' is not connected to the root Body.", nameof(parts));
        }

        AssemblyId = normalizedAssemblyId;
        Parts = Array.AsReadOnly(partArray);
    }

    internal string AssemblyId { get; }
    internal IReadOnlyList<Ra2VoxelAssemblyPartSpec> Parts { get; }
}
