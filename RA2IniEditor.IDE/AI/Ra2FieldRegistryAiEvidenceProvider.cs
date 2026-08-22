using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2FieldRegistryAiEvidenceProvider : IRa2AiFieldEvidenceProvider
{
    internal const int DefaultMaxEvidenceCount = 16;
    internal const int HardMaxEvidenceCount = 24;
    private const int MaxSelectedProfiles = 6;
    private const int MaxProfileCandidateKeys = 80;

    public IReadOnlyList<Ra2AiFieldEvidence> Retrieve(
        IRa2FieldDefinitionProvider? fieldProvider,
        IFieldRegistryProvenanceProvider? provenanceProvider,
        Ra2SectionKind sectionKind,
        string? keyName,
        string? selectedText,
        string? promptText,
        int maxCount,
        Ra2AiConversationContext? conversationContext = null,
        Ra2AiCurrentSubject? currentSubject = null)
    {
        if (fieldProvider is null)
            return [];

        int effectiveMaxCount = NormalizeMaxCount(maxCount);
        if (effectiveMaxCount <= 0)
            return [];

        Dictionary<string, Candidate> candidates = new(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(keyName) &&
            fieldProvider.TryGetField(sectionKind, keyName.Trim(), out Ra2FieldDefinition exactDefinition))
        {
            AddOrUpdate(
                candidates,
                exactDefinition,
                score: 1000,
                matchReason: "current key exact");
        }

        SearchTerms selectedTerms = SearchTerms.FromSelectedText(selectedText);
        SearchTerms promptTerms = SearchTerms.FromPromptText(promptText);
        if (selectedTerms.HasAny || promptTerms.HasAny)
        {
            foreach (Ra2FieldDefinition definition in fieldProvider.GetFields(sectionKind))
            {
                Candidate? selectedCandidate = MatchDefinition(definition, selectedTerms, selected: true);
                if (selectedCandidate is not null)
                    AddOrUpdate(candidates, selectedCandidate.Definition, selectedCandidate.Score, selectedCandidate.MatchReason);

                Candidate? promptCandidate = MatchDefinition(definition, promptTerms, selected: false);
                if (promptCandidate is not null)
                    AddOrUpdate(candidates, promptCandidate.Definition, promptCandidate.Score, promptCandidate.MatchReason);
            }
        }

        AddPreviousDraftFieldKeys(candidates, fieldProvider, sectionKind, conversationContext);
        AddDraftEvidenceProfiles(candidates, fieldProvider, sectionKind, promptText, currentSubject);

        return BuildEvidence(candidates.Values, provenanceProvider, sectionKind, effectiveMaxCount);
    }

    private static int NormalizeMaxCount(int maxCount)
    {
        if (maxCount < 0)
            return 0;

        if (maxCount == 0)
            return DefaultMaxEvidenceCount;

        return Math.Min(maxCount, HardMaxEvidenceCount);
    }

    private static Candidate? MatchDefinition(Ra2FieldDefinition definition, SearchTerms terms, bool selected)
    {
        if (!terms.HasAny)
            return null;

        if (terms.Keys.Contains(definition.Key) || terms.All.Contains(definition.Key))
        {
            return new Candidate(
                definition,
                selected ? 850 : 700,
                selected ? "selected key" : "prompt key");
        }

        if (definition.Aliases.Any(alias => terms.All.Contains(alias)))
        {
            return new Candidate(
                definition,
                selected ? 780 : 650,
                selected ? "selected alias" : "prompt alias");
        }

        if (!string.IsNullOrWhiteSpace(definition.DisplayName) &&
            ContainsAnyTerm(definition.DisplayName, terms.All))
        {
            return new Candidate(
                definition,
                selected ? 520 : 450,
                selected ? "selected display text" : "prompt display text");
        }

        if (!string.IsNullOrWhiteSpace(definition.Description) &&
            ContainsAnyTerm(definition.Description, terms.All))
        {
            return new Candidate(
                definition,
                selected ? 420 : 320,
                selected ? "selected description" : "prompt description");
        }

        return null;
    }

    private static bool ContainsAnyTerm(string text, IReadOnlySet<string> terms)
    {
        foreach (string term in terms)
        {
            if (term.Length >= 3 && text.Contains(term, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void AddDraftEvidenceProfiles(
        Dictionary<string, Candidate> candidates,
        IRa2FieldDefinitionProvider fieldProvider,
        Ra2SectionKind sectionKind,
        string? promptText,
        Ra2AiCurrentSubject? currentSubject)
    {
        IReadOnlyList<ProfileMatch> profiles = SelectEvidenceProfiles(promptText, currentSubject);
        if (profiles.Count == 0)
            return;

        HashSet<string> requestedKeys = new(StringComparer.OrdinalIgnoreCase);
        int candidateKeyCount = 0;
        foreach (ProfileMatch profile in profiles.Take(MaxSelectedProfiles))
        {
            foreach (string key in profile.Profile.Keys)
            {
                if (!requestedKeys.Add(key))
                    continue;

                candidateKeyCount++;
                if (candidateKeyCount > MaxProfileCandidateKeys)
                    return;

                if (!fieldProvider.TryGetField(sectionKind, key, out Ra2FieldDefinition definition))
                    continue;

                AddOrUpdate(
                    candidates,
                    definition,
                    profile.Score,
                    $"draft profile {profile.Profile.Name}");
            }
        }
    }

    private static void AddPreviousDraftFieldKeys(
        Dictionary<string, Candidate> candidates,
        IRa2FieldDefinitionProvider fieldProvider,
        Ra2SectionKind sectionKind,
        Ra2AiConversationContext? conversationContext)
    {
        if (conversationContext is null || conversationContext.Turns.Count == 0)
            return;

        foreach (string key in ExtractPreviousDraftFieldKeys(conversationContext))
        {
            if (!fieldProvider.TryGetField(sectionKind, key, out Ra2FieldDefinition definition))
                continue;

            AddOrUpdate(
                candidates,
                definition,
                score: 820,
                matchReason: "previous assistant draft field key");
        }
    }

    private static IReadOnlyList<string> ExtractPreviousDraftFieldKeys(Ra2AiConversationContext conversationContext)
    {
        HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
        foreach (Ra2AiConversationTurn turn in conversationContext.Turns.Where(static turn =>
            turn.Role == Ra2AiConversationRole.Assistant && turn.IsDraftResponse))
        {
            foreach (string key in SearchTerms.ExtractIniKeys(turn.Text))
                keys.Add(key);
        }

        return keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<ProfileMatch> SelectEvidenceProfiles(string? promptText, Ra2AiCurrentSubject? currentSubject)
    {
        List<ProfileMatch> profiles = [];
        AddSubjectKindProfiles(profiles, currentSubject);
        AddFollowUpIntentProfiles(profiles, promptText);

        if (string.IsNullOrWhiteSpace(promptText) || !IsDraftLikePrompt(promptText))
            return DeduplicateProfiles(profiles);

        profiles.Add(new(UnitCoreProfile, 220));

        if (ContainsAny(promptText, "vehicle", "tank", "car", "ifv", "apc", "\u8f7d\u5177", "\u8f66\u8f86", "\u8f66", "\u5766\u514b", "\u6218\u8f66", "\u88c5\u7532"))
            profiles.Add(new(VehicleCoreProfile, 260));

        if (ContainsAny(promptText, "infantry", "soldier", "\u6b65\u5175", "\u58eb\u5175", "\u5355\u4f4d\u5175"))
            profiles.Add(new(InfantryCoreProfile, 260));

        if (ContainsAny(promptText, "building", "structure", "base defense", "\u70ae\u5854", "\u9632\u5fa1\u5854", "\u5efa\u7b51", "\u5efa\u7b51\u7269"))
            profiles.Add(new(BuildingCoreProfile, 260));

        if (ContainsAny(promptText, "weapon", "missile", "\u70ae", "\u6b66\u5668", "\u5bfc\u5f39", "\u673a\u70ae", "\u706b\u70ae"))
            profiles.Add(new(WeaponCoreProfile, 250));

        if (ContainsAny(promptText, "projectile", "missile", "rocket", "\u629b\u5c04\u4f53", "\u5f39\u4f53", "\u5bfc\u5f39", "\u706b\u7bad"))
            profiles.Add(new(ProjectileCoreProfile, 245));

        if (ContainsAny(promptText, "warhead", "damage type", "\u5f39\u5934", "\u4f24\u5bb3\u7c7b\u578b"))
            profiles.Add(new(WarheadCoreProfile, 245));

        if (ContainsAny(promptText, "anti air", "anti-air", "antiair", "aa", "air defense", "aircraft", "\u9632\u7a7a", "\u5bf9\u7a7a", "\u98de\u673a", "\u7a7a\u519b"))
            profiles.Add(new(AntiAirWeaponProfile, 300));

        if (ContainsAny(promptText, "ground attack", "anti ground", "\u5bf9\u5730", "\u5730\u9762\u653b\u51fb"))
            profiles.Add(new(GroundAttackWeaponProfile, 285));

        if (ContainsAny(promptText, "deploy", "deployer", "transform", "\u90e8\u7f72", "\u5c55\u5f00", "\u53d8\u5f62", "\u5c55\u5f00\u540e", "\u90e8\u7f72\u540e"))
            profiles.Add(new(DeployTransformProfile, 300));

        if (ContainsAny(promptText, "transport", "passenger", "passengers", "\u8f7d\u5458", "\u8fd0\u8f93", "\u8fd0\u5175", "\u4e58\u5458"))
            profiles.Add(new(TransportProfile, 300));

        if (ContainsAny(promptText, "stealth", "cloak", "cloaking", "scout", "\u9690\u5f62", "\u6f5c\u884c", "\u4fa6\u5bdf", "\u65a5\u5019"))
            profiles.Add(new(StealthScoutProfile, 300));

        if (ContainsAny(promptText, "sensor", "detector", "radar", "detect", "sight", "\u4f20\u611f\u5668", "\u4fa6\u6d4b", "\u63a2\u6d4b", "\u96f7\u8fbe", "\u89c6\u91ce"))
            profiles.Add(new(SensorProfile, 295));

        if (ContainsAny(promptText, "repair", "regeneration", "self repair", "\u7ef4\u4fee", "\u81ea\u4fee", "\u81ea\u6211\u4fee\u590d", "\u6062\u590d"))
            profiles.Add(new(SelfRepairProfile, 290));

        if (ContainsAny(promptText, "garrison", "passenger", "passengers", "\u9a7b\u519b", "\u4e58\u5ba2", "\u8f7d\u5458"))
            profiles.Add(new(GarrisonPassengerProfile, 285));

        if (ContainsAny(promptText, "build limit", "prerequisite", "tech", "\u5efa\u9020\u9650\u5236", "\u524d\u7f6e", "\u79d1\u6280", "\u79d1\u6280\u7b49\u7ea7"))
            profiles.Add(new(BuildLimitTechPrerequisiteProfile, 285));

        if (ContainsAny(promptText, "veteran", "veterancy", "elite", "\u8001\u5175", "\u5347\u7ea7", "\u7cbe\u82f1"))
            profiles.Add(new(VeterancyProfile, 280));

        if (ContainsAny(promptText, "voxel", "image", "cameo", "art", "vxl", "\u56fe\u50cf", "\u7d20\u6750", "\u56fe\u6807", "\u6a21\u578b"))
            profiles.Add(new(ArtVoxelProfile, 275));

        if (ContainsAny(promptText, "shp", "image", "cameo", "art", "\u6b65\u5175\u56fe\u50cf", "\u5efa\u7b51\u56fe\u50cf"))
            profiles.Add(new(ArtShpProfile, 275));

        return DeduplicateProfiles(profiles);
    }
    private static void AddSubjectKindProfiles(List<ProfileMatch> profiles, Ra2AiCurrentSubject? currentSubject)
    {
        if (currentSubject is null)
            return;

        switch (currentSubject.Kind)
        {
            case Ra2AiSubjectKind.Unit:
                profiles.Add(new(UnitCoreProfile, 245));
                profiles.Add(new(VehicleCoreProfile, 240));
                break;
            case Ra2AiSubjectKind.Weapon:
                profiles.Add(new(WeaponCoreProfile, 245));
                profiles.Add(new(ProjectileCoreProfile, 220));
                profiles.Add(new(WarheadCoreProfile, 220));
                break;
            case Ra2AiSubjectKind.Projectile:
                profiles.Add(new(ProjectileCoreProfile, 245));
                break;
            case Ra2AiSubjectKind.Warhead:
                profiles.Add(new(WarheadCoreProfile, 245));
                break;
            case Ra2AiSubjectKind.Art:
                profiles.Add(new(ArtVoxelProfile, 235));
                profiles.Add(new(ArtShpProfile, 230));
                break;
        }
    }

    private static void AddFollowUpIntentProfiles(List<ProfileMatch> profiles, string? promptText)
    {
        if (string.IsNullOrWhiteSpace(promptText))
            return;

        if (ContainsAny(promptText, "allied", "soviet", "yuri", "faction", "country", "owner", "requiredhouses", "forbiddenhouses", "\u76df\u519b", "\u82cf\u519b", "\u5c24\u91cc", "\u9635\u8425", "\u56fd\u5bb6", "\u6240\u5c5e"))
            profiles.Add(new(FactionOwnerProfile, 760));

        if (ContainsAny(promptText, "anti air", "anti-air", "antiair", "aa", "air defense", "aircraft", "missile", "weapon", "\u9632\u7a7a", "\u5bf9\u7a7a", "\u98de\u673a", "\u7a7a\u519b", "\u5bfc\u5f39", "\u6b66\u5668"))
            profiles.Add(new(FollowUpAntiAirWeaponProfile, 755));

        if (ContainsAny(promptText, "deploy", "deployer", "transform", "\u90e8\u7f72", "\u5c55\u5f00", "\u53d8\u5f62"))
            profiles.Add(new(DeployTransformProfile, 750));

        if (ContainsAny(promptText, "transport", "passenger", "passengers", "\u8fd0\u8f93", "\u8f7d\u4eba", "\u4e58\u5ba2"))
            profiles.Add(new(FollowUpTransportProfile, 750));

        if (ContainsAny(promptText, "stealth", "cloak", "cloaking", "scout", "sensor", "detector", "radar", "detect", "\u9690\u5f62", "\u4fa6\u5bdf", "\u6f5c\u884c", "\u63a2\u6d4b", "\u96f7\u8fbe"))
            profiles.Add(new(FollowUpStealthScoutProfile, 750));

    }

    private static IReadOnlyList<ProfileMatch> DeduplicateProfiles(IEnumerable<ProfileMatch> profiles)
    {
        return profiles
            .GroupBy(profile => profile.Profile.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(profile => profile.Score).First())
            .OrderByDescending(profile => profile.Score)
            .ThenBy(profile => profile.Profile.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsDraftLikePrompt(string promptText)
        => ContainsAny(
            promptText,
            "generate",
            "draft",
            "prototype",
            "design",
            "make",
            "create",
            "unit",
            "vehicle",
            "weapon",
            "设计",
            "生成",
            "草稿",
            "原型",
            "配置",
            "单位",
            "载具",
            "车辆",
            "战车",
            "坦克",
            "步兵",
            "建筑",
            "武器",
            "炮",
            "车");

    private static bool ContainsAny(string text, params string[] keywords)
    {
        foreach (string keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void AddOrUpdate(
        Dictionary<string, Candidate> candidates,
        Ra2FieldDefinition definition,
        double score,
        string matchReason)
    {
        Candidate candidate = new(definition, score, matchReason);
        if (!candidates.TryGetValue(definition.Key, out Candidate? existing) ||
            candidate.Score > existing.Score)
        {
            candidates[definition.Key] = candidate;
        }
    }

    private static IReadOnlyList<Ra2AiFieldEvidence> BuildEvidence(
        IEnumerable<Candidate> candidates,
        IFieldRegistryProvenanceProvider? provenanceProvider,
        Ra2SectionKind sectionKind,
        int maxCount)
    {
        return Array.AsReadOnly(candidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Definition.Key, StringComparer.OrdinalIgnoreCase)
            .Take(maxCount)
            .Select(candidate => CreateEvidence(candidate, provenanceProvider, sectionKind))
            .ToArray());
    }

    private static Ra2AiFieldEvidence CreateEvidence(
        Candidate candidate,
        IFieldRegistryProvenanceProvider? provenanceProvider,
        Ra2SectionKind sectionKind)
    {
        Ra2FieldDefinition definition = candidate.Definition;
        FieldRegistryProvenanceLookupResult? provenance = provenanceProvider?.TryGetFieldWithProvenance(sectionKind, definition.Key);
        string? sourceName = provenance is { Found: true }
            ? provenance.SourceName
            : definition.SourceKind.ToString();
        string? provenanceText = provenance is { Found: true }
            ? provenance.Scope.ToString()
            : null;

        string? sectionKindText = definition.AppliesTo.Count == 0
            ? null
            : string.Join(", ", definition.AppliesTo.Select(kind => kind.ToString()).OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
        string? example = definition.Examples.Count > 0
            ? definition.Examples[0].Value
            : definition.ValueMetadata.AllowedValues.FirstOrDefault()?.Value;

        return new Ra2AiFieldEvidence(
            definition.Key,
            definition.DisplayName,
            sectionKindText,
            definition.ValueMetadata.ValueKind.ToString(),
            definition.Description,
            example,
            sourceName,
            provenanceText,
            candidate.MatchReason,
            candidate.Score);
    }

    private sealed record Candidate(Ra2FieldDefinition Definition, double Score, string MatchReason);

    private sealed record DraftEvidenceProfile(string Name, IReadOnlyList<string> Keys);

    private sealed record ProfileMatch(DraftEvidenceProfile Profile, double Score);

    private static readonly DraftEvidenceProfile UnitCoreProfile = new(
        "UnitCore",
        [
            "Name",
            "UIName",
            "Image",
            "Prerequisite",
            "Primary",
            "Secondary",
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
            "ThreatPosed"
        ]);

    private static readonly DraftEvidenceProfile VehicleCoreProfile = new(
        "VehicleCore",
        [
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
            "Tracked"
        ]);

    private static readonly DraftEvidenceProfile InfantryCoreProfile = new(
        "InfantryCore",
        [
            "OccupyWeapon",
            "EliteOccupyWeapon",
            "Occupier",
            "Crawls",
            "Fraidycat",
            "Civilian",
            "Pip",
            "PhysicalSize"
        ]);

    private static readonly DraftEvidenceProfile BuildingCoreProfile = new(
        "BuildingCore",
        [
            "Power",
            "Powered",
            "BaseNormal",
            "Capturable",
            "Unsellable",
            "ClickRepairable",
            "Adjacent",
            "Bib"
        ]);

    private static readonly DraftEvidenceProfile WeaponCoreProfile = new(
        "WeaponCore",
        [
            "Damage",
            "ROF",
            "Range",
            "Projectile",
            "Speed",
            "Warhead",
            "Report",
            "Anim",
            "Burst"
        ]);

    private static readonly DraftEvidenceProfile ProjectileCoreProfile = new(
        "ProjectileCore",
        [
            "AA",
            "AG",
            "Arm",
            "Shadow",
            "Proximity",
            "Ranged",
            "Image",
            "Rotates",
            "Inviso",
            "SubjectToCliffs",
            "SubjectToElevation",
            "SubjectToWalls"
        ]);

    private static readonly DraftEvidenceProfile WarheadCoreProfile = new(
        "WarheadCore",
        [
            "Verses",
            "CellSpread",
            "PercentAtMax",
            "InfDeath",
            "Wall",
            "Wood",
            "Conventional",
            "ProneDamage"
        ]);

    private static readonly DraftEvidenceProfile AntiAirWeaponProfile = new(
        "AntiAirWeapon",
        [
            "Primary",
            "Secondary",
            "ElitePrimary",
            "EliteSecondary",
            "Projectile",
            "Warhead",
            "AA",
            "AG",
            "Range",
            "GuardRange"
        ]);

    private static readonly DraftEvidenceProfile FactionOwnerProfile = new(
        "FactionOwner",
        [
            "Owner",
            "RequiredHouses",
            "ForbiddenHouses",
            "Prerequisite",
            "UIName",
            "Name",
            "Image"
        ]);

    private static readonly DraftEvidenceProfile FollowUpAntiAirWeaponProfile = new(
        "FollowUpAntiAirWeapon",
        [
            "Primary",
            "Secondary",
            "ElitePrimary",
            "EliteSecondary",
            "Damage",
            "ROF",
            "Range",
            "Projectile",
            "Warhead",
            "AA",
            "AG",
            "Verses"
        ]);

    private static readonly DraftEvidenceProfile GroundAttackWeaponProfile = new(
        "GroundAttackWeapon",
        [
            "Primary",
            "Secondary",
            "Projectile",
            "Warhead",
            "AG",
            "Range",
            "MinimumRange"
        ]);

    private static readonly DraftEvidenceProfile DeployTransformProfile = new(
        "DeployTransform",
        [
            "DeploysInto",
            "UndeploysInto",
            "DeployToFire",
            "DeployFire",
            "IsSimpleDeployer",
            "Deployer",
            "DeployTime"
        ]);

    private static readonly DraftEvidenceProfile TransportProfile = new(
        "Transport",
        [
            "Passengers",
            "PipScale",
            "OpenTopped",
            "SizeLimit",
            "EnterTransportSound",
            "LeaveTransportSound"
        ]);

    private static readonly DraftEvidenceProfile FollowUpTransportProfile = new(
        "FollowUpTransport",
        [
            "Passengers",
            "PipScale",
            "OpenTopped",
            "SizeLimit"
        ]);

    private static readonly DraftEvidenceProfile StealthScoutProfile = new(
        "StealthScout",
        [
            "Cloakable",
            "CloakingSpeed",
            "Sensors",
            "SensorsSight",
            "DetectDisguise",
            "DefaultToGuardArea",
            "Sight",
            "Speed"
        ]);

    private static readonly DraftEvidenceProfile FollowUpStealthScoutProfile = new(
        "FollowUpStealthScout",
        [
            "Cloakable",
            "CloakingSpeed",
            "Sensors",
            "SensorsSight",
            "DetectDisguise",
            "Sight",
            "Speed"
        ]);

    private static readonly DraftEvidenceProfile SensorProfile = new(
        "Sensor",
        [
            "Sensors",
            "SensorsSight",
            "DetectDisguise",
            "DetectDisguiseRange",
            "PsychicDetectionRadius",
            "Sight"
        ]);

    private static readonly DraftEvidenceProfile SelfRepairProfile = new(
        "SelfRepair",
        [
            "SelfHealing",
            "Repairable",
            "ClickRepairable",
            "RepairRate",
            "RepairStep"
        ]);

    private static readonly DraftEvidenceProfile GarrisonPassengerProfile = new(
        "GarrisonPassenger",
        [
            "Passengers",
            "SizeLimit",
            "PipScale",
            "OpenTopped",
            "Occupier",
            "CanBeOccupied",
            "MaxNumberOccupants"
        ]);

    private static readonly DraftEvidenceProfile BuildLimitTechPrerequisiteProfile = new(
        "BuildLimitTechPrerequisite",
        [
            "BuildLimit",
            "Prerequisite",
            "PrerequisiteOverride",
            "TechLevel",
            "Owner",
            "RequiredHouses",
            "ForbiddenHouses"
        ]);

    private static readonly DraftEvidenceProfile VeterancyProfile = new(
        "Veterancy",
        [
            "Trainable",
            "VeteranAbilities",
            "EliteAbilities",
            "ElitePrimary",
            "EliteSecondary"
        ]);

    private static readonly DraftEvidenceProfile ArtVoxelProfile = new(
        "ArtVoxel",
        [
            "Image",
            "Voxel",
            "VoxelBarrel",
            "Remapable",
            "Cameo",
            "AltCameo",
            "TurretOffset"
        ]);

    private static readonly DraftEvidenceProfile ArtShpProfile = new(
        "ArtSHP",
        [
            "Image",
            "Cameo",
            "AltCameo",
            "Sequence",
            "Remapable",
            "Foundation"
        ]);

    private sealed class SearchTerms
    {
        private SearchTerms(IReadOnlySet<string> keys, IReadOnlySet<string> all)
        {
            Keys = keys;
            All = all;
        }

        public IReadOnlySet<string> Keys { get; }

        public IReadOnlySet<string> All { get; }

        public bool HasAny => Keys.Count > 0 || All.Count > 0;

        public static SearchTerms FromPromptText(string? text)
            => new(new HashSet<string>(StringComparer.OrdinalIgnoreCase), ExtractTerms(text));

        public static SearchTerms FromSelectedText(string? text)
            => new(ExtractIniKeys(text), ExtractTerms(text));

        public static IReadOnlySet<string> ExtractIniKeys(string? text)
        {
            HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(text))
                return keys;

            foreach (string rawLine in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('['))
                    continue;

                int equalsIndex = line.IndexOf('=');
                if (equalsIndex <= 0)
                    continue;

                string key = line[..equalsIndex].Trim();
                if (!string.IsNullOrWhiteSpace(key))
                    keys.Add(key);
            }

            return keys;
        }

        private static IReadOnlySet<string> ExtractTerms(string? text)
        {
            HashSet<string> terms = new(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(text))
                return terms;

            int start = -1;
            for (int i = 0; i <= text.Length; i++)
            {
                bool isTermChar = i < text.Length &&
                    (char.IsLetterOrDigit(text[i]) || text[i] is '_' or '-' or '.');
                if (isTermChar)
                {
                    if (start < 0)
                        start = i;

                    continue;
                }

                if (start < 0)
                    continue;

                string term = text[start..i].Trim();
                if (term.Length >= 2)
                    terms.Add(term);

                start = -1;
            }

            return terms;
        }
    }
}
