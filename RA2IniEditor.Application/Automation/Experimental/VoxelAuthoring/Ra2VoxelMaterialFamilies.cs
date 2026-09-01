namespace RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring;

internal sealed record Ra2VoxelMaterialFamilyRoleSelection(
    string BoundRoleId,
    Ra2VoxelStyleRoleCategory Category,
    byte BaseIndex,
    byte HighlightIndex,
    byte ShadowIndex,
    bool HighlightFallback,
    bool ShadowFallback);

internal sealed class Ra2VoxelMaterialFamilySelection
{
    private readonly Ra2VoxelMaterialFamilyRoleSelection[] _families;

    internal Ra2VoxelMaterialFamilySelection(IEnumerable<Ra2VoxelMaterialFamilyRoleSelection> families)
    {
        _families = (families ?? throw new ArgumentNullException(nameof(families)))
            .OrderBy(value => value.BoundRoleId, StringComparer.Ordinal)
            .ToArray();
        SelectionHash = Ra2VoxelColourContractIdentity.ComputeHash(writer =>
        {
            Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, "ra2-voxel-material-family/1");
            writer.Write(_families.Length);
            foreach (Ra2VoxelMaterialFamilyRoleSelection family in _families)
            {
                Ra2VoxelSceneSnapshot.WriteCanonicalString(writer, family.BoundRoleId);
                writer.Write((int)family.Category);
                writer.Write(family.BaseIndex);
                writer.Write(family.HighlightIndex);
                writer.Write(family.ShadowIndex);
                writer.Write(family.HighlightFallback);
                writer.Write(family.ShadowFallback);
            }
        });
    }

    internal IReadOnlyList<Ra2VoxelMaterialFamilyRoleSelection> Families => Array.AsReadOnly(_families);
    internal string SelectionHash { get; }
    internal Ra2VoxelMaterialFamilyRoleSelection? Find(string boundRoleId) =>
        _families.SingleOrDefault(value => string.Equals(value.BoundRoleId, boundRoleId, StringComparison.Ordinal));
}

internal static class Ra2VoxelMaterialFamilySelector
{
    internal const string Revision = "material-family-selector/1";

    internal static Ra2VoxelMaterialFamilySelection Select(
        Ra2VoxelPaletteProfile palette,
        Ra2CompiledVoxelStylePlan plan,
        Ra2VoxelSemanticColourBindingPlan bindingPlan)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(bindingPlan);
        Dictionary<string, Ra2CompiledVoxelStyleRole> roles = plan.Roles.ToDictionary(value => value.Id, StringComparer.Ordinal);
        List<Ra2VoxelMaterialFamilyRoleSelection> families = [];
        foreach (Ra2VoxelSemanticColourBinding binding in bindingPlan.Bindings
                     .Where(value => value.BindingMode == Ra2VoxelSemanticColourBindingMode.DirectRole &&
                                     value.Requirement is not (Ra2VoxelSemanticColourRequirementKind.ApprovedRemap or
                                         Ra2VoxelSemanticColourRequirementKind.DarkOpening))
                     .OrderBy(value => value.RoleId, StringComparer.Ordinal))
        {
            if (!roles.TryGetValue(binding.RoleId, out Ra2CompiledVoxelStyleRole? role))
                throw new ArgumentException("A bound material role is missing from the compiled plan.");
            byte[] ramp = Enumerable.Range((role.PaletteIndex / 16) * 16, 16)
                .Select(value => checked((byte)value))
                .Where(value => !palette.IsTransparent(value) && !palette.IsRemap(value))
                .ToArray();
            double baseline = Luminance(palette[role.PaletteIndex]);
            byte highlight = ramp.Where(value => Luminance(palette[value]) > baseline)
                .OrderBy(value => Luminance(palette[value]) - baseline)
                .ThenBy(value => value)
                .FirstOrDefault(role.PaletteIndex);
            byte shadow = ramp.Where(value => Luminance(palette[value]) < baseline)
                .OrderBy(value => baseline - Luminance(palette[value]))
                .ThenBy(value => value)
                .FirstOrDefault(role.PaletteIndex);
            families.Add(new(role.Id, role.Category, role.PaletteIndex, highlight, shadow,
                highlight == role.PaletteIndex, shadow == role.PaletteIndex));
        }
        return new(families);
    }

    private static double Luminance(Ra2Rgba32 colour) =>
        (0.2126d * colour.Red) + (0.7152d * colour.Green) + (0.0722d * colour.Blue);
}
