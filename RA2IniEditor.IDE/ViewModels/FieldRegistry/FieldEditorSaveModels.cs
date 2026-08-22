using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.ViewModels.FieldRegistry;

internal enum FieldEditorSaveTarget
{
    Project,
    Global
}

internal sealed class FieldEditorAllowedValueDraft
{
    public FieldEditorAllowedValueDraft(string value, string? displayName = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Allowed value cannot be empty.", nameof(value));

        Value = value.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public string Value { get; }

    public string? DisplayName { get; }

    public string? Description { get; }
}

internal sealed class FieldEditorDraft
{
    public FieldEditorDraft(
        string key,
        Ra2SectionKind sectionKind,
        FieldEditorKind editorKind,
        Ra2FieldValueKind valueKind,
        Ra2FieldBooleanValueStyle booleanStyle,
        string? enumName,
        IReadOnlyList<FieldEditorAllowedValueDraft> allowedValues,
        string? displayName,
        IReadOnlyList<string> aliases,
        string? description,
        FieldEditorSaveTarget saveTarget,
        string separator = ",",
        IReadOnlyList<string>? allowedValueInputErrors = null)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        SectionKind = sectionKind;
        EditorKind = editorKind;
        ValueKind = valueKind;
        BooleanStyle = booleanStyle;
        EnumName = string.IsNullOrWhiteSpace(enumName) ? null : enumName.Trim();
        AllowedValues = allowedValues ?? throw new ArgumentNullException(nameof(allowedValues));
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Aliases = aliases ?? throw new ArgumentNullException(nameof(aliases));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SaveTarget = saveTarget;
        Separator = separator;
        AllowedValueInputErrors = allowedValueInputErrors ?? [];
    }

    public string Key { get; }

    public Ra2SectionKind SectionKind { get; }

    public FieldEditorKind EditorKind { get; }

    public Ra2FieldValueKind ValueKind { get; }

    public Ra2FieldBooleanValueStyle BooleanStyle { get; }

    public string? EnumName { get; }

    public IReadOnlyList<FieldEditorAllowedValueDraft> AllowedValues { get; }

    public string? DisplayName { get; }

    public IReadOnlyList<string> Aliases { get; }

    public string? Description { get; }

    public FieldEditorSaveTarget SaveTarget { get; }

    public string Separator { get; }

    public IReadOnlyList<string> AllowedValueInputErrors { get; }
}

internal enum FieldEditorValidationSeverity
{
    Info,
    Warning,
    Error
}

internal sealed class FieldEditorValidationIssue
{
    public FieldEditorValidationIssue(FieldEditorValidationSeverity severity, string code, string message)
    {
        Severity = severity;
        Code = string.IsNullOrWhiteSpace(code) ? throw new ArgumentException("Issue code cannot be empty.", nameof(code)) : code;
        Message = string.IsNullOrWhiteSpace(message) ? throw new ArgumentException("Issue message cannot be empty.", nameof(message)) : message;
    }

    public FieldEditorValidationSeverity Severity { get; }

    public string Code { get; }

    public string Message { get; }
}

internal enum FieldEditorSaveOperationKind
{
    Add,
    Update,
    OverrideBuiltIn,
    NoChange,
    Blocked
}

internal sealed class FieldEditorSavePreview
{
    public FieldEditorSavePreview(
        FieldEditorSaveOperationKind operationKind,
        FieldEditorSaveTarget target,
        string key,
        Ra2SectionKind sectionKind,
        string summary,
        string persistedJsonPreview,
        IReadOnlyList<FieldEditorValidationIssue> issues,
        bool canSave)
    {
        OperationKind = operationKind;
        Target = target;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        SectionKind = sectionKind;
        Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        PersistedJsonPreview = persistedJsonPreview ?? throw new ArgumentNullException(nameof(persistedJsonPreview));
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
        CanSave = canSave;
    }

    public FieldEditorSaveOperationKind OperationKind { get; }

    public FieldEditorSaveTarget Target { get; }

    public string Key { get; }

    public Ra2SectionKind SectionKind { get; }

    public string Summary { get; }

    public string PersistedJsonPreview { get; }

    public IReadOnlyList<FieldEditorValidationIssue> Issues { get; }

    public bool CanSave { get; }
}

internal sealed class FieldEditorSaveContext
{
    public FieldEditorSaveContext(
        IRa2FieldDefinitionProvider effectiveProvider,
        IFieldRegistryProvenanceProvider provenanceProvider,
        string? projectRootPath,
        string globalFieldRegistryRootPath)
    {
        EffectiveProvider = effectiveProvider ?? throw new ArgumentNullException(nameof(effectiveProvider));
        ProvenanceProvider = provenanceProvider ?? throw new ArgumentNullException(nameof(provenanceProvider));
        ProjectRootPath = string.IsNullOrWhiteSpace(projectRootPath) ? null : projectRootPath;
        GlobalFieldRegistryRootPath = string.IsNullOrWhiteSpace(globalFieldRegistryRootPath)
            ? throw new ArgumentException("Global field registry root path cannot be empty.", nameof(globalFieldRegistryRootPath))
            : globalFieldRegistryRootPath;
    }

    public IRa2FieldDefinitionProvider EffectiveProvider { get; }

    public IFieldRegistryProvenanceProvider ProvenanceProvider { get; }

    public string? ProjectRootPath { get; }

    public string GlobalFieldRegistryRootPath { get; }
}

internal sealed class FieldEditorSaveApplyResult
{
    public FieldEditorSaveApplyResult(
        bool success,
        string message,
        FieldRegistryApplyWriteResult? writeResult,
        IReadOnlyList<FieldEditorValidationIssue> issues)
    {
        Success = success;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Apply result message cannot be empty.", nameof(message))
            : message;
        WriteResult = writeResult;
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
    }

    public bool Success { get; }

    public string Message { get; }

    public FieldRegistryApplyWriteResult? WriteResult { get; }

    public IReadOnlyList<FieldEditorValidationIssue> Issues { get; }
}
