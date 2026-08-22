using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiFieldEvidenceProviderTests
{
    [Fact]
    public void Retrieve_ExactCurrentKeyReturnsMatchingEvidence()
    {
        Ra2FieldDefinition strength = Define("Strength", "Object hit points.");
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(strength),
            new TestProvenanceProvider(strength),
            Ra2SectionKind.Infantry,
            keyName: "Strength",
            selectedText: null,
            promptText: null,
            maxCount: 8);

        Ra2AiFieldEvidence item = Assert.Single(evidence);
        Assert.Equal("Strength", item.Key);
        Assert.Equal("current key exact", item.MatchReason);
        Assert.Equal("BuiltIn", item.Provenance);
        Assert.Equal("BuiltIn", item.SourceName);
    }

    [Fact]
    public void Retrieve_UnknownKeyReturnsEmptyEvidenceSafely()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(Define("Strength", "Object hit points.")),
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: "DoesNotExist",
            selectedText: null,
            promptText: null,
            maxCount: 8);

        Assert.Empty(evidence);
    }

    [Fact]
    public void Retrieve_PromptKeywordCanMatchFieldKey()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(
                Define("Strength", "Object hit points."),
                Define("Armor", "Armor type.")),
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: null,
            selectedText: null,
            promptText: "Explain armor options",
            maxCount: 8);

        Ra2AiFieldEvidence item = Assert.Single(evidence);
        Assert.Equal("Armor", item.Key);
        Assert.Equal("prompt key", item.MatchReason);
    }

    [Fact]
    public void Retrieve_PromptKeywordCanMatchDisplayNameAliasAndDescription()
    {
        Ra2FieldDefinition strength = new(
            "Strength",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            "Object hit points.",
            Ra2FieldValueMetadata.Unknown,
            "Hit Points",
            ["HP"]);
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> aliasEvidence = provider.Retrieve(
            new TestFieldProvider(strength),
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: null,
            selectedText: null,
            promptText: "explain HP",
            maxCount: 8);
        IReadOnlyList<Ra2AiFieldEvidence> descriptionEvidence = provider.Retrieve(
            new TestFieldProvider(strength),
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: null,
            selectedText: null,
            promptText: "hit points",
            maxCount: 8);

        Assert.Equal("prompt alias", Assert.Single(aliasEvidence).MatchReason);
        Assert.Equal("prompt display text", Assert.Single(descriptionEvidence).MatchReason);
    }

    [Fact]
    public void Retrieve_SelectedTextCanMatchIniKeys()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(
                Define("Strength", "Object hit points."),
                Define("Primary", "Primary weapon reference.")),
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: null,
            selectedText: "Primary=120mm",
            promptText: null,
            maxCount: 8);

        Ra2AiFieldEvidence item = Assert.Single(evidence);
        Assert.Equal("Primary", item.Key);
        Assert.Equal("selected key", item.MatchReason);
    }

    [Fact]
    public void Retrieve_ResultCountIsBoundedByRequestedTopNAndHardCap()
    {
        Ra2FieldDefinition[] definitions = Enumerable.Range(1, 40)
            .Select(index => Define($"WeaponField{index:00}", "Shared weapon evidence."))
            .ToArray();
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> topEight = provider.Retrieve(
            new TestFieldProvider(definitions),
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: null,
            selectedText: null,
            promptText: "weapon",
            maxCount: 8);
        IReadOnlyList<Ra2AiFieldEvidence> hardCapped = provider.Retrieve(
            new TestFieldProvider(definitions),
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: null,
            selectedText: null,
            promptText: "weapon",
            maxCount: 50);

        Assert.Equal(8, topEight.Count);
        Assert.Equal(Ra2FieldRegistryAiEvidenceProvider.HardMaxEvidenceCount, hardCapped.Count);
    }

    [Fact]
    public void Retrieve_DraftVehiclePromptSelectsUnitCoreAndVehicleCoreProfiles()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Strength", "Armor", "Speed", "Turret", "Locomotor")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "design a light vehicle unit",
            maxCount: 12);

        Assert.Contains(evidence, item => item.Key == "Strength" && item.MatchReason == "draft profile UnitCore");
        Assert.Contains(evidence, item => item.Key == "Turret" && item.MatchReason == "draft profile VehicleCore");
    }

    [Fact]
    public void Retrieve_AntiAirVehiclePromptSelectsAntiAirWeaponProfile()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Primary", "Projectile", "Warhead", "AA", "AG", "GuardRange")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "design a light anti air vehicle unit",
            maxCount: 12);

        Assert.Contains(evidence, item => item.Key == "AA" && item.MatchReason == "draft profile FollowUpAntiAirWeapon");
        Assert.Contains(evidence, item => item.Key == "GuardRange" && item.MatchReason == "draft profile AntiAirWeapon");
    }

    [Fact]
    public void Retrieve_DeployAntiAirPromptSelectsDeployTransformAndAntiAirProfiles()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("DeploysInto", "UndeploysInto", "DeployToFire", "AA", "Range")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "design a deployed anti air vehicle unit",
            maxCount: 12);

        Assert.Contains(evidence, item => item.Key == "DeploysInto" && item.MatchReason == "draft profile DeployTransform");
        Assert.Contains(evidence, item => item.Key == "AA" && item.MatchReason == "draft profile FollowUpAntiAirWeapon");
    }

    [Fact]
    public void Retrieve_TransportPromptSelectsTransportProfile()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Passengers", "PipScale", "OpenTopped", "SizeLimit")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "design a transport vehicle unit",
            maxCount: 12);

        Assert.Contains(evidence, item => item.Key == "Passengers" && item.MatchReason == "draft profile FollowUpTransport");
        Assert.Contains(evidence, item => item.Key == "OpenTopped" && item.MatchReason == "draft profile FollowUpTransport");
    }

    [Fact]
    public void Retrieve_StealthScoutPromptSelectsStealthScoutProfile()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Cloakable", "CloakingSpeed", "Sensors", "SensorsSight", "Sight")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "design a stealth scout vehicle unit",
            maxCount: 12);

        Assert.Contains(evidence, item => item.Key == "Cloakable" && item.MatchReason == "draft profile FollowUpStealthScout");
        Assert.Contains(evidence, item => item.Key == "Sensors" && item.MatchReason == "draft profile FollowUpStealthScout");
    }

    [Fact]
    public void Retrieve_AlliedFollowUpReturnsConfirmedFactionOwnerFields()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Owner", "RequiredHouses", "ForbiddenHouses", "Prerequisite", "UIName", "Name", "Image")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "change this unit to allied",
            maxCount: 16,
            currentSubject: CurrentUnitSubject());

        Assert.Contains(evidence, item => item.Key == "Owner" && item.MatchReason == "draft profile FactionOwner");
        Assert.Contains(evidence, item => item.Key == "RequiredHouses" && item.MatchReason == "draft profile FactionOwner");
        Assert.Contains(evidence, item => item.Key == "ForbiddenHouses" && item.MatchReason == "draft profile FactionOwner");
    }

    [Fact]
    public void Retrieve_SovietFollowUpReturnsFactionOwnerFields()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Owner", "RequiredHouses", "ForbiddenHouses", "Prerequisite", "UIName", "Name", "Image")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "make it a soviet country owned unit",
            maxCount: 16);

        Assert.Contains(evidence, item => item.Key == "Owner");
        Assert.Contains(evidence, item => item.Key == "RequiredHouses");
        Assert.Contains(evidence, item => item.Key == "ForbiddenHouses");
    }

    [Fact]
    public void Retrieve_PreviousAssistantDraftFieldKeysAreConfirmedAsEvidence()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();
        Ra2AiConversationContext conversationContext = new()
        {
            Turns =
            [
                new Ra2AiConversationTurn
                {
                    Role = Ra2AiConversationRole.Assistant,
                    IsDraftResponse = true,
                    Text = "```ini\n[LAAV]\nStrength=200\nArmor=light\nPrimary=LAAVMissile\nOwner=<TODO_OWNER>\n```"
                }
            ],
            TotalCharacterCount = 92
        };

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Strength", "Armor", "Primary", "Owner")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "continue editing",
            maxCount: 16,
            conversationContext: conversationContext);

        Assert.Contains(evidence, item => item.Key == "Strength" && item.MatchReason == "previous assistant draft field key");
        Assert.Contains(evidence, item => item.Key == "Armor" && item.MatchReason == "previous assistant draft field key");
        Assert.Contains(evidence, item => item.Key == "Primary" && item.MatchReason == "previous assistant draft field key");
        Assert.Contains(evidence, item => item.Key == "Owner" && item.MatchReason == "previous assistant draft field key");
    }

    [Fact]
    public void Retrieve_CurrentUnitSubjectTriggersUnitCoreAndVehicleCoreProfiles()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Strength", "Armor", "Primary", "Turret", "Locomotor")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "continue modifying this unit",
            maxCount: 12,
            currentSubject: CurrentUnitSubject());

        Assert.Contains(evidence, item => item.Key == "Strength" && item.MatchReason == "draft profile UnitCore");
        Assert.Contains(evidence, item => item.Key == "Turret" && item.MatchReason == "draft profile VehicleCore");
    }

    [Fact]
    public void Retrieve_AntiAirFollowUpReturnsWeaponProjectileWarheadAndAaFields()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Primary", "Projectile", "Warhead", "AA", "AG", "Damage", "ROF", "Range", "Verses")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "add anti air missile weapon",
            maxCount: 16);

        Assert.Contains(evidence, item => item.Key == "Primary");
        Assert.Contains(evidence, item => item.Key == "Projectile");
        Assert.Contains(evidence, item => item.Key == "Warhead");
        Assert.Contains(evidence, item => item.Key == "AA");
        Assert.Contains(evidence, item => item.Key == "AG");
    }

    [Fact]
    public void Retrieve_UnavailableProfileSeedKeysAreNotReturned()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(Define("Owner", "Owner field.")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "change this unit to allied anti air",
            maxCount: 12);

        Ra2AiFieldEvidence item = Assert.Single(evidence);
        Assert.Equal("Owner", item.Key);
        Assert.Equal("draft profile FactionOwner", item.MatchReason);
    }

    [Fact]
    public void Retrieve_DraftProfileEvidenceRemainsBounded()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany(
                "Name",
                "UIName",
                "Image",
                "Prerequisite",
                "Primary",
                "Secondary",
                "ElitePrimary",
                "EliteSecondary",
                "Strength",
                "Armor",
                "Speed",
                "Sight",
                "Cost",
                "TechLevel",
                "Owner",
                "RequiredHouses",
                "ForbiddenHouses",
                "Category",
                "BuildCat",
                "Trainable",
                "ThreatPosed",
                "Turret",
                "Crusher",
                "Crewed",
                "Weight",
                "Size",
                "Locomotor",
                "MovementZone",
                "SpeedType",
                "ROT",
                "Accelerates",
                "IsTilter",
                "Tracked",
                "AA",
                "AG",
                "Range",
                "GuardRange")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "design a light anti air vehicle unit",
            maxCount: 50,
            currentSubject: CurrentUnitSubject());

        Assert.Equal(Ra2FieldRegistryAiEvidenceProvider.HardMaxEvidenceCount, evidence.Count);
    }

    [Fact]
    public void Retrieve_ExactCurrentKeyStillHasPriorityOverDraftProfiles()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Armor", "Primary", "AA")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: "Armor",
            selectedText: null,
            promptText: "design a light anti air vehicle unit",
            maxCount: 1);

        Ra2AiFieldEvidence item = Assert.Single(evidence);
        Assert.Equal("Armor", item.Key);
        Assert.Equal("current key exact", item.MatchReason);
    }

    [Fact]
    public void Retrieve_NonDraftFieldExplanationDoesNotAddDraftProfiles()
    {
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            new TestFieldProvider(DefineMany("Armor", "Strength", "Turret")),
            provenanceProvider: null,
            Ra2SectionKind.Vehicle,
            keyName: null,
            selectedText: null,
            promptText: "Explain armor options",
            maxCount: 12);

        Ra2AiFieldEvidence item = Assert.Single(evidence);
        Assert.Equal("Armor", item.Key);
        Assert.Equal("prompt key", item.MatchReason);
    }

    [Fact]
    public void Retrieve_DoesNotRequireRegistryReloadOrProviderMutation()
    {
        TestFieldProvider fieldProvider = new(Define("Armor", "Armor type."));
        Ra2FieldRegistryAiEvidenceProvider provider = new();

        IReadOnlyList<Ra2AiFieldEvidence> evidence = provider.Retrieve(
            fieldProvider,
            provenanceProvider: null,
            Ra2SectionKind.Infantry,
            keyName: null,
            selectedText: null,
            promptText: "armor",
            maxCount: 8);

        Assert.Single(evidence);
        Assert.Equal(0, fieldProvider.MutationCount);
        Assert.Equal(0, fieldProvider.ReloadCount);
        Assert.True(fieldProvider.GetFieldsCount > 0);
    }

    private static Ra2AiCurrentSubject CurrentUnitSubject()
        => new()
        {
            Kind = Ra2AiSubjectKind.Unit,
            SubjectId = "LAAV",
            Source = Ra2AiSubjectSource.LastAssistantDraft,
            Summary = "Prior assistant draft unit.",
            Confidence = 0.9,
            IsDraft = true
        };

    private static Ra2FieldDefinition Define(string key, string description)
        => new(key, [Ra2SectionKind.Infantry], FieldEditorKind.Text, Ra2FieldSourceKind.BuiltIn, description);

    private static Ra2FieldDefinition[] DefineMany(params string[] keys)
        => keys.Select(key => Define(key, $"{key} field.")).ToArray();

    private sealed class TestFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly IReadOnlyList<Ra2FieldDefinition> _definitions;

        public TestFieldProvider(params Ra2FieldDefinition[] definitions)
        {
            _definitions = Array.AsReadOnly(definitions);
        }

        public int GetFieldsCount { get; private set; }

        public int MutationCount { get; }

        public int ReloadCount { get; }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
        {
            GetFieldsCount++;
            return _definitions;
        }

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }

    private sealed class TestProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        private readonly Ra2FieldDefinition _definition;

        public TestProvenanceProvider(Ra2FieldDefinition definition)
        {
            _definition = definition;
        }

        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(
            Ra2SectionKind sectionKind,
            string key)
            => string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase)
                ? FieldRegistryProvenanceLookupResult.BuiltIn(_definition)
                : FieldRegistryProvenanceLookupResult.NotFound;
    }
}
