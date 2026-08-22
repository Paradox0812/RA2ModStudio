namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;

internal interface IRa2FieldImportDraftBuilder
{
    IReadOnlyList<Ra2FieldImportDraftRow> BuildDraft(Ra2IniFieldHarvestResult result);

    FieldRegistryHarvestPreviewDraft BuildPreviewFromDraft(IReadOnlyList<Ra2FieldImportDraftRow> rows);
}
