using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply;

internal sealed class FieldRegistryApplyPlanItem
{
    public FieldRegistryApplyPlanItem(
        string key,
        Ra2SectionKind appliesTo,
        FieldRegistryApplyOperationKind operationKind,
        FieldRegistryApplyTargetScope targetScope,
        FieldRegistryProvenanceScope existingScope,
        string existingSourceName,
        Ra2FieldDefinition previewDefinition,
        string message)
    {
        Key = string.IsNullOrWhiteSpace(key)
            ? throw new ArgumentException("Apply plan item key cannot be empty.", nameof(key))
            : key;
        AppliesTo = appliesTo;
        OperationKind = operationKind;
        TargetScope = targetScope;
        ExistingScope = existingScope;
        ExistingSourceName = existingSourceName ?? string.Empty;
        PreviewDefinition = previewDefinition ?? throw new ArgumentNullException(nameof(previewDefinition));
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Apply plan item message cannot be empty.", nameof(message))
            : message;
    }

    public string Key { get; }

    public Ra2SectionKind AppliesTo { get; }

    public FieldRegistryApplyOperationKind OperationKind { get; }

    public FieldRegistryApplyTargetScope TargetScope { get; }

    public FieldRegistryProvenanceScope ExistingScope { get; }

    public string ExistingSourceName { get; }

    public Ra2FieldDefinition PreviewDefinition { get; }

    public string Message { get; }
}
