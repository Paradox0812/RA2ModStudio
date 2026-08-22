using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry;

internal static class Ra2FieldAppliesToNormalizer
{
    private static readonly Dictionary<string, Ra2SectionKind[]> CompositeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Building or Vehicle"] = [Ra2SectionKind.Building, Ra2SectionKind.Vehicle],
        ["Building/Vehicle"] = [Ra2SectionKind.Building, Ra2SectionKind.Vehicle],
        ["Building 或 Vehicle"] = [Ra2SectionKind.Building, Ra2SectionKind.Vehicle],
        ["Techno or SW"] = [Ra2SectionKind.Techno, Ra2SectionKind.SuperWeapon],
        ["Techno or SuperWeapon"] = [Ra2SectionKind.Techno, Ra2SectionKind.SuperWeapon],
        ["Techno/SW"] = [Ra2SectionKind.Techno, Ra2SectionKind.SuperWeapon],
        ["Techno 或 SW"] = [Ra2SectionKind.Techno, Ra2SectionKind.SuperWeapon]
    };

    private static readonly Dictionary<string, Ra2SectionKind> AliasMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Art"] = Ra2SectionKind.ArtObject,
        ["UnitArt"] = Ra2SectionKind.ArtObject,
        ["Sound"] = Ra2SectionKind.Sound,
        ["Side"] = Ra2SectionKind.Side,
        ["AttachEffect"] = Ra2SectionKind.AttachEffect,
        ["Shield"] = Ra2SectionKind.Shield,
        ["LaserTrail"] = Ra2SectionKind.LaserTrail,
        ["DigitalDisplay"] = Ra2SectionKind.DigitalDisplay,
        ["Banner"] = Ra2SectionKind.Banner,
        ["Insignia"] = Ra2SectionKind.Insignia,
        ["Radiation"] = Ra2SectionKind.Radiation,
        ["EVA"] = Ra2SectionKind.Eva,
        ["Eva"] = Ra2SectionKind.Eva,
        ["Tiberium"] = Ra2SectionKind.Tiberium,
        ["AircraftType"] = Ra2SectionKind.Aircraft,
        ["InfantryType"] = Ra2SectionKind.Infantry,
        ["VehicleType"] = Ra2SectionKind.Vehicle,
        ["BuildingType"] = Ra2SectionKind.Building,
        ["WeaponType"] = Ra2SectionKind.Weapon,
        ["ProjectileType"] = Ra2SectionKind.Projectile,
        ["WarheadType"] = Ra2SectionKind.Warhead,
        ["SuperWeaponType"] = Ra2SectionKind.SuperWeapon,
        ["AnimationType"] = Ra2SectionKind.Animation,
        ["VoxelAnimType"] = Ra2SectionKind.VoxelAnim,
        ["ParticleType"] = Ra2SectionKind.Particle,
        ["ParticleSystemType"] = Ra2SectionKind.ParticleSystem,
        ["TerrainType"] = Ra2SectionKind.Terrain,
        ["OverlayType"] = Ra2SectionKind.Overlay,
        ["SmudgeType"] = Ra2SectionKind.Smudge,
        ["CountryType"] = Ra2SectionKind.Country
    };

    public static bool TryNormalize(string? raw, out IReadOnlyList<Ra2SectionKind> kinds, out string? warning)
    {
        kinds = [];
        warning = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            warning = "appliesTo value is empty.";
            return false;
        }

        string value = raw.Trim();
        if (CompositeMappings.TryGetValue(value, out Ra2SectionKind[]? compositeKinds))
        {
            kinds = Array.AsReadOnly(compositeKinds);
            return true;
        }

        if (AliasMappings.TryGetValue(value, out Ra2SectionKind mappedKind))
        {
            kinds = Array.AsReadOnly([mappedKind]);
            return true;
        }

        if (Enum.TryParse(value, ignoreCase: true, out Ra2SectionKind parsed))
        {
            kinds = Array.AsReadOnly([parsed]);
            return true;
        }

        warning = $"unknown appliesTo value '{value}'.";
        return false;
    }
}
