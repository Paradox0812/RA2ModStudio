using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;

internal sealed class Ra2FieldImportDraftBuilder : IRa2FieldImportDraftBuilder
{
    private readonly Ra2AllowedValuesTextParser _allowedValuesTextParser;

    public Ra2FieldImportDraftBuilder()
        : this(new Ra2AllowedValuesTextParser())
    {
    }

    public Ra2FieldImportDraftBuilder(Ra2AllowedValuesTextParser allowedValuesTextParser)
    {
        _allowedValuesTextParser = allowedValuesTextParser ?? throw new ArgumentNullException(nameof(allowedValuesTextParser));
    }

    public IReadOnlyList<Ra2FieldImportDraftRow> BuildDraft(Ra2IniFieldHarvestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return Array.AsReadOnly(result.Rows
            .Select(row => new Ra2FieldImportDraftRow(
                isEnabled: true,
                row.Key,
                row.SectionKind,
                row.OccurrenceCount,
                string.Join(", ", row.SampleValues),
                row.InferredEditorKind,
                row.InferredValueKind,
                row.InferredBooleanStyle,
                FormatAllowedValues(row.InferredAllowedValues),
                displayName: null,
                description: null,
                sourceNote: string.Join(", ", row.SourceNames),
                issueSummary: string.Join("; ", row.Issues.Select(issue => issue.Message))))
            .ToArray());
    }

    public FieldRegistryHarvestPreviewDraft BuildPreviewFromDraft(IReadOnlyList<Ra2FieldImportDraftRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        List<Ra2FieldDefinition> definitions = new();
        List<FieldRegistryHarvestValidationIssue> issues = new();
        foreach (Ra2FieldImportDraftRow row in rows.Where(row => row.IsEnabled))
        {
            Ra2AllowedValuesTextParseResult allowedValues = _allowedValuesTextParser.Parse(row.AllowedValuesText);
            foreach (string warning in allowedValues.Warnings)
            {
                issues.Add(new FieldRegistryHarvestValidationIssue(
                    row.SourceNote ?? "Field import draft",
                    0,
                    row.Key,
                    FieldRegistryHarvestValidationSeverity.Warning,
                    warning));
            }

            AddMetadataValidationIssues(row, allowedValues.Values, issues);
            definitions.Add(new Ra2FieldDefinition(
                row.Key,
                [row.SectionKind],
                row.EditorKind,
                Ra2FieldSourceKind.User,
                row.Description,
                CreateMetadata(row, allowedValues.Values)));
        }

        return new FieldRegistryHarvestPreviewDraft(
            Array.AsReadOnly(definitions.ToArray()),
            Array.AsReadOnly(issues.ToArray()));
    }

    private static Ra2FieldValueMetadata CreateMetadata(
        Ra2FieldImportDraftRow row,
        IReadOnlyList<Ra2FieldAllowedValue> allowedValues)
    {
        if (row.ValueKind == Ra2FieldValueKind.Unknown ||
            row.ValueKind == Ra2FieldValueKind.String)
        {
            return Ra2FieldValueMetadata.Unknown;
        }

        return new Ra2FieldValueMetadata(
            row.ValueKind,
            row.BooleanStyle,
            allowedValues,
            enumName: null,
            separator: row.ValueKind is Ra2FieldValueKind.EnumList or Ra2FieldValueKind.ReferenceList ? "," : ",");
    }

    private static void AddMetadataValidationIssues(
        Ra2FieldImportDraftRow row,
        IReadOnlyList<Ra2FieldAllowedValue> allowedValues,
        List<FieldRegistryHarvestValidationIssue> issues)
    {
        if (row.ValueKind is Ra2FieldValueKind.Enum or Ra2FieldValueKind.EnumList &&
            allowedValues.Count == 0)
        {
            issues.Add(Warning(row, "Enum fields should define at least one allowed value."));
        }

        if (row.ValueKind == Ra2FieldValueKind.Boolean &&
            row.BooleanStyle == Ra2FieldBooleanValueStyle.Custom &&
            allowedValues.Count == 0)
        {
            issues.Add(Warning(row, "Custom boolean fields should define allowed values."));
        }
    }

    private static FieldRegistryHarvestValidationIssue Warning(Ra2FieldImportDraftRow row, string message)
    {
        return new FieldRegistryHarvestValidationIssue(
            row.SourceNote ?? "Field import draft",
            0,
            row.Key,
            FieldRegistryHarvestValidationSeverity.Warning,
            message);
    }

    private static string FormatAllowedValues(IReadOnlyList<Ra2FieldAllowedValue> values)
    {
        return string.Join(Environment.NewLine, values.Select(FormatAllowedValue));
    }

    private static string FormatAllowedValue(Ra2FieldAllowedValue value)
    {
        if (!string.IsNullOrWhiteSpace(value.Description))
            return $"{value.Value}|{value.DisplayName}|{value.Description}";

        if (!string.IsNullOrWhiteSpace(value.DisplayName))
            return $"{value.Value}|{value.DisplayName}";

        return value.Value;
    }
}
