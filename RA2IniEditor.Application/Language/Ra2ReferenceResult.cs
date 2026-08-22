using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Language;

internal sealed class Ra2ReferenceResult
{
    public Ra2ReferenceResult(
        string targetName,
        Ra2SectionKind targetKind,
        IReadOnlyList<Ra2ReferenceItem> items)
    {
        TargetName = targetName;
        TargetKind = targetKind;
        Items = items;
    }

    public string TargetName { get; }

    public Ra2SectionKind TargetKind { get; }

    public IReadOnlyList<Ra2ReferenceItem> Items { get; }
}
