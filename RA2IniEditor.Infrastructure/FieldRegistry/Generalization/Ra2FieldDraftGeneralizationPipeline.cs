using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Generalization;

internal sealed class Ra2FieldDraftGeneralizationPipeline
{
    private static readonly Ra2SectionKind[] TechnoConcreteKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft,
        Ra2SectionKind.Building
    ];

    private static readonly Ra2SectionKind[] UnitConcreteKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft
    ];

    public Ra2FieldDraftGeneralizationResult Generalize(FieldRegistryHarvestPreviewDraft previewDraft)
    {
        ArgumentNullException.ThrowIfNull(previewDraft);

        List<Ra2FieldDefinition> working = previewDraft.Definitions.ToList();
        List<Ra2FieldDraftGeneralizationNotice> notices = new();
        List<Ra2FieldDraftGeneralizationWarning> warnings = new();

        foreach (string key in working
            .Select(definition => definition.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ToArray())
        {
            if (HasAllSingleKindDefinitions(working, key, TechnoConcreteKinds))
            {
                TryGeneralizeKey(working, key, Ra2SectionKind.Techno, TechnoConcreteKinds, notices, warnings);
                continue;
            }

            TryGeneralizeKey(working, key, Ra2SectionKind.Unit, UnitConcreteKinds, notices, warnings);
        }

        FieldRegistryHarvestPreviewDraft generalizedDraft = new(
            Array.AsReadOnly(working.ToArray()),
            previewDraft.Issues);

        return new Ra2FieldDraftGeneralizationResult(
            generalizedDraft,
            Array.AsReadOnly(notices.ToArray()),
            Array.AsReadOnly(warnings.ToArray()));
    }

    private static bool TryGeneralizeKey(
        List<Ra2FieldDefinition> definitions,
        string key,
        Ra2SectionKind targetKind,
        IReadOnlyList<Ra2SectionKind> concreteKinds,
        List<Ra2FieldDraftGeneralizationNotice> notices,
        List<Ra2FieldDraftGeneralizationWarning> warnings)
    {
        Dictionary<Ra2SectionKind, Ra2FieldDefinition> concreteDefinitions = new();
        foreach (Ra2SectionKind concreteKind in concreteKinds)
        {
            Ra2FieldDefinition? definition = FindDefinitionForKind(definitions, key, concreteKind);
            if (definition is null)
                return false;

            concreteDefinitions[concreteKind] = definition;
        }

        IReadOnlyList<Ra2FieldDefinition> candidates = concreteKinds
            .Select(kind => concreteDefinitions[kind])
            .Distinct()
            .ToArray();
        if (!AreDefinitionsCompatible(concreteKinds.Select(kind => concreteDefinitions[kind]).ToArray(), out string incompatibilityReason))
        {
            warnings.Add(new Ra2FieldDraftGeneralizationWarning(
                key,
                targetKind,
                concreteKinds,
                $"Skipped {key} generalization to {targetKind}: {incompatibilityReason}"));
            return false;
        }

        Ra2FieldDefinition? existingTarget = FindDefinitionForKind(definitions, key, targetKind);
        Ra2FieldDefinition generalized = CreateGeneralizedDefinition(
            key,
            targetKind,
            existingTarget is null ? candidates : [existingTarget, .. candidates]);

        definitions.RemoveAll(definition =>
            string.Equals(definition.Key, key, StringComparison.OrdinalIgnoreCase) &&
            definition.AppliesTo.Any(kind => concreteKinds.Contains(kind) || kind == targetKind));
        definitions.Add(generalized);

        notices.Add(new Ra2FieldDraftGeneralizationNotice(
            key,
            targetKind,
            concreteKinds,
            $"{key} generalized from {string.Join(", ", concreteKinds)} to {targetKind}."));
        return true;
    }

    private static bool HasAllSingleKindDefinitions(
        IEnumerable<Ra2FieldDefinition> definitions,
        string key,
        IReadOnlyList<Ra2SectionKind> concreteKinds)
    {
        return concreteKinds.All(kind => FindDefinitionForKind(definitions, key, kind) is not null);
    }

    private static Ra2FieldDefinition? FindDefinitionForKind(
        IEnumerable<Ra2FieldDefinition> definitions,
        string key,
        Ra2SectionKind sectionKind)
    {
        return definitions.FirstOrDefault(definition =>
            string.Equals(definition.Key, key, StringComparison.OrdinalIgnoreCase) &&
            definition.AppliesTo.Contains(sectionKind));
    }

    private static bool AreDefinitionsCompatible(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        out string reason)
    {
        Ra2FieldDefinition first = definitions[0];
        foreach (Ra2FieldDefinition definition in definitions.Skip(1))
        {
            if (definition.EditorKind != first.EditorKind)
            {
                reason = $"EditorKind differs ({first.EditorKind} vs {definition.EditorKind}).";
                return false;
            }

            if (!AreValueMetadataCompatible(first.ValueMetadata, definition.ValueMetadata, out reason))
                return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool AreValueMetadataCompatible(
        Ra2FieldValueMetadata left,
        Ra2FieldValueMetadata right,
        out string reason)
    {
        if (left.ValueKind != right.ValueKind)
        {
            reason = $"ValueKind differs ({left.ValueKind} vs {right.ValueKind}).";
            return false;
        }

        if (left.BooleanStyle != right.BooleanStyle)
        {
            reason = $"BooleanStyle differs ({left.BooleanStyle} vs {right.BooleanStyle}).";
            return false;
        }

        if (!string.Equals(left.Separator, right.Separator, StringComparison.Ordinal))
        {
            reason = "List separator differs.";
            return false;
        }

        if (!AreEnumNamesCompatible(left.EnumName, right.EnumName))
        {
            reason = "EnumName differs.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool AreEnumNamesCompatible(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return true;

        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static Ra2FieldDefinition CreateGeneralizedDefinition(
        string key,
        Ra2SectionKind targetKind,
        IReadOnlyList<Ra2FieldDefinition> sourceDefinitions)
    {
        Ra2FieldDefinition first = sourceDefinitions[0];
        return new Ra2FieldDefinition(
            key,
            [targetKind],
            first.EditorKind,
            ResolveSourceKind(sourceDefinitions),
            ResolveFirstText(sourceDefinitions.Select(definition => definition.Description)),
            MergeValueMetadata(sourceDefinitions),
            ResolveFirstText(sourceDefinitions.Select(definition => definition.DisplayName)),
            sourceDefinitions.SelectMany(definition => definition.Aliases).ToArray());
    }

    private static Ra2FieldSourceKind ResolveSourceKind(IReadOnlyList<Ra2FieldDefinition> sourceDefinitions)
    {
        Ra2FieldSourceKind firstConcrete = sourceDefinitions
            .Select(definition => definition.SourceKind)
            .FirstOrDefault(kind => kind != Ra2FieldSourceKind.Unknown);
        return firstConcrete == default ? Ra2FieldSourceKind.Unknown : firstConcrete;
    }

    private static string? ResolveFirstText(IEnumerable<string?> values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static Ra2FieldValueMetadata MergeValueMetadata(IReadOnlyList<Ra2FieldDefinition> sourceDefinitions)
    {
        Ra2FieldValueMetadata first = sourceDefinitions[0].ValueMetadata;
        Dictionary<string, Ra2FieldAllowedValue> allowedValues = new(StringComparer.OrdinalIgnoreCase);
        foreach (Ra2FieldAllowedValue value in sourceDefinitions.SelectMany(definition => definition.ValueMetadata.AllowedValues))
            allowedValues.TryAdd(value.Value, value);

        string? enumName = sourceDefinitions
            .Select(definition => definition.ValueMetadata.EnumName)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        return new Ra2FieldValueMetadata(
            first.ValueKind,
            first.BooleanStyle,
            Array.AsReadOnly(allowedValues.Values
                .OrderBy(value => value.Value, StringComparer.OrdinalIgnoreCase)
                .ToArray()),
            enumName,
            first.Separator);
    }
}

internal sealed class Ra2FieldDraftGeneralizationResult
{
    public Ra2FieldDraftGeneralizationResult(
        FieldRegistryHarvestPreviewDraft previewDraft,
        IReadOnlyList<Ra2FieldDraftGeneralizationNotice> notices,
        IReadOnlyList<Ra2FieldDraftGeneralizationWarning> warnings)
    {
        PreviewDraft = previewDraft ?? throw new ArgumentNullException(nameof(previewDraft));
        Notices = notices ?? throw new ArgumentNullException(nameof(notices));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public FieldRegistryHarvestPreviewDraft PreviewDraft { get; }

    public IReadOnlyList<Ra2FieldDraftGeneralizationNotice> Notices { get; }

    public IReadOnlyList<Ra2FieldDraftGeneralizationWarning> Warnings { get; }
}

internal sealed class Ra2FieldDraftGeneralizationNotice
{
    public Ra2FieldDraftGeneralizationNotice(
        string key,
        Ra2SectionKind targetKind,
        IReadOnlyList<Ra2SectionKind> sourceKinds,
        string message)
    {
        Key = key;
        TargetKind = targetKind;
        SourceKinds = sourceKinds;
        Message = message;
    }

    public string Key { get; }

    public Ra2SectionKind TargetKind { get; }

    public IReadOnlyList<Ra2SectionKind> SourceKinds { get; }

    public string Message { get; }
}

internal sealed class Ra2FieldDraftGeneralizationWarning
{
    public Ra2FieldDraftGeneralizationWarning(
        string key,
        Ra2SectionKind targetKind,
        IReadOnlyList<Ra2SectionKind> sourceKinds,
        string message)
    {
        Key = key;
        TargetKind = targetKind;
        SourceKinds = sourceKinds;
        Message = message;
    }

    public string Key { get; }

    public Ra2SectionKind TargetKind { get; }

    public IReadOnlyList<Ra2SectionKind> SourceKinds { get; }

    public string Message { get; }
}
