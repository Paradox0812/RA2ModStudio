using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelSemanticPartRole
{
    Unknown = 0,
    BodyShell,
    Turret,
    Barrel,
    Wheel,
    Track,
    Antenna,
    Attachment
}

internal enum Ra2VoxelSemanticMaterialRole
{
    Unknown = 0,
    PaintedSurface,
    Glass,
    Rubber,
    BareMetal,
    Light,
    DarkOpening,
    Accent
}

internal enum Ra2VoxelSemanticRemapIntent
{
    None = 0,
    Candidate,
    ExplicitlyApproved
}

internal enum Ra2VoxelSemanticAssignmentSource
{
    Unknown = 0,
    AgentSuggestion,
    HumanOverride
}

internal sealed record Ra2VoxelSemanticRegionEvidence(
    string RegionId,
    string? MirrorRegionId,
    int CellCount,
    int MinimumX,
    int MaximumX,
    int MinimumY,
    int MaximumY,
    int MinimumZ,
    int MaximumZ,
    double SurfaceRatio,
    double MirrorCoverage,
    IReadOnlyList<byte> Selected);

internal sealed class Ra2VoxelSemanticEvidencePackage
{
    private readonly Ra2VoxelSemanticRegionEvidence[] _regions;

    internal Ra2VoxelSemanticEvidencePackage(
        string sourceSnapshotHash,
        int xSize,
        int ySize,
        int zSize,
        IEnumerable<Ra2VoxelSemanticRegionEvidence> regions)
    {
        SourceSnapshotHash = RequireHash(sourceSnapshotHash);
        XSize = xSize;
        YSize = ySize;
        ZSize = zSize;
        _regions = (regions ?? throw new ArgumentNullException(nameof(regions))).ToArray();
        if (_regions.Length is < 1 or > 48 || _regions.Select(value => value.RegionId).Distinct(StringComparer.Ordinal).Count() != _regions.Length)
            throw new ArgumentException("Semantic evidence must contain unique bounded regions.", nameof(regions));
        if (_regions.Any(value => value.Selected.Count == 0 || value.Selected.Count > Ra2VoxelSceneSnapshot.MaximumOccupancyCount ||
            value.Selected.Any(selected => selected is not (0 or 1))))
            throw new ArgumentException("A semantic region contains an invalid binary mask.", nameof(regions));
        PackageHash = ComputeHash();
    }

    internal string SourceSnapshotHash { get; }
    internal int XSize { get; }
    internal int YSize { get; }
    internal int ZSize { get; }
    internal IReadOnlyList<Ra2VoxelSemanticRegionEvidence> Regions => Array.AsReadOnly(_regions);
    internal string PackageHash { get; }

    internal string ToPromptText(string? userInstructions)
    {
        StringBuilder text = new();
        text.AppendLine("semantic_evidence_schema: ra2-voxel-semantic/1");
        text.AppendLine($"source_snapshot_hash: {SourceSnapshotHash}");
        text.AppendLine($"evidence_hash: {PackageHash}");
        text.AppendLine($"grid: {XSize}x{YSize}x{ZSize}");
        text.AppendLine("coordinates: X=left/right, Y=front/back depth, Z=up; evidence contains geometry facts only, no image pixels or inferred colours");
        if (!string.IsNullOrWhiteSpace(userInstructions))
            text.AppendLine($"user_semantic_instructions: {NormalizeLine(userInstructions, 2048)}");
        foreach (Ra2VoxelSemanticRegionEvidence region in _regions)
        {
            text.Append("region: ").Append(region.RegionId)
                .Append(" mirror=").Append(region.MirrorRegionId ?? "none")
                .Append(" cells=").Append(region.CellCount)
                .Append(" bounds=").Append(region.MinimumX).Append("..").Append(region.MaximumX).Append(',')
                .Append(region.MinimumY).Append("..").Append(region.MaximumY).Append(',')
                .Append(region.MinimumZ).Append("..").Append(region.MaximumZ)
                .Append(" surface_ratio=").Append(region.SurfaceRatio.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
                .Append(" mirror_coverage=").Append(region.MirrorCoverage.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
                .AppendLine();
        }
        return text.ToString();
    }

    private string ComputeHash()
    {
        StringBuilder canonical = new();
        canonical.Append(SourceSnapshotHash).Append('|').Append(XSize).Append('|').Append(YSize).Append('|').Append(ZSize);
        foreach (Ra2VoxelSemanticRegionEvidence region in _regions.OrderBy(value => value.RegionId, StringComparer.Ordinal))
        {
            canonical.Append('|').Append(region.RegionId).Append('|').Append(region.MirrorRegionId)
                .Append('|').Append(region.CellCount).Append('|').Append(region.MinimumX).Append('|').Append(region.MaximumX)
                .Append('|').Append(region.MinimumY).Append('|').Append(region.MaximumY).Append('|').Append(region.MinimumZ)
                .Append('|').Append(region.MaximumZ).Append('|').Append(region.SurfaceRatio.ToString("R", System.Globalization.CultureInfo.InvariantCulture))
                .Append('|').Append(region.MirrorCoverage.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
    }

    private static string RequireHash(string value) => value.Length == 64 && value.All(char.IsAsciiHexDigit)
        ? value.ToUpperInvariant()
        : throw new ArgumentException("A canonical SHA-256 value is required.", nameof(value));

    private static string NormalizeLine(string value, int maximum)
    {
        string normalized = string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= maximum ? normalized : normalized[..maximum];
    }
}

internal static class Ra2VoxelSemanticEvidenceBuilder
{
    private const int XBands = 2;
    private const int YBands = 4;
    private const int ZBands = 3;

    internal static Ra2VoxelSemanticEvidencePackage Build(Ra2VoxelSceneSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.OccupancyCount == 0)
            throw new ArgumentException("Semantic evidence requires occupied voxels.", nameof(snapshot));

        Dictionary<(int X, int Y, int Z), List<int>> bins = [];
        HashSet<Ra2VoxelCoordinate> occupied = snapshot.Cells.Select(cell => cell.Coordinate).ToHashSet();
        for (int index = 0; index < snapshot.Cells.Count; index++)
        {
            if ((index & 4095) == 0) cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate coordinate = snapshot.Cells[index].Coordinate;
            var key = (Band(coordinate.X, snapshot.Part.XSize, XBands),
                Band(coordinate.Y, snapshot.Part.YSize, YBands),
                Band(coordinate.Z, snapshot.Part.ZSize, ZBands));
            if (!bins.TryGetValue(key, out List<int>? indices)) bins[key] = indices = [];
            indices.Add(index);
        }

        List<Ra2VoxelSemanticRegionEvidence> regions = [];
        foreach (((int x, int y, int z) key, List<int> indices) in bins.OrderBy(value => value.Key.Z).ThenBy(value => value.Key.Y).ThenBy(value => value.Key.X))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string id = $"spatial-x{key.x}-y{key.y}-z{key.z}";
            string mirror = $"spatial-x{XBands - 1 - key.x}-y{key.y}-z{key.z}";
            byte[] selected = new byte[snapshot.OccupancyCount];
            int surface = 0;
            int mirrored = 0;
            foreach (int index in indices)
            {
                selected[index] = 1;
                Ra2VoxelCoordinate c = snapshot.Cells[index].Coordinate;
                if (IsSurface(c, occupied)) surface++;
                if (occupied.Contains(new(snapshot.Part.XSize - 1 - c.X, c.Y, c.Z))) mirrored++;
            }
            Ra2VoxelCoordinate[] coordinates = indices.Select(index => snapshot.Cells[index].Coordinate).ToArray();
            regions.Add(new(id, mirror, indices.Count,
                coordinates.Min(c => c.X), coordinates.Max(c => c.X),
                coordinates.Min(c => c.Y), coordinates.Max(c => c.Y),
                coordinates.Min(c => c.Z), coordinates.Max(c => c.Z),
                surface / (double)indices.Count, mirrored / (double)indices.Count,
                Array.AsReadOnly(selected)));
        }
        return new(snapshot.CanonicalHash, snapshot.Part.XSize, snapshot.Part.YSize, snapshot.Part.ZSize, regions);
    }

    internal static IReadOnlyList<Ra2VoxelExplicitMask> MaterializeMasks(
        Ra2VoxelSemanticEvidencePackage evidence,
        IEnumerable<Ra2VoxelSemanticEffectiveAssignment> assignments)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        Dictionary<string, Ra2VoxelSemanticEffectiveAssignment> byRegion = (assignments ?? throw new ArgumentNullException(nameof(assignments)))
            .ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        List<Ra2VoxelExplicitMask> masks = [];
        foreach (Ra2VoxelSemanticRegionEvidence region in evidence.Regions)
        {
            if (!byRegion.TryGetValue(region.RegionId, out Ra2VoxelSemanticEffectiveAssignment? assignment) ||
                assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.Unknown)
                continue;
            masks.Add(new($"semantic.{region.RegionId}", evidence.SourceSnapshotHash, region.Selected));
        }
        return Array.AsReadOnly(masks.ToArray());
    }

    private static int Band(int value, int size, int count) => Math.Min(count - 1, value * count / Math.Max(1, size));

    private static bool IsSurface(Ra2VoxelCoordinate c, HashSet<Ra2VoxelCoordinate> occupied) =>
        !occupied.Contains(new(c.X - 1, c.Y, c.Z)) || !occupied.Contains(new(c.X + 1, c.Y, c.Z)) ||
        !occupied.Contains(new(c.X, c.Y - 1, c.Z)) || !occupied.Contains(new(c.X, c.Y + 1, c.Z)) ||
        !occupied.Contains(new(c.X, c.Y, c.Z - 1)) || !occupied.Contains(new(c.X, c.Y, c.Z + 1));
}

internal sealed record Ra2VoxelSemanticAssignment(
    string RegionId,
    Ra2VoxelSemanticPartRole PartRole,
    Ra2VoxelSemanticMaterialRole MaterialRole,
    Ra2VoxelSemanticRemapIntent RemapIntent,
    double Confidence,
    string Reason);

internal sealed record Ra2VoxelSemanticEffectiveAssignment(
    string RegionId,
    Ra2VoxelSemanticPartRole PartRole,
    Ra2VoxelSemanticMaterialRole MaterialRole,
    Ra2VoxelSemanticRemapIntent RemapIntent,
    Ra2VoxelSemanticAssignmentSource Source,
    double Confidence,
    string Reason);

internal static class Ra2VoxelSemanticLayerResolver
{
    internal static IReadOnlyList<Ra2VoxelSemanticEffectiveAssignment> Resolve(
        Ra2VoxelSemanticEvidencePackage evidence,
        IEnumerable<Ra2VoxelSemanticAssignment>? suggestions,
        IEnumerable<Ra2VoxelSemanticAssignment>? humanOverrides)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        Dictionary<string, Ra2VoxelSemanticAssignment> ai = Normalize(evidence, suggestions, allowApprovedRemap: false);
        Dictionary<string, Ra2VoxelSemanticAssignment> human = Normalize(evidence, humanOverrides, allowApprovedRemap: true);
        return Array.AsReadOnly(evidence.Regions.Select(region =>
        {
            if (human.TryGetValue(region.RegionId, out Ra2VoxelSemanticAssignment? manual))
                return Effective(manual, Ra2VoxelSemanticAssignmentSource.HumanOverride);
            if (ai.TryGetValue(region.RegionId, out Ra2VoxelSemanticAssignment? suggested))
                return Effective(suggested with { RemapIntent = suggested.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved
                    ? Ra2VoxelSemanticRemapIntent.Candidate : suggested.RemapIntent }, Ra2VoxelSemanticAssignmentSource.AgentSuggestion);
            return new(region.RegionId, Ra2VoxelSemanticPartRole.Unknown, Ra2VoxelSemanticMaterialRole.Unknown,
                Ra2VoxelSemanticRemapIntent.None, Ra2VoxelSemanticAssignmentSource.Unknown, 0d, "未分类");
        }).ToArray());
    }

    private static Dictionary<string, Ra2VoxelSemanticAssignment> Normalize(
        Ra2VoxelSemanticEvidencePackage evidence,
        IEnumerable<Ra2VoxelSemanticAssignment>? values,
        bool allowApprovedRemap)
    {
        HashSet<string> valid = evidence.Regions.Select(value => value.RegionId).ToHashSet(StringComparer.Ordinal);
        Dictionary<string, Ra2VoxelSemanticAssignment> result = new(StringComparer.Ordinal);
        foreach (Ra2VoxelSemanticAssignment item in values ?? [])
        {
            if (!valid.Contains(item.RegionId) || !Enum.IsDefined(item.PartRole) || !Enum.IsDefined(item.MaterialRole) ||
                !Enum.IsDefined(item.RemapIntent) || !double.IsFinite(item.Confidence) || item.Confidence is < 0d or > 1d)
                continue;
            if (!allowApprovedRemap && item.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved)
                continue;
            result[item.RegionId] = item with { Reason = NormalizeReason(item.Reason) };
        }
        return result;
    }

    private static Ra2VoxelSemanticEffectiveAssignment Effective(Ra2VoxelSemanticAssignment value, Ra2VoxelSemanticAssignmentSource source) =>
        new(value.RegionId, value.PartRole, value.MaterialRole, value.RemapIntent, source, value.Confidence, value.Reason);

    private static string NormalizeReason(string? value)
    {
        string normalized = string.Join(" ", (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 512 ? normalized : normalized[..512];
    }
}

internal sealed record Ra2VoxelSemanticStyleIntegrationResult(
    Ra2CompiledVoxelStylePlan Plan,
    IReadOnlyList<Ra2VoxelExplicitMask> Masks,
    IReadOnlyList<string> UnresolvedRegions);

internal static class Ra2VoxelSemanticStyleIntegrator
{
    internal static Ra2VoxelSemanticStyleIntegrationResult Integrate(
        Ra2CompiledVoxelStylePlan normalizedPlan,
        Ra2VoxelSemanticMaskComposition composition,
        Ra2VoxelSemanticColourRequirements requirements,
        Ra2VoxelSemanticColourBindingPlan bindingPlan,
        string rawCompiledPlanHash)
    {
        ArgumentNullException.ThrowIfNull(normalizedPlan);
        ArgumentNullException.ThrowIfNull(composition);
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(bindingPlan);
        if (!string.Equals(composition.SourceSnapshotHash, requirements.SourceSnapshotHash, StringComparison.Ordinal) ||
            !string.Equals(composition.CompositionHash, requirements.CompositionHash, StringComparison.Ordinal) ||
            composition.CellCount != requirements.CellCount ||
            !string.Equals(bindingPlan.RequirementShapeHash, requirements.RequirementShapeHash, StringComparison.Ordinal) ||
            !string.Equals(bindingPlan.CompiledPlanHash, rawCompiledPlanHash, StringComparison.Ordinal))
        {
            throw new ArgumentException("Semantic colour integration identities do not match.");
        }

        Dictionary<string, Ra2CompiledVoxelStyleRole> roles = normalizedPlan.Roles
            .ToDictionary(role => role.Id, StringComparer.Ordinal);
        List<Ra2CompiledVoxelStyleRule> rules = normalizedPlan.Rules
            .Where(rule => rule.Region != Ra2VoxelStyleRegionKind.ExplicitMask)
            .ToList();
        List<Ra2VoxelExplicitMask> masks = [];
        List<string> unresolved = [];
        foreach (Ra2VoxelSemanticColourBinding binding in bindingPlan.Bindings
                     .Where(value => value.Requirement != Ra2VoxelSemanticColourRequirementKind.PaintedSurface &&
                                     value.Requirement != Ra2VoxelSemanticColourRequirementKind.ApprovedRemap)
                     .OrderBy(value => value.Requirement))
        {
            if (!roles.TryGetValue(binding.RoleId, out Ra2CompiledVoxelStyleRole? role))
                throw new ArgumentException("A semantic binding role is missing from the normalized style plan.");
            byte[] selected = new byte[composition.CellCount];
            for (int index = 0; index < composition.CellCount; index++)
            {
                if (Matches(binding.Requirement, composition[index].MaterialRole))
                    selected[index] = 1;
            }
            string maskId = $"semantic.binding.{Format(binding.Requirement)}";
            masks.Add(new(maskId, composition.SourceSnapshotHash, selected));
            rules.Add(new(
                Ra2VoxelStyleRegionKind.ExplicitMask,
                role.Id,
                Ra2VoxelStyleEvidenceKind.ExplicitUserMask,
                maskId,
                IsPaintable: true,
                role.SourceScopeIds));
        }

        Ra2VoxelSemanticColourBinding? remapBinding = bindingPlan.Bindings.SingleOrDefault(value =>
            value.Requirement == Ra2VoxelSemanticColourRequirementKind.ApprovedRemap);
        if (remapBinding is not null)
        {
            if (!roles.TryGetValue(remapBinding.RoleId, out Ra2CompiledVoxelStyleRole? remapRole))
                throw new ArgumentException("The approved remap binding role is missing from the normalized style plan.");
            byte[] selected = composition.Assignments
                .Select(value => value.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved ? (byte)1 : (byte)0)
                .ToArray();
            const string maskId = "semantic.binding.approved-remap";
            masks.Add(new(maskId, composition.SourceSnapshotHash, selected));
            rules.Add(new(
                Ra2VoxelStyleRegionKind.ExplicitMask,
                remapRole.Id,
                Ra2VoxelStyleEvidenceKind.ExplicitUserMask,
                maskId,
                IsPaintable: true,
                remapRole.SourceScopeIds));
        }

        Ra2CompiledVoxelStylePlan plan = new(
            normalizedPlan.Title,
            normalizedPlan.Summary,
            normalizedPlan.SourcePackHash,
            normalizedPlan.PaletteHash,
            normalizedPlan.CompilerRevision + "+semantic-binding/1",
            normalizedPlan.ModelIdentity,
            remapBinding is null ? Ra2VoxelStyleRemapPolicy.None : Ra2VoxelStyleRemapPolicy.ExplicitMask,
            normalizedPlan.InteriorRoleId,
            normalizedPlan.Roles,
            rules,
            normalizedPlan.UnresolvedAssumptions.Concat(unresolved));
        return new(plan, Array.AsReadOnly(masks.ToArray()), Array.AsReadOnly(unresolved.ToArray()));
    }

    internal static Ra2VoxelSemanticStyleIntegrationResult Integrate(
        Ra2CompiledVoxelStylePlan basePlan,
        Ra2VoxelSemanticEvidencePackage evidence,
        IEnumerable<Ra2VoxelSemanticEffectiveAssignment> assignments)
    {
        ArgumentNullException.ThrowIfNull(basePlan);
        ArgumentNullException.ThrowIfNull(evidence);
        Ra2VoxelSemanticEffectiveAssignment[] effective = (assignments ?? throw new ArgumentNullException(nameof(assignments))).ToArray();
        Dictionary<Ra2VoxelStyleRoleCategory, Ra2CompiledVoxelStyleRole> roles = basePlan.Roles
            .GroupBy(role => role.Category)
            .ToDictionary(group => group.Key, group => group.First());
        List<Ra2CompiledVoxelStyleRule> rules = basePlan.Rules.ToList();
        List<Ra2VoxelExplicitMask> masks = [];
        List<string> unresolved = [];
        Dictionary<string, Ra2VoxelSemanticRegionEvidence> regions = evidence.Regions.ToDictionary(value => value.RegionId, StringComparer.Ordinal);
        bool hasApprovedRemap = false;

        foreach (Ra2VoxelSemanticEffectiveAssignment assignment in effective)
        {
            if (!regions.TryGetValue(assignment.RegionId, out Ra2VoxelSemanticRegionEvidence? region) ||
                assignment.MaterialRole == Ra2VoxelSemanticMaterialRole.Unknown)
                continue;
            Ra2VoxelStyleRoleCategory category = MapCategory(assignment.MaterialRole);
            if (assignment.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved)
            {
                category = Ra2VoxelStyleRoleCategory.Remap;
                hasApprovedRemap = true;
            }
            if (!roles.TryGetValue(category, out Ra2CompiledVoxelStyleRole? role))
            {
                unresolved.Add($"{assignment.RegionId}: 风格计划没有 {assignment.MaterialRole} 对应的颜色角色");
                continue;
            }
            string maskId = $"semantic.{assignment.RegionId}";
            masks.Add(new(maskId, evidence.SourceSnapshotHash, region.Selected));
            rules.Add(new(
                Ra2VoxelStyleRegionKind.ExplicitMask,
                role.Id,
                Ra2VoxelStyleEvidenceKind.ExplicitUserMask,
                maskId,
                IsPaintable: true,
                role.SourceScopeIds));
        }

        Ra2CompiledVoxelStylePlan plan = new(
            basePlan.Title,
            basePlan.Summary,
            basePlan.SourcePackHash,
            basePlan.PaletteHash,
            basePlan.CompilerRevision + "+semantic-mask/1",
            basePlan.ModelIdentity,
            hasApprovedRemap ? Ra2VoxelStyleRemapPolicy.ExplicitMask : basePlan.RemapPolicy,
            basePlan.InteriorRoleId,
            basePlan.Roles,
            rules,
            basePlan.UnresolvedAssumptions.Concat(unresolved));
        return new(plan, Array.AsReadOnly(masks.ToArray()), Array.AsReadOnly(unresolved.ToArray()));
    }

    internal static Ra2VoxelSemanticStyleIntegrationResult Integrate(
        Ra2CompiledVoxelStylePlan basePlan,
        Ra2VoxelSemanticMaskComposition composition)
    {
        ArgumentNullException.ThrowIfNull(basePlan);
        ArgumentNullException.ThrowIfNull(composition);
        Dictionary<Ra2VoxelStyleRoleCategory, Ra2CompiledVoxelStyleRole> roles = basePlan.Roles
            .GroupBy(role => role.Category)
            .ToDictionary(group => group.Key, group => group.First());
        List<Ra2CompiledVoxelStyleRule> rules = basePlan.Rules.ToList();
        List<Ra2VoxelExplicitMask> masks = [];
        List<string> unresolved = [];
        bool hasApprovedRemap = false;
        var groups = composition.Assignments
            .Select((assignment, index) => (assignment, index))
            .Where(value => value.assignment.MaterialRole != Ra2VoxelSemanticMaterialRole.Unknown)
            .GroupBy(value => new
            {
                value.assignment.PartRole,
                value.assignment.MaterialRole,
                value.assignment.RemapIntent
            })
            .OrderBy(value => value.Key.MaterialRole)
            .ThenBy(value => value.Key.PartRole)
            .ThenBy(value => value.Key.RemapIntent)
            .ToArray();
        int groupIndex = 0;
        foreach (var group in groups)
        {
            Ra2VoxelStyleRoleCategory category = MapCategory(group.Key.MaterialRole);
            if (group.Key.RemapIntent == Ra2VoxelSemanticRemapIntent.ExplicitlyApproved)
            {
                category = Ra2VoxelStyleRoleCategory.Remap;
                hasApprovedRemap = true;
            }
            if (!roles.TryGetValue(category, out Ra2CompiledVoxelStyleRole? role))
            {
                unresolved.Add($"{group.Key.PartRole}/{group.Key.MaterialRole}: 风格计划没有对应的颜色角色");
                continue;
            }
            byte[] selected = new byte[composition.CellCount];
            foreach ((Ra2VoxelSemanticEffectiveAssignment _, int index) in group)
                selected[index] = 1;
            string maskId = $"semantic.composed.{groupIndex++}";
            masks.Add(new(maskId, composition.SourceSnapshotHash, selected));
            rules.Add(new(
                Ra2VoxelStyleRegionKind.ExplicitMask,
                role.Id,
                Ra2VoxelStyleEvidenceKind.ExplicitUserMask,
                maskId,
                IsPaintable: true,
                role.SourceScopeIds));
        }

        Ra2CompiledVoxelStylePlan plan = new(
            basePlan.Title,
            basePlan.Summary,
            basePlan.SourcePackHash,
            basePlan.PaletteHash,
            basePlan.CompilerRevision + "+semantic-mask/2",
            basePlan.ModelIdentity,
            hasApprovedRemap ? Ra2VoxelStyleRemapPolicy.ExplicitMask : basePlan.RemapPolicy,
            basePlan.InteriorRoleId,
            basePlan.Roles,
            rules,
            basePlan.UnresolvedAssumptions.Concat(unresolved));
        return new(plan, Array.AsReadOnly(masks.ToArray()), Array.AsReadOnly(unresolved.ToArray()));
    }

    private static Ra2VoxelStyleRoleCategory MapCategory(Ra2VoxelSemanticMaterialRole value) => value switch
    {
        Ra2VoxelSemanticMaterialRole.PaintedSurface => Ra2VoxelStyleRoleCategory.BodyBase,
        Ra2VoxelSemanticMaterialRole.Glass => Ra2VoxelStyleRoleCategory.Glass,
        Ra2VoxelSemanticMaterialRole.Rubber => Ra2VoxelStyleRoleCategory.Rubber,
        Ra2VoxelSemanticMaterialRole.BareMetal => Ra2VoxelStyleRoleCategory.BareMetal,
        Ra2VoxelSemanticMaterialRole.Light => Ra2VoxelStyleRoleCategory.Accent,
        Ra2VoxelSemanticMaterialRole.DarkOpening => Ra2VoxelStyleRoleCategory.BodyDark,
        Ra2VoxelSemanticMaterialRole.Accent => Ra2VoxelStyleRoleCategory.Accent,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static bool Matches(
        Ra2VoxelSemanticColourRequirementKind requirement,
        Ra2VoxelSemanticMaterialRole material) => requirement switch
    {
        Ra2VoxelSemanticColourRequirementKind.Glass => material == Ra2VoxelSemanticMaterialRole.Glass,
        Ra2VoxelSemanticColourRequirementKind.Rubber => material == Ra2VoxelSemanticMaterialRole.Rubber,
        Ra2VoxelSemanticColourRequirementKind.BareMetal => material == Ra2VoxelSemanticMaterialRole.BareMetal,
        Ra2VoxelSemanticColourRequirementKind.Light => material == Ra2VoxelSemanticMaterialRole.Light,
        Ra2VoxelSemanticColourRequirementKind.DarkOpening => material == Ra2VoxelSemanticMaterialRole.DarkOpening,
        Ra2VoxelSemanticColourRequirementKind.Accent => material == Ra2VoxelSemanticMaterialRole.Accent,
        _ => false
    };

    private static string Format(Ra2VoxelSemanticColourRequirementKind requirement)
        => requirement.ToString().ToLowerInvariant();
}
