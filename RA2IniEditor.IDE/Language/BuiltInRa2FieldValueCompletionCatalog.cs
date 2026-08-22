using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal sealed class BuiltInRa2FieldValueCompletionCatalog : IRa2FieldValueCompletionCatalog
{
    private static readonly Dictionary<string, Ra2BooleanValueSet> BooleanFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Crusher"] = Ra2BooleanValueSet.YesNo,
        ["Powered"] = Ra2BooleanValueSet.YesNo,
        ["Trainable"] = Ra2BooleanValueSet.YesNo,
        ["Selectable"] = Ra2BooleanValueSet.YesNo,
        ["RadarInvisible"] = Ra2BooleanValueSet.YesNo,
        ["Insignificant"] = Ra2BooleanValueSet.YesNo,
        ["IsBaseDefense"] = Ra2BooleanValueSet.YesNo,
        ["Landable"] = Ra2BooleanValueSet.YesNo,
        ["CanPassiveAquire"] = Ra2BooleanValueSet.YesNo,
        ["CanRetaliate"] = Ra2BooleanValueSet.YesNo
    };

    private static readonly Dictionary<string, string[]> EnumFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Armor"] =
        [
            "none",
            "flak",
            "plate",
            "light",
            "medium",
            "heavy",
            "wood",
            "steel",
            "concrete",
            "special_1",
            "special_2"
        ],
        ["Category"] =
        [
            "Soldier",
            "AFV",
            "AirPower",
            "Support",
            "Transport",
            "VIP",
            "Civilian"
        ]
    };

    private static readonly Dictionary<string, string[]> ListFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Owner"] =
        [
            "Americans",
            "Alliance",
            "French",
            "Germans",
            "British",
            "Africans",
            "Arabs",
            "Confederation",
            "Russians",
            "YuriCountry"
        ],
        ["VeteranAbilities"] =
        [
            "FASTER",
            "STRONGER",
            "FIREPOWER",
            "ROF",
            "SIGHT",
            "CLOAK",
            "TIBERIUM_PROOF",
            "VEIN_PROOF",
            "SELF_HEAL",
            "EXPLODES",
            "RADAR_INVISIBLE",
            "SENSORS",
            "FEARLESS",
            "C4",
            "GUARD_AREA",
            "CRUSHER"
        ],
        ["EliteAbilities"] =
        [
            "FASTER",
            "STRONGER",
            "FIREPOWER",
            "ROF",
            "SIGHT",
            "CLOAK",
            "TIBERIUM_PROOF",
            "VEIN_PROOF",
            "SELF_HEAL",
            "EXPLODES",
            "RADAR_INVISIBLE",
            "SENSORS",
            "FEARLESS",
            "C4",
            "GUARD_AREA",
            "CRUSHER"
        ]
    };

    public IReadOnlyList<Ra2FieldValueCompletionCandidate> GetCandidates(
        Ra2FieldValueCompletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.FieldDefinition is null || string.IsNullOrWhiteSpace(request.Key))
            return [];

        if (request.FieldDefinition.ValueMetadata.HasSchema)
            return GetKnownValueCandidatesForSchemaBackfill(request);

        return request.FieldDefinition.EditorKind switch
        {
            FieldEditorKind.Boolean => GetBooleanCandidates(request),
            FieldEditorKind.Enum => GetEnumCandidates(request),
            FieldEditorKind.MultiSelect => GetListCandidates(request),
            _ => []
        };
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> GetKnownValueCandidatesForSchemaBackfill(
        Ra2FieldValueCompletionRequest request)
    {
        return request.FieldDefinition!.ValueMetadata.ValueKind switch
        {
            Ra2FieldValueKind.Enum => GetEnumCandidates(request),
            Ra2FieldValueKind.EnumList => GetListCandidates(request),
            _ => []
        };
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> GetBooleanCandidates(
        Ra2FieldValueCompletionRequest request)
    {
        if (BooleanFields.TryGetValue(request.Key, out Ra2BooleanValueSet? valueSet))
            return CreateBooleanCandidates(valueSet, request.Context.CurrentTokenPrefix);

        return GetBooleanPrefixFallbackCandidates(request.Context.CurrentTokenPrefix);
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> CreateBooleanCandidates(
        Ra2BooleanValueSet valueSet,
        string prefix)
    {
        return CreateValueCandidates(
            [valueSet.TrueValue, valueSet.FalseValue],
            "Boolean",
            "Boolean value.",
            prefix,
            priority: 100);
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> GetBooleanPrefixFallbackCandidates(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return [];

        string normalizedPrefix = prefix.Trim();
        if (StartsWithPrefix("true", normalizedPrefix))
            return CreateValueCandidates(["true"], "Boolean", "Boolean value.", normalizedPrefix, priority: 40);

        if (StartsWithPrefix("false", normalizedPrefix))
            return CreateValueCandidates(["false"], "Boolean", "Boolean value.", normalizedPrefix, priority: 40);

        if (StartsWithPrefix("yes", normalizedPrefix))
            return CreateValueCandidates(["yes"], "Boolean", "Boolean value.", normalizedPrefix, priority: 40);

        if (StartsWithPrefix("no", normalizedPrefix))
            return CreateValueCandidates(["no"], "Boolean", "Boolean value.", normalizedPrefix, priority: 40);

        return [];
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> GetEnumCandidates(
        Ra2FieldValueCompletionRequest request)
    {
        return EnumFields.TryGetValue(request.Key, out string[]? values)
            ? CreateValueCandidates(values, "Enum", "Enum value.", request.Context.CurrentTokenPrefix, priority: 90)
            : [];
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> GetListCandidates(
        Ra2FieldValueCompletionRequest request)
    {
        if (!ListFields.TryGetValue(request.Key, out string[]? values))
            return [];

        HashSet<string> existingTokens = new(request.Context.ExistingTokens, StringComparer.OrdinalIgnoreCase);
        return CreateValueCandidates(
                values.Where(value => !existingTokens.Contains(value)),
                "List",
                "List value.",
                request.Context.CurrentTokenPrefix,
                priority: 80)
            .ToArray();
    }

    private static IReadOnlyList<Ra2FieldValueCompletionCandidate> CreateValueCandidates(
        IEnumerable<string> values,
        string displayName,
        string description,
        string prefix,
        int priority)
    {
        return values
            .Where(value => StartsWithPrefix(value, prefix))
            .Select(value => new Ra2FieldValueCompletionCandidate(
                value,
                displayName,
                description,
                Ra2CompletionItemKind.Value,
                priority,
                Ra2FieldValueCompletionSourceKind.BuiltIn))
            .ToArray();
    }

    private static bool StartsWithPrefix(string value, string prefix)
        => string.IsNullOrEmpty(prefix) || value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
}
