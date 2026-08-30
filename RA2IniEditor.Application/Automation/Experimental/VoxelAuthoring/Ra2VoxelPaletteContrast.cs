namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal sealed record Ra2VoxelPaletteContrastFacts(
    double MinimumBodyLuminanceSeparationBefore,
    double MinimumBodyLuminanceSeparationAfter,
    int ChangedRoleCount,
    bool ExactPaletteSelectionsPreserved);

internal sealed record Ra2VoxelPaletteContrastResult(
    Ra2CompiledVoxelStylePlan Plan,
    Ra2VoxelPaletteContrastFacts Facts);

/// <summary>
/// Produces a review-only palette candidate with readable body shading. Explicit palette selections,
/// semantic materials, remap roles, rules, source scopes and the input plan are never changed.
/// </summary>
internal static class Ra2VoxelPaletteContrastOptimizer
{
    internal const string Revision = "palette-contrast-v1";
    private const double MinimumUsefulSeparation = 10d;

    internal static Ra2VoxelPaletteContrastResult Optimize(
        Ra2CompiledVoxelStylePlan source,
        Ra2VoxelPaletteProfile palette)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(palette);
        if (!string.Equals(source.PaletteHash, palette.ProfileHash, StringComparison.Ordinal))
            throw new ArgumentException("The style plan does not belong to the active palette.", nameof(palette));

        Ra2CompiledVoxelStyleRole[] originalRoles = source.Roles.ToArray();
        double before = MinimumBodySeparation(originalRoles, palette);
        if (before >= MinimumUsefulSeparation)
            return new(source, new(before, before, 0, true));

        Ra2CompiledVoxelStyleRole? baseRole = originalRoles.FirstOrDefault(role =>
            role.Category == Ra2VoxelStyleRoleCategory.BodyBase);
        if (baseRole is null)
            return new(source, new(before, before, 0, true));

        Ra2Rgba32 baseColour = palette[baseRole.PaletteIndex];
        double baseLuminance = Luminance(baseColour);
        List<Ra2CompiledVoxelStyleRole> roles = new(originalRoles.Length);
        int changed = 0;
        foreach (Ra2CompiledVoxelStyleRole role in originalRoles)
        {
            if (role.RequestedExactPaletteIndex.HasValue || !TryGetTargetOffset(role.Category, out double offset))
            {
                roles.Add(role);
                continue;
            }

            byte selected = SelectPaletteIndex(palette, baseColour, Math.Clamp(baseLuminance + offset, 0d, 255d));
            if (selected != role.PaletteIndex)
                changed++;
            roles.Add(role with { PaletteIndex = selected });
        }

        if (changed == 0)
            return new(source, new(before, before, 0, true));

        string note = "Body shading palette indices were adjusted deterministically for review contrast; exact, semantic and remap selections were preserved.";
        string[] assumptions = source.UnresolvedAssumptions
            .Append(note)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        Ra2CompiledVoxelStylePlan candidate = new(
            source.Title,
            source.Summary,
            source.SourcePackHash,
            source.PaletteHash,
            string.Concat(source.CompilerRevision, "+", Revision),
            source.ModelIdentity,
            source.RemapPolicy,
            source.InteriorRoleId,
            roles,
            source.Rules,
            assumptions);
        double after = MinimumBodySeparation(candidate.Roles, palette);
        return new(candidate, new(before, after, changed, ExactSelectionsMatch(originalRoles, candidate.Roles)));
    }

    private static byte SelectPaletteIndex(
        Ra2VoxelPaletteProfile palette,
        Ra2Rgba32 anchor,
        double targetLuminance)
    {
        int selected = -1;
        double minimumScore = double.MaxValue;
        for (int index = 0; index < Ra2VoxelPaletteProfile.ColourCount; index++)
        {
            byte paletteIndex = checked((byte)index);
            if (palette.IsTransparent(paletteIndex) || palette.IsRemap(paletteIndex))
                continue;
            Ra2Rgba32 colour = palette[paletteIndex];
            double lumaError = Luminance(colour) - targetLuminance;
            double red = colour.Red - anchor.Red;
            double green = colour.Green - anchor.Green;
            double blue = colour.Blue - anchor.Blue;
            // Luminance drives readability; chroma distance keeps the selected family visually coherent.
            double score = (lumaError * lumaError * 4d) + red * red + green * green + blue * blue;
            if (score < minimumScore)
            {
                minimumScore = score;
                selected = index;
            }
        }
        if (selected < 0)
            throw new InvalidOperationException("The active palette has no eligible opaque non-remap colour.");
        return checked((byte)selected);
    }

    private static bool TryGetTargetOffset(Ra2VoxelStyleRoleCategory category, out double offset)
    {
        offset = category switch
        {
            Ra2VoxelStyleRoleCategory.BodyLight => 30d,
            Ra2VoxelStyleRoleCategory.BodyBase => 0d,
            Ra2VoxelStyleRoleCategory.BodyMid => -20d,
            Ra2VoxelStyleRoleCategory.BodyDark => -42d,
            Ra2VoxelStyleRoleCategory.Underside => -58d,
            _ => 0d
        };
        return category is Ra2VoxelStyleRoleCategory.BodyLight or
            Ra2VoxelStyleRoleCategory.BodyBase or
            Ra2VoxelStyleRoleCategory.BodyMid or
            Ra2VoxelStyleRoleCategory.BodyDark or
            Ra2VoxelStyleRoleCategory.Underside;
    }

    private static double MinimumBodySeparation(
        IEnumerable<Ra2CompiledVoxelStyleRole> roles,
        Ra2VoxelPaletteProfile palette)
    {
        double[] values = roles
            .Where(role => TryGetTargetOffset(role.Category, out _))
            .Select(role => Luminance(palette[role.PaletteIndex]))
            .Distinct()
            .OrderBy(value => value)
            .ToArray();
        if (values.Length < 2)
            return 0d;
        return values.Zip(values.Skip(1), (left, right) => right - left).Min();
    }

    private static bool ExactSelectionsMatch(
        IReadOnlyList<Ra2CompiledVoxelStyleRole> source,
        IReadOnlyList<Ra2CompiledVoxelStyleRole> candidate)
        => source.Where(role => role.RequestedExactPaletteIndex.HasValue).All(role =>
            candidate.Single(value => value.Id == role.Id).PaletteIndex == role.PaletteIndex);

    private static double Luminance(Ra2Rgba32 colour)
        => (0.2126d * colour.Red) + (0.7152d * colour.Green) + (0.0722d * colour.Blue);
}
