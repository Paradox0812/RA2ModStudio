using System.Globalization;
using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Application.Language;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation;

internal enum Ra2SuperWeaponEffectReferencePolicy
{
    ExistingTechnoList = 0,
    ExistingWarhead
}

internal sealed record Ra2SuperWeaponProfileDefinition(
    string ProfileId,
    int Version,
    string EngineFamily,
    string SuperWeaponType,
    Ra2SuperWeaponEffectReferencePolicy EffectReferencePolicy,
    IReadOnlyList<string> CompatibleAiTargetingValues);

internal sealed class Ra2SuperWeaponProfilePreparationResult
{
    private Ra2SuperWeaponProfilePreparationResult(
        Ra2ContentTemplateDefinition? definition,
        IReadOnlyList<KeyValuePair<string, string>> arguments,
        Ra2AutomationTemplateExpansionFailureKind failureKind,
        string message)
    {
        Definition = definition;
        Arguments = arguments;
        FailureKind = failureKind;
        Message = message;
    }

    public bool Succeeded => Definition is not null;
    public Ra2ContentTemplateDefinition? Definition { get; }
    public IReadOnlyList<KeyValuePair<string, string>> Arguments { get; }
    public Ra2AutomationTemplateExpansionFailureKind FailureKind { get; }
    public string Message { get; }

    public static Ra2SuperWeaponProfilePreparationResult Success(
        Ra2ContentTemplateDefinition definition,
        IReadOnlyList<KeyValuePair<string, string>> arguments)
        => new(definition, arguments, Ra2AutomationTemplateExpansionFailureKind.None, "SuperWeapon profile prepared.");

    public static Ra2SuperWeaponProfilePreparationResult Failure(
        Ra2AutomationTemplateExpansionFailureKind kind,
        string message)
        => new(null, [], kind, message);
}

/// <summary>来源冻结、进程生命周期且不参与序列化的 SuperWeapon typed profile catalog。</summary>
internal static class Ra2SuperWeaponProfileCatalog
{
    internal const string UnitDeliveryTemplateId = "ares-unitdelivery-superweapon-complete";
    internal const string GenericWarheadTemplateId = "ares-genericwarhead-superweapon-complete";
    internal const int CurrentVersion = 1;

    private static readonly Ra2SuperWeaponProfileDefinition UnitDelivery = new(
        UnitDeliveryTemplateId,
        CurrentVersion,
        "Ares3",
        "UnitDelivery",
        Ra2SuperWeaponEffectReferencePolicy.ExistingTechnoList,
        Array.AsReadOnly(["ParaDrop", "None"]));

    private static readonly Ra2SuperWeaponProfileDefinition GenericWarhead = new(
        GenericWarheadTemplateId,
        CurrentVersion,
        "Ares3",
        "GenericWarhead",
        Ra2SuperWeaponEffectReferencePolicy.ExistingWarhead,
        Array.AsReadOnly(["Offensive", "None"]));

    private static readonly IReadOnlyDictionary<string, Ra2SuperWeaponProfileDefinition> Profiles =
        new Dictionary<string, Ra2SuperWeaponProfileDefinition>(StringComparer.Ordinal)
        {
            [UnitDelivery.ProfileId] = UnitDelivery,
            [GenericWarhead.ProfileId] = GenericWarhead
        };

    internal static bool IsProfile(string templateId) => Profiles.ContainsKey(templateId);

    internal static IReadOnlyList<Ra2AutomationTemplateDescriptor> CreateDescriptors()
        => Array.AsReadOnly<Ra2AutomationTemplateDescriptor>(
        [
            CreateDescriptor(UnitDelivery, "Ares UnitDelivery 支援能力", "注册并创建引用既存 TechnoTypes 的完整 UnitDelivery SuperWeapon。"),
            CreateDescriptor(GenericWarhead, "Ares GenericWarhead 超武", "注册并创建引用既存 Warhead 的完整 GenericWarhead SuperWeapon。")
        ]);

    internal static Ra2SuperWeaponProfilePreparationResult Prepare(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationTemplateExpansionRequest request,
        IRa2AutomationDocumentQueryService queryService,
        CancellationToken cancellationToken)
    {
        if (!Profiles.TryGetValue(request.TemplateId, out Ra2SuperWeaponProfileDefinition? profile))
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.TemplateNotFound, "SuperWeapon profile was not found.");
        if (request.TemplateVersion != profile.Version)
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.TemplateVersionMismatch, "SuperWeapon profile version is not available.");

        Dictionary<string, string> values = new(StringComparer.Ordinal);
        foreach (Ra2AutomationTemplateArgument argument in request.Arguments)
        {
            if (!values.TryAdd(argument.Name, argument.Value))
                return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument, $"Argument '{argument.Name}' was supplied more than once.");
        }

        string[] commonRequired =
        [
            "superWeaponId", "providerMode", "uiName", "name", "isPowered", "rechargeTime",
            "action", "sidebarImage", "showTimer", "disableableFromShell", "aiTargeting"
        ];
        string[] effectRequired = profile.EffectReferencePolicy == Ra2SuperWeaponEffectReferencePolicy.ExistingTechnoList
            ? ["deliveryTypeIds", "deliveryOwner"]
            : ["warheadId", "damage"];
        HashSet<string> allowed = commonRequired
            .Concat(effectRequired)
            .Concat(["providerBuildingId", "providerSlot"])
            .ToHashSet(StringComparer.Ordinal);
        if (values.Keys.Any(key => !allowed.Contains(key)))
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.UnknownArgument, "SuperWeapon profile contains an unknown argument.");
        string? missing = commonRequired.Concat(effectRequired).FirstOrDefault(name => !values.ContainsKey(name));
        if (missing is not null)
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.MissingRequiredArgument, $"Required argument '{missing}' was not supplied.");

        Lazy<Ra2DocumentSemanticModel> objectModel = new(() =>
            new Ra2DocumentSemanticModelBuilder().Build(
                new Ra2DocumentSnapshot(snapshot.FilePath, snapshot.Text, snapshot.Version),
                snapshot.FieldRegistry.Provider));
        if (values.TryGetValue("providerBuildingId", out string? providerAlias))
        {
            values["providerBuildingId"] = ResolveExistingSectionIdentity(
                objectModel,
                providerAlias,
                kind => kind == Ra2SectionKind.Building,
                cancellationToken);
        }
        if (profile.EffectReferencePolicy == Ra2SuperWeaponEffectReferencePolicy.ExistingTechnoList)
        {
            values["deliveryTypeIds"] = string.Join(',', values["deliveryTypeIds"]
                .Split(',', StringSplitOptions.TrimEntries)
                .Select(alias => ResolveExistingSectionIdentity(
                    objectModel,
                    alias,
                    IsTechnoKind,
                    cancellationToken)));
        }
        else
        {
            values["warheadId"] = ResolveExistingSectionIdentity(
                objectModel,
                values["warheadId"],
                kind => kind == Ra2SectionKind.Warhead,
                cancellationToken);
        }

        if (!Ra2ContentTemplateValidation.IsValidIdentifier(values["superWeaponId"]) ||
            !Ra2ContentTemplateValidation.IsValidIdentifier(values["sidebarImage"]) ||
            string.IsNullOrWhiteSpace(values["uiName"]) ||
            string.IsNullOrWhiteSpace(values["name"]) ||
            !Ra2ContentTemplateValidation.IsValidBoundedValue(values["uiName"]) ||
            !Ra2ContentTemplateValidation.IsValidBoundedValue(values["name"]) ||
            !Ra2ContentTemplateValidation.IsValidBoundedValue(values["action"]) ||
            string.IsNullOrWhiteSpace(values["action"]) ||
            !double.TryParse(values["rechargeTime"], NumberStyles.Float, CultureInfo.InvariantCulture, out double rechargeTime) ||
            !double.IsFinite(rechargeTime) || rechargeTime < 0 ||
            !IsBoolean(values["isPowered"]) || !IsBoolean(values["showTimer"]) || !IsBoolean(values["disableableFromShell"]) ||
            !profile.CompatibleAiTargetingValues.Contains(values["aiTargeting"], StringComparer.OrdinalIgnoreCase))
        {
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "SuperWeapon common arguments are invalid or incompatible with the selected profile.");
        }

        bool buildingProvider = string.Equals(values["providerMode"], "building", StringComparison.OrdinalIgnoreCase);
        bool alwaysGranted = string.Equals(values["providerMode"], "always-granted", StringComparison.OrdinalIgnoreCase);
        if (!buildingProvider && !alwaysGranted)
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "providerMode must be building or always-granted.");

        if (buildingProvider)
        {
            if (!values.TryGetValue("providerBuildingId", out string? providerId) ||
                !Ra2ContentTemplateValidation.IsValidIdentifier(providerId) ||
                !values.TryGetValue("providerSlot", out string? providerSlot) ||
                providerSlot is not ("SuperWeapon" or "SuperWeapon2"))
            {
                return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "Building provider requires a valid providerBuildingId and SuperWeapon or SuperWeapon2 slot.");
            }

            Ra2SuperWeaponProfilePreparationResult? providerFailure = ValidateReference(
                snapshot,
                queryService,
                providerId,
                kind => kind == Ra2SectionKind.Building,
                "provider Building",
                cancellationToken);
            if (providerFailure is not null)
                return providerFailure;
        }
        else if (values.ContainsKey("providerBuildingId") || values.ContainsKey("providerSlot"))
        {
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "AlwaysGranted profile cannot also declare a provider Building or slot.");
        }

        if (profile.EffectReferencePolicy == Ra2SuperWeaponEffectReferencePolicy.ExistingTechnoList)
        {
            string[] ids = values["deliveryTypeIds"].Split(',', StringSplitOptions.TrimEntries);
            if (ids.Length is < 1 or > 16 || ids.Any(id => !Ra2ContentTemplateValidation.IsValidIdentifier(id)) ||
                ids.Distinct(StringComparer.OrdinalIgnoreCase).Count() != ids.Length)
            {
                return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "deliveryTypeIds must contain 1..16 unique canonical identifiers.");
            }
            if (!new[] { "invoker", "neutral", "special", "civilian" }.Contains(values["deliveryOwner"], StringComparer.OrdinalIgnoreCase))
                return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "deliveryOwner is not an Ares UnitDelivery owner value.");

            foreach (string id in ids)
            {
                Ra2SuperWeaponProfilePreparationResult? referenceFailure = ValidateReference(
                    snapshot,
                    queryService,
                    id,
                    kind => kind is Ra2SectionKind.Techno or Ra2SectionKind.Infantry or Ra2SectionKind.Unit or Ra2SectionKind.Vehicle or Ra2SectionKind.Aircraft or Ra2SectionKind.Building,
                    "delivery TechnoType",
                    cancellationToken);
                if (referenceFailure is not null)
                    return referenceFailure;
            }

            values["deliveryTypeIds"] = string.Join(',', ids);
            values["deliveryOwner"] = values["deliveryOwner"].ToLowerInvariant();
        }
        else
        {
            if (!Ra2ContentTemplateValidation.IsValidIdentifier(values["warheadId"]) ||
                !int.TryParse(values["damage"], NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            {
                return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "GenericWarhead requires a valid existing warheadId and integer damage.");
            }

            Ra2SuperWeaponProfilePreparationResult? referenceFailure = ValidateReference(
                snapshot,
                queryService,
                values["warheadId"],
                kind => kind == Ra2SectionKind.Warhead,
                "Warhead",
                cancellationToken);
            if (referenceFailure is not null)
                return referenceFailure;
        }

        values["providerMode"] = buildingProvider ? "building" : "always-granted";
        values["aiTargeting"] = profile.CompatibleAiTargetingValues.First(value =>
            string.Equals(value, values["aiTargeting"], StringComparison.OrdinalIgnoreCase));
        Ra2ContentTemplateDefinition definition = CreateDefinition(profile, values, buildingProvider);
        KeyValuePair<string, string>[] normalizedArguments = definition.Parameters
            .Where(parameter => values.ContainsKey(parameter.Name))
            .Select(parameter => new KeyValuePair<string, string>(parameter.Name, values[parameter.Name]))
            .ToArray();
        return Ra2SuperWeaponProfilePreparationResult.Success(definition, normalizedArguments);
    }

    private static Ra2AutomationTemplateDescriptor CreateDescriptor(
        Ra2SuperWeaponProfileDefinition profile,
        string displayName,
        string summary)
    {
        List<Ra2AutomationTemplateParameterDescriptor> parameters =
        [
            new("superWeaponId", Ra2AutomationTemplateParameterKind.Identifier, true, null),
            new("providerMode", Ra2AutomationTemplateParameterKind.String, true, null),
            new("providerBuildingId", Ra2AutomationTemplateParameterKind.Identifier, false, null),
            new("providerSlot", Ra2AutomationTemplateParameterKind.String, false, null),
            new("uiName", Ra2AutomationTemplateParameterKind.String, true, null),
            new("name", Ra2AutomationTemplateParameterKind.String, true, null),
            new("isPowered", Ra2AutomationTemplateParameterKind.Boolean, true, null),
            new("rechargeTime", Ra2AutomationTemplateParameterKind.Float, true, null),
            new("action", Ra2AutomationTemplateParameterKind.String, true, null),
            new("sidebarImage", Ra2AutomationTemplateParameterKind.Reference, true, null),
            new("showTimer", Ra2AutomationTemplateParameterKind.Boolean, true, null),
            new("disableableFromShell", Ra2AutomationTemplateParameterKind.Boolean, true, null),
            new("aiTargeting", Ra2AutomationTemplateParameterKind.String, true, null)
        ];
        if (profile.EffectReferencePolicy == Ra2SuperWeaponEffectReferencePolicy.ExistingTechnoList)
        {
            parameters.Add(new("deliveryTypeIds", Ra2AutomationTemplateParameterKind.String, true, null));
            parameters.Add(new("deliveryOwner", Ra2AutomationTemplateParameterKind.String, true, null));
        }
        else
        {
            parameters.Add(new("warheadId", Ra2AutomationTemplateParameterKind.Reference, true, null));
            parameters.Add(new("damage", Ra2AutomationTemplateParameterKind.Integer, true, null));
        }

        return new Ra2AutomationTemplateDescriptor(
            profile.ProfileId,
            profile.Version,
            displayName,
            summary,
            Ra2AutomationTemplateOutputKind.CompleteObject,
            parameters,
            isProjectTemplate: false,
            producesAssetManifest: false);
    }

    private static Ra2ContentTemplateDefinition CreateDefinition(
        Ra2SuperWeaponProfileDefinition profile,
        IReadOnlyDictionary<string, string> values,
        bool buildingProvider)
    {
        List<Ra2ContentTemplateParameter> parameters = values.Keys
            .Select(name => new Ra2ContentTemplateParameter(
                name,
                name == "damage" ? Ra2ContentTemplateParameterKind.Integer :
                name == "rechargeTime" ? Ra2ContentTemplateParameterKind.Float :
                name is "isPowered" or "showTimer" or "disableableFromShell" ? Ra2ContentTemplateParameterKind.Boolean :
                name is "superWeaponId" or "providerBuildingId" or "sidebarImage" or "warheadId" ? Ra2ContentTemplateParameterKind.Identifier :
                Ra2ContentTemplateParameterKind.String,
                required: true))
            .ToList();

        List<Ra2ContentTemplateFieldSpec> fields =
        [
            SourceField("Type", Ra2ContentTemplateValueSource.Literal(profile.SuperWeaponType)),
            SourceField("Action", Ra2ContentTemplateValueSource.Parameter("action")),
            SourceField("UIName", Ra2ContentTemplateValueSource.Parameter("uiName")),
            SourceField("Name", Ra2ContentTemplateValueSource.Parameter("name")),
            SourceField("IsPowered", Ra2ContentTemplateValueSource.Parameter("isPowered")),
            SourceField("RechargeTime", Ra2ContentTemplateValueSource.Parameter("rechargeTime")),
            SourceField("SidebarImage", Ra2ContentTemplateValueSource.Parameter("sidebarImage")),
            SourceField("ShowTimer", Ra2ContentTemplateValueSource.Parameter("showTimer")),
            SourceField("DisableableFromShell", Ra2ContentTemplateValueSource.Parameter("disableableFromShell")),
            SourceField("SW.AITargeting", Ra2ContentTemplateValueSource.Parameter("aiTargeting"))
        ];
        if (profile.EffectReferencePolicy == Ra2SuperWeaponEffectReferencePolicy.ExistingTechnoList)
        {
            fields.Add(SourceField("Deliver.Types", Ra2ContentTemplateValueSource.Parameter("deliveryTypeIds")));
            fields.Add(SourceField("Deliver.Owner", Ra2ContentTemplateValueSource.Parameter("deliveryOwner")));
        }
        else
        {
            fields.Add(SourceField("SW.Damage", Ra2ContentTemplateValueSource.Parameter("damage")));
            fields.Add(SourceField("SW.Warhead", Ra2ContentTemplateValueSource.Parameter("warheadId")));
        }
        if (!buildingProvider)
            fields.Add(SourceField("SW.AlwaysGranted", Ra2ContentTemplateValueSource.Literal("yes")));

        List<Ra2ContentTemplateSectionSpec> sections =
        [
            new(
                Ra2ContentTemplateValueSource.Parameter("superWeaponId"),
                Ra2SectionKind.SuperWeapon,
                fields)
        ];
        if (buildingProvider)
        {
            sections.Insert(0, new Ra2ContentTemplateSectionSpec(
                Ra2ContentTemplateValueSource.Parameter("providerBuildingId"),
                Ra2SectionKind.Building,
                [SourceField(values["providerSlot"], Ra2ContentTemplateValueSource.Parameter("superWeaponId"))],
                Ra2ContentTemplateSectionTargetMode.RequireExisting));
        }

        return new Ra2ContentTemplateDefinition(
            profile.ProfileId,
            profile.Version,
            $"Complete Ares {profile.SuperWeaponType} SuperWeapon",
            parameters,
            sections,
            [new Ra2ContentTemplateRegistrationSpec(
                "SuperWeaponTypes",
                Ra2ContentTemplateValueSource.Parameter("superWeaponId"),
                Ra2SectionKind.SuperWeapon)]);
    }

    private static Ra2ContentTemplateFieldSpec SourceField(string key, Ra2ContentTemplateValueSource value)
        => new(key, value, Ra2ContentTemplateFieldValidationPolicy.SourceBounded);

    private static Ra2SuperWeaponProfilePreparationResult? ValidateReference(
        Ra2AutomationDocumentSnapshot snapshot,
        IRa2AutomationDocumentQueryService queryService,
        string sectionName,
        Func<Ra2SectionKind, bool> accepts,
        string displayName,
        CancellationToken cancellationToken)
    {
        Ra2AutomationSectionQueryResult query = queryService.GetSection(
            snapshot,
            new Ra2AutomationSectionQuery(sectionName),
            cancellationToken);
        if (!query.Succeeded || query.Section is null)
        {
            Ra2AutomationTemplateExpansionFailureKind kind = query.FailureKind switch
            {
                Ra2AutomationSectionQueryFailureKind.Canceled => Ra2AutomationTemplateExpansionFailureKind.Canceled,
                Ra2AutomationSectionQueryFailureKind.DocumentTooLarge => Ra2AutomationTemplateExpansionFailureKind.DocumentTooLarge,
                _ => Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound
            };
            return Ra2SuperWeaponProfilePreparationResult.Failure(kind, $"Required {displayName} section '{sectionName}' was not found uniquely.");
        }
        if (!accepts(query.Section.Kind))
            return Ra2SuperWeaponProfilePreparationResult.Failure(Ra2AutomationTemplateExpansionFailureKind.RequiredSectionKindMismatch, $"Section '{sectionName}' is not a compatible {displayName}.");
        return null;
    }

    private static string ResolveExistingSectionIdentity(
        Lazy<Ra2DocumentSemanticModel> objectModel,
        string candidate,
        Func<Ra2SectionKind, bool> accepts,
        CancellationToken cancellationToken)
    {
        string trimmed = candidate.Trim();
        if (trimmed.Length == 0)
            return trimmed;

        cancellationToken.ThrowIfCancellationRequested();
        Ra2DocumentSemanticModel model = objectModel.Value;
        cancellationToken.ThrowIfCancellationRequested();
        if (model.Sections.Any(section => string.Equals(section.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
            return trimmed;

        Ra2SectionSymbol[] matches = model.Sections
            .Where(section => accepts(section.Kind) && HasIdentityAlias(model, section.Name, trimmed))
            .ToArray();
        return matches.Length == 1 ? matches[0].Name : trimmed;
    }

    private static bool HasIdentityAlias(
        Ra2DocumentSemanticModel model,
        string sectionName,
        string candidate)
    {
        if (AliasEquals(sectionName, candidate))
            return true;

        foreach (Ra2KeyValueSymbol field in model.KeyValues)
        {
            if (!string.Equals(field.SectionName, sectionName, StringComparison.OrdinalIgnoreCase) ||
                !(string.Equals(field.Key, "Name", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(field.Key, "UIName", StringComparison.OrdinalIgnoreCase)) ||
                string.IsNullOrWhiteSpace(field.Value))
            {
                continue;
            }

            if (AliasEquals(field.Value, candidate))
                return true;
            int separator = field.Value.LastIndexOf(':');
            if (separator >= 0 && AliasEquals(field.Value[(separator + 1)..], candidate))
                return true;
        }

        return false;
    }

    private static bool AliasEquals(string left, string right)
    {
        string normalizedLeft = NormalizeAlias(left);
        return normalizedLeft.Length > 0 &&
               string.Equals(normalizedLeft, NormalizeAlias(right), StringComparison.Ordinal);
    }

    private static string NormalizeAlias(string value)
        => string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();

    private static bool IsTechnoKind(Ra2SectionKind kind)
        => kind is Ra2SectionKind.Techno or
            Ra2SectionKind.Infantry or
            Ra2SectionKind.Unit or
            Ra2SectionKind.Vehicle or
            Ra2SectionKind.Aircraft or
            Ra2SectionKind.Building;

    private static bool IsBoolean(string value)
        => value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
           value.Equals("false", StringComparison.OrdinalIgnoreCase);
}
