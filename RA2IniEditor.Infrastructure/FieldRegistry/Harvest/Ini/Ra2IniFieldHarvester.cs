using System.Globalization;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;

internal sealed class Ra2IniFieldHarvester : IRa2IniFieldHarvester
{
    private const int MaxSampleValues = 5;
    private const int MaxEnumValues = 12;

    private static readonly Ra2SectionKind[] UnitGeneralizationKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft
    ];

    private static readonly Ra2SectionKind[] TechnoGeneralizationKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft,
        Ra2SectionKind.Building
    ];

    private static readonly Dictionary<string, Ra2SectionKind> RegistrySectionKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["InfantryTypes"] = Ra2SectionKind.Infantry,
        ["VehicleTypes"] = Ra2SectionKind.Vehicle,
        ["AircraftTypes"] = Ra2SectionKind.Aircraft,
        ["BuildingTypes"] = Ra2SectionKind.Building,
        ["WeaponTypes"] = Ra2SectionKind.Weapon,
        ["ProjectileTypes"] = Ra2SectionKind.Projectile,
        ["WarheadTypes"] = Ra2SectionKind.Warhead,
        ["Animations"] = Ra2SectionKind.Animation,
        ["VoxelAnims"] = Ra2SectionKind.VoxelAnimation,
        ["SuperWeaponTypes"] = Ra2SectionKind.SuperWeapon
    };

    private static readonly Dictionary<string, Ra2SectionKind> FixedSectionKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["General"] = Ra2SectionKind.Global,
        ["AudioVisual"] = Ra2SectionKind.Global,
        ["AI"] = Ra2SectionKind.AI,
        ["Countries"] = Ra2SectionKind.Country,
        ["CountryTypes"] = Ra2SectionKind.Country
    };

    public Ra2IniFieldHarvestResult HarvestCurrentText(Ra2IniFieldHarvestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<ParsedIniLine> lines = ParseLines(request.Text);
        Dictionary<string, Ra2SectionKind> objectKinds = BuildObjectKindIndex(lines);
        Dictionary<(string Key, Ra2SectionKind Kind), FieldAccumulator> accumulators = new();
        int skippedNumericKeyCount = 0;

        string? currentSection = null;
        foreach (ParsedIniLine line in lines)
        {
            if (line.SectionName is not null)
            {
                currentSection = line.SectionName;
                continue;
            }

            if (currentSection is null || line.Key is null)
                continue;

            if (RegistrySectionKinds.ContainsKey(currentSection))
                continue;

            if (IsNumericKey(line.Key))
            {
                skippedNumericKeyCount++;
                continue;
            }

            Ra2SectionKind sectionKind = ResolveSectionKind(currentSection, objectKinds);
            (string Key, Ra2SectionKind Kind) identity = (line.Key, sectionKind);
            if (!accumulators.TryGetValue(identity, out FieldAccumulator? accumulator))
            {
                accumulator = new FieldAccumulator(line.Key, sectionKind);
                accumulators[identity] = accumulator;
            }

            accumulator.AddValue(line.Value ?? string.Empty, request.SourceName);
        }

        IReadOnlyList<FieldAccumulator> visibleAccumulators = GeneralizeCommonAccumulators(accumulators.Values);
        Dictionary<string, IReadOnlyList<string>> sharedValuesByKey = visibleAccumulators
            .GroupBy(accumulator => accumulator.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)Array.AsReadOnly(group
                    .SelectMany(accumulator => accumulator.Values)
                    .ToArray()),
                StringComparer.OrdinalIgnoreCase);

        List<Ra2IniFieldHarvestRow> rows = new();
        foreach (FieldAccumulator accumulator in visibleAccumulators
            .OrderBy(value => value.SectionKind)
            .ThenBy(value => value.Key, StringComparer.OrdinalIgnoreCase))
        {
            rows.Add(BuildRow(accumulator, request, sharedValuesByKey[accumulator.Key]));
        }

        return new Ra2IniFieldHarvestResult(
            Array.AsReadOnly(rows.ToArray()),
            Array.AsReadOnly(Array.Empty<FieldRegistryHarvestValidationIssue>()),
            skippedNumericKeyCount);
    }

    private static IReadOnlyList<FieldAccumulator> GeneralizeCommonAccumulators(IEnumerable<FieldAccumulator> accumulators)
    {
        List<FieldAccumulator> result = new();
        foreach (IGrouping<string, FieldAccumulator> group in accumulators.GroupBy(
            accumulator => accumulator.Key,
            StringComparer.OrdinalIgnoreCase))
        {
            List<FieldAccumulator> remaining = group.ToList();
            if (TryCreateGeneralizedAccumulator(remaining, TechnoGeneralizationKinds, Ra2SectionKind.Techno, out FieldAccumulator? techno) &&
                techno is not null)
            {
                result.Add(techno);
                RemoveKinds(remaining, TechnoGeneralizationKinds);
            }
            else if (TryCreateGeneralizedAccumulator(remaining, UnitGeneralizationKinds, Ra2SectionKind.Unit, out FieldAccumulator? unit) &&
                     unit is not null)
            {
                result.Add(unit);
                RemoveKinds(remaining, UnitGeneralizationKinds);
            }

            result.AddRange(remaining);
        }

        return Array.AsReadOnly(result.ToArray());
    }

    private static bool TryCreateGeneralizedAccumulator(
        IReadOnlyList<FieldAccumulator> accumulators,
        IReadOnlyList<Ra2SectionKind> requiredKinds,
        Ra2SectionKind generalizedKind,
        out FieldAccumulator? result)
    {
        result = null;
        FieldAccumulator[] matches = requiredKinds
            .Select(kind => accumulators.FirstOrDefault(accumulator => accumulator.SectionKind == kind))
            .Where(accumulator => accumulator is not null)
            .Cast<FieldAccumulator>()
            .ToArray();
        if (matches.Length != requiredKinds.Count)
            return false;

        result = new FieldAccumulator(matches[0].Key, generalizedKind);
        foreach (FieldAccumulator match in matches)
            result.AddFrom(match);

        return true;
    }

    private static void RemoveKinds(List<FieldAccumulator> accumulators, IReadOnlyList<Ra2SectionKind> kinds)
    {
        accumulators.RemoveAll(accumulator => kinds.Contains(accumulator.SectionKind));
    }

    private static IReadOnlyList<ParsedIniLine> ParseLines(string text)
    {
        List<ParsedIniLine> result = new();
        int lineNumber = 1;
        foreach (string rawLine in EnumerateLines(text))
        {
            string trimmed = rawLine.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
            {
                result.Add(ParsedIniLine.Empty(lineNumber));
                lineNumber++;
                continue;
            }

            if (TryParseSection(trimmed, out string sectionName))
            {
                result.Add(ParsedIniLine.Section(lineNumber, sectionName));
                lineNumber++;
                continue;
            }

            int equalsIndex = rawLine.IndexOf('=');
            if (equalsIndex > 0)
            {
                string key = rawLine[..equalsIndex].Trim();
                string value = StripInlineComment(rawLine[(equalsIndex + 1)..]).Trim();
                if (key.Length > 0)
                {
                    result.Add(ParsedIniLine.KeyValue(lineNumber, key, value));
                    lineNumber++;
                    continue;
                }
            }

            result.Add(ParsedIniLine.Empty(lineNumber));
            lineNumber++;
        }

        return Array.AsReadOnly(result.ToArray());
    }

    private static Dictionary<string, Ra2SectionKind> BuildObjectKindIndex(IReadOnlyList<ParsedIniLine> lines)
    {
        Dictionary<string, Ra2SectionKind> result = new(StringComparer.OrdinalIgnoreCase);
        string? currentSection = null;
        foreach (ParsedIniLine line in lines)
        {
            if (line.SectionName is not null)
            {
                currentSection = line.SectionName;
                continue;
            }

            if (currentSection is null ||
                line.Value is null ||
                !RegistrySectionKinds.TryGetValue(currentSection, out Ra2SectionKind kind))
            {
                continue;
            }

            string objectId = line.Value.Trim();
            if (objectId.Length > 0)
                result.TryAdd(objectId, kind);
        }

        return result;
    }

    private static Ra2IniFieldHarvestRow BuildRow(
        FieldAccumulator accumulator,
        Ra2IniFieldHarvestRequest request,
        IReadOnlyList<string> inferenceValues)
    {
        IReadOnlyList<string> sampleValues = Array.AsReadOnly(accumulator.Values
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxSampleValues)
            .ToArray());
        FieldInference inference = InferField(accumulator.Key, inferenceValues);
        List<FieldRegistryHarvestValidationIssue> issues = new(inference.Issues);
        if (request.ExistingDefinitions.Any(definition => AppliesTo(definition, accumulator.SectionKind) &&
                string.Equals(definition.Key, accumulator.Key, StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(new FieldRegistryHarvestValidationIssue(
                request.SourceName,
                0,
                accumulator.Key,
                FieldRegistryHarvestValidationSeverity.Info,
                "Field already exists in the current field registry."));
        }

        return new Ra2IniFieldHarvestRow(
            accumulator.Key,
            accumulator.SectionKind,
            accumulator.OccurrenceCount,
            sampleValues,
            Array.AsReadOnly(accumulator.SourceNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray()),
            inference.EditorKind,
            inference.ValueKind,
            inference.BooleanStyle,
            inference.AllowedValues,
            Array.AsReadOnly(issues.ToArray()));
    }

    private static FieldInference InferField(string key, IReadOnlyList<string> rawValues)
    {
        string[] values = rawValues
            .Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        List<FieldRegistryHarvestValidationIssue> issues = new();

        if (values.Length == 0)
            return TextInference("Unable to infer value type from empty samples.");

        if (TryInferBoolean(values, issues, out FieldInference? booleanInference))
            return booleanInference!;

        if (values.All(IsInteger))
            return new FieldInference(FieldEditorKind.Integer, Ra2FieldValueKind.Integer, Ra2FieldBooleanValueStyle.Unknown, [], []);

        if (values.All(IsNumber) && values.Any(value => value.Contains('.', StringComparison.Ordinal)))
            return new FieldInference(FieldEditorKind.Float, Ra2FieldValueKind.Float, Ra2FieldBooleanValueStyle.Unknown, [], []);

        if (values.Any(value => value.Contains(',', StringComparison.Ordinal)))
        {
            Ra2FieldAllowedValue[] allowedValues = values
                .SelectMany(value => value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Select(value => new Ra2FieldAllowedValue(value))
                .ToArray();
            FieldRegistryHarvestValidationIssue warning = allowedValues.Length > MaxEnumValues
                ? Warning(key, $"List values were inferred from {allowedValues.Length} distinct current INI tokens and may be incomplete; please confirm before applying.")
                : Warning(key, "List values were inferred from current INI samples and may be incomplete.");
            return new FieldInference(
                FieldEditorKind.MultiSelect,
                Ra2FieldValueKind.EnumList,
                Ra2FieldBooleanValueStyle.Unknown,
                Array.AsReadOnly(allowedValues),
                [warning]);
        }

        if (values.All(IsEnumLikeValue))
        {
            Ra2FieldAllowedValue[] allowedValues = values
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Select(value => new Ra2FieldAllowedValue(value))
                .ToArray();
            FieldRegistryHarvestValidationIssue warning = values.Length > MaxEnumValues
                ? Warning(key, $"Enum values were inferred from {values.Length} distinct current INI samples and may need pruning.")
                : Warning(key, "Enum values were inferred from current INI samples and may be incomplete.");
            return new FieldInference(
                FieldEditorKind.Enum,
                Ra2FieldValueKind.Enum,
                Ra2FieldBooleanValueStyle.Unknown,
                Array.AsReadOnly(allowedValues),
                [warning]);
        }

        return TextInference("Unable to reliably infer value type; please confirm manually.");

        FieldInference TextInference(string message)
            => new(
                FieldEditorKind.Text,
                Ra2FieldValueKind.String,
                Ra2FieldBooleanValueStyle.Unknown,
                [],
                [Warning(key, message)]);
    }

    private static bool TryInferBoolean(
        IReadOnlyList<string> values,
        List<FieldRegistryHarvestValidationIssue> issues,
        out FieldInference? inference)
    {
        inference = null;
        HashSet<string> lowerValues = new(values.Select(value => value.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase);
        bool allYesNo = lowerValues.All(value => value is "yes" or "no");
        bool allTrueFalse = lowerValues.All(value => value is "true" or "false");
        bool allBoolean = lowerValues.All(value => value is "yes" or "no" or "true" or "false");
        if (allYesNo)
        {
            inference = BooleanInference(Ra2FieldBooleanValueStyle.YesNo, ["no", "yes"], issues);
            return true;
        }

        if (allTrueFalse)
        {
            inference = BooleanInference(Ra2FieldBooleanValueStyle.TrueFalse, ["false", "true"], issues);
            return true;
        }

        if (allBoolean)
        {
            Ra2FieldAllowedValue[] allowedValues = lowerValues
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Select(value => new Ra2FieldAllowedValue(value))
                .ToArray();
            issues.Add(Warning(null, "Mixed boolean styles were detected; please confirm manually."));
            inference = new FieldInference(
                FieldEditorKind.Boolean,
                Ra2FieldValueKind.Boolean,
                Ra2FieldBooleanValueStyle.Custom,
                Array.AsReadOnly(allowedValues),
                Array.AsReadOnly(issues.ToArray()));
            return true;
        }

        return false;
    }

    private static FieldInference BooleanInference(
        Ra2FieldBooleanValueStyle style,
        IReadOnlyList<string> values,
        IReadOnlyList<FieldRegistryHarvestValidationIssue> issues)
    {
        return new FieldInference(
            FieldEditorKind.Boolean,
            Ra2FieldValueKind.Boolean,
            style,
            Array.AsReadOnly(values.Select(value => new Ra2FieldAllowedValue(value)).ToArray()),
            issues);
    }

    private static bool AppliesTo(Ra2FieldDefinition definition, Ra2SectionKind sectionKind)
    {
        return definition.AppliesTo.Count == 0 ||
               definition.AppliesTo.Contains(sectionKind) ||
               EnumerateAbstractLookupKinds(sectionKind).Any(definition.AppliesTo.Contains) ||
               definition.AppliesTo.Contains(Ra2SectionKind.Unknown) ||
               definition.AppliesTo.Contains(Ra2SectionKind.Global);
    }

    private static IEnumerable<Ra2SectionKind> EnumerateAbstractLookupKinds(Ra2SectionKind sectionKind)
    {
        if (sectionKind is Ra2SectionKind.Infantry or Ra2SectionKind.Vehicle or Ra2SectionKind.Aircraft)
            yield return Ra2SectionKind.Unit;

        if (sectionKind is Ra2SectionKind.Infantry or
            Ra2SectionKind.Vehicle or
            Ra2SectionKind.Aircraft or
            Ra2SectionKind.Building or
            Ra2SectionKind.Unit)
        {
            yield return Ra2SectionKind.Techno;
        }
    }

    private static bool TryParseSection(string trimmed, out string sectionName)
    {
        sectionName = string.Empty;
        if (!trimmed.StartsWith('[') || !trimmed.EndsWith(']') || trimmed.Length <= 2)
            return false;

        sectionName = trimmed[1..^1].Trim();
        return sectionName.Length > 0;
    }

    private static Ra2SectionKind ResolveSectionKind(string sectionName, IReadOnlyDictionary<string, Ra2SectionKind> objectKinds)
    {
        if (objectKinds.TryGetValue(sectionName, out Ra2SectionKind objectKind))
            return objectKind;

        if (FixedSectionKinds.TryGetValue(sectionName, out Ra2SectionKind fixedKind))
            return fixedKind;

        return Ra2SectionKind.Unknown;
    }

    private static FieldRegistryHarvestValidationIssue Warning(string? key, string message)
        => new("Current INI", 0, key, FieldRegistryHarvestValidationSeverity.Warning, message);

    private static bool IsInteger(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);

    private static bool IsNumber(string value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _);

    private static bool IsNumericKey(string key)
        => key.Length > 0 && key.All(char.IsDigit);

    private static bool IsEnumLikeValue(string value)
        => value.Length <= 64 && value.All(character => !char.IsWhiteSpace(character) && character is not '=' and not '[' and not ']');

    private static string StripInlineComment(string value)
    {
        int index = value.IndexOf(';');
        return index < 0 ? value : value[..index];
    }

    private static IEnumerable<string> EnumerateLines(string text)
    {
        using StringReader reader = new(text);
        string? line;
        while ((line = reader.ReadLine()) is not null)
            yield return line;
    }

    private sealed class FieldAccumulator
    {
        private readonly List<string> _values = new();
        private readonly HashSet<string> _sourceNames = new(StringComparer.OrdinalIgnoreCase);

        public FieldAccumulator(string key, Ra2SectionKind sectionKind)
        {
            Key = key;
            SectionKind = sectionKind;
        }

        public string Key { get; }

        public Ra2SectionKind SectionKind { get; }

        public int OccurrenceCount { get; private set; }

        public IReadOnlyList<string> Values => _values;

        public IReadOnlySet<string> SourceNames => _sourceNames;

        public void AddValue(string value, string sourceName)
        {
            OccurrenceCount++;
            _values.Add(value);
            _sourceNames.Add(sourceName);
        }

        public void AddFrom(FieldAccumulator accumulator)
        {
            ArgumentNullException.ThrowIfNull(accumulator);

            OccurrenceCount += accumulator.OccurrenceCount;
            _values.AddRange(accumulator.Values);
            foreach (string sourceName in accumulator.SourceNames)
                _sourceNames.Add(sourceName);
        }
    }

    private sealed record ParsedIniLine(int LineNumber, string? SectionName, string? Key, string? Value)
    {
        public static ParsedIniLine Empty(int lineNumber) => new(lineNumber, null, null, null);

        public static ParsedIniLine Section(int lineNumber, string sectionName) => new(lineNumber, sectionName, null, null);

        public static ParsedIniLine KeyValue(int lineNumber, string key, string value) => new(lineNumber, null, key, value);
    }

    private sealed record FieldInference(
        FieldEditorKind EditorKind,
        Ra2FieldValueKind ValueKind,
        Ra2FieldBooleanValueStyle BooleanStyle,
        IReadOnlyList<Ra2FieldAllowedValue> AllowedValues,
        IReadOnlyList<FieldRegistryHarvestValidationIssue> Issues);
}
