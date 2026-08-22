using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Cleanup;

/// <summary>
/// Describes active field registry directories that should be scanned for safe generalization candidates.
/// </summary>
public sealed class FieldRegistryGeneralizationCleanupRequest
{
    public FieldRegistryGeneralizationCleanupRequest(
        string globalActiveDirectoryPath,
        string? projectActiveDirectoryPath = null)
    {
        GlobalActiveDirectoryPath = globalActiveDirectoryPath ?? throw new ArgumentNullException(nameof(globalActiveDirectoryPath));
        ProjectActiveDirectoryPath = projectActiveDirectoryPath;
    }

    public string GlobalActiveDirectoryPath { get; }

    public string? ProjectActiveDirectoryPath { get; }
}

/// <summary>
/// Describes a conservative request to apply safe single-pack cleanup rows.
/// </summary>
internal sealed class FieldRegistryGeneralizationCleanupApplyRequest
{
    public FieldRegistryGeneralizationCleanupApplyRequest(
        FieldRegistryApplyTargetScope targetScope,
        string? projectRootPath,
        string globalFieldRegistryRootPath,
        DateTimeOffset? timestamp = null)
    {
        TargetScope = targetScope;
        ProjectRootPath = string.IsNullOrWhiteSpace(projectRootPath) ? null : projectRootPath;
        GlobalFieldRegistryRootPath = string.IsNullOrWhiteSpace(globalFieldRegistryRootPath)
            ? throw new ArgumentException("Global field registry root path cannot be empty.", nameof(globalFieldRegistryRootPath))
            : globalFieldRegistryRootPath;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    public FieldRegistryApplyTargetScope TargetScope { get; }

    public string? ProjectRootPath { get; }

    public string GlobalFieldRegistryRootPath { get; }

    public DateTimeOffset Timestamp { get; }
}

/// <summary>
/// Contains the result of applying field registry generalization cleanup.
/// </summary>
public sealed class FieldRegistryGeneralizationCleanupApplyResult
{
    public FieldRegistryGeneralizationCleanupApplyResult(
        string targetFilePath,
        string? backupDirectoryPath,
        string? manifestFilePath,
        int addedCount,
        int updatedCount,
        int removedCount,
        int skippedCount,
        IReadOnlyList<string> warnings)
    {
        TargetFilePath = string.IsNullOrWhiteSpace(targetFilePath)
            ? throw new ArgumentException("Target file path cannot be empty.", nameof(targetFilePath))
            : targetFilePath;
        BackupDirectoryPath = backupDirectoryPath;
        ManifestFilePath = manifestFilePath;
        AddedCount = addedCount;
        UpdatedCount = updatedCount;
        RemovedCount = removedCount;
        SkippedCount = skippedCount;
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public string TargetFilePath { get; }

    public string? BackupDirectoryPath { get; }

    public string? ManifestFilePath { get; }

    public int AddedCount { get; }

    public int UpdatedCount { get; }

    public int RemovedCount { get; }

    public int SkippedCount { get; }

    public IReadOnlyList<string> Warnings { get; }
}

/// <summary>
/// Contains readonly field registry cleanup candidates.
/// </summary>
public sealed class FieldRegistryGeneralizationCleanupPlan
{
    internal FieldRegistryGeneralizationCleanupPlan(
        IReadOnlyList<FieldRegistryGeneralizationCleanupRow> rows,
        IReadOnlyList<string> warnings)
    {
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public IReadOnlyList<FieldRegistryGeneralizationCleanupRow> Rows { get; }

    public IReadOnlyList<string> Warnings { get; }
}

/// <summary>
/// Describes one possible replacement of repeated concrete field definitions with an abstract section definition.
/// </summary>
public sealed class FieldRegistryGeneralizationCleanupRow
{
    internal FieldRegistryGeneralizationCleanupRow(
        string scope,
        string key,
        Ra2SectionKind targetSectionKind,
        IReadOnlyList<Ra2SectionKind> sourceSectionKinds,
        IReadOnlyList<string> sourceFileNames,
        FieldEditorKind editorKind,
        Ra2FieldValueKind valueKind,
        int sourceFieldCount,
        int mergedAllowedValueCount,
        string actionText)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Key = key ?? throw new ArgumentNullException(nameof(key));
        TargetSectionKind = targetSectionKind;
        SourceSectionKinds = sourceSectionKinds ?? throw new ArgumentNullException(nameof(sourceSectionKinds));
        SourceFileNames = sourceFileNames ?? throw new ArgumentNullException(nameof(sourceFileNames));
        EditorKind = editorKind;
        ValueKind = valueKind;
        SourceFieldCount = sourceFieldCount;
        MergedAllowedValueCount = mergedAllowedValueCount;
        ActionText = actionText ?? throw new ArgumentNullException(nameof(actionText));
    }

    public string Scope { get; }

    public string Key { get; }

    public Ra2SectionKind TargetSectionKind { get; }

    public IReadOnlyList<Ra2SectionKind> SourceSectionKinds { get; }

    public IReadOnlyList<string> SourceFileNames { get; }

    public FieldEditorKind EditorKind { get; }

    public Ra2FieldValueKind ValueKind { get; }

    public int SourceFieldCount { get; }

    public int MergedAllowedValueCount { get; }

    public string ActionText { get; }
}
