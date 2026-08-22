using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestNormalizeOptions
{
    public static FieldRegistryHarvestNormalizeOptions Default { get; } = new();

    public Ra2FieldSourceKind DefaultSourceKind { get; init; } = Ra2FieldSourceKind.External;

    public FieldEditorKind DefaultEditorKind { get; init; } = FieldEditorKind.Text;

    public Ra2SectionKind DefaultAppliesTo { get; init; } = Ra2SectionKind.Unknown;

    public bool AllowUnknownAppliesTo { get; init; } = true;

    public bool AllowUnknownEditorKind { get; init; } = true;
}
