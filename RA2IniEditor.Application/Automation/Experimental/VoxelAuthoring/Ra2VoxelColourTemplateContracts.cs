using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal static class Ra2VoxelColourContractIdentity
{
    internal static string ComputeHash(Action<BinaryWriter> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        write(writer);
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    internal static string RequireSha256(string value, string parameterName)
        => value is not null && value.Length == 64 && value.All(char.IsAsciiHexDigit)
            ? value.ToUpperInvariant()
            : throw new ArgumentException("A canonical SHA-256 value is required.", parameterName);

    internal static string RequireIdentifier(string value, string parameterName, int maximumLength = 96)
    {
        string normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is < 1 || normalized.Length > maximumLength ||
            !char.IsAsciiLetterOrDigit(normalized[0]) ||
            normalized.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('.' or '-' or '_')))
        {
            throw new ArgumentException("A bounded canonical identifier is required.", parameterName);
        }
        return normalized;
    }

    internal static string RequireSingleLine(string value, string parameterName, int maximumLength)
    {
        string normalized = string.Join(" ", (value ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length is < 1 || normalized.Length > maximumLength || normalized.IndexOf('\0') >= 0)
            throw new ArgumentException("A bounded single-line value is required.", parameterName);
        return normalized;
    }
}

internal enum Ra2VoxelBaseColourFailureKind
{
    None = 0,
    PaletteMismatch,
    TransparentIndex,
    RemapIndex
}

internal sealed class Ra2VoxelBaseColourSelection
{
    private Ra2VoxelBaseColourSelection(
        string paletteProfileHash,
        byte paletteIndex,
        Ra2Rgba32 resolvedRgba,
        string selectionHash)
    {
        PaletteProfileHash = paletteProfileHash;
        PaletteIndex = paletteIndex;
        ResolvedRgba = resolvedRgba;
        SelectionHash = selectionHash;
    }

    internal string PaletteProfileHash { get; }
    internal byte PaletteIndex { get; }
    internal Ra2Rgba32 ResolvedRgba { get; }
    internal string SelectionHash { get; }
    internal string Source => "HumanPaletteSelection";

    internal static Ra2VoxelBaseColourSelectionResult Create(
        Ra2VoxelPaletteProfile palette,
        string paletteProfileHash,
        byte paletteIndex)
    {
        ArgumentNullException.ThrowIfNull(palette);
        if (!string.Equals(palette.ProfileHash, paletteProfileHash, StringComparison.OrdinalIgnoreCase))
            return new(Ra2VoxelBaseColourFailureKind.PaletteMismatch, "The selected base colour belongs to another palette.", null);
        if (palette.IsTransparent(paletteIndex) || palette[paletteIndex].Alpha != byte.MaxValue)
            return new(Ra2VoxelBaseColourFailureKind.TransparentIndex, "A transparent palette entry cannot be the body base colour.", null);
        if (palette.IsRemap(paletteIndex))
            return new(Ra2VoxelBaseColourFailureKind.RemapIndex, "A remap palette entry cannot be the body base colour.", null);

        string normalizedHash = palette.ProfileHash.ToUpperInvariant();
        string hash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-base-colour/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, normalizedHash);
            writer.Write(paletteIndex);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "HumanPaletteSelection");
        });
        return new(
            Ra2VoxelBaseColourFailureKind.None,
            string.Empty,
            new Ra2VoxelBaseColourSelection(normalizedHash, paletteIndex, palette[paletteIndex], hash));
    }
}

internal sealed record Ra2VoxelBaseColourSelectionResult(
    Ra2VoxelBaseColourFailureKind FailureKind,
    string Message,
    Ra2VoxelBaseColourSelection? Selection)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelBaseColourFailureKind.None && Selection is not null;
}

internal enum Ra2VoxelColourEdgePolicy
{
    None = 0,
    Subtle,
    Strong
}

internal enum Ra2VoxelMaterialSeparationPolicy
{
    Conservative = 0,
    Balanced,
    Strong
}

internal enum Ra2VoxelAccentPolicy
{
    PreserveMask = 0,
    EmphasizeSmallMask
}

internal enum Ra2VoxelQuantizationFallback
{
    WarnAndPreserveIntent = 0,
    Block
}

internal enum Ra2VoxelDualSurfacePolicy
{
    UnderPreferred = 0,
    BodyBase,
    TopPreferred
}

internal enum Ra2VoxelUndersideDirectionPolicy
{
    DarkerRequired = 0,
    EitherDirection,
    DarkerPreferred
}

internal enum Ra2VoxelTechniqueSpatialProfile
{
    BalancedVolume = 0,
    StrongMacroReadability,
    SubtleMatte,
    MaterialPriority,
    CompactClarity
}

internal sealed class Ra2VoxelColourTechniquePolicy
{
    internal const string LuminanceMetricId = "rec709-srgb-byte-luma-v1";
    internal const string ColourFamilyMetricId = "indexed-ramp-oklab-v2";

    internal Ra2VoxelColourTechniquePolicy(
        string techniqueId,
        string revision,
        string displayName,
        string description,
        int topLuminanceOffset,
        int sideLuminanceOffset,
        int darkLuminanceOffset,
        int preferredUndersideLuminanceOffset,
        Ra2VoxelColourEdgePolicy edgePolicy,
        int edgeLuminanceOffset,
        Ra2VoxelMaterialSeparationPolicy materialSeparationPolicy,
        int minimumBodyLuminanceSeparation,
        int darkOpeningMinimumDelta,
        Ra2VoxelAccentPolicy accentPolicy,
        Ra2VoxelQuantizationFallback quantizationFallback,
        Ra2VoxelTechniqueSpatialProfile spatialProfile = Ra2VoxelTechniqueSpatialProfile.BalancedVolume,
        int preferredBodyBandCount = 4,
        Ra2VoxelBoundaryIntent allowedBoundaryIntents = Ra2VoxelBoundaryIntent.RaisedBevel |
            Ra2VoxelBoundaryIntent.StructuralSeam | Ra2VoxelBoundaryIntent.ContactShadow,
        bool preserveMesoDetails = true,
        bool compressMicroDetails = true,
        double maximumAccentVisibleShare = 0.03d,
        double maximumAccentComponentShare = 0.015d,
        int maximumAccentLuminanceJump = 96,
        int minimumAccentRun = 2)
    {
        TechniqueId = Ra2VoxelColourContractIdentity.RequireIdentifier(techniqueId, nameof(techniqueId));
        Revision = Ra2VoxelColourContractIdentity.RequireIdentifier(revision, nameof(revision));
        DisplayName = Ra2VoxelColourContractIdentity.RequireSingleLine(displayName, nameof(displayName), 64);
        Description = Ra2VoxelColourContractIdentity.RequireSingleLine(description, nameof(description), 256);
        if (topLuminanceOffset is < 0 or > 64 || sideLuminanceOffset is < -64 or > 0 ||
            darkLuminanceOffset is < -96 or > -1 || preferredUndersideLuminanceOffset is < -96 or > 96 ||
            edgeLuminanceOffset is < 0 or > 64 || minimumBodyLuminanceSeparation is < 1 or > 32 ||
            darkOpeningMinimumDelta is < 12 or > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(topLuminanceOffset), "Technique luminance policy is outside the v1 bounds.");
        }
        if (!Enum.IsDefined(edgePolicy) || !Enum.IsDefined(materialSeparationPolicy) ||
            !Enum.IsDefined(accentPolicy) || !Enum.IsDefined(quantizationFallback) ||
            !Enum.IsDefined(spatialProfile))
        {
            throw new ArgumentException("Technique policy contains an unknown enum value.");
        }
        if ((edgePolicy == Ra2VoxelColourEdgePolicy.None) != (edgeLuminanceOffset == 0))
            throw new ArgumentException("Edge offset and edge policy are inconsistent.", nameof(edgeLuminanceOffset));
        const Ra2VoxelBoundaryIntent knownBoundaryIntents = Ra2VoxelBoundaryIntent.RaisedBevel |
            Ra2VoxelBoundaryIntent.StructuralSeam | Ra2VoxelBoundaryIntent.DeepOpening |
            Ra2VoxelBoundaryIntent.ContactShadow | Ra2VoxelBoundaryIntent.MaterialInterface |
            Ra2VoxelBoundaryIntent.PanelLine | Ra2VoxelBoundaryIntent.Silhouette |
            Ra2VoxelBoundaryIntent.DecorativeMark;
        if (preferredBodyBandCount is < 3 or > 6 ||
            (allowedBoundaryIntents & ~knownBoundaryIntents) != 0 ||
            !double.IsFinite(maximumAccentVisibleShare) || maximumAccentVisibleShare is <= 0d or > 0.25d ||
            !double.IsFinite(maximumAccentComponentShare) || maximumAccentComponentShare is <= 0d or > 0.15d ||
            maximumAccentComponentShare > maximumAccentVisibleShare ||
            maximumAccentLuminanceJump is < 16 or > 255 || minimumAccentRun is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(preferredBodyBandCount),
                "Technique spatial/detail/accent policy is outside the Rev.7 bounds.");
        }

        TopLuminanceOffset = topLuminanceOffset;
        SideLuminanceOffset = sideLuminanceOffset;
        DarkLuminanceOffset = darkLuminanceOffset;
        PreferredUndersideLuminanceOffset = preferredUndersideLuminanceOffset;
        EdgePolicy = edgePolicy;
        EdgeLuminanceOffset = edgeLuminanceOffset;
        MaterialSeparationPolicy = materialSeparationPolicy;
        MinimumBodyLuminanceSeparation = minimumBodyLuminanceSeparation;
        DarkOpeningMinimumDelta = darkOpeningMinimumDelta;
        AccentPolicy = accentPolicy;
        QuantizationFallback = quantizationFallback;
        SpatialProfile = spatialProfile;
        PreferredBodyBandCount = preferredBodyBandCount;
        AllowedBoundaryIntents = allowedBoundaryIntents;
        PreserveMesoDetails = preserveMesoDetails;
        CompressMicroDetails = compressMicroDetails;
        MaximumAccentVisibleShare = maximumAccentVisibleShare;
        MaximumAccentComponentShare = maximumAccentComponentShare;
        MaximumAccentLuminanceJump = maximumAccentLuminanceJump;
        MinimumAccentRun = minimumAccentRun;
        PolicyHash = ComputeHash();
    }

    internal string TechniqueId { get; }
    internal string Revision { get; }
    internal string DisplayName { get; }
    internal string Description { get; }
    internal int TopLuminanceOffset { get; }
    internal int SideLuminanceOffset { get; }
    internal int DarkLuminanceOffset { get; }
    internal int PreferredUndersideLuminanceOffset { get; }
    internal Ra2VoxelColourEdgePolicy EdgePolicy { get; }
    internal int EdgeLuminanceOffset { get; }
    internal Ra2VoxelMaterialSeparationPolicy MaterialSeparationPolicy { get; }
    internal int MinimumBodyLuminanceSeparation { get; }
    internal int DarkOpeningMinimumDelta { get; }
    internal Ra2VoxelAccentPolicy AccentPolicy { get; }
    internal Ra2VoxelQuantizationFallback QuantizationFallback { get; }
    internal Ra2VoxelTechniqueSpatialProfile SpatialProfile { get; }
    internal int PreferredBodyBandCount { get; }
    internal Ra2VoxelBoundaryIntent AllowedBoundaryIntents { get; }
    internal bool PreserveMesoDetails { get; }
    internal bool CompressMicroDetails { get; }
    internal double MaximumAccentVisibleShare { get; }
    internal double MaximumAccentComponentShare { get; }
    internal int MaximumAccentLuminanceJump { get; }
    internal int MinimumAccentRun { get; }
    internal string PolicyHash { get; }

    private string ComputeHash() => Ra2VoxelColourContractIdentity.ComputeHash(writer =>
    {
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-colour-technique/1");
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, TechniqueId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Revision);
        writer.Write(TopLuminanceOffset);
        writer.Write(SideLuminanceOffset);
        writer.Write(DarkLuminanceOffset);
        writer.Write(PreferredUndersideLuminanceOffset);
        writer.Write((int)EdgePolicy);
        writer.Write(EdgeLuminanceOffset);
        writer.Write((int)MaterialSeparationPolicy);
        writer.Write(MinimumBodyLuminanceSeparation);
        writer.Write(DarkOpeningMinimumDelta);
        writer.Write((int)AccentPolicy);
        writer.Write((int)QuantizationFallback);
        writer.Write((int)SpatialProfile);
        writer.Write(PreferredBodyBandCount);
        writer.Write((int)AllowedBoundaryIntents);
        writer.Write(PreserveMesoDetails);
        writer.Write(CompressMicroDetails);
        writer.Write(MaximumAccentVisibleShare);
        writer.Write(MaximumAccentComponentShare);
        writer.Write(MaximumAccentLuminanceJump);
        writer.Write(MinimumAccentRun);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, LuminanceMetricId);
        Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, ColourFamilyMetricId);
    });
}

internal static class Ra2VoxelColourTechniqueCatalog
{
    private static readonly Ra2VoxelColourTechniquePolicy[] Policies =
    [
        new("balanced-rts-volume", "3", "RTS 均衡体积", "四到五级连续形体色阶、主要倒角与均衡材质分离。",
            18, -8, -28, -38, Ra2VoxelColourEdgePolicy.Subtle, 24,
            Ra2VoxelMaterialSeparationPolicy.Balanced, 8, 18,
            Ra2VoxelAccentPolicy.PreserveMask, Ra2VoxelQuantizationFallback.WarnAndPreserveIntent,
            Ra2VoxelTechniqueSpatialProfile.BalancedVolume, 5,
            Ra2VoxelBoundaryIntent.RaisedBevel | Ra2VoxelBoundaryIntent.StructuralSeam |
                Ra2VoxelBoundaryIntent.ContactShadow, true, true, 0.030d, 0.015d, 88, 2),
        new("strong-silhouette-readability", "3", "强轮廓可读", "压缩细碎层次并强化宏观前后、上下和侧面体块。",
            28, -12, -38, -52, Ra2VoxelColourEdgePolicy.Strong, 34,
            Ra2VoxelMaterialSeparationPolicy.Strong, 12, 24,
            Ra2VoxelAccentPolicy.EmphasizeSmallMask, Ra2VoxelQuantizationFallback.WarnAndPreserveIntent,
            Ra2VoxelTechniqueSpatialProfile.StrongMacroReadability, 4,
            Ra2VoxelBoundaryIntent.RaisedBevel | Ra2VoxelBoundaryIntent.ContactShadow,
            true, true, 0.025d, 0.012d, 104, 2),
        new("subtle-matte-shading", "3", "克制哑光层次", "使用宽阔低对比色阶并抑制高亮边与微小装饰。",
            12, -5, -20, -28, Ra2VoxelColourEdgePolicy.Subtle, 15,
            Ra2VoxelMaterialSeparationPolicy.Conservative, 6, 14,
            Ra2VoxelAccentPolicy.PreserveMask, Ra2VoxelQuantizationFallback.WarnAndPreserveIntent,
            Ra2VoxelTechniqueSpatialProfile.SubtleMatte, 5,
            Ra2VoxelBoundaryIntent.StructuralSeam | Ra2VoxelBoundaryIntent.ContactShadow,
            true, true, 0.015d, 0.008d, 64, 3),
        new("semantic-material-separation", "3", "材质分离优先", "主体层次克制，优先保留材质内部色阶和材质差异。",
            16, -7, -26, -36, Ra2VoxelColourEdgePolicy.Subtle, 34,
            Ra2VoxelMaterialSeparationPolicy.Strong, 8, 18,
            Ra2VoxelAccentPolicy.PreserveMask, Ra2VoxelQuantizationFallback.Block,
            Ra2VoxelTechniqueSpatialProfile.MaterialPriority, 3,
            Ra2VoxelBoundaryIntent.StructuralSeam | Ra2VoxelBoundaryIntent.ContactShadow |
                Ra2VoxelBoundaryIntent.MaterialInterface, true, true, 0.025d, 0.012d, 80, 2),
        new("compact-unit-clarity", "3", "小型单位清晰化", "压缩微细节为三个大色块和少量关键识别点。",
            24, -10, -34, -46, Ra2VoxelColourEdgePolicy.Strong, 30,
            Ra2VoxelMaterialSeparationPolicy.Strong, 10, 22,
            Ra2VoxelAccentPolicy.EmphasizeSmallMask, Ra2VoxelQuantizationFallback.WarnAndPreserveIntent,
            Ra2VoxelTechniqueSpatialProfile.CompactClarity, 3,
            Ra2VoxelBoundaryIntent.RaisedBevel | Ra2VoxelBoundaryIntent.ContactShadow,
            false, true, 0.020d, 0.010d, 112, 2)
    ];

    internal static IReadOnlyList<Ra2VoxelColourTechniquePolicy> All { get; } = Array.AsReadOnly(Policies);
    internal static Ra2VoxelColourTechniquePolicy Default => Policies[0];

    internal static Ra2VoxelColourTechniquePolicy? Find(string techniqueId)
        => Policies.SingleOrDefault(policy => string.Equals(policy.TechniqueId, techniqueId, StringComparison.Ordinal));
}

internal sealed class Ra2VoxelUnitAdaptationPolicy
{
    internal Ra2VoxelUnitAdaptationPolicy(
        string adaptationId,
        string revision,
        Ra2VoxelUnitClass unitClass,
        string colouringSkillId,
        Ra2VoxelUndersideDirectionPolicy undersideDirection,
        Ra2VoxelDualSurfacePolicy dualSurfacePolicy,
        bool forceNeedsReview)
    {
        AdaptationId = Ra2VoxelColourContractIdentity.RequireIdentifier(adaptationId, nameof(adaptationId));
        Revision = Ra2VoxelColourContractIdentity.RequireIdentifier(revision, nameof(revision));
        ColouringSkillId = Ra2VoxelColourContractIdentity.RequireIdentifier(colouringSkillId, nameof(colouringSkillId));
        if (!Enum.IsDefined(unitClass) || !Enum.IsDefined(undersideDirection) || !Enum.IsDefined(dualSurfacePolicy))
            throw new ArgumentException("Unit adaptation contains an unknown enum value.");
        UnitClass = unitClass;
        UndersideDirection = undersideDirection;
        DualSurfacePolicy = dualSurfacePolicy;
        ForceNeedsReview = forceNeedsReview;
        PolicyHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-unit-adaptation/1");
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, AdaptationId);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, Revision);
            writer.Write((int)UnitClass);
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, ColouringSkillId);
            writer.Write((int)UndersideDirection);
            writer.Write((int)DualSurfacePolicy);
            writer.Write(ForceNeedsReview);
        });
    }

    internal string AdaptationId { get; }
    internal string Revision { get; }
    internal Ra2VoxelUnitClass UnitClass { get; }
    internal string ColouringSkillId { get; }
    internal Ra2VoxelUndersideDirectionPolicy UndersideDirection { get; }
    internal Ra2VoxelDualSurfacePolicy DualSurfacePolicy { get; }
    internal bool ForceNeedsReview { get; }
    internal string PolicyHash { get; }
}

internal static class Ra2VoxelUnitAdaptationCatalog
{
    private static readonly Ra2VoxelUnitAdaptationPolicy[] Policies =
    [
        new("ground", "2", Ra2VoxelUnitClass.Ground, "ra2-ground-voxel-colour-techniques",
            Ra2VoxelUndersideDirectionPolicy.DarkerRequired, Ra2VoxelDualSurfacePolicy.UnderPreferred, false),
        new("air", "2", Ra2VoxelUnitClass.Air, "ra2-air-voxel-colour-techniques",
            Ra2VoxelUndersideDirectionPolicy.EitherDirection, Ra2VoxelDualSurfacePolicy.BodyBase, false),
        new("large-surface", "2", Ra2VoxelUnitClass.LargeSurface, "ra2-large-surface-voxel-colour-techniques",
            Ra2VoxelUndersideDirectionPolicy.DarkerPreferred, Ra2VoxelDualSurfacePolicy.TopPreferred, false),
        new("unknown", "2", Ra2VoxelUnitClass.Unknown, "ra2-voxel-colour-techniques",
            Ra2VoxelUndersideDirectionPolicy.EitherDirection, Ra2VoxelDualSurfacePolicy.BodyBase, true)
    ];

    internal static IReadOnlyList<Ra2VoxelUnitAdaptationPolicy> All { get; } = Array.AsReadOnly(Policies);

    internal static Ra2VoxelUnitAdaptationPolicy For(Ra2VoxelUnitClass unitClass)
        => Policies.Single(policy => policy.UnitClass == unitClass);
}
