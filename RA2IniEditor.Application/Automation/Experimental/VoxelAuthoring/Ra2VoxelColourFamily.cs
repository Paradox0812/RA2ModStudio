namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal enum Ra2VoxelBodyColourRole
{
    BodyBase = 0,
    BodyLight,
    BodyMid,
    BodyDark,
    Underside,
    EdgeOrRidge
}

internal enum Ra2VoxelColourFamilyFailureKind
{
    None = 0,
    PaletteMismatch,
    PaletteFamilyUnavailable,
    PolicyInvalid
}

internal sealed record Ra2VoxelColourFamilyRoleSelection(
    Ra2VoxelBodyColourRole Role,
    byte PaletteIndex,
    double TargetLuminance,
    double ActualLuminance,
    double AnchorHueDriftDegrees,
    double AnchorChromaDelta,
    bool FamilyFallback);

internal sealed class Ra2VoxelColourFamilySelection
{
    private readonly Ra2VoxelColourFamilyRoleSelection[] _roles;
    private readonly string[] _warnings;

    internal Ra2VoxelColourFamilySelection(
        IEnumerable<Ra2VoxelColourFamilyRoleSelection> roles,
        IEnumerable<string> warnings)
    {
        _roles = roles.OrderBy(value => value.Role).ToArray();
        _warnings = warnings.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
        SelectionHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-colour-family-selection/2");
            writer.Write(_roles.Length);
            foreach (Ra2VoxelColourFamilyRoleSelection role in _roles)
            {
                writer.Write((int)role.Role);
                writer.Write(role.PaletteIndex);
                writer.Write(role.TargetLuminance);
                writer.Write(role.ActualLuminance);
                writer.Write(role.AnchorHueDriftDegrees);
                writer.Write(role.AnchorChromaDelta);
                writer.Write(role.FamilyFallback);
            }
            writer.Write(_warnings.Length);
            foreach (string warning in _warnings)
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, warning);
        });
    }

    internal IReadOnlyList<Ra2VoxelColourFamilyRoleSelection> Roles => Array.AsReadOnly(_roles);
    internal IReadOnlyList<string> Warnings => Array.AsReadOnly(_warnings);
    internal string SelectionHash { get; }
    internal Ra2VoxelColourFamilyRoleSelection this[Ra2VoxelBodyColourRole role] => _roles.Single(value => value.Role == role);
}

internal sealed record Ra2VoxelColourFamilyResult(
    Ra2VoxelColourFamilyFailureKind FailureKind,
    string Message,
    Ra2VoxelColourFamilySelection? Selection)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelColourFamilyFailureKind.None && Selection is not null;
}

/// <summary>
/// Shared deterministic selector for ordinary normalization and the review-only contrast candidate.
/// BodyBase is an immutable human selection; every derived role remains in its OKLab anchor family.
/// </summary>
internal static class Ra2VoxelColourFamilySelector
{
    internal const string Revision = "indexed-ramp-oklab-family-selector/2";
    private const double ChromaticHueDriftLimit = 30d;
    private const double ChromaticChromaDeltaLimit = 0.12d;
    private const double NeutralAnchorChromaLimit = 0.035d;
    private const double NeutralCandidateChromaLimit = 0.055d;

    internal static Ra2VoxelColourFamilyResult Select(
        Ra2VoxelPaletteProfile palette,
        Ra2VoxelBaseColourSelection baseColour,
        Ra2VoxelColourTechniquePolicy technique,
        Ra2VoxelUnitAdaptationPolicy adaptation,
        Ra2CompiledVoxelStylePlan? rawPlan = null,
        bool contrast = false)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(baseColour);
        ArgumentNullException.ThrowIfNull(technique);
        ArgumentNullException.ThrowIfNull(adaptation);
        if (!string.Equals(palette.ProfileHash, baseColour.PaletteProfileHash, StringComparison.OrdinalIgnoreCase) ||
            palette.IsTransparent(baseColour.PaletteIndex) || palette.IsRemap(baseColour.PaletteIndex) ||
            palette[baseColour.PaletteIndex] != baseColour.ResolvedRgba)
        {
            return Failure(Ra2VoxelColourFamilyFailureKind.PaletteMismatch,
                "The body base selection does not match the active palette.");
        }
        if (!string.Equals(technique.PolicyHash, Ra2VoxelColourTechniqueCatalog.Find(technique.TechniqueId)?.PolicyHash,
                StringComparison.Ordinal) ||
            adaptation.UnitClass != Ra2VoxelUnitAdaptationCatalog.For(adaptation.UnitClass).UnitClass ||
            !string.Equals(adaptation.PolicyHash, Ra2VoxelUnitAdaptationCatalog.For(adaptation.UnitClass).PolicyHash,
                StringComparison.Ordinal))
        {
            return Failure(Ra2VoxelColourFamilyFailureKind.PolicyInvalid,
                "The colour technique or unit adaptation policy identity is invalid.");
        }

        Candidate anchor = Candidate.Create(baseColour.PaletteIndex, palette[baseColour.PaletteIndex]);
        Candidate[] preferredFamily = Enumerable.Range(0, Ra2VoxelPaletteProfile.ColourCount)
            .Select(index => checked((byte)index))
            .Where(index => !palette.IsTransparent(index) && !palette.IsRemap(index))
            .Select(index => Candidate.Create(index, palette[index]))
            .Where(candidate => IsPreferredFamily(anchor, candidate))
            .OrderBy(candidate => candidate.Index)
            .ToArray();
        if (preferredFamily.Length == 0)
            return Failure(Ra2VoxelColourFamilyFailureKind.PaletteFamilyUnavailable,
                "The active palette has no eligible colour in the selected anchor family.");

        int minimum = technique.MinimumBodyLuminanceSeparation;
        Candidate[] indexedRamp = preferredFamily
            .Where(candidate => candidate.Index / 16 == anchor.Index / 16)
            .ToArray();
        bool indexedHierarchyComplete = CanSatisfyBodyHierarchy(indexedRamp, anchor, minimum, adaptation);
        bool usesIndexedRamp = indexedHierarchyComplete ||
            (anchor.Chroma >= NeutralAnchorChromaLimit && indexedRamp.Length >= 4);
        Candidate[] family = usesIndexedRamp ? indexedRamp : preferredFamily;
        double boost = contrast ? minimum : 0d;
        double topTarget = Clamp(anchor.Luminance + technique.TopLuminanceOffset + boost);
        double midTarget = Clamp(anchor.Luminance + technique.SideLuminanceOffset - boost);
        double darkTarget = Clamp(anchor.Luminance + technique.DarkLuminanceOffset - boost);
        double preferredUnderOffset = technique.PreferredUndersideLuminanceOffset;
        if (adaptation.UndersideDirection == Ra2VoxelUndersideDirectionPolicy.EitherDirection &&
            rawPlan?.Roles.FirstOrDefault(role => role.Category == Ra2VoxelStyleRoleCategory.Underside) is { } rawUnder)
        {
            double rawUnderLuminance = Luminance(palette[rawUnder.PaletteIndex]);
            preferredUnderOffset = rawUnderLuminance >= anchor.Luminance
                ? Math.Abs(preferredUnderOffset)
                : -Math.Abs(preferredUnderOffset);
        }
        double underTarget = Clamp(anchor.Luminance + preferredUnderOffset +
            (contrast ? Math.Sign(preferredUnderOffset == 0d ? -1d : preferredUnderOffset) * boost : 0d));
        double edgeTarget = Clamp(anchor.Luminance + technique.EdgeLuminanceOffset + boost);

        List<string> warnings = [];
        if (usesIndexedRamp && !indexedHierarchyComplete)
            warnings.Add("IndexedPaletteRampIncomplete");
        else if (!usesIndexedRamp && anchor.Chroma >= NeutralAnchorChromaLimit)
            warnings.Add("IndexedPaletteRampUnavailable");
        try
        {
            List<Ra2VoxelColourFamilyRoleSelection> selected =
            [
                Selection(Ra2VoxelBodyColourRole.BodyBase, anchor, anchor.Luminance, anchor, false)
            ];
            Candidate light = Pick(
            Ra2VoxelBodyColourRole.BodyLight,
            topTarget,
            candidate => candidate.Luminance >= anchor.Luminance + minimum,
            candidate => Math.Max(0d, anchor.Luminance + minimum - candidate.Luminance));
        Candidate mid = Pick(
            Ra2VoxelBodyColourRole.BodyMid,
            midTarget,
            candidate => candidate.Luminance <= anchor.Luminance - minimum,
            candidate => Math.Max(0d, candidate.Luminance - (anchor.Luminance - minimum)));
        Candidate dark = Pick(
            Ra2VoxelBodyColourRole.BodyDark,
            darkTarget,
            candidate => candidate.Luminance <= mid.Luminance - minimum,
            candidate => Math.Max(0d, candidate.Luminance - (mid.Luminance - minimum)));

        Func<Candidate, bool> underRelation = adaptation.UndersideDirection switch
        {
            Ra2VoxelUndersideDirectionPolicy.DarkerRequired =>
                candidate => candidate.Luminance <= dark.Luminance - minimum,
            Ra2VoxelUndersideDirectionPolicy.EitherDirection =>
                candidate => Math.Abs(candidate.Luminance - anchor.Luminance) >= minimum,
            Ra2VoxelUndersideDirectionPolicy.DarkerPreferred =>
                candidate => candidate.Luminance < anchor.Luminance,
            _ => _ => false
        };
        Func<Candidate, double> underViolation = adaptation.UndersideDirection switch
        {
            Ra2VoxelUndersideDirectionPolicy.DarkerRequired =>
                candidate => Math.Max(0d, candidate.Luminance - (dark.Luminance - minimum)),
            Ra2VoxelUndersideDirectionPolicy.EitherDirection =>
                candidate => Math.Max(0d, minimum - Math.Abs(candidate.Luminance - anchor.Luminance)),
            Ra2VoxelUndersideDirectionPolicy.DarkerPreferred =>
                candidate => Math.Max(0d, candidate.Luminance - anchor.Luminance),
            _ => _ => double.MaxValue
        };
        Candidate under = Pick(
            Ra2VoxelBodyColourRole.Underside,
            underTarget,
            underRelation,
            underViolation,
            softPreference: adaptation.UndersideDirection == Ra2VoxelUndersideDirectionPolicy.DarkerPreferred);
        Candidate edge = technique.EdgePolicy == Ra2VoxelColourEdgePolicy.None
            ? anchor
            : Pick(
                Ra2VoxelBodyColourRole.EdgeOrRidge,
                edgeTarget,
                candidate => candidate.Luminance >= anchor.Luminance + minimum,
                candidate => Math.Max(0d, anchor.Luminance + minimum - candidate.Luminance));

            selected.Add(Selection(Ra2VoxelBodyColourRole.BodyLight, light, topTarget, anchor,
                !light.Satisfies(candidate => candidate.Luminance >= anchor.Luminance + minimum)));
            selected.Add(Selection(Ra2VoxelBodyColourRole.BodyMid, mid, midTarget, anchor,
                !mid.Satisfies(candidate => candidate.Luminance <= anchor.Luminance - minimum)));
            selected.Add(Selection(Ra2VoxelBodyColourRole.BodyDark, dark, darkTarget, anchor,
                !dark.Satisfies(candidate => candidate.Luminance <= mid.Luminance - minimum)));
            selected.Add(Selection(Ra2VoxelBodyColourRole.Underside, under, underTarget, anchor, !underRelation(under)));
            selected.Add(Selection(Ra2VoxelBodyColourRole.EdgeOrRidge, edge, edgeTarget, anchor,
                technique.EdgePolicy != Ra2VoxelColourEdgePolicy.None &&
                edge.Luminance < anchor.Luminance + minimum));
            return new(Ra2VoxelColourFamilyFailureKind.None, string.Empty,
                new Ra2VoxelColourFamilySelection(selected, warnings));
        }
        catch (PaletteFamilyUnavailableException exception)
        {
            return Failure(Ra2VoxelColourFamilyFailureKind.PaletteFamilyUnavailable,
                $"The active palette cannot satisfy the required {exception.Role} relation inside the selected anchor family.");
        }

        Candidate Pick(
            Ra2VoxelBodyColourRole role,
            double target,
            Func<Candidate, bool> relation,
            Func<Candidate, double> relationViolation,
            bool softPreference = false)
        {
            Candidate? exact = family
                .Where(relation)
                .OrderBy(candidate => Math.Abs(candidate.Luminance - target))
                .ThenBy(candidate => candidate.DistanceSquared(anchor))
                .ThenBy(candidate => candidate.Index)
                .Cast<Candidate?>()
                .FirstOrDefault();
            if (exact is Candidate value)
                return value;
            if (!softPreference && technique.QuantizationFallback == Ra2VoxelQuantizationFallback.Block)
                throw new PaletteFamilyUnavailableException(role);
            Candidate fallback = family
                .OrderBy(relationViolation)
                .ThenBy(candidate => Math.Abs(candidate.Luminance - target))
                .ThenBy(candidate => candidate.DistanceSquared(anchor))
                .ThenBy(candidate => candidate.Index)
                .First();
            warnings.Add($"PaletteFamilyFallback:{role}");
            return fallback;
        }
    }

    private static Ra2VoxelColourFamilyRoleSelection Selection(
        Ra2VoxelBodyColourRole role,
        Candidate candidate,
        double target,
        Candidate anchor,
        bool fallback)
    {
        double hue = anchor.Chroma < NeutralAnchorChromaLimit || candidate.Chroma < 0.000001d
            ? 0d
            : HueDifference(anchor.HueDegrees, candidate.HueDegrees);
        return new(role, candidate.Index, target, candidate.Luminance, hue,
            Math.Abs(candidate.Chroma - anchor.Chroma), fallback);
    }

    private static bool IsPreferredFamily(Candidate anchor, Candidate candidate)
        => anchor.Chroma < NeutralAnchorChromaLimit
            ? candidate.Chroma <= NeutralCandidateChromaLimit
            : HueDifference(anchor.HueDegrees, candidate.HueDegrees) <= ChromaticHueDriftLimit &&
              Math.Abs(candidate.Chroma - anchor.Chroma) <= ChromaticChromaDeltaLimit;

    private static bool CanSatisfyBodyHierarchy(
        IReadOnlyList<Candidate> candidates,
        Candidate anchor,
        int minimum,
        Ra2VoxelUnitAdaptationPolicy adaptation)
    {
        bool hasLight = candidates.Any(candidate => candidate.Luminance >= anchor.Luminance + minimum);
        Candidate? mid = candidates
            .Where(candidate => candidate.Luminance <= anchor.Luminance - minimum)
            .OrderByDescending(candidate => candidate.Luminance)
            .Cast<Candidate?>()
            .FirstOrDefault();
        if (!hasLight || mid is null)
            return false;
        Candidate? dark = candidates
            .Where(candidate => candidate.Luminance <= mid.Value.Luminance - minimum)
            .OrderByDescending(candidate => candidate.Luminance)
            .Cast<Candidate?>()
            .FirstOrDefault();
        if (dark is null)
            return false;
        return adaptation.UndersideDirection switch
        {
            Ra2VoxelUndersideDirectionPolicy.DarkerRequired =>
                candidates.Any(candidate => candidate.Luminance <= dark.Value.Luminance - minimum),
            Ra2VoxelUndersideDirectionPolicy.EitherDirection =>
                candidates.Any(candidate => Math.Abs(candidate.Luminance - anchor.Luminance) >= minimum),
            Ra2VoxelUndersideDirectionPolicy.DarkerPreferred =>
                candidates.Any(candidate => candidate.Luminance < dark.Value.Luminance),
            _ => false
        };
    }

    internal static double Luminance(Ra2Rgba32 colour)
        => (0.2126d * colour.Red) + (0.7152d * colour.Green) + (0.0722d * colour.Blue);

    private static double Clamp(double value) => Math.Clamp(value, 0d, 255d);

    private static double HueDifference(double left, double right)
    {
        double value = Math.Abs(left - right) % 360d;
        return value > 180d ? 360d - value : value;
    }

    private static Ra2VoxelColourFamilyResult Failure(Ra2VoxelColourFamilyFailureKind kind, string message)
        => new(kind, message, null);

    private sealed class PaletteFamilyUnavailableException(Ra2VoxelBodyColourRole role) : Exception
    {
        internal Ra2VoxelBodyColourRole Role { get; } = role;
    }

    private readonly record struct Candidate(
        byte Index,
        double Luminance,
        double L,
        double A,
        double B,
        double Chroma,
        double HueDegrees)
    {
        internal static Candidate Create(byte index, Ra2Rgba32 colour)
        {
            static double Linear(byte value)
            {
                double channel = value / 255d;
                return channel <= 0.04045d
                    ? channel / 12.92d
                    : Math.Pow((channel + 0.055d) / 1.055d, 2.4d);
            }

            double red = Linear(colour.Red);
            double green = Linear(colour.Green);
            double blue = Linear(colour.Blue);
            double l = Math.Cbrt(0.4122214708d * red + 0.5363325363d * green + 0.0514459929d * blue);
            double m = Math.Cbrt(0.2119034982d * red + 0.6806995451d * green + 0.1073969566d * blue);
            double s = Math.Cbrt(0.0883024619d * red + 0.2817188376d * green + 0.6299787005d * blue);
            double okL = 0.2104542553d * l + 0.793617785d * m - 0.0040720468d * s;
            double okA = 1.9779984951d * l - 2.428592205d * m + 0.4505937099d * s;
            double okB = 0.0259040371d * l + 0.7827717662d * m - 0.808675766d * s;
            double chroma = Math.Sqrt(okA * okA + okB * okB);
            double hue = Math.Atan2(okB, okA) * 180d / Math.PI;
            if (hue < 0d) hue += 360d;
            return new(index, Ra2VoxelColourFamilySelector.Luminance(colour), okL, okA, okB, chroma, hue);
        }

        internal double DistanceSquared(Candidate other)
        {
            double l = L - other.L;
            double a = A - other.A;
            double b = B - other.B;
            return l * l + a * a + b * b;
        }

        internal bool Satisfies(Func<Candidate, bool> predicate) => predicate(this);
    }
}
