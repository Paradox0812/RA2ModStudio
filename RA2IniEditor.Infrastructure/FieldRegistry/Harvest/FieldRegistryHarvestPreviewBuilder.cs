using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestPreviewBuilder : IFieldRegistryHarvestPreviewBuilder
{
    public FieldRegistryHarvestPreviewDraft BuildPreview(FieldRegistryHarvestNormalizeResult normalizeResult)
    {
        ArgumentNullException.ThrowIfNull(normalizeResult);

        List<Ra2FieldDefinition> definitions = new();
        List<FieldRegistryHarvestValidationIssue> issues = new(normalizeResult.Issues);

        foreach (FieldRegistryHarvestNormalizedCandidate candidate in normalizeResult.Candidates)
        {
            try
            {
                definitions.Add(new Ra2FieldDefinition(
                    candidate.Key,
                    candidate.AppliesTo,
                    candidate.EditorKind,
                    candidate.SourceKind,
                    candidate.Description,
                    CreateValueMetadata(candidate.EditorKind)));
            }
            catch (ArgumentException ex)
            {
                issues.Add(new FieldRegistryHarvestValidationIssue(
                    candidate.SourceName,
                    candidate.LineNumber,
                    candidate.Key,
                    FieldRegistryHarvestValidationSeverity.Error,
                    $"Failed to build preview definition for '{candidate.Key}': {ex.Message}"));
            }
        }

        return new FieldRegistryHarvestPreviewDraft(
            Array.AsReadOnly(definitions.ToArray()),
            Array.AsReadOnly(issues.ToArray()));
    }

    private static Ra2FieldValueMetadata CreateValueMetadata(FieldEditorKind editorKind)
    {
        return editorKind == FieldEditorKind.Boolean
            ? new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean, Ra2FieldBooleanValueStyle.YesNo)
            : Ra2FieldValueMetadata.Unknown;
    }
}
