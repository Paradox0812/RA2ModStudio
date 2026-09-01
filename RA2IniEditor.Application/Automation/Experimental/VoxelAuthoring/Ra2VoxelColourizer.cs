using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

[Flags]
internal enum Ra2VoxelGeometryRegionBits : byte
{
    None = 0,
    TopExposed = 1 << 0,
    SideExposed = 1 << 1,
    UnderExposed = 1 << 2,
    EdgeOrRidge = 1 << 3,
    Interior = 1 << 4,
    LateralSideExposed = 1 << 5,
    LongitudinalEndExposed = 1 << 6
}

[Flags]
internal enum Ra2VoxelColourReviewFlags
{
    None = 0,
    StylePlanReviewRequired = 1 << 0,
    TextOnlyCoarseStyle = 1 << 1,
    SemanticMaskReviewRequired = 1 << 2,
    RemapReviewRequired = 1 << 3,
    PaletteErrorReviewRequired = 1 << 4,
    PivotReviewRequired = 1 << 5,
    NormalsNotGenerated = 1 << 6,
    HvaNotGenerated = 1 << 7,
    GameValidationNotRun = 1 << 8
}

internal enum Ra2VoxelColourizationFailureKind
{
    None = 0,
    PaletteMismatch,
    MaskSnapshotMismatch,
    MaskShapeMismatch,
    MissingMask,
    UnsupportedRegion,
    CoverageViolation,
    ResourceLimitExceeded,
    AnalysisFailed,
    Cancelled
}

internal sealed class Ra2VoxelGeometryRegionMask
{
    private readonly byte[] _regions;

    internal Ra2VoxelGeometryRegionMask(string sourceSnapshotHash, IEnumerable<byte> regions)
    {
        SourceSnapshotHash = RequireSha256(sourceSnapshotHash, nameof(sourceSnapshotHash));
        _regions = (regions ?? throw new ArgumentNullException(nameof(regions))).ToArray();
        if (_regions.Length > Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
            throw new ArgumentOutOfRangeException(nameof(regions));
        if (_regions.Any(value => (value & ~(byte)(Ra2VoxelGeometryRegionBits.TopExposed |
            Ra2VoxelGeometryRegionBits.SideExposed | Ra2VoxelGeometryRegionBits.UnderExposed |
            Ra2VoxelGeometryRegionBits.EdgeOrRidge | Ra2VoxelGeometryRegionBits.Interior |
            Ra2VoxelGeometryRegionBits.LateralSideExposed |
            Ra2VoxelGeometryRegionBits.LongitudinalEndExposed)) != 0))
        {
            throw new ArgumentException("Geometry region mask contains unsupported bits.", nameof(regions));
        }
        MaskHash = ComputeMaskHash("geometry-mask/2", SourceSnapshotHash, _regions);
    }

    internal string SourceSnapshotHash { get; }
    internal int CellCount => _regions.Length;
    internal string MaskHash { get; }
    internal Ra2VoxelGeometryRegionBits this[int index] => (Ra2VoxelGeometryRegionBits)_regions[index];

    private static string RequireSha256(string value, string parameterName)
        => value.Length == 64 && value.All(char.IsAsciiHexDigit)
            ? value
            : throw new ArgumentException("A canonical SHA-256 value is required.", parameterName);

    internal static string ComputeMaskHash(string prefix, string sourceHash, ReadOnlySpan<byte> bytes)
    {
        byte[] header = Encoding.UTF8.GetBytes(prefix + "\n" + sourceHash + "\n");
        byte[] input = new byte[header.Length + bytes.Length];
        header.CopyTo(input, 0);
        bytes.CopyTo(input.AsSpan(header.Length));
        return Convert.ToHexString(SHA256.HashData(input));
    }
}

internal sealed class Ra2VoxelExplicitMask
{
    private readonly byte[] _selected;

    internal Ra2VoxelExplicitMask(string maskId, string sourceSnapshotHash, IEnumerable<byte> selected)
    {
        MaskId = Ra2VoxelSceneSnapshot.ValidateIdentity(maskId, nameof(maskId));
        if (sourceSnapshotHash.Length != 64 || sourceSnapshotHash.Any(character => !char.IsAsciiHexDigit(character)))
            throw new ArgumentException("A canonical source snapshot hash is required.", nameof(sourceSnapshotHash));
        SourceSnapshotHash = sourceSnapshotHash;
        _selected = (selected ?? throw new ArgumentNullException(nameof(selected))).ToArray();
        if (_selected.Length > Ra2VoxelSceneSnapshot.MaximumOccupancyCount || _selected.Any(value => value is not (0 or 1)))
            throw new ArgumentException("An explicit voxel mask must contain bounded binary values.", nameof(selected));
        MaskHash = Ra2VoxelGeometryRegionMask.ComputeMaskHash("explicit-mask/1:" + MaskId, SourceSnapshotHash, _selected);
    }

    internal string MaskId { get; }
    internal string SourceSnapshotHash { get; }
    internal int CellCount => _selected.Length;
    internal int SelectedCount => _selected.Count(value => value != 0);
    internal string MaskHash { get; }
    internal bool IsSelected(int index) => _selected[index] != 0;
    internal IReadOnlyList<byte> Selected => Array.AsReadOnly(_selected);
}

internal sealed record Ra2VoxelColourCount(string Id, int CellCount);

internal sealed class Ra2VoxelColourizationFacts
{
    internal Ra2VoxelColourizationFacts(
        string sourceSnapshotHash,
        string stylePlanHash,
        string resultSnapshotHash,
        string geometryMaskHash,
        int occupancyCount,
        bool geometryAndOccupancyUnchanged,
        bool isUniformColour,
        long maximumSquaredPaletteError,
        IEnumerable<Ra2VoxelColourCount> roleCounts,
        IEnumerable<Ra2VoxelColourCount> regionCounts,
        IEnumerable<string> unresolvedRules,
        IEnumerable<string> appliedRoleIds,
        Ra2VoxelColourReviewFlags reviewFlags)
    {
        SourceSnapshotHash = sourceSnapshotHash;
        StylePlanHash = stylePlanHash;
        ResultSnapshotHash = resultSnapshotHash;
        GeometryMaskHash = geometryMaskHash;
        OccupancyCount = occupancyCount;
        GeometryAndOccupancyUnchanged = geometryAndOccupancyUnchanged;
        IsUniformColour = isUniformColour;
        MaximumSquaredPaletteError = maximumSquaredPaletteError;
        RoleCounts = Array.AsReadOnly(roleCounts.OrderBy(value => value.Id, StringComparer.Ordinal).ToArray());
        RegionCounts = Array.AsReadOnly(regionCounts.OrderBy(value => value.Id, StringComparer.Ordinal).ToArray());
        UnresolvedRules = Array.AsReadOnly(unresolvedRules.OrderBy(value => value, StringComparer.Ordinal).ToArray());
        AppliedRoleIds = Array.AsReadOnly(appliedRoleIds.ToArray());
        if (AppliedRoleIds.Count != occupancyCount)
            throw new ArgumentException("Applied role count must match occupancy.", nameof(appliedRoleIds));
        ReviewFlags = reviewFlags;
    }

    internal string SourceSnapshotHash { get; }
    internal string StylePlanHash { get; }
    internal string ResultSnapshotHash { get; }
    internal string GeometryMaskHash { get; }
    internal int OccupancyCount { get; }
    internal bool GeometryAndOccupancyUnchanged { get; }
    internal bool IsUniformColour { get; }
    internal long MaximumSquaredPaletteError { get; }
    internal IReadOnlyList<Ra2VoxelColourCount> RoleCounts { get; }
    internal IReadOnlyList<Ra2VoxelColourCount> RegionCounts { get; }
    internal IReadOnlyList<string> UnresolvedRules { get; }
    internal IReadOnlyList<string> AppliedRoleIds { get; }
    internal Ra2VoxelColourReviewFlags ReviewFlags { get; }
}

internal sealed record Ra2VoxelColourizationResult(
    Ra2VoxelColourizationFailureKind FailureKind,
    string Message,
    Ra2VoxelSceneSnapshot? Snapshot,
    Ra2VoxelGeometryRegionMask? GeometryMask,
    Ra2VoxelColourizationFacts? Facts)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelColourizationFailureKind.None && Snapshot is not null && Facts is not null;
}

internal static class Ra2VoxelColourizer
{
    internal static Ra2VoxelGeometryRegionMask BuildGeometryMask(
        Ra2VoxelSceneSnapshot snapshot,
        CancellationToken cancellationToken = default)
        => BuildGeometryMask(snapshot, null, cancellationToken);

    internal static Ra2VoxelGeometryRegionMask BuildGeometryMask(
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelColourEdgePolicy? edgePolicy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        byte[] regions = new byte[snapshot.OccupancyCount];
        bool longitudinalAxisIsY = snapshot.Part.YSize >= snapshot.Part.XSize;
        for (int index = 0; index < snapshot.Cells.Count; index++)
        {
            if ((index & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelCoordinate coordinate = snapshot.Cells[index].Coordinate;
            bool positiveX = Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, Ra2VoxelFaceDirection.PositiveX);
            bool negativeX = Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, Ra2VoxelFaceDirection.NegativeX);
            bool positiveY = Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, Ra2VoxelFaceDirection.PositiveY);
            bool negativeY = Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, Ra2VoxelFaceDirection.NegativeY);
            bool top = Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, Ra2VoxelFaceDirection.PositiveZ);
            bool under = Ra2VoxelNeighbourhood.IsFaceExposed(snapshot, coordinate, Ra2VoxelFaceDirection.NegativeZ);
            bool xFamily = positiveX || negativeX;
            bool yFamily = positiveY || negativeY;
            int familyCount = (xFamily ? 1 : 0) + (yFamily ? 1 : 0) + (top || under ? 1 : 0);
            Ra2VoxelGeometryRegionBits bits = Ra2VoxelGeometryRegionBits.None;
            if (top) bits |= Ra2VoxelGeometryRegionBits.TopExposed;
            if (under) bits |= Ra2VoxelGeometryRegionBits.UnderExposed;
            if (xFamily || yFamily) bits |= Ra2VoxelGeometryRegionBits.SideExposed;
            if (longitudinalAxisIsY)
            {
                if (xFamily) bits |= Ra2VoxelGeometryRegionBits.LateralSideExposed;
                if (yFamily) bits |= Ra2VoxelGeometryRegionBits.LongitudinalEndExposed;
            }
            else
            {
                if (yFamily) bits |= Ra2VoxelGeometryRegionBits.LateralSideExposed;
                if (xFamily) bits |= Ra2VoxelGeometryRegionBits.LongitudinalEndExposed;
            }
            bool edgeOrRidge = edgePolicy switch
            {
                Ra2VoxelColourEdgePolicy.None => false,
                Ra2VoxelColourEdgePolicy.Subtle => familyCount >= 3,
                Ra2VoxelColourEdgePolicy.Strong => familyCount >= 3 ||
                    (top && (xFamily || yFamily)),
                null => familyCount >= 2,
                _ => false
            };
            if (edgeOrRidge) bits |= Ra2VoxelGeometryRegionBits.EdgeOrRidge;
            if (familyCount == 0) bits |= Ra2VoxelGeometryRegionBits.Interior;
            regions[index] = (byte)bits;
        }
        return new(snapshot.CanonicalHash, regions);
    }

    internal static Ra2VoxelColourizationResult Colourize(
        Ra2VoxelSceneSnapshot source,
        Ra2CompiledVoxelStylePlan plan,
        IEnumerable<Ra2VoxelExplicitMask>? explicitMasks = null,
        CancellationToken cancellationToken = default)
        => ColourizeCore(source, plan, explicitMasks, null, null, cancellationToken);

    internal static Ra2VoxelColourizationResult Colourize(
        Ra2VoxelSceneSnapshot source,
        Ra2CompiledVoxelStylePlan plan,
        IEnumerable<Ra2VoxelExplicitMask>? explicitMasks,
        Ra2VoxelDualSurfacePolicy dualSurfacePolicy,
        CancellationToken cancellationToken = default)
        => ColourizeCore(source, plan, explicitMasks, dualSurfacePolicy, null, cancellationToken);

    internal static Ra2VoxelColourizationResult Colourize(
        Ra2VoxelSceneSnapshot source,
        Ra2CompiledVoxelStylePlan plan,
        IEnumerable<Ra2VoxelExplicitMask>? explicitMasks,
        Ra2VoxelDualSurfacePolicy dualSurfacePolicy,
        Ra2VoxelColourEdgePolicy edgePolicy,
        CancellationToken cancellationToken = default)
        => ColourizeCore(source, plan, explicitMasks, dualSurfacePolicy, edgePolicy, cancellationToken);

    private static Ra2VoxelColourizationResult ColourizeCore(
        Ra2VoxelSceneSnapshot source,
        Ra2CompiledVoxelStylePlan plan,
        IEnumerable<Ra2VoxelExplicitMask>? explicitMasks,
        Ra2VoxelDualSurfacePolicy? dualSurfacePolicy,
        Ra2VoxelColourEdgePolicy? edgePolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(plan);
        if (!string.Equals(source.Palette.ProfileHash, plan.PaletteHash, StringComparison.Ordinal))
            return Failure(Ra2VoxelColourizationFailureKind.PaletteMismatch, "The style plan palette does not match the source snapshot.");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelGeometryRegionMask geometry = BuildGeometryMask(source, edgePolicy, cancellationToken);
            Dictionary<string, Ra2VoxelExplicitMask> masks = (explicitMasks ?? [])
                .ToDictionary(mask => mask.MaskId, StringComparer.Ordinal);
            foreach (Ra2VoxelExplicitMask mask in masks.Values)
            {
                if (!string.Equals(mask.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal))
                    return Failure(Ra2VoxelColourizationFailureKind.MaskSnapshotMismatch, "An explicit mask belongs to another snapshot.");
                if (mask.CellCount != source.OccupancyCount)
                    return Failure(Ra2VoxelColourizationFailureKind.MaskShapeMismatch, "An explicit mask cell count does not match the snapshot.");
            }

            Dictionary<string, Ra2CompiledVoxelStyleRole> roles = plan.Roles.ToDictionary(role => role.Id, StringComparer.Ordinal);
            Ra2CompiledVoxelStyleRule? baseRule = plan.Rules.SingleOrDefault(rule =>
                rule.IsPaintable && rule.Region == Ra2VoxelStyleRegionKind.WholePart);
            if (baseRule is null || !roles.TryGetValue(baseRule.RoleId, out Ra2CompiledVoxelStyleRole? baseRole) ||
                !roles.TryGetValue(plan.InteriorRoleId, out Ra2CompiledVoxelStyleRole? interiorRole))
            {
                return Failure(Ra2VoxelColourizationFailureKind.CoverageViolation, "The style plan does not cover the complete snapshot.");
            }

            byte[] indices = Enumerable.Repeat(baseRole.PaletteIndex, source.OccupancyCount).ToArray();
            string[] appliedRoles = Enumerable.Repeat(baseRole.Id, source.OccupancyCount).ToArray();
            ApplyGeometryRegion(Ra2VoxelStyleRegionKind.Interior, Ra2VoxelGeometryRegionBits.Interior, interiorRole);
            if (dualSurfacePolicy is null)
            {
                ApplyRule(Ra2VoxelStyleRegionKind.SideExposed, Ra2VoxelGeometryRegionBits.SideExposed);
                ApplyRule(Ra2VoxelStyleRegionKind.TopExposed, Ra2VoxelGeometryRegionBits.TopExposed);
                ApplyRule(Ra2VoxelStyleRegionKind.UnderExposed, Ra2VoxelGeometryRegionBits.UnderExposed);
            }
            else
            {
                ApplyExclusivePrimarySurfaces(dualSurfacePolicy.Value);
            }
            ApplyRule(Ra2VoxelStyleRegionKind.EdgeOrRidge, Ra2VoxelGeometryRegionBits.EdgeOrRidge);

            List<string> unresolved = [];
            foreach (Ra2CompiledVoxelStyleRule rule in plan.Rules.Where(rule => !rule.IsPaintable))
                unresolved.Add($"{rule.Region}:{rule.RoleId}:{rule.Evidence}");
            foreach (Ra2CompiledVoxelStyleRule rule in plan.Rules.Where(rule =>
                         rule.IsPaintable && rule.Region == Ra2VoxelStyleRegionKind.ExplicitMask))
            {
                if (rule.MaskId is null || !masks.TryGetValue(rule.MaskId, out Ra2VoxelExplicitMask? mask))
                    return Failure(Ra2VoxelColourizationFailureKind.MissingMask, "A paintable semantic rule has no validated explicit mask.");
                Ra2CompiledVoxelStyleRole role = roles[rule.RoleId];
                for (int index = 0; index < indices.Length; index++)
                {
                    if (mask.IsSelected(index))
                    {
                        indices[index] = role.PaletteIndex;
                        appliedRoles[index] = role.Id;
                    }
                }
            }
            if (plan.Rules.Any(rule => rule.IsPaintable && rule.Region is
                    Ra2VoxelStyleRegionKind.DonorMask or Ra2VoxelStyleRegionKind.SourceMaterialMask))
            {
                return Failure(Ra2VoxelColourizationFailureKind.UnsupportedRegion, "Donor and source-material masks are not implemented in Stage 1E.");
            }

            List<Ra2VoxelCell> cells = new(source.OccupancyCount);
            for (int index = 0; index < source.Cells.Count; index++)
            {
                if ((index & 4095) == 0)
                    cancellationToken.ThrowIfCancellationRequested();
                cells.Add(new(source.Cells[index].Coordinate, indices[index]));
            }
            List<KeyValuePair<string, string>> sourceHashes = source.SourceArtifactHashes.ToList();
            sourceHashes.RemoveAll(pair => string.Equals(pair.Key, "voxel-style-plan", StringComparison.Ordinal));
            sourceHashes.Add(new("voxel-style-plan", plan.PlanHash));
            Ra2VoxelSceneSnapshot output = new(source.SceneId, source.Part, source.Palette, cells, sourceHashes);
            bool geometryUnchanged = source.Cells.Select(cell => cell.Coordinate)
                .SequenceEqual(output.Cells.Select(cell => cell.Coordinate));
            long maximumError = plan.Roles.Max(role => PaletteError(role, source.Palette));
            Ra2VoxelColourReviewFlags flags = Ra2VoxelColourReviewFlags.StylePlanReviewRequired |
                Ra2VoxelColourReviewFlags.PivotReviewRequired |
                Ra2VoxelColourReviewFlags.NormalsNotGenerated |
                Ra2VoxelColourReviewFlags.HvaNotGenerated |
                Ra2VoxelColourReviewFlags.GameValidationNotRun;
            bool hasPaintableSemanticMask = plan.Rules.Any(rule => rule.IsPaintable &&
                rule.Region == Ra2VoxelStyleRegionKind.ExplicitMask);
            if (!hasPaintableSemanticMask)
                flags |= Ra2VoxelColourReviewFlags.TextOnlyCoarseStyle;
            if (plan.Rules.Any(rule => !rule.IsPaintable && rule.Evidence == Ra2VoxelStyleEvidenceKind.InferredTextOnly))
                flags |= Ra2VoxelColourReviewFlags.SemanticMaskReviewRequired;
            if (plan.RemapPolicy != Ra2VoxelStyleRemapPolicy.None || plan.Roles.Any(role => role.Category == Ra2VoxelStyleRoleCategory.Remap))
                flags |= Ra2VoxelColourReviewFlags.RemapReviewRequired;
            if (maximumError > 0)
                flags |= Ra2VoxelColourReviewFlags.PaletteErrorReviewRequired;

            Ra2VoxelColourizationFacts facts = new(
                source.CanonicalHash,
                plan.PlanHash,
                output.CanonicalHash,
                geometry.MaskHash,
                output.OccupancyCount,
                geometryUnchanged && source.OccupancyCount == output.OccupancyCount,
                indices.Distinct().Take(2).Count() == 1,
                maximumError,
                appliedRoles.GroupBy(value => value, StringComparer.Ordinal).Select(group => new Ra2VoxelColourCount(group.Key, group.Count())),
                RegionCounts(geometry),
                unresolved,
                appliedRoles,
                flags);
            if (!facts.GeometryAndOccupancyUnchanged)
                return Failure(Ra2VoxelColourizationFailureKind.AnalysisFailed, "Voxel colourization changed geometry or occupancy.");
            return new(Ra2VoxelColourizationFailureKind.None, string.Empty, output, geometry, facts);

            void ApplyRule(Ra2VoxelStyleRegionKind region, Ra2VoxelGeometryRegionBits bit)
            {
                Ra2CompiledVoxelStyleRule? rule = plan.Rules.SingleOrDefault(candidate => candidate.IsPaintable && candidate.Region == region);
                if (rule is not null)
                    ApplyGeometryRegion(region, bit, roles[rule.RoleId]);
            }

            void ApplyExclusivePrimarySurfaces(Ra2VoxelDualSurfacePolicy policy)
            {
                Ra2CompiledVoxelStyleRole? sideRole = FindRole(Ra2VoxelStyleRegionKind.SideExposed);
                Ra2CompiledVoxelStyleRole? topRole = FindRole(Ra2VoxelStyleRegionKind.TopExposed);
                Ra2CompiledVoxelStyleRole? underRole = FindRole(Ra2VoxelStyleRegionKind.UnderExposed);
                Ra2CompiledVoxelStyleRole? endRole = roles.Values.FirstOrDefault(value =>
                    value.Category == Ra2VoxelStyleRoleCategory.BodyMid);
                for (int index = 0; index < indices.Length; index++)
                {
                    Ra2VoxelGeometryRegionBits bits = geometry[index];
                    bool top = (bits & Ra2VoxelGeometryRegionBits.TopExposed) != 0;
                    bool under = (bits & Ra2VoxelGeometryRegionBits.UnderExposed) != 0;
                    bool side = (bits & Ra2VoxelGeometryRegionBits.SideExposed) != 0;
                    bool longitudinalEnd = (bits & Ra2VoxelGeometryRegionBits.LongitudinalEndExposed) != 0;
                    Ra2CompiledVoxelStyleRole? selected = null;
                    if (top && under && !side)
                    {
                        selected = policy switch
                        {
                            Ra2VoxelDualSurfacePolicy.UnderPreferred => underRole,
                            Ra2VoxelDualSurfacePolicy.TopPreferred => topRole,
                            Ra2VoxelDualSurfacePolicy.BodyBase => baseRole,
                            _ => throw new InvalidOperationException("Unknown dual-surface policy.")
                        };
                    }
                    else if (top)
                    {
                        selected = topRole;
                    }
                    else if (longitudinalEnd)
                    {
                        selected = endRole ?? sideRole;
                    }
                    else if (side)
                    {
                        selected = sideRole;
                    }
                    else if (under)
                    {
                        selected = underRole;
                    }
                    if (selected is not null)
                    {
                        indices[index] = selected.PaletteIndex;
                        appliedRoles[index] = selected.Id;
                    }
                }

                Ra2CompiledVoxelStyleRole? FindRole(Ra2VoxelStyleRegionKind region)
                {
                    Ra2CompiledVoxelStyleRule? rule = plan.Rules.SingleOrDefault(candidate =>
                        candidate.IsPaintable && candidate.Region == region);
                    return rule is null ? null : roles[rule.RoleId];
                }
            }

            void ApplyGeometryRegion(Ra2VoxelStyleRegionKind _, Ra2VoxelGeometryRegionBits bit, Ra2CompiledVoxelStyleRole role)
            {
                for (int index = 0; index < indices.Length; index++)
                {
                    if ((geometry[index] & bit) != 0)
                    {
                        indices[index] = role.PaletteIndex;
                        appliedRoles[index] = role.Id;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            return Failure(Ra2VoxelColourizationFailureKind.Cancelled, "Voxel colourization was cancelled.");
        }
        catch (ArgumentException)
        {
            return Failure(Ra2VoxelColourizationFailureKind.AnalysisFailed, "Voxel colourization inputs are inconsistent.");
        }
        catch (InvalidOperationException)
        {
            return Failure(Ra2VoxelColourizationFailureKind.AnalysisFailed, "Voxel colourization could not resolve a deterministic rule set.");
        }
    }

    private static IEnumerable<Ra2VoxelColourCount> RegionCounts(Ra2VoxelGeometryRegionMask mask)
    {
        foreach (Ra2VoxelGeometryRegionBits bit in new[]
                 {
                     Ra2VoxelGeometryRegionBits.TopExposed, Ra2VoxelGeometryRegionBits.SideExposed,
                     Ra2VoxelGeometryRegionBits.UnderExposed, Ra2VoxelGeometryRegionBits.EdgeOrRidge,
                     Ra2VoxelGeometryRegionBits.Interior, Ra2VoxelGeometryRegionBits.LateralSideExposed,
                     Ra2VoxelGeometryRegionBits.LongitudinalEndExposed
                 })
        {
            int count = Enumerable.Range(0, mask.CellCount).Count(index => (mask[index] & bit) != 0);
            yield return new(bit.ToString(), count);
        }
    }

    private static long PaletteError(Ra2CompiledVoxelStyleRole role, Ra2VoxelPaletteProfile palette)
    {
        if (role.RequestedColour is not Ra2Rgba32 requested)
            return 0;
        Ra2Rgba32 actual = palette[role.PaletteIndex];
        long red = requested.Red - actual.Red;
        long green = requested.Green - actual.Green;
        long blue = requested.Blue - actual.Blue;
        return red * red + green * green + blue * blue;
    }

    private static Ra2VoxelColourizationResult Failure(Ra2VoxelColourizationFailureKind kind, string message)
        => new(kind, message, null, null, null);
}
