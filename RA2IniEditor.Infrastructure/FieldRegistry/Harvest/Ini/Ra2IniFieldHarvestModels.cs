using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;

internal sealed class Ra2IniFieldHarvestRequest
{
    public Ra2IniFieldHarvestRequest(
        string sourceName,
        string text,
        IReadOnlyList<Ra2FieldDefinition> existingDefinitions)
    {
        SourceName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ExistingDefinitions = existingDefinitions ?? throw new ArgumentNullException(nameof(existingDefinitions));
    }

    public string SourceName { get; }

    public string Text { get; }

    public IReadOnlyList<Ra2FieldDefinition> ExistingDefinitions { get; }
}

internal sealed class Ra2IniProjectFieldHarvestRequest
{
    public Ra2IniProjectFieldHarvestRequest(
        string? currentFilePath,
        string? currentText,
        IReadOnlyList<string> discoveredIniFilePaths,
        IReadOnlySet<string> excludedDirectoryNames)
    {
        CurrentFilePath = currentFilePath;
        CurrentText = currentText;
        DiscoveredIniFilePaths = discoveredIniFilePaths ?? throw new ArgumentNullException(nameof(discoveredIniFilePaths));
        ExcludedDirectoryNames = excludedDirectoryNames ?? throw new ArgumentNullException(nameof(excludedDirectoryNames));
    }

    public string? CurrentFilePath { get; }

    public string? CurrentText { get; }

    public IReadOnlyList<string> DiscoveredIniFilePaths { get; }

    public IReadOnlySet<string> ExcludedDirectoryNames { get; }
}

internal sealed class Ra2IniFieldHarvestResult
{
    public Ra2IniFieldHarvestResult(
        IReadOnlyList<Ra2IniFieldHarvestRow> rows,
        IReadOnlyList<FieldRegistryHarvestValidationIssue> issues)
        : this(rows, issues, skippedNumericKeyCount: 0)
    {
    }

    public Ra2IniFieldHarvestResult(
        IReadOnlyList<Ra2IniFieldHarvestRow> rows,
        IReadOnlyList<FieldRegistryHarvestValidationIssue> issues,
        int skippedNumericKeyCount)
    {
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
        SkippedNumericKeyCount = Math.Max(0, skippedNumericKeyCount);
    }

    public IReadOnlyList<Ra2IniFieldHarvestRow> Rows { get; }

    public IReadOnlyList<FieldRegistryHarvestValidationIssue> Issues { get; }

    public int SkippedNumericKeyCount { get; }
}

internal sealed class Ra2IniFieldHarvestRow
{
    public Ra2IniFieldHarvestRow(
        string key,
        Ra2SectionKind sectionKind,
        int occurrenceCount,
        IReadOnlyList<string> sampleValues,
        IReadOnlyList<string> sourceNames,
        FieldEditorKind inferredEditorKind,
        Ra2FieldValueKind inferredValueKind,
        Ra2FieldBooleanValueStyle inferredBooleanStyle,
        IReadOnlyList<Ra2FieldAllowedValue> inferredAllowedValues,
        IReadOnlyList<FieldRegistryHarvestValidationIssue> issues)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        SectionKind = sectionKind;
        OccurrenceCount = occurrenceCount;
        SampleValues = sampleValues ?? throw new ArgumentNullException(nameof(sampleValues));
        SourceNames = sourceNames ?? throw new ArgumentNullException(nameof(sourceNames));
        InferredEditorKind = inferredEditorKind;
        InferredValueKind = inferredValueKind;
        InferredBooleanStyle = inferredBooleanStyle;
        InferredAllowedValues = inferredAllowedValues ?? throw new ArgumentNullException(nameof(inferredAllowedValues));
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
    }

    public string Key { get; }

    public Ra2SectionKind SectionKind { get; }

    public int OccurrenceCount { get; }

    public IReadOnlyList<string> SampleValues { get; }

    public IReadOnlyList<string> SourceNames { get; }

    public FieldEditorKind InferredEditorKind { get; }

    public Ra2FieldValueKind InferredValueKind { get; }

    public Ra2FieldBooleanValueStyle InferredBooleanStyle { get; }

    public IReadOnlyList<Ra2FieldAllowedValue> InferredAllowedValues { get; }

    public IReadOnlyList<FieldRegistryHarvestValidationIssue> Issues { get; }
}

internal sealed class Ra2FieldImportDraftRow
{
    public Ra2FieldImportDraftRow(
        bool isEnabled,
        string key,
        Ra2SectionKind sectionKind,
        int occurrenceCount,
        string sampleValueSummary,
        FieldEditorKind editorKind,
        Ra2FieldValueKind valueKind,
        Ra2FieldBooleanValueStyle booleanStyle,
        string allowedValuesText,
        string? displayName,
        string? description,
        string? sourceNote,
        string issueSummary)
    {
        IsEnabled = isEnabled;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        SectionKind = sectionKind;
        OccurrenceCount = occurrenceCount;
        SampleValueSummary = sampleValueSummary ?? throw new ArgumentNullException(nameof(sampleValueSummary));
        EditorKind = editorKind;
        ValueKind = valueKind;
        BooleanStyle = booleanStyle;
        AllowedValuesText = allowedValuesText ?? throw new ArgumentNullException(nameof(allowedValuesText));
        DisplayName = displayName;
        Description = description;
        SourceNote = sourceNote;
        IssueSummary = issueSummary ?? throw new ArgumentNullException(nameof(issueSummary));
    }

    public bool IsEnabled { get; set; }

    public string Key { get; }

    public Ra2SectionKind SectionKind { get; set; }

    public int OccurrenceCount { get; }

    public string SampleValueSummary { get; }

    public FieldEditorKind EditorKind { get; set; }

    public Ra2FieldValueKind ValueKind { get; set; }

    public Ra2FieldBooleanValueStyle BooleanStyle { get; set; }

    public string AllowedValuesText { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? SourceNote { get; set; }

    public string IssueSummary { get; }
}

internal sealed class Ra2AllowedValuesTextParseResult
{
    public Ra2AllowedValuesTextParseResult(
        IReadOnlyList<Ra2FieldAllowedValue> values,
        IReadOnlyList<string> warnings)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public IReadOnlyList<Ra2FieldAllowedValue> Values { get; }

    public IReadOnlyList<string> Warnings { get; }
}
