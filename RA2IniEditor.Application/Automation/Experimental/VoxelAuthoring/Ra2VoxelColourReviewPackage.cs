using System.Security.Cryptography;
using System.Text.Json;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelColourReviewPackageFailureKind
{
    None = 0,
    InvalidColourizationResult,
    HashMismatch,
    MaskMismatch,
    ResourceLimitExceeded,
    AnalysisFailed
}

internal sealed record Ra2VoxelStyleSourceFact(string ScopeId, string SourceHash, int CharacterCount);

internal sealed class Ra2VoxelReviewArtifact
{
    private readonly byte[] _content;

    internal Ra2VoxelReviewArtifact(string fileName, string mediaType, ReadOnlySpan<byte> content)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileName.Contains('/') || fileName.Contains('\\'))
        {
            throw new ArgumentException("A safe leaf file name is required.", nameof(fileName));
        }
        if (string.IsNullOrWhiteSpace(mediaType) || mediaType.Length > 128)
            throw new ArgumentException("A bounded media type is required.", nameof(mediaType));
        _content = content.ToArray();
        FileName = fileName;
        MediaType = mediaType;
        ContentSha256 = Convert.ToHexString(SHA256.HashData(_content));
    }

    internal string FileName { get; }
    internal string MediaType { get; }
    internal string ContentSha256 { get; }
    internal ReadOnlyMemory<byte> Content => _content;
}

internal sealed record Ra2VoxelColourReviewPackageResult(
    Ra2VoxelColourReviewPackageFailureKind FailureKind,
    string Message,
    IReadOnlyList<Ra2VoxelReviewArtifact> Artifacts)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelColourReviewPackageFailureKind.None;
}

/// <summary>
/// Builds immutable, path-free review artifacts. The caller owns any later, explicitly authorized file write.
/// </summary>
internal static class Ra2VoxelColourReviewPackageBuilder
{
    internal const int MaximumArtifactCount = 8;
    internal const int MaximumPackageByteLength = 384 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    internal static Ra2VoxelColourReviewPackageResult Build(
        IEnumerable<Ra2VoxelStyleSourceFact> sourceFacts,
        Ra2VoxelSceneSnapshot source,
        Ra2CompiledVoxelStylePlan plan,
        Ra2VoxelColourizationResult colourization,
        IEnumerable<Ra2VoxelExplicitMask>? explicitMasks = null,
        Ra2VoxelColourQualityReport? quality = null)
    {
        ArgumentNullException.ThrowIfNull(sourceFacts);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(colourization);
        if (!colourization.IsSuccess || colourization.Snapshot is null ||
            colourization.GeometryMask is null || colourization.Facts is null)
        {
            return Failure(Ra2VoxelColourReviewPackageFailureKind.InvalidColourizationResult,
                "A successful colourization result is required.");
        }

        try
        {
            Ra2VoxelSceneSnapshot result = colourization.Snapshot;
            Ra2VoxelGeometryRegionMask geometry = colourization.GeometryMask;
            Ra2VoxelColourizationFacts facts = colourization.Facts;
            if (!string.Equals(source.CanonicalHash, facts.SourceSnapshotHash, StringComparison.Ordinal) ||
                !string.Equals(plan.PlanHash, facts.StylePlanHash, StringComparison.Ordinal) ||
                !string.Equals(result.CanonicalHash, facts.ResultSnapshotHash, StringComparison.Ordinal) ||
                !string.Equals(geometry.MaskHash, facts.GeometryMaskHash, StringComparison.Ordinal) ||
                !string.Equals(source.Palette.ProfileHash, plan.PaletteHash, StringComparison.Ordinal))
            {
                return Failure(Ra2VoxelColourReviewPackageFailureKind.HashMismatch,
                    "Review inputs do not belong to one colourization transaction.");
            }
            if (quality is not null &&
                !string.Equals(quality.CandidateHash, result.CanonicalHash, StringComparison.Ordinal))
            {
                return Failure(Ra2VoxelColourReviewPackageFailureKind.HashMismatch,
                    "The colour quality report belongs to another candidate.");
            }
            if (!facts.GeometryAndOccupancyUnchanged || geometry.CellCount != source.OccupancyCount ||
                !string.Equals(geometry.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal))
            {
                return Failure(Ra2VoxelColourReviewPackageFailureKind.MaskMismatch,
                    "Review masks do not match the canonical source snapshot.");
            }

            Ra2VoxelStyleSourceFact[] sources = sourceFacts.ToArray();
            if (sources.Length is < 1 or > 8 || sources.Sum(item => (long)item.CharacterCount) > 65_536 ||
                sources.Select(item => item.ScopeId).Distinct(StringComparer.Ordinal).Count() != sources.Length ||
                sources.Any(item =>
                    string.IsNullOrWhiteSpace(item.ScopeId) || item.ScopeId.Length > 128 ||
                    !IsSha256(item.SourceHash) || item.CharacterCount is < 1 or > 32_768))
            {
                return Failure(Ra2VoxelColourReviewPackageFailureKind.ResourceLimitExceeded,
                    "Review source facts exceed their safe bounds.");
            }

            Ra2VoxelExplicitMask[] masks = (explicitMasks ?? []).ToArray();
            if (masks.Length > Ra2VoxelStylePlanCompiler.MaximumRuleCount ||
                masks.Select(mask => mask.MaskId).Distinct(StringComparer.Ordinal).Count() != masks.Length ||
                masks.Any(mask => !string.Equals(mask.SourceSnapshotHash, source.CanonicalHash, StringComparison.Ordinal) ||
                                  mask.CellCount != source.OccupancyCount))
            {
                return Failure(Ra2VoxelColourReviewPackageFailureKind.MaskMismatch,
                    "An explicit review mask belongs to another snapshot.");
            }

            List<Ra2VoxelReviewArtifact> artifacts =
            [
                JsonArtifact("style-source-pack.json", BuildSourcePackJson(sources, plan)),
                JsonArtifact("compiled-style-plan.json", BuildPlanJson(plan)),
                JsonArtifact("colour-review-report.json", BuildReportJson(source, result, plan, facts, masks, quality)),
                new("palette-swatch.png", "image/png", BuildPaletteSwatch(source.Palette, plan)),
                new("region-mask.png", "image/png", BuildRegionMask(source, geometry)),
                new("body-coloured.vox", "application/octet-stream", Ra2MagicaVoxelCodec.Write(result)),
                new("body-coloured-slicestack.png", "image/png",
                    Ra2VoxelSliceStackCodec.ExportPng(result, Ra2VxlseSliceDirection.Downward))
            ];

            Ra2VoxelExplicitMask[] remapMasks = FindRemapMasks(plan, masks);
            if (remapMasks.Length > 0)
                artifacts.Insert(5, new("remap-mask.png", "image/png", BuildExplicitMask(source, remapMasks)));

            if (artifacts.Count > MaximumArtifactCount || artifacts.Sum(item => (long)item.Content.Length) > MaximumPackageByteLength)
            {
                return Failure(Ra2VoxelColourReviewPackageFailureKind.ResourceLimitExceeded,
                    "The review artifact package exceeds its safe bounds.");
            }
            return new(Ra2VoxelColourReviewPackageFailureKind.None, string.Empty, artifacts.AsReadOnly());
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return Failure(Ra2VoxelColourReviewPackageFailureKind.AnalysisFailed,
                "The colour review package could not be built safely.");
        }
    }

    private static byte[] BuildSourcePackJson(Ra2VoxelStyleSourceFact[] sources, Ra2CompiledVoxelStylePlan plan) =>
        JsonSerializer.SerializeToUtf8Bytes(new
        {
            schema_version = 1,
            source_pack_hash = plan.SourcePackHash,
            sources = sources.Select(item => new
            {
                scope_id = item.ScopeId,
                source_hash = item.SourceHash,
                character_count = item.CharacterCount
            })
        }, JsonOptions);

    private static byte[] BuildPlanJson(Ra2CompiledVoxelStylePlan plan) =>
        JsonSerializer.SerializeToUtf8Bytes(new
        {
            schema_version = Ra2CompiledVoxelStylePlan.CurrentSchemaVersion,
            plan_hash = plan.PlanHash,
            plan.Title,
            plan.Summary,
            source_pack_hash = plan.SourcePackHash,
            palette_hash = plan.PaletteHash,
            compiler_revision = plan.CompilerRevision,
            model_identity = plan.ModelIdentity,
            remap_policy = plan.RemapPolicy.ToString(),
            interior_role_id = plan.InteriorRoleId,
            roles = plan.Roles.Select(role => new
            {
                role.Id,
                category = role.Category.ToString(),
                palette_index = role.PaletteIndex,
                requested_exact_palette_index = role.RequestedExactPaletteIndex,
                requested_colour = role.RequestedColour is Ra2Rgba32 colour
                    ? new[] { (int)colour.Red, (int)colour.Green, (int)colour.Blue, (int)colour.Alpha }
                    : null,
                source_scope_ids = role.SourceScopeIds
            }),
            rules = plan.Rules.Select(rule => new
            {
                region = rule.Region.ToString(),
                role_id = rule.RoleId,
                evidence = rule.Evidence.ToString(),
                mask_id = rule.MaskId,
                is_paintable = rule.IsPaintable,
                source_scope_ids = rule.SourceScopeIds
            }),
            unresolved_assumptions = plan.UnresolvedAssumptions
        }, JsonOptions);

    private static byte[] BuildReportJson(
        Ra2VoxelSceneSnapshot source,
        Ra2VoxelSceneSnapshot result,
        Ra2CompiledVoxelStylePlan plan,
        Ra2VoxelColourizationFacts facts,
        Ra2VoxelExplicitMask[] masks,
        Ra2VoxelColourQualityReport? quality) =>
        JsonSerializer.SerializeToUtf8Bytes(new
        {
            schema_version = 1,
            source_snapshot_hash = source.CanonicalHash,
            result_snapshot_hash = result.CanonicalHash,
            style_plan_hash = plan.PlanHash,
            palette_hash = source.Palette.ProfileHash,
            geometry_mask_hash = facts.GeometryMaskHash,
            occupancy_count = facts.OccupancyCount,
            geometry_and_occupancy_unchanged = facts.GeometryAndOccupancyUnchanged,
            is_uniform_colour = facts.IsUniformColour,
            maximum_squared_palette_error = facts.MaximumSquaredPaletteError,
            role_counts = facts.RoleCounts,
            region_counts = facts.RegionCounts,
            unresolved_rules = facts.UnresolvedRules,
            review_flags = Enum.GetValues<Ra2VoxelColourReviewFlags>()
                .Where(flag => flag != Ra2VoxelColourReviewFlags.None && facts.ReviewFlags.HasFlag(flag))
                .Select(flag => flag.ToString()),
            explicit_masks = masks.Select(mask => new
            {
                mask.MaskId,
                mask.MaskHash,
                mask.CellCount,
                selected_count = mask.Selected.Count(value => value != 0)
            }),
            quality = quality is null ? null : new
            {
                report_hash = quality.ReportHash,
                bundle_hash = quality.BundleHash,
                state = quality.State.ToString(),
                visual_acceptance = quality.VisualAcceptance.ToString(),
                warnings = quality.Warnings,
                metrics = quality.Metrics,
                distribution = quality.Distribution
            },
            claims = new
            {
                project_adopted = false,
                vxl_generated = false,
                hva_generated = false,
                game_validated = false
            }
        }, JsonOptions);

    private static byte[] BuildPaletteSwatch(Ra2VoxelPaletteProfile palette, Ra2CompiledVoxelStylePlan plan)
    {
        const int width = 32;
        const int height = 16;
        byte[] rgba = new byte[width * height * 4];
        HashSet<byte> selected = plan.Roles.Select(role => role.PaletteIndex).ToHashSet();
        for (int index = 0; index < 256; index++)
        {
            Ra2Rgba32 colour = palette[(byte)index];
            int row = index / 16;
            int column = index % 16;
            WritePixel(column, row, colour.Red, colour.Green, colour.Blue, colour.Alpha);
            bool isSelected = selected.Contains((byte)index);
            byte muted = (byte)((colour.Red + colour.Green + colour.Blue) / 3);
            WritePixel(column + 16, row,
                isSelected ? colour.Red : muted,
                isSelected ? colour.Green : muted,
                isSelected ? colour.Blue : muted,
                isSelected ? (byte)Math.Max(colour.Alpha, (byte)160) : (byte)48);
        }
        return Ra2PngRgbaCodec.Encode(new(width, height, Ra2VxlseSliceDirection.Downward,
            1, 1, 1, palette.ProfileHash, rgba));

        void WritePixel(int x, int y, byte red, byte green, byte blue, byte alpha)
        {
            int offset = ((y * width) + x) * 4;
            rgba[offset] = red;
            rgba[offset + 1] = green;
            rgba[offset + 2] = blue;
            rgba[offset + 3] = alpha;
        }
    }

    private static byte[] BuildRegionMask(Ra2VoxelSceneSnapshot source, Ra2VoxelGeometryRegionMask geometry)
    {
        Ra2VoxelPaletteProfile palette = CreateDiagnosticPalette(index => GeometryRegionColour((Ra2VoxelGeometryRegionBits)index));
        List<Ra2VoxelCell> cells = new(source.OccupancyCount);
        for (int index = 0; index < source.Cells.Count; index++)
            cells.Add(new(source.Cells[index].Coordinate, (byte)geometry[index]));
        Ra2VoxelSceneSnapshot diagnostic = new("VOXEL_STYLE_REGION_REVIEW", source.Part, palette, cells,
            [new("source-snapshot", source.CanonicalHash), new("geometry-mask", geometry.MaskHash)]);
        return Ra2VoxelSliceStackCodec.ExportPng(diagnostic, Ra2VxlseSliceDirection.Downward);
    }

    private static byte[] BuildExplicitMask(Ra2VoxelSceneSnapshot source, Ra2VoxelExplicitMask[] masks)
    {
        Ra2VoxelPaletteProfile palette = CreateDiagnosticPalette(index => index == 1
            ? new Ra2Rgba32(255, 48, 192)
            : new Ra2Rgba32(32, 36, 44));
        List<Ra2VoxelCell> cells = new(source.OccupancyCount);
        for (int index = 0; index < source.Cells.Count; index++)
            cells.Add(new(source.Cells[index].Coordinate, masks.Any(mask => mask.IsSelected(index)) ? (byte)1 : (byte)2));
        Ra2VoxelSceneSnapshot diagnostic = new("VOXEL_STYLE_REMAP_REVIEW", source.Part, palette, cells,
            [new("source-snapshot", source.CanonicalHash)]);
        return Ra2VoxelSliceStackCodec.ExportPng(diagnostic, Ra2VxlseSliceDirection.Downward);
    }

    private static Ra2VoxelExplicitMask[] FindRemapMasks(Ra2CompiledVoxelStylePlan plan, Ra2VoxelExplicitMask[] masks)
    {
        HashSet<string> remapRoleIds = plan.Roles
            .Where(role => role.Category == Ra2VoxelStyleRoleCategory.Remap)
            .Select(role => role.Id)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> remapMaskIds = plan.Rules
            .Where(rule => rule.IsPaintable && rule.MaskId is not null && remapRoleIds.Contains(rule.RoleId))
            .Select(rule => rule.MaskId!)
            .ToHashSet(StringComparer.Ordinal);
        return masks.Where(mask => remapMaskIds.Contains(mask.MaskId)).ToArray();
    }

    private static Ra2VoxelPaletteProfile CreateDiagnosticPalette(Func<byte, Ra2Rgba32> colourFactory)
    {
        Ra2Rgba32[] colours = Enumerable.Range(0, 256).Select(index => colourFactory((byte)index)).ToArray();
        colours[0] = new Ra2Rgba32(0, 0, 0, 0);
        return new("voxel-style-review", colours, [0], []);
    }

    internal static Ra2Rgba32 GeometryRegionColour(Ra2VoxelGeometryRegionBits bits)
    {
        if ((bits & Ra2VoxelGeometryRegionBits.EdgeOrRidge) != 0) return new(255, 188, 32);
        if ((bits & Ra2VoxelGeometryRegionBits.TopExposed) != 0) return new(72, 184, 255);
        if ((bits & Ra2VoxelGeometryRegionBits.UnderExposed) != 0) return new(116, 72, 184);
        if ((bits & Ra2VoxelGeometryRegionBits.SideExposed) != 0) return new(64, 192, 112);
        if ((bits & Ra2VoxelGeometryRegionBits.Interior) != 0) return new(80, 88, 104);
        return new(192, 48, 48);
    }

    private static Ra2VoxelReviewArtifact JsonArtifact(string fileName, byte[] content) =>
        new(fileName, "application/json", content);

    private static bool IsSha256(string value) =>
        value.Length == 64 && value.All(char.IsAsciiHexDigit);

    private static Ra2VoxelColourReviewPackageResult Failure(
        Ra2VoxelColourReviewPackageFailureKind kind,
        string message) => new(kind, message, Array.Empty<Ra2VoxelReviewArtifact>());
}
