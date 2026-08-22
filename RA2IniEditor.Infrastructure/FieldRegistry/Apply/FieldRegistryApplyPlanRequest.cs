using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply;

internal sealed class FieldRegistryApplyPlanRequest
{
    public FieldRegistryApplyPlanRequest(
        FieldRegistryHarvestPreviewDraft previewDraft,
        FieldRegistryHarvestDiffResult diffResult,
        FieldRegistryApplyTargetScope targetScope,
        FieldRegistryApplyMode mode)
    {
        PreviewDraft = previewDraft ?? throw new ArgumentNullException(nameof(previewDraft));
        DiffResult = diffResult ?? throw new ArgumentNullException(nameof(diffResult));
        TargetScope = targetScope;
        Mode = mode;
    }

    public FieldRegistryHarvestPreviewDraft PreviewDraft { get; }

    public FieldRegistryHarvestDiffResult DiffResult { get; }

    public FieldRegistryApplyTargetScope TargetScope { get; }

    public FieldRegistryApplyMode Mode { get; }
}
