using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class BuiltInFieldRegistryPackLoaderTests
{
    [Fact]
    public void Load_LoadsEmbeddedV32PackWithExamples()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.True(result.Definitions.Count > 2000);
        Assert.Contains(result.Definitions, definition =>
            definition.Key == "AALimit" &&
            definition.SourceKind == Ra2FieldSourceKind.Yuri);
        Assert.Contains(result.Definitions, definition => definition.Examples.Count > 0);
    }

    [Fact]
    public void Load_DoesNotLoadOlderPacksWhenV32IsAvailable()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.NotEmpty(result.LoadedDefinitions);
        Assert.All(result.LoadedDefinitions, loaded =>
            Assert.Equal("builtin-yr-ares-phobos-fallback-v3.2.fields.json", loaded.SourceFileName));
        Assert.DoesNotContain(
            result.LoadedDefinitions,
            loaded => loaded.SourceFileName.Contains("fallback-v2", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(loaded.SourceFileName, "builtin-yr-ares-phobos-fallback-v3.fields.json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Load_ExamplesDoNotBecomeAllowedValues()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Ra2FieldDefinition definition = Assert.Single(
            result.Definitions,
            candidate => candidate.Key == "AALimit" &&
                         candidate.AppliesTo.Contains(Ra2SectionKind.Global));

        Assert.NotEmpty(definition.Examples);
        Assert.Empty(definition.ValueMetadata.AllowedValues);
    }

    [Fact]
    public void Load_V32ProjectileAAHasChineseDescriptionAndExamples()
    {
        Ra2FieldDefinition definition = FindDefinition("AA", Ra2SectionKind.Projectile);

        Assert.Equal(FieldEditorKind.Boolean, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Boolean, definition.ValueMetadata.ValueKind);
        Assert.Contains("空中", definition.Description);
        Assert.Contains(definition.Examples, example => example.Value == "yes");
        Assert.Contains(definition.Examples, example => example.Value == "no");
    }

    [Fact]
    public void Load_V32PrimaryHasReferenceMetadataAndDescription()
    {
        Ra2FieldDefinition definition = FindDefinition("Primary", Ra2SectionKind.Techno);

        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Reference, definition.ValueMetadata.ValueKind);
        Assert.Contains("Weapon", definition.Description);
        Assert.NotEmpty(definition.Examples);
    }

    [Fact]
    public void Load_V32WeaponProjectileHasReferenceMetadataAndDescription()
    {
        Ra2FieldDefinition definition = FindDefinition("Projectile", Ra2SectionKind.Weapon);

        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Reference, definition.ValueMetadata.ValueKind);
        Assert.Contains("Projectile", definition.Description);
    }

    [Fact]
    public void Load_V32WeaponWarheadHasReferenceMetadataAndDescription()
    {
        Ra2FieldDefinition definition = FindDefinition("Warhead", Ra2SectionKind.Weapon);

        Assert.Equal(FieldEditorKind.Reference, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Reference, definition.ValueMetadata.ValueKind);
        Assert.Contains("Warhead", definition.Description);
    }

    [Fact]
    public void Load_V32TechnoArmorHasAllowedValues()
    {
        Ra2FieldDefinition definition = FindDefinition("Armor", Ra2SectionKind.Techno);

        Assert.Equal(FieldEditorKind.Enum, definition.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Enum, definition.ValueMetadata.ValueKind);
        Assert.Contains(definition.ValueMetadata.AllowedValues, value => value.Value == "heavy");
        Assert.Contains(definition.ValueMetadata.AllowedValues, value => value.Value == "light");
        Assert.Contains(definition.ValueMetadata.AllowedValues, value => value.Value == "medium");
    }

    [Fact]
    public void Load_V32WarheadVersesHasDescriptionAndIsNotBoolean()
    {
        Ra2FieldDefinition definition = FindDefinition("Verses", Ra2SectionKind.Warhead);

        Assert.NotEqual(FieldEditorKind.Boolean, definition.EditorKind);
        Assert.NotEqual(Ra2FieldValueKind.Boolean, definition.ValueMetadata.ValueKind);
        Assert.Contains("伤害", definition.Description);
    }

    [Theory]
    [MemberData(nameof(BatchACanonicalDescriptionData))]
    public void Load_V32BatchACanonicalRowsUseVerifiedDescriptions(string key, Ra2SectionKind sectionKind, string expectedDescription)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedDescription, definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(BatchBVerifiedDescriptionData))]
    public void Load_V32BatchBVerifiedRowsUseSourceBackedDescriptions(string key, Ra2SectionKind sectionKind, string expectedDescription)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedDescription, definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32BatchBConcreteRowsUseVerifiedEditorKinds()
    {
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("BuildCat", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Crewed", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Crewed", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Turret", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Turret", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("ThreatPosed", Ra2SectionKind.Techno).EditorKind);
    }


    [Theory]
    [MemberData(nameof(AiLowQualityDescriptionData))]
    public void Load_V32AiLowQualityRowsUseSourceBackedDescriptions(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32AiLowQualityBatchAddsGlobalDumbThreatCoefficientRows()
    {
        Assert.Equal(FieldEditorKind.Float, FindDefinition("DumbMyEffectivenessCoefficient", Ra2SectionKind.Global).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("DumbTargetEffectivenessCoefficient", Ra2SectionKind.Global).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("DumbTargetSpecialThreatCoefficient", Ra2SectionKind.Global).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("DumbTargetStrengthCoefficient", Ra2SectionKind.Global).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("DumbTargetDistanceCoefficient", Ra2SectionKind.Global).EditorKind);

        Assert.Contains("[General]", FindDefinition("DumbMyEffectivenessCoefficient", Ra2SectionKind.AI).Description);
        Assert.Contains("[General]", FindDefinition("DumbTargetEffectivenessCoefficient", Ra2SectionKind.Techno).Description);
    }

    [Theory]
    [MemberData(nameof(AiCrossContextDescriptionData))]
    public void Load_V32AiCrossContextRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(AiPageBatchDescriptionData))]
    public void Load_V32AiPageBatchRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }


    [Theory]
    [MemberData(nameof(TechnoTypesCommonDescriptionData))]
    public void Load_V32TechnoTypesCommonRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesCommonBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("Primary", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("Secondary", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Speed", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("TechLevel", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Cost", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("Armor", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Sight", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Text, FindDefinition("Owner", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Text, FindDefinition("Prerequisite", Ra2SectionKind.Vehicle).EditorKind);
    }

    [Theory]
    [MemberData(nameof(TechnoTypesCombatMobilityDescriptionData))]
    public void Load_V32TechnoTypesCombatMobilityRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesCombatMobilityBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("GuardRange", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("ROT", Ra2SectionKind.Techno).EditorKind);
        Assert.Equal(FieldEditorKind.Text, FindDefinition("ROT", Ra2SectionKind.Weapon).EditorKind);
        Assert.Equal(FieldEditorKind.Text, FindDefinition("Locomotor", Ra2SectionKind.Warhead).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("MovementZone", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("SpeedType", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Text, FindDefinition("MovementRestrictedTo", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Reload", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Ammo", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("PipWrap", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Passengers", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Size", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("Category", Ra2SectionKind.Vehicle).EditorKind);
    }

    [Theory]
    [MemberData(nameof(TechnoTypesTargetingTransportDescriptionData))]
    public void Load_V32TechnoTypesTargetingTransportRowsUseSourceBackedDescriptions(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesTargetingTransportBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("SizeLimit", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("OpenTopped", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("DeploysInto", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("UndeploysInto", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("DeployFire", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("DeployFireWeapon", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("DeployTime", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("DeployToLand", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Naval", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Underwater", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("JumpJet", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("BalloonHover", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("HoverAttack", Ra2SectionKind.Vehicle).EditorKind);
    }


    [Theory]
    [MemberData(nameof(TechnoTypesProductionVeterancyDescriptionData))]
    public void Load_V32TechnoTypesProductionVeterancyRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesProductionVeterancyBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("AllowedToStartInMultiplayer", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("AllowedToStartInMultiplayer", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("CrateGoodie", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Trainable", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Insignificant", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("NoMovingFire", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("OpportunityFire", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("ToProtect", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("ThreatAvoidanceCoefficient", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Soylent", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Bounty", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.MultiSelect, FindDefinition("VeteranAbilities", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.MultiSelect, FindDefinition("EliteAbilities", Ra2SectionKind.Aircraft).EditorKind);
    }


    [Theory]
    [MemberData(nameof(TechnoTypesCombatBehaviorDescriptionData))]
    public void Load_V32TechnoTypesCombatBehaviorRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesCombatBehaviorBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Cloakable", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("CloakingSpeed", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("RadarInvisible", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Sensors", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("SensorsSight", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("DetectDisguise", Ra2SectionKind.Techno).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("CanDisguise", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("DisguiseWhenStill", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("PermaDisguise", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("ImmuneToPoison", Ra2SectionKind.Techno).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("ImmuneToPsionicWeapons", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("TypeImmune", Ra2SectionKind.Building).EditorKind);
    }

    [Theory]
    [MemberData(nameof(TechnoTypesWeaponTargetingDescriptionData))]
    public void Load_V32TechnoTypesWeaponTargetingRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesWeaponTargetingBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("DistributedFire", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("FireAngle", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("CanPassiveAquire", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("CanRetaliate", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("PreventAttackMove", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("LandTargeting", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("NavalTargeting", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("UseFireParticles", Ra2SectionKind.Weapon).EditorKind);
    }


    [Theory]
    [MemberData(nameof(TechnoTypesAircraftSpawnDescriptionData))]
    public void Load_V32TechnoTypesAircraftSpawnRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesAircraftSpawnBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("Spawns", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("SpawnsNumber", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("SpawnRegenRate", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("SpawnReloadRate", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("MissileSpawn", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Spawned", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Text, FindDefinition("Dock", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("AirportBound", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Fighter", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Crashable", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("PitchSpeed", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("PitchAngle", Ra2SectionKind.Vehicle).EditorKind);
    }

    [Theory]
    [MemberData(nameof(TechnoTypesJumpjetFlightTuningDescriptionData))]
    public void Load_V32TechnoTypesJumpjetFlightTuningRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesJumpjetFlightTuningBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("JumpjetTurnRate", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("JumpjetSpeed", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("JumpjetClimb", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("JumpjetCrash", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("JumpjetHeight", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("JumpjetAccel", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("JumpjetWobbles", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("JumpjetNoWobbles", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("JumpjetDeviation", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("JumpjetAccel", Ra2SectionKind.Warhead).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("SlowdownDistance", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("AccelerationFactor", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("DeaccelerationFactor", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Weight", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("PhysicalSize", Ra2SectionKind.Infantry).EditorKind);
    }

    [Theory]
    [MemberData(nameof(TechnoTypesEconomyResourceCrushDescriptionData))]
    public void Load_V32TechnoTypesEconomyResourceCrushRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesEconomyResourceCrushBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Storage", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("PipScale", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("Pip", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Points", Ra2SectionKind.Aircraft).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Bunkerable", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("IFVMode", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Crushable", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Crusher", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("OmniCrusher", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("OmniCrushResistant", Ra2SectionKind.Infantry).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("CrushSound", Ra2SectionKind.Vehicle).EditorKind);
    }

    [Theory]
    [MemberData(nameof(TechnoTypesRepairPowerCaptureFactoryRadarDescriptionData))]
    public void Load_V32TechnoTypesRepairPowerCaptureFactoryRadarRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32TechnoTypesRepairPowerCaptureFactoryRadarBatchAddsSpecificObjectContextRows()
    {
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Repairable", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("SelfHealing", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("TiberiumHeal", Ra2SectionKind.Global).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("PoweredUnit", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("PowersUnit", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("EngineerRepairable", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("MaxNumberOccupants", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("Factory", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Radar", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("SuperWeapon", Ra2SectionKind.Building).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Harvester", Ra2SectionKind.Vehicle).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("InfantryAbsorb", Ra2SectionKind.Building).EditorKind);
    }

    [Theory]
    [MemberData(nameof(WeaponCoreBigBatchDescriptionData))]
    public void Load_V32WeaponCoreBigBatchRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32WeaponCoreBigBatchAddsSpecificContextRows()
    {
        Assert.Equal(FieldEditorKind.Float, FindDefinition("Damage", Ra2SectionKind.Animation).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Damage", Ra2SectionKind.VoxelAnim).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("Warhead", Ra2SectionKind.Animation).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("Warhead", Ra2SectionKind.VoxelAnim).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("Report", Ra2SectionKind.Animation).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("Range", Ra2SectionKind.SuperWeapon).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Range", Ra2SectionKind.Sound).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Lobber", Ra2SectionKind.Weapon).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Suicide", Ra2SectionKind.Weapon).EditorKind);
        Assert.Equal(FieldEditorKind.Enum, FindDefinition("AreaFire.Target", Ra2SectionKind.Weapon).EditorKind);
        Assert.Equal(FieldEditorKind.MultiSelect, FindDefinition("CanTarget", Ra2SectionKind.Weapon).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("KeepRange", Ra2SectionKind.Weapon).EditorKind);
    }

    [Fact]
    public void Load_V32BatchAChangesDoNotPatchBroadFallbackRows()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();
        Dictionary<string, string> canonicalDescriptions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Damage"] = "设置武器造成的基础伤害点数；实际应用到目标前，该值会继续受到 Warhead 的 Verses、PercentAtMax、特殊弹头逻辑等修正。",
            ["ROF"] = "设置该武器发射后的再装填 / 射击间隔，单位为游戏帧；Burst 完成后通常再应用 ROF，且会受所属方、老兵能力、驻军或地堡倍率影响。",
            ["Range"] = "设置武器最大射程，单位为格；RA2/YR 中 Range=-2 可表示无限射程，但会带来 GuardRange、MinimumRange、飞行器攻击和部分弹体逻辑 caveat。",
            ["Projectile"] = "设置该武器使用的 Projectile section；Projectile 决定弹体移动 / 表现方式，并在命中或到达目标后调用对应 Warhead。",
            ["Warhead"] = "设置该武器命中后使用的 Warhead section；Warhead 决定伤害倍率、范围扩散、命中特效和大量特殊弹头效果。",
            ["Verses"] = "设置 Warhead 对各 Armor 类型的伤害倍率列表；列表顺序需对应当前 ArmorTypes。0% 通常表示目标不受该弹头直接伤害，也会影响强制攻击和还击等判定。",
            ["CellSpread"] = "设置 Warhead 的爆炸 / 伤害扩散半径，单位为格；没有 CellSpread 时通常只影响命中格，扩散伤害会结合 PercentAtMax 按距离衰减。",
            ["PercentAtMax"] = "设置 Warhead 在 CellSpread 最远端的伤害倍率；命中点到最大扩散距离之间的伤害会按该值线性插值衰减。",
            ["AA"] = "设置该 Projectile 是否允许武器攻击空中目标。AA=yes 允许弹体朝飞行对象移动；RA2/YR 中伞兵等特殊目标还会受 AG 与 LandTargeting 等逻辑影响。",
            ["AG"] = "设置该 Projectile 是否允许武器攻击地面或水面移动目标；RA2/YR 中 AG=no 主要限制强制攻击地面和部分索敌/光标行为，必要时还要结合 LandTargeting。"
        };
        Ra2SectionKind[] broadFallbackKinds =
        [
            Ra2SectionKind.Techno,
            Ra2SectionKind.Unit,
            Ra2SectionKind.Infantry,
            Ra2SectionKind.Vehicle,
            Ra2SectionKind.Aircraft,
            Ra2SectionKind.Building,
            Ra2SectionKind.Unknown,
            Ra2SectionKind.Global
        ];

        Assert.DoesNotContain(result.Definitions, definition =>
            canonicalDescriptions.TryGetValue(definition.Key, out string? canonicalDescription) &&
            definition.AppliesTo.Any(broadFallbackKinds.Contains) &&
            string.Equals(definition.Description, canonicalDescription, StringComparison.Ordinal));
    }

    [Fact]
    public void Load_V32HasNoUnsupportedAppliesToWarnings()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Warnings, warning => warning.Contains("unknown appliesTo", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Warnings, warning => warning.Contains("unknown schema", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Warnings, warning => warning.Contains("Text", StringComparison.OrdinalIgnoreCase) && warning.Contains("schema", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Load_V32LoadsArtFieldsAsArtObject()
    {
        Ra2FieldDefinition definition = FindDefinition("AltPalette", Ra2SectionKind.ArtObject);

        Assert.DoesNotContain(Ra2SectionKind.Unknown, definition.AppliesTo);
    }

    [Fact]
    public void Load_V32DoesNotRetainUnverifiedAutoExtractedBuildingOrVehicleRow()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, candidate =>
            candidate.Key == "AllowWeaponSelectAgainstWalls" &&
            candidate.AppliesTo.Contains(Ra2SectionKind.Building) &&
            candidate.AppliesTo.Contains(Ra2SectionKind.Vehicle));
    }

    [Fact]
    public void Load_V32IncludesIdentityGarrisonPatch()
    {
        Assert.Contains("Name:E1", FindDefinition("UIName", Ra2SectionKind.Techno).Examples.Select(example => example.Value));
        Ra2FieldDefinition countryUiName = FindDefinition("UIName", Ra2SectionKind.Country);
        Ra2FieldDefinition occupier = FindDefinition("Occupier", Ra2SectionKind.Infantry);
        Ra2FieldDefinition occupyWeapon = FindDefinition("OccupyWeapon", Ra2SectionKind.Infantry);
        Ra2FieldDefinition eliteOccupyWeapon = FindDefinition("EliteOccupyWeapon", Ra2SectionKind.Infantry);
        Ra2FieldDefinition openTransportWeapon = FindDefinition("OpenTransportWeapon", Ra2SectionKind.Infantry);

        Assert.Contains("Name:Americans", countryUiName.Examples.Select(example => example.Value));
        Assert.Equal("阵营或国家的本地化名称标签，通常指向 CSF 文本。", countryUiName.Description);
        Assert.Equal(FieldEditorKind.Boolean, occupier.EditorKind);
        Assert.Equal("是否允许该步兵作为建筑驻军单位。", occupier.Description);
        Assert.Equal(FieldEditorKind.Reference, occupyWeapon.EditorKind);
        Assert.Equal(FieldEditorKind.Reference, eliteOccupyWeapon.EditorKind);
        Assert.Equal(FieldEditorKind.Integer, openTransportWeapon.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Integer, openTransportWeapon.ValueMetadata.ValueKind);
        Assert.Contains("-1", openTransportWeapon.Examples.Select(example => example.Value));
        Assert.Contains("0", openTransportWeapon.Examples.Select(example => example.Value));
        Assert.Contains("1", openTransportWeapon.Examples.Select(example => example.Value));
        Assert.All(
            new[] { countryUiName, occupier, occupyWeapon, eliteOccupyWeapon, openTransportWeapon },
            definition => Assert.StartsWith("manual-curated-identity-garrison-patch", definition.RegistryQuality));
    }

    [Fact]
    public void Load_ReturnsCachedResultOnRepeatedCalls()
    {
        BuiltInFieldRegistryPackLoader loader = new();

        LocalFieldRegistryLoadResult first = loader.Load();
        LocalFieldRegistryLoadResult second = loader.Load();

        Assert.Same(first, second);
    }

    [Fact]
    public void ClearCache_ForcesNextLoadToCreateNewResult()
    {
        BuiltInFieldRegistryPackLoader loader = new();

        LocalFieldRegistryLoadResult first = loader.Load();
        loader.ClearCache();
        LocalFieldRegistryLoadResult second = loader.Load();

        Assert.NotSame(first, second);
        Assert.Equal(first.Definitions.Count, second.Definitions.Count);
    }

    [Theory]
    [InlineData("AIAlternateProductionCreditCutoff")]
    [InlineData("AIAutoDeployFrameDelay")]
    [InlineData("AISuperDefenseDistance")]
    [InlineData("BaseDefenseDelay")]
    [InlineData("DisabledDisguiseDetectionPercent")]
    [InlineData("DissolveUnfilledTeamDelay")]
    [InlineData("FillEarliestTeamProbability")]
    [InlineData("GameSpeedBias")]
    [InlineData("MaximumBuildingPlacementFailures")]
    [InlineData("SuspendDelay")]
    [InlineData("ThreatPerOccupant")]
    [InlineData("UseMinDefenseRule")]
    public void Load_V32SupersededGlobalRowsDoNotRetainBroadTechnoFallback(string key)
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.Contains(result.Definitions, definition =>
            definition.Key == key && definition.AppliesTo.Contains(Ra2SectionKind.Global));
        Assert.DoesNotContain(result.Definitions, definition =>
            definition.Key == key && definition.AppliesTo.Contains(Ra2SectionKind.Techno));
    }

    [Theory]
    [InlineData("CSF.Color", "文字颜色")]
    [InlineData("CSF.VariableFormat", "变量值")]
    [InlineData("Delay", "再次显示")]
    [InlineData("Duration", "显示时长")]
    [InlineData("PCX", "PCX 横幅")]
    [InlineData("SHP", "SHP 横幅")]
    [InlineData("SHP.Palette", "PAL 调色板")]
    public void Load_V32BannerRowsUseOfficialPhobosDescriptions(string key, string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, Ra2SectionKind.Banner);

        Assert.Contains(expectedText, definition.Description);
        Assert.Equal("source-verified-phobos-banner-20260720", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Theory]
    [InlineData("AnimationLength", Ra2SectionKind.Terrain, FieldEditorKind.Integer, "动画使用的帧数")]
    [InlineData("ConditionYellow.Terrain", Ra2SectionKind.Global, FieldEditorKind.Float, "生命值比例")]
    [InlineData("MinimapColor", Ra2SectionKind.Terrain, FieldEditorKind.Integer, "小地图")]
    [InlineData("Palette", Ra2SectionKind.Terrain, FieldEditorKind.Text, "调色板")]
    [InlineData("SpawnsTiberium.CellsPerAnim", Ra2SectionKind.Terrain, FieldEditorKind.Integer, "填充的格数")]
    [InlineData("SpawnsTiberium.GrowthStage", Ra2SectionKind.Terrain, FieldEditorKind.Integer, "生长阶段")]
    [InlineData("SpawnsTiberium.Range", Ra2SectionKind.Terrain, FieldEditorKind.Integer, "半径")]
    [InlineData("SpawnsTiberium.Type", Ra2SectionKind.Terrain, FieldEditorKind.Text, "类型索引")]
    [InlineData("MinimapColor", Ra2SectionKind.Tiberium, FieldEditorKind.Integer, "矿石类型")]
    public void Load_V32TerrainAndTiberiumRowsUseOfficialPhobosDescriptions(
        string key,
        Ra2SectionKind sectionKind,
        FieldEditorKind editorKind,
        string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(editorKind, definition.EditorKind);
        Assert.Contains(expectedText, definition.Description);
        Assert.Equal("source-verified-phobos-terrain-tiberium-20260720", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Theory]
    [InlineData("File.Flag", "旗帜 PCX")]
    [InlineData("File.LoadScreen", "载入画面 SHP")]
    [InlineData("File.LoadScreenPAL", "PAL 调色板")]
    [InlineData("File.Taunt", "01 至 08")]
    [InlineData("ListIndex", "下拉列表")]
    [InlineData("LoadScreenText.Brief", "说明 CSF 标签")]
    [InlineData("LoadScreenText.Color", "[Colors]")]
    [InlineData("LoadScreenText.Name", "国家名称")]
    [InlineData("LoadScreenText.SpecialName", "特色武器名称")]
    [InlineData("MenuText.Status", "状态栏")]
    [InlineData("RandomSelectionWeight", "相对权重")]
    public void Load_V32CountryUiRowsUseOfficialAresDescriptions(string key, string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, Ra2SectionKind.Country);

        Assert.Contains(expectedText, definition.Description);
        Assert.Equal("source-verified-ares-country-ui-20260720", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Theory]
    [InlineData("PowerDelta.ConditionRed", "切换为红色")]
    [InlineData("PowerDelta.ConditionYellow", "切换为黄色")]
    [InlineData("ToolTipBlur", "资源开销")]
    public void Load_V32PhobosUiSettingsUseGlobalContext(string key, string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, Ra2SectionKind.Global);

        Assert.Contains(expectedText, definition.Description);
        Assert.Equal("source-verified-phobos-ui-settings-20260720", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
        Assert.DoesNotContain(Ra2SectionKind.Side, definition.AppliesTo);
    }

    [Theory]
    [InlineData("Allied")]
    [InlineData("Russian")]
    [InlineData("Yuri")]
    public void Load_V32EvaVoiceTypeKeysUseOfficialAresDescriptions(string key)
    {
        Ra2FieldDefinition definition = FindDefinition(key, Ra2SectionKind.Eva);

        Assert.Contains("音频文件名", definition.Description);
        Assert.Equal("source-verified-ares-eva-types-20260720", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Theory]
    [InlineData("Priority")]
    [InlineData("Text")]
    [InlineData("Type")]
    public void Load_V32DoesNotRetainUnverifiedFixedEvaKeys(string key)
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            definition.Key == key && definition.AppliesTo.Contains(Ra2SectionKind.Eva));
    }

    [Theory]
    [InlineData("Image.ConditionRed", Ra2SectionKind.Aircraft, "ConditionRed")]
    [InlineData("Image.ConditionYellow", Ra2SectionKind.Aircraft, "ConditionYellow")]
    [InlineData("Prerequisite.Lists", Ra2SectionKind.Techno, "额外")]
    [InlineData("Prerequisite.Negative", Ra2SectionKind.Techno, "阻止")]
    [InlineData("Prerequisite.RequiredTheaters", Ra2SectionKind.Techno, "剧场")]
    [InlineData("Prerequisite.StolenTechs", Ra2SectionKind.Techno, "被窃技术")]
    [InlineData("EVA.Sold", Ra2SectionKind.Building, "EVA 消息")]
    [InlineData("SellSound", Ra2SectionKind.Vehicle, "报告音效")]
    public void Load_V32SmallOfficialFamiliesUseVerifiedDescriptions(
        string key,
        Ra2SectionKind sectionKind,
        string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Contains(expectedText, definition.Description);
        Assert.NotNull(definition.RegistryQuality);
        Assert.StartsWith("source-verified-", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Theory]
    [InlineData("EngineerRepairAmount", Ra2SectionKind.Infantry, "工程师")]
    [InlineData("EngineerRepairAmount", Ra2SectionKind.Building, "受损建筑")]
    [InlineData("InfantryAutoDeploy", Ra2SectionKind.Infantry, "自动部署")]
    [InlineData("PowersUp.Buildings", Ra2SectionKind.Building, "多个 BuildingTypes")]
    [InlineData("PowersUp.Owner", Ra2SectionKind.Building, "目标建筑所有者")]
    [InlineData("ProneSpeed", Ra2SectionKind.Infantry, "匍匐移动")]
    [InlineData("ProneSpeed.Crawls", Ra2SectionKind.Global, "Crawls=yes")]
    [InlineData("ProneSpeed.NoCrawls", Ra2SectionKind.Global, "Crawls=no")]
    [InlineData("Slaved.OwnerWhenMasterKilled", Ra2SectionKind.Infantry, "suicide")]
    public void Load_V32InfantryFamiliesUseOfficialPhobosContexts(
        string key,
        Ra2SectionKind sectionKind,
        string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Contains(expectedText, definition.Description);
        Assert.NotNull(definition.RegistryQuality);
        Assert.StartsWith("source-verified-phobos-", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Fact]
    public void Load_V32DefaultDisguiseUsesSideWithoutInfantryFallback()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.Contains(result.Definitions, definition =>
            definition.Key == "DefaultDisguise" && definition.AppliesTo.Contains(Ra2SectionKind.Side));
        Assert.DoesNotContain(result.Definitions, definition =>
            definition.Key == "DefaultDisguise" && definition.AppliesTo.Contains(Ra2SectionKind.Infantry));
    }

    [Theory]
    [InlineData("Trailer.SpawnDelay", Ra2SectionKind.VoxelAnim, FieldEditorKind.Integer, "游戏帧")]
    [InlineData("Gas.MaxDriftSpeed", Ra2SectionKind.Particle, FieldEditorKind.Integer, "ParticleType")]
    [InlineData("Bolt.Arcs", Ra2SectionKind.LaserTrail, FieldEditorKind.Integer, "默认值为 8")]
    public void Load_V32SmallExtensionRowsUseOfficialPhobosContexts(
        string key,
        Ra2SectionKind sectionKind,
        FieldEditorKind editorKind,
        string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(editorKind, definition.EditorKind);
        Assert.Contains(expectedText, definition.Description);
        Assert.Equal("source-verified-phobos-small-extensions-20260720", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Theory]
    [InlineData("Attack", FieldEditorKind.Integer, "attack 阶段")]
    [InlineData("Control", FieldEditorKind.Text, "PREDELAY")]
    [InlineData("FShift", FieldEditorKind.Text, "频率偏移")]
    [InlineData("MinVolume", FieldEditorKind.Integer, "最低音量")]
    [InlineData("Priority", FieldEditorKind.Text, "优先级")]
    [InlineData("Type", FieldEditorKind.Text, "GLOBAL")]
    [InlineData("Volume", FieldEditorKind.Integer, "总体音量")]
    public void Load_V32SoundRowsUseDirectModEncDescriptions(
        string key,
        FieldEditorKind editorKind,
        string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, Ra2SectionKind.Sound);

        Assert.Equal(editorKind, definition.EditorKind);
        Assert.Contains(expectedText, definition.Description);
        Assert.Equal("source-verified-modenc-sound-20260720", definition.RegistryQuality);
        Assert.DoesNotContain("推断型字段", definition.Description);
    }

    [Theory]
    [InlineData("tempValue", Ra2SectionKind.AI)]
    [InlineData("Threat", Ra2SectionKind.AI)]
    [InlineData("Limit", Ra2SectionKind.Sound)]
    public void Load_V32DoesNotRetainSourceInsufficientSmallContextRows(string key, Ra2SectionKind sectionKind)
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            definition.Key == key && definition.AppliesTo.Contains(sectionKind));
    }

    [Theory]
    [InlineData(Ra2SectionKind.ArtObject)]
    [InlineData(Ra2SectionKind.Building)]
    [InlineData(Ra2SectionKind.Warhead)]
    [InlineData(Ra2SectionKind.Weapon)]
    [InlineData(Ra2SectionKind.Global)]
    [InlineData(Ra2SectionKind.Vehicle)]
    [InlineData(Ra2SectionKind.Techno)]
    public void Load_V32ReviewedContextsDoNotRetainUniformInferredTemplates(Ra2SectionKind sectionKind)
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();
        Ra2FieldDefinition[] contextDefinitions = result.Definitions
            .Where(definition => definition.AppliesTo.Contains(sectionKind))
            .ToArray();

        Assert.NotEmpty(contextDefinitions);
        Assert.DoesNotContain(contextDefinitions, definition =>
            definition.Description?.StartsWith("推断型字段：", StringComparison.Ordinal) == true);
    }

    [Theory]
    [InlineData("AARate", "Culling")]
    [InlineData("CustomGS", "GUITabSound")]
    [InlineData("Gunner", "NoRearm.UnderEMP")]
    [InlineData("NoReload.Temporal", "SmallFire")]
    [InlineData("SmallVisceroid", "ZVelocityRange")]
    public void Load_V32ReviewedTechnoKeyRangesDoNotRetainUniformInferredTemplates(string firstKey, string lastKey)
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            definition.AppliesTo.Contains(Ra2SectionKind.Techno) &&
            string.Compare(definition.Key, firstKey, StringComparison.OrdinalIgnoreCase) >= 0 &&
            string.Compare(definition.Key, lastKey, StringComparison.OrdinalIgnoreCase) <= 0 &&
            definition.Description?.StartsWith("推断型字段：", StringComparison.Ordinal) == true);
    }

    [Theory]
    [InlineData(Ra2FieldSourceKind.Phobos, "AbsorbOverDamage", "IsHideable")]
    [InlineData(Ra2FieldSourceKind.Phobos, "IsHouseColor", "ZShapePointMove.OnBuildup")]
    [InlineData(Ra2FieldSourceKind.Yuri, "ActivateSound", "DeployedFire")]
    [InlineData(Ra2FieldSourceKind.Yuri, "DeployedIdle", "PowersUpBuilding")]
    [InlineData(Ra2FieldSourceKind.Yuri, "PowerUp1Anim", "ZShapePointMove")]
    public void Load_V32ReviewedAutoExtractedKeyRangesDoNotRetainUnverifiedRows(
        Ra2FieldSourceKind sourceKind,
        string firstKey,
        string lastKey)
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            definition.SourceKind == sourceKind &&
            string.Compare(definition.Key, firstKey, StringComparison.OrdinalIgnoreCase) >= 0 &&
            string.Compare(definition.Key, lastKey, StringComparison.OrdinalIgnoreCase) <= 0 &&
            string.Equals(definition.RegistryQuality, "auto-extracted", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Load_V32DoesNotRetainAnyAutoExtractedRows()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            string.Equals(definition.RegistryQuality, "auto-extracted", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Load_V32NormalizesCommunityReviewedRowsToManualCuratedQuality()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            definition.RegistryQuality?.StartsWith("community-reviewed", StringComparison.OrdinalIgnoreCase) == true);
        Assert.Contains(result.Definitions, definition =>
            definition.Key == "AIDifficulty" &&
            definition.RegistryQuality?.StartsWith("manual-curated-community-reviewed", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void Load_V32RetainsOnlySpecificDescriptionsForReviewedInferredQualityFamilies()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();
        Ra2FieldDefinition[] reviewedInferredRows = result.Definitions
            .Where(definition =>
                definition.RegistryQuality?.StartsWith("source-assisted", StringComparison.OrdinalIgnoreCase) == true ||
                definition.RegistryQuality?.StartsWith("name-inferred", StringComparison.OrdinalIgnoreCase) == true)
            .ToArray();

        Assert.Equal(66, reviewedInferredRows.Length);
        Assert.All(reviewedInferredRows, definition =>
        {
            Assert.False(string.IsNullOrWhiteSpace(definition.Description));
            Assert.False(definition.Description.StartsWith("推断型字段：", StringComparison.Ordinal));
        });
    }

    [Fact]
    public void Load_V32HasNoEmptyOrUnrecognizedRegistryQuality()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.All(result.Definitions, definition =>
        {
            Assert.False(string.IsNullOrWhiteSpace(definition.RegistryQuality));
            Assert.NotEqual(Ra2FieldTrustLevel.Unknown, Ra2FieldTrustClassifier.Classify(definition).Level);
        });
    }

    private static Ra2FieldDefinition FindDefinition(string key, Ra2SectionKind sectionKind)
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();
        return Assert.Single(result.Definitions, definition =>
            definition.Key == key &&
            definition.AppliesTo.Contains(sectionKind));
    }

    public static TheoryData<string, Ra2SectionKind, string> BatchACanonicalDescriptionData => new()
    {
        { "Damage", Ra2SectionKind.Weapon, "设置武器造成的基础伤害点数；实际应用到目标前，该值会继续受到 Warhead 的 Verses、PercentAtMax、特殊弹头逻辑等修正。" },
        { "ROF", Ra2SectionKind.Weapon, "设置该武器发射后的再装填 / 射击间隔，单位为游戏帧；Burst 完成后通常再应用 ROF，且会受所属方、老兵能力、驻军或地堡倍率影响。" },
        { "Range", Ra2SectionKind.Weapon, "设置武器最大射程，单位为格；RA2/YR 中 Range=-2 可表示无限射程，但会带来 GuardRange、MinimumRange、飞行器攻击和部分弹体逻辑 caveat。" },
        { "Projectile", Ra2SectionKind.Weapon, "设置该武器使用的 Projectile section；Projectile 决定弹体移动 / 表现方式，并在命中或到达目标后调用对应 Warhead。" },
        { "Warhead", Ra2SectionKind.Weapon, "设置该武器命中后使用的 Warhead section；Warhead 决定伤害倍率、范围扩散、命中特效和大量特殊弹头效果。" },
        { "Verses", Ra2SectionKind.Warhead, "设置 Warhead 对各 Armor 类型的伤害倍率列表；列表顺序需对应当前 ArmorTypes。0% 通常表示目标不受该弹头直接伤害，也会影响强制攻击和还击等判定。" },
        { "CellSpread", Ra2SectionKind.Warhead, "设置 Warhead 的爆炸 / 伤害扩散半径，单位为格；没有 CellSpread 时通常只影响命中格，扩散伤害会结合 PercentAtMax 按距离衰减。" },
        { "PercentAtMax", Ra2SectionKind.Warhead, "设置 Warhead 在 CellSpread 最远端的伤害倍率；命中点到最大扩散距离之间的伤害会按该值线性插值衰减。" },
        { "AA", Ra2SectionKind.Projectile, "设置该 Projectile 是否允许武器攻击空中目标。AA=yes 允许弹体朝飞行对象移动；RA2/YR 中伞兵等特殊目标还会受 AG 与 LandTargeting 等逻辑影响。" },
        { "AG", Ra2SectionKind.Projectile, "设置该 Projectile 是否允许武器攻击地面或水面移动目标；RA2/YR 中 AG=no 主要限制强制攻击地面和部分索敌/光标行为，必要时还要结合 LandTargeting。" }
    };

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> AiLowQualityDescriptionData => new()
    {
        { "AALimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "旧式 AI 基地构成上限参数" },
        { "AARatio", Ra2SectionKind.AI, FieldEditorKind.Float, "旧式 AI 基地构成比例参数" },
        { "BarracksLimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "兵营类建筑数量" },
        { "BarracksRatio", Ra2SectionKind.AI, FieldEditorKind.Float, "兵营类建筑期望占比" },
        { "DefenseLimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "基础地面防御建筑数量" },
        { "DefenseRatio", Ra2SectionKind.AI, FieldEditorKind.Float, "基础地面防御建筑期望占比" },
        { "HelipadLimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "直升机场类建筑数量" },
        { "HelipadRatio", Ra2SectionKind.AI, FieldEditorKind.Float, "直升机场类建筑期望占比" },
        { "RefineryLimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "矿石精炼厂数量" },
        { "RefineryRatio", Ra2SectionKind.AI, FieldEditorKind.Float, "矿石精炼厂期望占比" },
        { "TeslaLimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "特斯拉线圈类防御建筑数量" },
        { "TeslaRatio", Ra2SectionKind.AI, FieldEditorKind.Float, "特斯拉线圈类防御建筑期望占比" },
        { "WarLimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "战车工厂数量" },
        { "WarRatio", Ra2SectionKind.AI, FieldEditorKind.Float, "战车工厂期望占比" },
        { "DumbMyEffectivenessCoefficient", Ra2SectionKind.Global, FieldEditorKind.Float, "ThreatPosed 低于自身" },
        { "DumbTargetEffectivenessCoefficient", Ra2SectionKind.Global, FieldEditorKind.Float, "目标有效性权重" },
        { "DumbTargetSpecialThreatCoefficient", Ra2SectionKind.Global, FieldEditorKind.Float, "SpecialThreatValue=1" },
        { "DumbTargetStrengthCoefficient", Ra2SectionKind.Global, FieldEditorKind.Float, "目标强度高于自身" },
        { "DumbTargetDistanceCoefficient", Ra2SectionKind.Global, FieldEditorKind.Float, "超出自身武器射程" }
    };

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> AiCrossContextDescriptionData => new()
    {
        { "Owner", Ra2SectionKind.AI, FieldEditorKind.Text, "AI 行来自旧资料抽取" },
        { "Prerequisite", Ra2SectionKind.AI, FieldEditorKind.Text, "RA2/YR AI 建筑队列通常不按普通 Prerequisite 判断" },
        { "Sight", Ra2SectionKind.AI, FieldEditorKind.Text, "AI 的目标选择通常不基于目标是否处于可见视野内" },
        { "AirstripRatio", Ra2SectionKind.AI, FieldEditorKind.Float, "Airstrip/AFLD 类空军设施的期望占比" },
        { "AirstripLimit", Ra2SectionKind.AI, FieldEditorKind.Integer, "Airstrip/AFLD 类空军设施的数量上限" },
        { "AirstripRatio", Ra2SectionKind.Global, FieldEditorKind.Text, "不是 [General] / Global 字段" },
        { "AirstripLimit", Ra2SectionKind.Global, FieldEditorKind.Text, "不是 [General] / Global 字段" },
        { "AirstripRatio", Ra2SectionKind.Techno, FieldEditorKind.Text, "不是 TechnoType 字段" },
        { "AirstripLimit", Ra2SectionKind.Techno, FieldEditorKind.Text, "不是 TechnoType 字段" }
    };

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> AiPageBatchDescriptionData => new()
    {
        { "BuildDefense", Ra2SectionKind.AI, FieldEditorKind.Text, "旧式标准防御建筑列表" },
        { "BuildAA", Ra2SectionKind.AI, FieldEditorKind.Text, "旧式防空防御建筑列表" },
        { "AlliedBaseDefenses", Ra2SectionKind.AI, FieldEditorKind.Text, "盟军侧别可建造的基地防御建筑" },
        { "AIForcePredictionFudge", Ra2SectionKind.AI, FieldEditorKind.Text, "根据玩家单位构成选择防御类型" },
        { "AttackInterval", Ra2SectionKind.AI, FieldEditorKind.Float, "平均发动攻击的间隔" },
        { "BlockagePathDelay", Ra2SectionKind.AI, FieldEditorKind.Integer, "单位为帧" },
        { "CompEasyBonus", Ra2SectionKind.AI, FieldEditorKind.Boolean, "简单电脑玩家" },
        { "Paranoid", Ra2SectionKind.AI, FieldEditorKind.Boolean, "协同针对人类玩家" },
        { "NodRegularPower", Ra2SectionKind.AI, FieldEditorKind.Text, "苏军侧别补建的常规电力建筑" },
        { "GDIWallDefenseCoefficient", Ra2SectionKind.AI, FieldEditorKind.Float, "墙体防御权重系数" },
        { "BuildDefense", Ra2SectionKind.Techno, FieldEditorKind.Text, "不是 TechnoType 字段" },
        { "AttackInterval", Ra2SectionKind.Global, FieldEditorKind.Text, "不是 [General] / Global 字段" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesCommonDescriptionData => new()
    {
        { "Primary", Ra2SectionKind.Techno, FieldEditorKind.Reference, "主武器" },
        { "Primary", Ra2SectionKind.Building, FieldEditorKind.Reference, "Weapon section" },
        { "Secondary", Ra2SectionKind.Techno, FieldEditorKind.Reference, "副武器" },
        { "Secondary", Ra2SectionKind.Vehicle, FieldEditorKind.Reference, "EliteSecondary" },
        { "Strength", Ra2SectionKind.Techno, FieldEditorKind.Integer, "生命值 / 耐久度" },
        { "Strength", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "Projectile / ObjectType" },
        { "Speed", Ra2SectionKind.Techno, FieldEditorKind.Integer, "移动速度" },
        { "Speed", Ra2SectionKind.Global, FieldEditorKind.Text, "不应作为 [General] 通用字段" },
        { "Speed", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "弹体的最高速度" },
        { "TechLevel", Ra2SectionKind.Techno, FieldEditorKind.Integer, "科技等级" },
        { "TechLevel", Ra2SectionKind.AI, FieldEditorKind.Text, "不是 [AI] 段字段" },
        { "Cost", Ra2SectionKind.Techno, FieldEditorKind.Integer, "基础建造价格" },
        { "Cost", Ra2SectionKind.Global, FieldEditorKind.Text, "不应作为 [General] 通用字段" },
        { "Armor", Ra2SectionKind.Techno, FieldEditorKind.Enum, "Warhead 的 Verses" },
        { "Armor", Ra2SectionKind.Global, FieldEditorKind.Text, "Difficulty、House、[Powerups] 与 TechnoTypes" },
        { "Armor", Ra2SectionKind.Projectile, FieldEditorKind.Text, "没有将该字段列为 Projectile canonical 字段" },
        { "Sight", Ra2SectionKind.Techno, FieldEditorKind.Integer, "黑幕" },
        { "Owner", Ra2SectionKind.Techno, FieldEditorKind.Text, "国家 / 阵营列表" },
        { "Owner", Ra2SectionKind.AI, FieldEditorKind.Text, "不应作为 [AI] 段有效字段" },
        { "Prerequisite", Ra2SectionKind.Techno, FieldEditorKind.Text, "建筑或特殊前置条件" },
        { "Prerequisite", Ra2SectionKind.AI, FieldEditorKind.Text, "不应作为 [AI] 段有效字段" }
    };

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesCombatMobilityDescriptionData => new()
    {
        { "GuardRange", Ra2SectionKind.Techno, FieldEditorKind.Integer, "警戒索敌" },
        { "GuardRange", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "主 / 副武器 Range" },
        { "ROT", Ra2SectionKind.Techno, FieldEditorKind.Integer, "转向速率" },
        { "ROT", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "转向 / 追踪能力" },
        { "ROT", Ra2SectionKind.Weapon, FieldEditorKind.Text, "不是 Weapon 字段" },
        { "Locomotor", Ra2SectionKind.Techno, FieldEditorKind.Text, "Locomotor CLSID" },
        { "Locomotor", Ra2SectionKind.Warhead, FieldEditorKind.Text, "IsLocomotor=yes" },
        { "Locomotor", Ra2SectionKind.Weapon, FieldEditorKind.Text, "不应作为 Weapon canonical 字段" },
        { "MovementZone", Ra2SectionKind.Techno, FieldEditorKind.Enum, "AI pathfinding" },
        { "MovementZone", Ra2SectionKind.Vehicle, FieldEditorKind.Enum, "SpeedType" },
        { "SpeedType", Ra2SectionKind.Techno, FieldEditorKind.Enum, "LandTypes" },
        { "SpeedType", Ra2SectionKind.Aircraft, FieldEditorKind.Enum, "通行 / 速度效果" },
        { "MovementRestrictedTo", Ra2SectionKind.Techno, FieldEditorKind.Text, "仅确认适用于 VehicleTypes" },
        { "MovementRestrictedTo", Ra2SectionKind.Vehicle, FieldEditorKind.Text, "指定 LandType" },
        { "Reload", Ra2SectionKind.Techno, FieldEditorKind.Integer, "单位为游戏帧" },
        { "Reload", Ra2SectionKind.Building, FieldEditorKind.Integer, "EmptyReload" },
        { "Ammo", Ra2SectionKind.Techno, FieldEditorKind.Integer, "-1 表示无限弹药" },
        { "Ammo", Ra2SectionKind.Aircraft, FieldEditorKind.Integer, "手动补给逻辑" },
        { "Ammo", Ra2SectionKind.Building, FieldEditorKind.Integer, "科技建筑" },
        { "PipWrap", Ra2SectionKind.Techno, FieldEditorKind.Integer, "PipScale=Ammo" },
        { "PipWrap", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "分层 pip 图形" },
        { "Passengers", Ra2SectionKind.Techno, FieldEditorKind.Integer, "不应直接应用到 Infantry" },
        { "Passengers", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "Category=Transport" },
        { "Passengers", Ra2SectionKind.Building, FieldEditorKind.Integer, "InfantryAbsorb=yes" },
        { "Size", Ra2SectionKind.Techno, FieldEditorKind.Integer, "不应直接应用到 Building" },
        { "Size", Ra2SectionKind.Infantry, FieldEditorKind.Integer, "SizeLimit" },
        { "Category", Ra2SectionKind.Techno, FieldEditorKind.Enum, "战术分类" },
        { "Category", Ra2SectionKind.Vehicle, FieldEditorKind.Enum, "AFV" },
        { "Category", Ra2SectionKind.Aircraft, FieldEditorKind.Enum, "AirPower" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesTargetingTransportDescriptionData => new()
    {
        { "SizeLimit", Ra2SectionKind.Techno, FieldEditorKind.Integer, "最大 Size 值" },
        { "SizeLimit", Ra2SectionKind.Aircraft, FieldEditorKind.Integer, "乘客自身 Size" },
        { "OpenTopped", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "乘客向外开火" },
        { "OpenTopped", Ra2SectionKind.Building, FieldEditorKind.Boolean, "建筑上使用" },
        { "DeploysInto", Ra2SectionKind.Techno, FieldEditorKind.Reference, "部署后转换成" },
        { "DeploysInto", Ra2SectionKind.Vehicle, FieldEditorKind.Reference, "BuildingType" },
        { "UndeploysInto", Ra2SectionKind.Techno, FieldEditorKind.Reference, "反部署后生成" },
        { "UndeploysInto", Ra2SectionKind.Building, FieldEditorKind.Reference, "VehicleType" },
        { "DeployFire", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "部署命令" },
        { "DeployFire", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "部署帧" },
        { "DeployFireWeapon", Ra2SectionKind.Techno, FieldEditorKind.Integer, "武器槽" },
        { "DeployFireWeapon", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "Gattling" },
        { "DeployTime", Ra2SectionKind.Techno, FieldEditorKind.Float, "单位为分钟" },
        { "DeployTime", Ra2SectionKind.Building, FieldEditorKind.Float, "工厂门动画" },
        { "DeployToLand", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "显式部署命令" },
        { "DeployToLand", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "Hover 或 Fly locomotor" },
        { "Naval", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "海军属性" },
        { "Naval", Ra2SectionKind.Building, FieldEditorKind.Boolean, "海军船坞" },
        { "Naval", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "海军单位" },
        { "Underwater", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "水下对象" },
        { "Underwater", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "沉没死亡逻辑" },
        { "JumpJet", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Jumpjet controls" },
        { "JumpJet", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "Locomotor" },
        { "BalloonHover", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "默认不降落" },
        { "BalloonHover", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "Vertical=yes" },
        { "HoverAttack", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "悬停攻击" },
        { "HoverAttack", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "随机移动" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesProductionVeterancyDescriptionData => new()
    {
        { "AllowedToStartInMultiplayer", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "开局部队候选" },
        { "AllowedToStartInMultiplayer", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "不建议让海军载具" },
        { "CrateGoodie", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "VehicleTypes 字段" },
        { "CrateGoodie", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "Unit crate" },
        { "Trainable", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "经验系统升级" },
        { "Trainable", Ra2SectionKind.Building, FieldEditorKind.Boolean, "建筑默认通常不可升级" },
        { "Insignificant", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "不计入得分" },
        { "Insignificant", Ra2SectionKind.Building, FieldEditorKind.Boolean, "超级武器" },
        { "NoMovingFire", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "禁止移动中开火" },
        { "OpportunityFire", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "VehicleTypes、InfantryTypes 与 AircraftTypes" },
        { "OpportunityFire", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "移动、采矿" },
        { "ToProtect", Ra2SectionKind.AI, FieldEditorKind.Text, "不应作为 AI 段有效字段" },
        { "ToProtect", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "守家 / 防御队伍" },
        { "ThreatAvoidanceCoefficient", Ra2SectionKind.Techno, FieldEditorKind.Float, "低威胁路径" },
        { "ThreatAvoidanceCoefficient", Ra2SectionKind.Vehicle, FieldEditorKind.Float, "矿车 / 采矿单位" },
        { "Soylent", Ra2SectionKind.Techno, FieldEditorKind.Integer, "资金返还值" },
        { "Soylent", Ra2SectionKind.Building, FieldEditorKind.Integer, "侧边栏出售" },
        { "Bounty", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Ares 扩展字段" },
        { "Bounty", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "击杀敌方单位或建筑" },
        { "VeteranAbilities", Ra2SectionKind.Techno, FieldEditorKind.MultiSelect, "老兵等级" },
        { "VeteranAbilities", Ra2SectionKind.Vehicle, FieldEditorKind.MultiSelect, "部分能力在 RA2/YR 中无效" },
        { "EliteAbilities", Ra2SectionKind.Techno, FieldEditorKind.MultiSelect, "精英等级" },
        { "EliteAbilities", Ra2SectionKind.Aircraft, FieldEditorKind.MultiSelect, "不会与 VeteranAbilities" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesCombatBehaviorDescriptionData => new()
    {
        { "Cloakable", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "InfantryTypes、VehicleTypes 与 BuildingTypes" },
        { "Cloakable", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "隐形装置" },
        { "CloakingSpeed", Ra2SectionKind.Techno, FieldEditorKind.Integer, "1 最快，10 最慢" },
        { "CloakingSpeed", Ra2SectionKind.Aircraft, FieldEditorKind.Integer, "隐形 / 显形阶段" },
        { "RadarInvisible", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "雷达 / 小地图" },
        { "RadarInvisible", Ra2SectionKind.Building, FieldEditorKind.Boolean, "非本方对象" },
        { "Sensors", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Sensors 侦测能力" },
        { "Sensors", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "空中单位通常不能" },
        { "SensorsSight", Ra2SectionKind.Techno, FieldEditorKind.Integer, "侦测半径" },
        { "SensorsSight", Ra2SectionKind.Building, FieldEditorKind.Integer, "SensorArray=yes" },
        { "DetectDisguise", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "CanDisguise=yes" },
        { "CanDisguise", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "伪装成其他单位" },
        { "CanDisguise", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "MakesDisguise" },
        { "DisguiseWhenStill", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "VehicleTypes 字段" },
        { "DisguiseWhenStill", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "DefaultMirageDisguises" },
        { "PermaDisguise", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "移动时是否保持伪装" },
        { "PermaDisguise", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "单位调色板" },
        { "ImmuneToVeins", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Veinhole=yes" },
        { "ImmuneToRadiation", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "Radiation=yes" },
        { "ImmuneToPsionics", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "PsychicDominator" },
        { "ImmuneToPsionicWeapons", Ra2SectionKind.Building, FieldEditorKind.Boolean, "PsychicDamage=yes" },
        { "ImmuneToPoison", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "Poison=yes" },
        { "TypeImmune", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Aircraft 上下文不应直接套用" },
        { "TypeImmune", Ra2SectionKind.Building, FieldEditorKind.Boolean, "同一身份且同一所属方" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesWeaponTargetingDescriptionData => new()
    {
        { "OmniFire", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "无需先转向目标" },
        { "OmniFire", Ra2SectionKind.Techno, FieldEditorKind.Text, "Weapon 字段" },
        { "DistributedFire", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "重新选择附近敌人" },
        { "DistributedFire", Ra2SectionKind.Building, FieldEditorKind.Boolean, "只攻击一次" },
        { "FireAngle", Ra2SectionKind.Techno, FieldEditorKind.Integer, "VehicleTypes 与 BuildingTypes" },
        { "FireAngle", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "垂直仰角" },
        { "CanPassiveAquire", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "自动获得射程内目标" },
        { "CanPassiveAquire", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "Aquire" },
        { "CanRetaliate", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "受到敌方攻击" },
        { "CanRetaliate", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "目标合法性" },
        { "PreventAttackMove", Ra2SectionKind.Global, FieldEditorKind.Text, "不是 [General] / Global" },
        { "PreventAttackMove", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "工程师" },
        { "NoAutoFire", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "威胁扫描" },
        { "Passive", Ra2SectionKind.Techno, FieldEditorKind.Text, "来源不足" },
        { "LandTargeting", Ra2SectionKind.Techno, FieldEditorKind.Integer, "陆地目标" },
        { "LandTargeting", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "副武器攻击陆地目标" },
        { "NavalTargeting", Ra2SectionKind.Techno, FieldEditorKind.Integer, "海军 / 水下目标" },
        { "NavalTargeting", Ra2SectionKind.Building, FieldEditorKind.Integer, "0-7" },
        { "FireOnce", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "只发射一次" },
        { "FireOnce", Ra2SectionKind.Techno, FieldEditorKind.Text, "Weapon 字段" },
        { "Burst", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "连续发射的弹数" },
        { "Burst", Ra2SectionKind.Techno, FieldEditorKind.Text, "Weapon 字段" },
        { "DecloakToFire", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "隐形对象" },
        { "DecloakToFire", Ra2SectionKind.Techno, FieldEditorKind.Text, "Weapon 字段" },
        { "UseFireParticles", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "AttachedParticleSystem" },
        { "UseFireParticles", Ra2SectionKind.Global, FieldEditorKind.Text, "不是 [General] / Global" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesAircraftSpawnDescriptionData => new()
    {
        { "Spawns", Ra2SectionKind.Techno, FieldEditorKind.Reference, "作为 spawner" },
        { "Spawns", Ra2SectionKind.Aircraft, FieldEditorKind.Reference, "AircraftType" },
        { "Spawns", Ra2SectionKind.ArtObject, FieldEditorKind.Reference, "shape debris" },
        { "Spawns", Ra2SectionKind.Global, FieldEditorKind.Text, "ParticleSystems" },
        { "SpawnsNumber", Ra2SectionKind.Techno, FieldEditorKind.Integer, "生成物数量" },
        { "SpawnsNumber", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "SpawnRegenRate" },
        { "SpawnRegenRate", Ra2SectionKind.Techno, FieldEditorKind.Integer, "再生计时" },
        { "SpawnRegenRate", Ra2SectionKind.Aircraft, FieldEditorKind.Integer, "导弹再装填时间" },
        { "SpawnReloadRate", Ra2SectionKind.Techno, FieldEditorKind.Integer, "返回后的再装填时间" },
        { "SpawnReloadRate", Ra2SectionKind.Building, FieldEditorKind.Integer, "MissileSpawn=yes" },
        { "MissileSpawn", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "spawned missile" },
        { "MissileSpawn", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "击杀可计入发射者经验" },
        { "Spawned", Ra2SectionKind.AI, FieldEditorKind.Text, "不应作为 [AI] 段有效字段" },
        { "Spawned", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Building 上下文不应直接套用" },
        { "Spawned", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "EVA_UnitLost" },
        { "Dock", Ra2SectionKind.Techno, FieldEditorKind.Text, "AircraftTypes 与 VehicleTypes" },
        { "Dock", Ra2SectionKind.Aircraft, FieldEditorKind.Text, "NumberOfDocks" },
        { "Dock", Ra2SectionKind.Vehicle, FieldEditorKind.Text, "UnitReload" },
        { "AirportBound", Ra2SectionKind.Techno, FieldEditorKind.Text, "AircraftTypes" },
        { "AirportBound", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "必须返回 Dock" },
        { "Landable", Ra2SectionKind.Techno, FieldEditorKind.Text, "AircraftTypes" },
        { "Landable", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "Selectable" },
        { "MoveToShroud", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "NoMove" },
        { "MoveToShroud", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "默认 no" },
        { "Fighter", Ra2SectionKind.Techno, FieldEditorKind.Text, "AircraftTypes" },
        { "Fighter", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "战斗机逻辑" },
        { "FlyBy", Ra2SectionKind.Techno, FieldEditorKind.Text, "AircraftTypes" },
        { "FlyBy", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "飞越目标位置" },
        { "FlyBack", Ra2SectionKind.Techno, FieldEditorKind.Text, "AircraftTypes" },
        { "FlyBack", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "锁定自身飞行路径" },
        { "Crashable", Ra2SectionKind.Techno, FieldEditorKind.Text, "jumpjet" },
        { "Crashable", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "坠落 / crash" },
        { "Crashable", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "空中爆炸" },
        { "PitchSpeed", Ra2SectionKind.Techno, FieldEditorKind.Float, "jumpjet VehicleTypes" },
        { "PitchSpeed", Ra2SectionKind.Aircraft, FieldEditorKind.Float, "Speed * PitchSpeed" },
        { "PitchAngle", Ra2SectionKind.Techno, FieldEditorKind.Float, "jumpjet VehicleTypes" },
        { "PitchAngle", Ra2SectionKind.Vehicle, FieldEditorKind.Float, "RollAngle" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesJumpjetFlightTuningDescriptionData => new()
    {
        { "JumpjetTurnRate", Ra2SectionKind.Techno, FieldEditorKind.Integer, "宽泛回退" },
        { "JumpjetTurnRate", Ra2SectionKind.Aircraft, FieldEditorKind.Integer, "转向速率" },
        { "JumpjetSpeed", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "飞行时的移动速度" },
        { "JumpjetClimb", Ra2SectionKind.Infantry, FieldEditorKind.Float, "垂直爬升 / 下降速度" },
        { "JumpjetCrash", Ra2SectionKind.Vehicle, FieldEditorKind.Float, "坠毁时的下降速度" },
        { "JumpjetHeight", Ra2SectionKind.Aircraft, FieldEditorKind.Integer, "巡航高度" },
        { "JumpjetAccel", Ra2SectionKind.Vehicle, FieldEditorKind.Float, "加速 / 减速系数" },
        { "JumpjetWobbles", Ra2SectionKind.Infantry, FieldEditorKind.Float, "每秒上下摆动" },
        { "JumpjetNoWobbles", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "禁用 JumpjetDeviation" },
        { "JumpjetDeviation", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "摆动的幅度" },
        { "JumpjetAccel", Ra2SectionKind.Warhead, FieldEditorKind.Float, "Phobos 扩展" },
        { "JumpjetNoWobbles", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "Phobos 扩展" },
        { "SlowdownDistance", Ra2SectionKind.Aircraft, FieldEditorKind.Integer, "开始应用减速" },
        { "AccelerationFactor", Ra2SectionKind.Vehicle, FieldEditorKind.Float, "加速到目标速度" },
        { "DeaccelerationFactor", Ra2SectionKind.Infantry, FieldEditorKind.Float, "接近目标单元" },
        { "Weight", Ra2SectionKind.Techno, FieldEditorKind.Text, "VehicleTypes" },
        { "Weight", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "载具重量" },
        { "PhysicalSize", Ra2SectionKind.Techno, FieldEditorKind.Text, "InfantryType" },
        { "PhysicalSize", Ra2SectionKind.Infantry, FieldEditorKind.Integer, "Z-fudge" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesRepairPowerCaptureFactoryRadarDescriptionData => new()
    {
        { "Repairable", Ra2SectionKind.Building, FieldEditorKind.Boolean, "维修光标" },
        { "Repairable", Ra2SectionKind.Techno, FieldEditorKind.Text, "BuildingTypes 字段" },
        { "SelfHealing", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "宽泛回退" },
        { "SelfHealing", Ra2SectionKind.Shield, FieldEditorKind.Float, "Phobos 护盾字段" },
        { "TiberiumHeal", Ra2SectionKind.Global, FieldEditorKind.Float, "Tiberian Sun" },
        { "TiberiumHeal", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "RA2/YR 中" },
        { "Powered", Ra2SectionKind.Building, FieldEditorKind.Boolean, "需要基地电力" },
        { "Powered", Ra2SectionKind.Shield, FieldEditorKind.Boolean, "Phobos 护盾字段" },
        { "PoweredUnit", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "PowersUnit" },
        { "PowersUnit", Ra2SectionKind.Building, FieldEditorKind.Reference, "PoweredUnit=yes" },
        { "Drainable", Ra2SectionKind.Building, FieldEditorKind.Boolean, "DrainWeapon=yes" },
        { "PoweredBy", Ra2SectionKind.Unit, FieldEditorKind.Reference, "Ares 扩展字段" },
        { "Overpowerable", Ra2SectionKind.Building, FieldEditorKind.Boolean, "overpowered" },
        { "Unsellable", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Ares 3.0" },
        { "Capturable", Ra2SectionKind.AI, FieldEditorKind.Text, "不是 [AI] 段字段" },
        { "NeedsEngineer", Ra2SectionKind.Building, FieldEditorKind.Boolean, "捕获前保持离线" },
        { "EngineerRepairable", Ra2SectionKind.Building, FieldEditorKind.Boolean, "Ares 扩展字段" },
        { "CanBeOccupied", Ra2SectionKind.Building, FieldEditorKind.Boolean, "驻军" },
        { "CanBeOccupied", Ra2SectionKind.AI, FieldEditorKind.Text, "不是 [AI] 段字段" },
        { "MaxNumberOccupants", Ra2SectionKind.Building, FieldEditorKind.Integer, "最多可容纳" },
        { "CanOccupyFire", Ra2SectionKind.Building, FieldEditorKind.Boolean, "OccupyWeapon" },
        { "LeaveRubble", Ra2SectionKind.Building, FieldEditorKind.Boolean, "残骸" },
        { "Bib", Ra2SectionKind.Building, FieldEditorKind.Boolean, "右下边缘单元" },
        { "FreeUnit", Ra2SectionKind.Building, FieldEditorKind.Reference, "免费生成" },
        { "Factory", Ra2SectionKind.Building, FieldEditorKind.Enum, "AircraftType" },
        { "WeaponsFactory", Ra2SectionKind.Building, FieldEditorKind.Boolean, "战车工厂逻辑" },
        { "UnitRepair", Ra2SectionKind.Building, FieldEditorKind.Boolean, "停靠该建筑并被维修" },
        { "Radar", Ra2SectionKind.Building, FieldEditorKind.Boolean, "雷达图" },
        { "SpySat", Ra2SectionKind.Building, FieldEditorKind.Boolean, "揭示" },
        { "SuperWeapon", Ra2SectionKind.Building, FieldEditorKind.Reference, "第一个 SuperWeaponType" },
        { "SuperWeapon2", Ra2SectionKind.Building, FieldEditorKind.Reference, "第二个 SuperWeaponType" },
        { "SuperWeapons", Ra2SectionKind.Global, FieldEditorKind.Integer, "最低 IQ" },
        { "NukeSilo", Ra2SectionKind.Building, FieldEditorKind.Boolean, "核弹发射井" },
        { "Refinery", Ra2SectionKind.Building, FieldEditorKind.Boolean, "矿厂" },
        { "Harvester", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "矿车" },
        { "DockUnload", Ra2SectionKind.Building, FieldEditorKind.Boolean, "卸载停靠点" },
        { "UnitAbsorb", Ra2SectionKind.Building, FieldEditorKind.Boolean, "载具是否可进入" },
        { "InfantryAbsorb", Ra2SectionKind.Building, FieldEditorKind.Boolean, "步兵是否可进入" },
        { "Hospital", Ra2SectionKind.Building, FieldEditorKind.Boolean, "步兵是否可被治疗" },
        { "Armory", Ra2SectionKind.Building, FieldEditorKind.Boolean, "提升到精英等级" },
        { "Cloning", Ra2SectionKind.Building, FieldEditorKind.Boolean, "克隆设施" },
        { "ConstructionYard", Ra2SectionKind.Building, FieldEditorKind.Boolean, "建造厂逻辑" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> TechnoTypesEconomyResourceCrushDescriptionData => new()
    {
        { "Storage", Ra2SectionKind.Techno, FieldEditorKind.Integer, "可储存或携带" },
        { "Storage", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "采矿车" },
        { "PipScale", Ra2SectionKind.Techno, FieldEditorKind.Enum, "乘客、弹药、资源储量" },
        { "PipScale", Ra2SectionKind.Building, FieldEditorKind.Enum, "Passengers" },
        { "Pip", Ra2SectionKind.Techno, FieldEditorKind.Text, "InfantryTypes 字段" },
        { "Pip", Ra2SectionKind.Infantry, FieldEditorKind.Enum, "pips2.shp" },
        { "Points", Ra2SectionKind.Techno, FieldEditorKind.Integer, "计分逻辑" },
        { "Points", Ra2SectionKind.Building, FieldEditorKind.Integer, "摧毁该对象" },
        { "Bunkerable", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "VehicleTypes 字段" },
        { "Bunkerable", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "Bunker=yes" },
        { "IFVMode", Ra2SectionKind.Techno, FieldEditorKind.Text, "InfantryTypes 和 VehicleTypes" },
        { "IFVMode", Ra2SectionKind.Infantry, FieldEditorKind.Integer, "Weapon1" },
        { "IFVMode", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "Weapon1" },
        { "Crushable", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "Crusher=yes" },
        { "Crushable", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "OmniCrusher=yes" },
        { "Crusher", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "宽泛回退" },
        { "Crusher", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "MovementZone" },
        { "OmniCrusher", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "VehicleTypes 字段" },
        { "OmniCrusher", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "Crusher=yes" },
        { "OmniCrushResistant", Ra2SectionKind.Techno, FieldEditorKind.Boolean, "VehicleTypes 与 InfantryTypes" },
        { "OmniCrushResistant", Ra2SectionKind.Vehicle, FieldEditorKind.Boolean, "不能被 OmniCrusher" },
        { "CrushSound", Ra2SectionKind.Techno, FieldEditorKind.Reference, "对象被碾压" },
        { "CrushSound", Ra2SectionKind.Infantry, FieldEditorKind.Reference, "Sound" },
        { "CrushSound", Ra2SectionKind.Vehicle, FieldEditorKind.Reference, "Crushable" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> WeaponCoreBigBatchDescriptionData => new()
    {
        { "Damage", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "基础伤害点数" },
        { "Damage", Ra2SectionKind.Animation, FieldEditorKind.Float, "按帧造成伤害" },
        { "Damage", Ra2SectionKind.Techno, FieldEditorKind.Text, "不是 TechnoType 字段" },
        { "ROF", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "Burst 完成后" },
        { "Range", Ra2SectionKind.Weapon, FieldEditorKind.Float, "Range=-2" },
        { "Range", Ra2SectionKind.Sound, FieldEditorKind.Integer, "声音能被听到" },
        { "Range", Ra2SectionKind.SuperWeapon, FieldEditorKind.Float, "目标指示圆环" },
        { "MinimumRange", Ra2SectionKind.Weapon, FieldEditorKind.Float, "最小距离" },
        { "Projectile", Ra2SectionKind.Weapon, FieldEditorKind.Reference, "Projectile section" },
        { "Projectile", Ra2SectionKind.Techno, FieldEditorKind.Text, "不应作为 TechnoType" },
        { "Warhead", Ra2SectionKind.Weapon, FieldEditorKind.Reference, "命中后使用" },
        { "Warhead", Ra2SectionKind.Animation, FieldEditorKind.Reference, "Animation 上" },
        { "Report", Ra2SectionKind.Weapon, FieldEditorKind.Reference, "SoundList" },
        { "Report", Ra2SectionKind.Animation, FieldEditorKind.Reference, "动画播放" },
        { "Anim", Ra2SectionKind.Weapon, FieldEditorKind.Reference, "muzzle flash" },
        { "Bright", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "战斗光照" },
        { "Lobber", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "很高的抛物线" },
        { "CellRangefinding", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "单元格中心" },
        { "RevealOnFire", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "被所有玩家揭露" },
        { "AreaFire", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "当前所在单元格" },
        { "AreaFire.Target", Ra2SectionKind.Weapon, FieldEditorKind.Enum, "AreaFire 武器目标选择方式" },
        { "LimboLaunch", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "Limbo 状态" },
        { "Suicide", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "立刻自毁" },
        { "Suicide", Ra2SectionKind.AI, FieldEditorKind.Text, "属于 TeamType 字段" },
        { "TurboBoost", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "速度加成" },
        { "TurboBoost", Ra2SectionKind.Global, FieldEditorKind.Float, "[CombatDamage]" },
        { "Burst", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "连续发射的弹数" },
        { "Burst.Delays", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "每一发指定独立延迟" },
        { "ROF.RandomDelay", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "ROF 随机附加延迟" },
        { "ChargeTurret.Delays", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "炮塔充能动画延迟" },
        { "DiskLaser.Radius", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "DiskLaser" },
        { "Bolt.Arcs", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "电弧数量" },
        { "DelayedFire.Duration", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "开火前延迟" },
        { "DelayedFire.Animation", Ra2SectionKind.Weapon, FieldEditorKind.Reference, "开火延迟开始时创建的动画" },
        { "DelayedFire.SkipInTransport", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "运输载具内" },
        { "ExtraRange.TargetMoving", Ra2SectionKind.Weapon, FieldEditorKind.Float, "目标处于移动状态" },
        { "ExtraWarheads", Ra2SectionKind.Weapon, FieldEditorKind.Reference, "额外引爆多个 Warhead" },
        { "ExtraWarheads.DamageOverrides", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "单独覆盖 Damage" },
        { "KeepRange", Ra2SectionKind.Weapon, FieldEditorKind.Float, "开火后" },
        { "Strafing.Shots", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "一次 strafing run" },
        { "Strafing.TargetCell", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "切换为地面格" },
        { "CanTarget", Ra2SectionKind.Weapon, FieldEditorKind.MultiSelect, "目标类型列表" },
        { "CanTargetHouses", Ra2SectionKind.Weapon, FieldEditorKind.MultiSelect, "所属方关系" },
        { "CanTarget.MaxHealth", Ra2SectionKind.Weapon, FieldEditorKind.Float, "生命比例" },
        { "OmniFire.TurnToTarget", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "仍尝试转向目标" },
        { "CylinderRangefinding", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "圆柱式射程判断" },
        { "KickOutPassengers", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "踢出乘客" },
        { "AttackNoThreatBuildings", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "非威胁建筑" },
    };

    [Theory]
    [MemberData(nameof(WarheadCoreBigBatchDescriptionData))]
    public void Load_V32WarheadCoreBigBatchRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("num ;", definition.Description);
        Assert.DoesNotContain("list ;", definition.Description);
        Assert.DoesNotContain("bool ;", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32WarheadCoreBigBatchAddsSpecificContextRows()
    {
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("AffectsAllies", Ra2SectionKind.Warhead).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("AffectsOwner", Ra2SectionKind.Warhead).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("CellSpread.MaxAffect", Ra2SectionKind.Warhead).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("ShakeXlo", Ra2SectionKind.Warhead).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("AnimList.PickRandom", Ra2SectionKind.Warhead).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("CombatLightChance", Ra2SectionKind.Warhead).EditorKind);
    }

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> WarheadCoreBigBatchDescriptionData => new()
    {
        { "Verses", Ra2SectionKind.Warhead, FieldEditorKind.Text, "Armor 类型的伤害倍率" },
        { "Verses", Ra2SectionKind.Weapon, FieldEditorKind.Text, "不是 Weapon 字段" },
        { "CellSpread", Ra2SectionKind.Warhead, FieldEditorKind.Float, "爆炸 / 伤害扩散半径" },
        { "PercentAtMax", Ra2SectionKind.Warhead, FieldEditorKind.Float, "最远端的伤害倍率" },
        { "Wood", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "树木" },
        { "Wall", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "Wall=yes" },
        { "Rocker", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "摇晃" },
        { "AnimList", Ra2SectionKind.Warhead, FieldEditorKind.Reference, "动画列表" },
        { "InfDeath", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "死亡动画" },
        { "Conventional", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "水花" },
        { "Tiberium", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "矿石" },
        { "ProneDamage", Ra2SectionKind.Warhead, FieldEditorKind.Float, "卧倒步兵" },
        { "Sparky", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "森林火" },
        { "Fire", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "冰面" },
        { "Fire", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "不是 Weapon 字段" },
        { "Bright", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "直接引爆 Warhead" },
        { "CLDisableRed", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "红色通道" },
        { "CLDisableGreen", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "绿色通道" },
        { "CLDisableBlue", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "蓝色通道" },
        { "CombatLightSize", Ra2SectionKind.Warhead, FieldEditorKind.Float, "战斗光照固定大小" },
        { "ShakeXlo", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "水平震动" },
        { "ShakeYhi", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "垂直震动" },
        { "AffectsAllies", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "友军" },
        { "AffectsAllies", Ra2SectionKind.Weapon, FieldEditorKind.Boolean, "不是 Weapon 字段" },
        { "AffectsEnemies", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "敌对所属方" },
        { "AffectsOwner", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "发射者所属方自身" },
        { "AffectsNeutral", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "中立所属方" },
        { "AffectsAbovePercent", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "高于指定阈值" },
        { "AffectsBelowPercent", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "低于指定阈值" },
        { "CellSpread.MaxAffect", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "最多被同一原因命中" },
        { "Deployed.Damage", Ra2SectionKind.Warhead, FieldEditorKind.Float, "已部署步兵" },
        { "InfDeathAnim", Ra2SectionKind.Warhead, FieldEditorKind.Reference, "自定义动画" },
        { "Ripple.Radius", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "水波效果" },
        { "AnimList.PickRandom", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "随机选择动画" },
        { "AnimList.CreationInterval", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "生成间隔" },
        { "AnimList.ScatterMin", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "最小距离" },
        { "SplashList", Ra2SectionKind.Warhead, FieldEditorKind.Reference, "水中命中" },
        { "CombatLightChance", Ra2SectionKind.Warhead, FieldEditorKind.Float, "概率" },
        { "CombatLightDetailLevel", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "最低显示细节等级" },
        { "CombatLightDetailLevel.CheckColored", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "仍检查 detail level" },
        { "CLIsBlack", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "黑色闪光" },
        { "Crit.Chance", Ra2SectionKind.Warhead, FieldEditorKind.Float, "暴击逻辑" },
        { "Crit.ExtraDamage", Ra2SectionKind.Warhead, FieldEditorKind.Integer, "额外伤害" },
        { "DamageAlliesMultiplier", Ra2SectionKind.Warhead, FieldEditorKind.Float, "友军目标" },
        { "DamageEnemiesMultiplier", Ra2SectionKind.Warhead, FieldEditorKind.Float, "敌方目标" },
        { "DamageOwnerMultiplier", Ra2SectionKind.Warhead, FieldEditorKind.Float, "发射者所属方" }
    };



    [Theory]
    [MemberData(nameof(ProjectileCoreDescriptionData))]
    public void Load_V32ProjectileCoreRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("布尔字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32ProjectileCoreBatchAddsAresAndPhobosProjectileRows()
    {
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("SubjectToBuildings", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("SubjectToTrenches", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Interceptable", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Text, FindDefinition("Trajectory", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Float, FindDefinition("Trajectory.Bombard.DetonationDistance", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Trajectory.Straight.ThroughBuilding", Ra2SectionKind.Projectile).EditorKind);
    }

    [Theory]
    [MemberData(nameof(ProjectileAdvancedDescriptionData))]
    public void Load_V32ProjectileAdvancedRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("布尔字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32ProjectileAdvancedBatchAddsCanonicalProjectileRows()
    {
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Airburst", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("AirburstWeapon", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("ShrapnelWeapon", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("ShrapnelCount", Ra2SectionKind.Projectile).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("BombParachute", Ra2SectionKind.Projectile).EditorKind);
    }

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> ProjectileCoreDescriptionData => new()
    {
        { "AA", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "空中目标" },
        { "AG", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "地面或水面" },
        { "AA", Ra2SectionKind.Weapon, FieldEditorKind.Text, "不是 Weapon 字段" },
        { "ROT", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "转向 / 追踪能力" },
        { "Image", Ra2SectionKind.Projectile, FieldEditorKind.Reference, "图像或 art(md).ini" },
        { "Shadow", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "绘制影子" },
        { "Proximity", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "近炸逻辑" },
        { "Ranged", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "ProjectileRange" },
        { "Arcing", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "抛物线轨迹" },
        { "Inaccurate", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "随机散布" },
        { "FlakScatter", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "假散布效果" },
        { "SubjectToCliffs", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "悬崖/高桥" },
        { "SubjectToElevation", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "ElevationModel" },
        { "SubjectToWalls", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "墙体阻挡" },
        { "SubjectToBuildings", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "solid buildings" },
        { "SubjectToTrenches", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "UC.PassThrough" },
        { "Acceleration", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "加速因子" },
        { "Vertical", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "直线/垂直类射线" },
        { "Dropping", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "空中投落" },
        { "Arm", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "引信启动延迟" },
        { "CourseLockDuration", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "保持初始航向" },
        { "Scalable", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "Trailer 动画 SpawnDelay" },
        { "Interceptable", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "可被拦截系统拦截" },
        { "SubjectToLand", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "陆地地形" },
        { "SubjectToWater", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "水域地形" },
        { "Trajectory", Ra2SectionKind.Projectile, FieldEditorKind.Text, "自定义轨迹类型" },
        { "Trajectory.Bombard.DetonationDistance", Ra2SectionKind.Projectile, FieldEditorKind.Float, "强制爆炸" },
        { "Trajectory.Parabola.BounceTimes", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "抛物线轨迹" },
        { "Trajectory.Straight.PassThrough", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "穿透" },
        { "Trajectory.Straight.ProximityWarhead", Ra2SectionKind.Projectile, FieldEditorKind.Reference, "近炸" },
        { "Trajectory.Straight.ThroughBuilding", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "穿越建筑" }
    };


    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> ProjectileAdvancedDescriptionData => new()
    {
        { "Airburst", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "空爆逻辑" },
        { "Airburst", Ra2SectionKind.Weapon, FieldEditorKind.Text, "不是 Weapon 字段" },
        { "AirburstWeapon", Ra2SectionKind.Projectile, FieldEditorKind.Reference, "子武器" },
        { "AirburstSpread", Ra2SectionKind.Projectile, FieldEditorKind.Float, "覆盖的单元距离" },
        { "Airburst.UseCluster", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "Cluster 指定数量" },
        { "AroundTarget", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "围绕原目标位置" },
        { "Splits", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "分裂逻辑" },
        { "RetargetAccuracy", Ra2SectionKind.Projectile, FieldEditorKind.Float, "继续瞄准原目标" },
        { "RetargetSelf.Probability", Ra2SectionKind.Projectile, FieldEditorKind.Float, "保留该目标" },
        { "Splits.TargetingDistance", Ra2SectionKind.Projectile, FieldEditorKind.Float, "单位为格" },
        { "ClusterScatter.Max", Ra2SectionKind.Projectile, FieldEditorKind.Float, "最大随机散布距离" },
        { "BallisticScatter.Max", Ra2SectionKind.Projectile, FieldEditorKind.Float, "假散布" },
        { "Gravity", Ra2SectionKind.Projectile, FieldEditorKind.Float, "个体重力值" },
        { "Parachuted", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "伞降炸弹" },
        { "BombParachute", Ra2SectionKind.Projectile, FieldEditorKind.Reference, "降落伞动画" },
        { "ReturnWeapon", Ra2SectionKind.Projectile, FieldEditorKind.Reference, "返回武器" },
        { "ShrapnelWeapon", Ra2SectionKind.Projectile, FieldEditorKind.Reference, "shrapnel 子武器" },
        { "ShrapnelCount", Ra2SectionKind.Projectile, FieldEditorKind.Integer, "发射数量" },
        { "Shrapnel.UseWeaponTargeting", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "targeting filters" },
        { "AirstrikeLineColor", Ra2SectionKind.Projectile, FieldEditorKind.Text, "不是 Projectile 字段" }
    };



    [Theory]
    [MemberData(nameof(ArtAnimationCoreDescriptionData))]
    public void Load_V32ArtAnimationCoreRowsUseSourceBackedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedDescriptionFragment)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedDescriptionFragment, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("布尔字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_V32ArtAnimationCoreBatchAddsAnimationPlaybackRows()
    {
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("Start", Ra2SectionKind.Animation).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("End", Ra2SectionKind.Animation).EditorKind);
        Assert.Equal(FieldEditorKind.Integer, FindDefinition("RandomRate", Ra2SectionKind.Animation).EditorKind);
        Assert.Equal(FieldEditorKind.Reference, FindDefinition("TrailerAnim", Ra2SectionKind.Animation).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("Theater", Ra2SectionKind.ArtObject).EditorKind);
        Assert.Equal(FieldEditorKind.Boolean, FindDefinition("AnimPalette", Ra2SectionKind.Projectile).EditorKind);
    }

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> ArtAnimationCoreDescriptionData => new()
    {
        { "Image", Ra2SectionKind.Animation, FieldEditorKind.Reference, "图像资源" },
        { "Image", Ra2SectionKind.Global, FieldEditorKind.Text, "不是 [General] / Global" },
        { "Theater", Ra2SectionKind.ArtObject, FieldEditorKind.Boolean, "地图 theater" },
        { "Normalized", Ra2SectionKind.Animation, FieldEditorKind.Boolean, "游戏速度" },
        { "LoopStart", Ra2SectionKind.Animation, FieldEditorKind.Integer, "0-based 起始帧" },
        { "LoopEnd", Ra2SectionKind.Animation, FieldEditorKind.Integer, "结束帧边界" },
        { "LoopCount", Ra2SectionKind.Animation, FieldEditorKind.Integer, "无限循环" },
        { "Rate", Ra2SectionKind.Animation, FieldEditorKind.Integer, "播放速率" },
        { "RandomRate", Ra2SectionKind.Animation, FieldEditorKind.Integer, "随机范围" },
        { "Start", Ra2SectionKind.Animation, FieldEditorKind.Integer, "首次播放" },
        { "End", Ra2SectionKind.Animation, FieldEditorKind.Integer, "帧数" },
        { "TrailerAnim", Ra2SectionKind.Animation, FieldEditorKind.Reference, "尾迹" },
        { "TrailerSeperation", Ra2SectionKind.Animation, FieldEditorKind.Integer, "间隔帧数" },
        { "SpawnCount", Ra2SectionKind.Animation, FieldEditorKind.Integer, "最多生成" },
        { "Translucent", Ra2SectionKind.Animation, FieldEditorKind.Boolean, "逐渐淡出" },
        { "UseNormalLight", Ra2SectionKind.Animation, FieldEditorKind.Boolean, "环境亮度" },
        { "AltPalette", Ra2SectionKind.Animation, FieldEditorKind.Boolean, "单位调色板" },
        { "AnimPalette", Ra2SectionKind.Projectile, FieldEditorKind.Boolean, "ANIM.PAL" },
        { "Next", Ra2SectionKind.Animation, FieldEditorKind.Reference, "下一个 Animation" },
        { "SpawnCount", Ra2SectionKind.Techno, FieldEditorKind.Text, "不是 TechnoType" },
        { "VisibleTo", Ra2SectionKind.Animation, FieldEditorKind.MultiSelect, "哪些玩家关系" },
        { "RestrictVisibilityIfCloaked", Ra2SectionKind.Animation, FieldEditorKind.Boolean, "隐形" },
        { "AttachedSystem", Ra2SectionKind.Animation, FieldEditorKind.Text, "粒子系统" },
        { "CreateUnit.Owner", Ra2SectionKind.Animation, FieldEditorKind.Text, "所属方" },
        { "CreateUnit.SpawnAnim", Ra2SectionKind.Animation, FieldEditorKind.Reference, "生成动画" },
        { "Damage.Delay", Ra2SectionKind.Animation, FieldEditorKind.Integer, "延迟帧数" },
        { "SmallFireCount", Ra2SectionKind.Animation, FieldEditorKind.Integer, "小型火焰动画数量" },
        { "LargeFireAnims", Ra2SectionKind.Animation, FieldEditorKind.Reference, "大型火焰动画类型" },
        { "SplashAnims", Ra2SectionKind.Animation, FieldEditorKind.Reference, "splash 动画列表" },
        { "AIAutoDeployMCV", Ra2SectionKind.Animation, FieldEditorKind.Boolean, "不是 AnimationType" }
    };



    [Theory]
    [MemberData(nameof(RetainedTechnoGuardrailDescriptionData))]
    public void Load_V32RetainedTechnoGuardrailsDoNotUseNeedsMoreEvidenceDescriptions(string key)
    {
        Ra2FieldDefinition definition = FindDefinition(key, Ra2SectionKind.Techno);

        Assert.DoesNotContain("来源不足待核验", definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);
    }

    public static TheoryData<string> RetainedTechnoGuardrailDescriptionData => new()
    {
        { "Agent" },
        { "Aggressive" }
    };


    [Theory]
    [MemberData(nameof(SuperWeaponSideCountryUiMegaBatchDescriptionData))]
    public void Load_V32SuperWeaponSideCountryUiMegaBatchRowsHaveSafeDescriptions(string key, Ra2SectionKind sectionKind, string expectedText, bool needsMoreEvidence)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Contains(expectedText, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);

        if (needsMoreEvidence)
            Assert.Contains("来源不足待核验", definition.Description);
    }

    public static TheoryData<string, Ra2SectionKind, string, bool> SuperWeaponSideCountryUiMegaBatchDescriptionData => new()
    {
        { "SW.Range", Ra2SectionKind.SuperWeapon, "Ares 通用超级武器参数", false },
        { "SW.AITargeting", Ra2SectionKind.SuperWeapon, "AI / 自动发射目标选择", false },
        { "ParaDrop.Types", Ra2SectionKind.SuperWeapon, "ParaDrop / AmerParaDrop", false },
        { "TabIndex", Ra2SectionKind.SuperWeapon, "超级武器按钮", false },
        { "AI.BaseDefenses", Ra2SectionKind.Side, "AI 基地防御默认配置", false },
        { "SuperWeaponSidebar.Interval", Ra2SectionKind.Side, "Side 级 UI / sidebar", false },
        { "ParaDrop.Types", Ra2SectionKind.Country, "Country 级 ParaDrop 默认值", false },
    };


    public static TheoryData<string, Ra2SectionKind, string> BatchBVerifiedDescriptionData => new()
    {
        { "BuildCat", Ra2SectionKind.Building, "设置该建筑所属的建造分类，主要影响侧边栏位置；Combat 会进入防御栏，其他常见值通常仍显示在主建筑栏。" },
        { "BuildCat", Ra2SectionKind.Techno, "经来源核验，BuildCat 是 BuildingTypes 字段，用于设置建筑建造分类与侧边栏位置；此 Techno 行仅用于避免旧占位符污染 Hover，不应作为非建筑对象的有效字段。" },
        { "Crewed", Ra2SectionKind.Building, "设置该建筑被摧毁时是否有乘员步兵逃出；若对象由 Suicide=yes 的自身武器摧毁，则不会留下乘员或乘客。" },
        { "Crewed", Ra2SectionKind.Techno, "经来源核验，Crewed 是 VehicleTypes、AircraftTypes 与 BuildingTypes 的布尔字段，控制对象摧毁时是否有乘员步兵逃出；此 Techno 行仅作宽泛回退，Infantry/Unit 上下文不应直接应用。" },
        { "Crewed", Ra2SectionKind.Vehicle, "设置该载具被摧毁时是否有乘员步兵逃出；若对象由 Suicide=yes 的自身武器摧毁，则不会留下乘员或乘客。" },
        { "Crewed", Ra2SectionKind.Aircraft, "设置该飞行器被摧毁时是否有乘员步兵逃出；若对象由 Suicide=yes 的自身武器摧毁，则不会留下乘员或乘客。" },
        { "Turret", Ra2SectionKind.Techno, "经来源核验，Turret 是 VehicleTypes 与 BuildingTypes 的布尔字段，用于声明对象是否拥有炮塔；此 Techno 行仅作宽泛回退，非载具/建筑上下文不应直接应用。" },
        { "Turret", Ra2SectionKind.Vehicle, "设置该载具是否拥有独立炮塔。Turret=yes 时游戏会按对象 Image 加 tur 后缀加载 VXL/HVA 炮塔文件；缺少对应文件可能导致崩溃。" },
        { "Turret", Ra2SectionKind.Building, "设置该建筑是否拥有炮塔；建筑炮塔通常配合 TurretAnim、TurretAnimIsVoxel、TurretAnimX/Y/ZAdjust 等字段指定和渲染。" },
        { "ThreatPosed", Ra2SectionKind.Techno, "设置 TechnoType 的威胁等级，供威胁系统和自动索敌参考；无武器目标不能只靠该值变成主动攻击目标，非建筑对象即使为 0 也仍可能被主动攻击。" },
        { "ThreatPosed", Ra2SectionKind.AI, "经来源核验，ThreatPosed 是 TechnoTypes 字段，用于对象威胁等级；AI 行来自旧资料抽取，不应作为 AITrigger、TaskForce、Script 或 TeamType 的有效字段。" }
    };

    [Theory]
    [MemberData(nameof(ArtVoxelTerrainSoundMegaBatchDescriptionData))]
    public void Load_V32ArtVoxelTerrainSoundMegaBatchRowsHaveSafeDescriptions(string key, Ra2SectionKind sectionKind, string expectedText, bool needsMoreEvidence)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Contains(expectedText, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);

        if (needsMoreEvidence)
            Assert.Contains("来源不足待核验", definition.Description);
    }

    public static TheoryData<string, Ra2SectionKind, string, bool> ArtVoxelTerrainSoundMegaBatchDescriptionData => new()
    {
        { "ActiveAnimThree", Ra2SectionKind.ArtObject, "第三组附属动画", false },
        { "Delay", Ra2SectionKind.Sound, "PREDELAY", false },
        { "DestroyAnim", Ra2SectionKind.Terrain, "TerrainType 扩展", false },
        { "LaserTrail.Types", Ra2SectionKind.VoxelAnim, "VoxelAnim 扩展", false },
        { "Pips.Shield", Ra2SectionKind.VoxelAnim, "不是 VoxelAnim 字段", false },
    };

    [Theory]
    [MemberData(nameof(AresPhobosExtensionsMegaBatchDescriptionData))]
    public void Load_V32AresPhobosExtensionsMegaBatchRowsHaveSafeDescriptions(string key, Ra2SectionKind sectionKind, string expectedText, bool needsMoreEvidence)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Contains(expectedText, definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("不直接用于 Hover", definition.Description);
        Assert.DoesNotContain("不能直接用于 Hover", definition.Description);
        Assert.DoesNotContain("placeholder", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", definition.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TBD", definition.Description, StringComparison.OrdinalIgnoreCase);

        if (needsMoreEvidence)
            Assert.Contains("来源不足待核验", definition.Description);
    }

    public static TheoryData<string, Ra2SectionKind, string, bool> AresPhobosExtensionsMegaBatchDescriptionData => new()
    {
        { "Duration", Ra2SectionKind.AttachEffect, "Phobos AttachEffectType 参数", false },
        { "Strength", Ra2SectionKind.Shield, "Phobos ShieldType 参数", false },
        { "DrawType", Ra2SectionKind.LaserTrail, "Phobos LaserTrailType 参数", false },
        { "LaserTrailN.FLH", Ra2SectionKind.LaserTrail, "TechnoType Image/art entry", false },
        { "InfoType", Ra2SectionKind.DigitalDisplay, "Phobos DigitalDisplay 参数", false },
        { "InsigniaFrame", Ra2SectionKind.Insignia, "Phobos veterancy insignia 参数", false },
        { "RadColor", Ra2SectionKind.Radiation, "Phobos RadiationType 参数", false },
        { "Shield.Penetrate.Types", Ra2SectionKind.Warhead, "Phobos Warhead shield interaction 参数", false },
        { "IronCurtain.Duration", Ra2SectionKind.Warhead, "Ares Warhead 参数", false },
        { "RadType", Ra2SectionKind.Weapon, "Phobos WeaponType 参数", false },
        { "Pips.Tiberiums.Frames", Ra2SectionKind.Building, "Phobos AudioVisual / Building pip 参数", false },
        { "ForceShield.KeptOnDeploy", Ra2SectionKind.Vehicle, "Phobos VehicleType 参数", false },
    };


    [Fact]
    public void Load_V32BuiltInRowsDoNotExposeDirectHoverRiskPlaceholders()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            definition.Description.Contains("原始英文说明", StringComparison.OrdinalIgnoreCase) ||
            definition.Description.Contains("不直接用于 Hover", StringComparison.OrdinalIgnoreCase) ||
            definition.Description.Contains("不能直接用于 Hover", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(definition.Description.Trim(), "整数型字段", StringComparison.Ordinal) ||
            string.Equals(definition.Description.Trim(), "数值型字段", StringComparison.Ordinal));
    }

    [Fact]
    public void Load_V32RuntimePackDoesNotRetainNeedsMoreEvidenceHoverGuardrails()
    {
        LocalFieldRegistryLoadResult result = new BuiltInFieldRegistryPackLoader().Load();

        Assert.DoesNotContain(result.Definitions, definition =>
            definition.Description is not null &&
            definition.Description.Contains("来源不足待核验", StringComparison.OrdinalIgnoreCase));
    }



    [Theory]
    [MemberData(nameof(UnresolvedRowsRecheckDescriptionData))]
    public void Load_V32UnresolvedRowsRecheckRowsUseVerifiedDescriptions(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedText, definition.Description);
        Assert.DoesNotContain("来源不足待核验", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
    }

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> UnresolvedRowsRecheckDescriptionData => new()
    {
        { "ExtendedAircraftMissions", Ra2SectionKind.Aircraft, FieldEditorKind.Boolean, "扩展飞机任务" },
        { "SpawnDistanceFromTarget", Ra2SectionKind.Aircraft, FieldEditorKind.Float, "固定生成距离" },
        { "IvanBomb.Delay", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "自动引爆前经过的帧数" },
        { "IvanBomb", Ra2SectionKind.Warhead, FieldEditorKind.Boolean, "疯狂伊文炸弹弹头" },
        { "EBoltZAdjust", Ra2SectionKind.Weapon, FieldEditorKind.Integer, "EBolt 绘制的 Z 偏移" },
        { "KeepTargetOnMove.Weapon", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "距离检查使用的武器槽" },
        { "TurretTravel", Ra2SectionKind.Vehicle, FieldEditorKind.Integer, "炮塔最大移动距离" },
        { "Autocreate", Ra2SectionKind.AI, FieldEditorKind.Text, "不是 [AI] 段普通字段" },
        { "LooseRecruit", Ra2SectionKind.AI, FieldEditorKind.Boolean, "具体效果仍需测试" }
    };


    [Theory]
    [MemberData(nameof(AiSchemaRecheckDescriptionData))]
    public void Load_V32AiSchemaRecheckRowsUseVerifiedDescriptionsOrGuardrails(string key, Ra2SectionKind sectionKind, FieldEditorKind expectedEditorKind, string expectedText)
    {
        Ra2FieldDefinition definition = FindDefinition(key, sectionKind);

        Assert.Equal(expectedEditorKind, definition.EditorKind);
        Assert.Contains(expectedText, definition.Description);
        Assert.DoesNotContain("来源不足待核验", definition.Description);
        Assert.DoesNotContain("原始英文说明", definition.Description);
        Assert.DoesNotContain("整数型字段", definition.Description);
        Assert.DoesNotContain("数值型字段", definition.Description);
    }

    public static TheoryData<string, Ra2SectionKind, FieldEditorKind, string> AiSchemaRecheckDescriptionData => new()
    {
        { "TaskForce", Ra2SectionKind.TeamType, FieldEditorKind.Reference, "指定该 TeamType 要组装的 TaskForce ID" },
        { "Script", Ra2SectionKind.TeamType, FieldEditorKind.Reference, "指定该 TeamType 要执行的 ScriptType ID" },
        { "Autocreate", Ra2SectionKind.TeamType, FieldEditorKind.Boolean, "地图预置可招募单位" },
        { "AreTeamMembersRecruitable", Ra2SectionKind.TeamType, FieldEditorKind.Boolean, "是否允许被其他队伍招募" },
        { "Priority", Ra2SectionKind.TeamType, FieldEditorKind.Integer, "招募优先级" },
        { "VeteranLevel", Ra2SectionKind.TeamType, FieldEditorKind.Integer, "初始等级" },
        { "Group", Ra2SectionKind.TaskForce, FieldEditorKind.Integer, "TaskForce 的 group ID" },
        { "x", Ra2SectionKind.Script, FieldEditorKind.Text, "ScriptType 节中的数字键行" },
        { "D1", Ra2SectionKind.AI, FieldEditorKind.Text, "AITriggerTypes 逗号分隔格式" },
        { "Agent", Ra2SectionKind.Infantry, FieldEditorKind.Boolean, "间谍 / 特工" },
        { "Spyable", Ra2SectionKind.Building, FieldEditorKind.Boolean, "间谍类步兵渗透" },
        { "AICaptureLowMoneyMark", Ra2SectionKind.Global, FieldEditorKind.Integer, "AI 何时被视为资金不足" }
    };

}
