using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation;

internal enum Ra2ContentRegistrationPolicy
{
    ExplicitNumberedList = 0,
    ReferenceReachable,
    StructuredTuple,
    CrossFileArtifact
}

internal sealed class Ra2ContentTemplateRegistrationSpec
{
    public Ra2ContentTemplateRegistrationSpec(
        string registrySectionName,
        Ra2ContentTemplateValueSource objectIdSource,
        Ra2SectionKind expectedObjectKind,
        Ra2ContentRegistrationPolicy policy = Ra2ContentRegistrationPolicy.ExplicitNumberedList)
    {
        RegistrySectionName = Ra2ContentTemplateValidation.ValidateName(
            registrySectionName,
            nameof(registrySectionName));
        ObjectIdSource = objectIdSource ?? throw new ArgumentNullException(nameof(objectIdSource));
        if (!Enum.IsDefined(expectedObjectKind) || expectedObjectKind == Ra2SectionKind.Unknown)
            throw new ArgumentOutOfRangeException(nameof(expectedObjectKind));
        if (!Enum.IsDefined(policy))
            throw new ArgumentOutOfRangeException(nameof(policy));

        ExpectedObjectKind = expectedObjectKind;
        Policy = policy;
    }

    public string RegistrySectionName { get; }
    public Ra2ContentTemplateValueSource ObjectIdSource { get; }
    public Ra2SectionKind ExpectedObjectKind { get; }
    public Ra2ContentRegistrationPolicy Policy { get; }
}

internal static class Ra2ContentRegistryKindCatalog
{
    private static readonly IReadOnlyDictionary<string, Ra2SectionKind> KnownKinds =
        new Dictionary<string, Ra2SectionKind>(StringComparer.OrdinalIgnoreCase)
        {
            ["InfantryTypes"] = Ra2SectionKind.Infantry,
            ["VehicleTypes"] = Ra2SectionKind.Vehicle,
            ["AircraftTypes"] = Ra2SectionKind.Aircraft,
            ["BuildingTypes"] = Ra2SectionKind.Building,
            ["WeaponTypes"] = Ra2SectionKind.Weapon,
            ["SuperWeaponTypes"] = Ra2SectionKind.SuperWeapon,
            ["Warheads"] = Ra2SectionKind.Warhead,
            ["WarheadTypes"] = Ra2SectionKind.Warhead,
            ["Projectiles"] = Ra2SectionKind.Projectile,
            ["ProjectileTypes"] = Ra2SectionKind.Projectile,
            ["Animations"] = Ra2SectionKind.Animation,
            ["VoxelAnims"] = Ra2SectionKind.VoxelAnim,
            ["Particles"] = Ra2SectionKind.Particle,
            ["ParticleSystems"] = Ra2SectionKind.ParticleSystem,
            ["TerrainTypes"] = Ra2SectionKind.Terrain,
            ["OverlayTypes"] = Ra2SectionKind.Overlay,
            ["AITriggerTypes"] = Ra2SectionKind.AITrigger,
            ["TaskForces"] = Ra2SectionKind.TaskForce,
            ["ScriptTypes"] = Ra2SectionKind.Script,
            ["TeamTypes"] = Ra2SectionKind.TeamType,
            ["ShieldTypes"] = Ra2SectionKind.Shield,
            ["AttachEffectTypes"] = Ra2SectionKind.AttachEffect,
            ["LaserTrailTypes"] = Ra2SectionKind.LaserTrail,
            ["DigitalDisplayTypes"] = Ra2SectionKind.DigitalDisplay,
            ["DigitalDisplays"] = Ra2SectionKind.DigitalDisplay,
            ["BannerTypes"] = Ra2SectionKind.Banner,
            ["InsigniaTypes"] = Ra2SectionKind.Insignia
        };

    public static bool TryGetObjectKind(string registrySectionName, out Ra2SectionKind objectKind)
        => KnownKinds.TryGetValue(registrySectionName, out objectKind);
}

internal sealed class Ra2ContentRegistrationAllocationState
{
    private readonly HashSet<int> _indexes;
    private readonly HashSet<string> _objectIds;

    private Ra2ContentRegistrationAllocationState(
        HashSet<int> indexes,
        HashSet<string> objectIds,
        int nextIndex)
    {
        _indexes = indexes;
        _objectIds = objectIds;
        NextIndex = nextIndex;
    }

    public int NextIndex { get; private set; }

    public bool ContainsObject(string objectId) => _objectIds.Contains(objectId);

    public bool TryReserve(string objectId, out int index)
    {
        index = NextIndex;
        if (NextIndex < 0)
            return false;

        _indexes.Add(index);
        _objectIds.Add(objectId);
        NextIndex = index == int.MaxValue ? -1 : index + 1;
        return true;
    }

    public static bool TryCreate(
        IReadOnlyList<Ra2AutomationFieldFact> fields,
        out Ra2ContentRegistrationAllocationState? state,
        out Ra2ContentTemplateCompilationFailureKind failureKind,
        out string message)
    {
        HashSet<int> indexes = [];
        HashSet<string> objectIds = new(StringComparer.OrdinalIgnoreCase);
        int maximumIndex = -1;

        foreach (Ra2AutomationFieldFact field in fields)
        {
            if (!int.TryParse(field.Key, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int index) ||
                index < 0 ||
                !Ra2ContentTemplateValidation.IsValidIdentifier(field.EffectiveValue))
            {
                state = null;
                failureKind = Ra2ContentTemplateCompilationFailureKind.InvalidRegistrationList;
                message = "The registration list contains a non-numeric key or invalid object identifier.";
                return false;
            }

            if (!indexes.Add(index))
            {
                state = null;
                failureKind = Ra2ContentTemplateCompilationFailureKind.InvalidRegistrationList;
                message = "The registration list contains duplicate numeric indexes.";
                return false;
            }

            string objectId = field.EffectiveValue.Trim();
            if (!objectIds.Add(objectId))
            {
                state = null;
                failureKind = Ra2ContentTemplateCompilationFailureKind.DuplicateRegistration;
                message = "The registration list contains the same object more than once.";
                return false;
            }

            maximumIndex = Math.Max(maximumIndex, index);
        }

        int nextIndex = maximumIndex == int.MaxValue ? -1 : maximumIndex + 1;
        state = new Ra2ContentRegistrationAllocationState(indexes, objectIds, nextIndex);
        failureKind = Ra2ContentTemplateCompilationFailureKind.None;
        message = "The registration list is valid.";
        return true;
    }
}
