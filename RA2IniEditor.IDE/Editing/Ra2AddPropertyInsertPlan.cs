using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2AddPropertyInsertPlan
{
    public Ra2AddPropertyInsertPlan(
        Ra2TextChange change,
        int caretOffset,
        IReadOnlyList<string>? warnings = null)
    {
        Change = change ?? throw new ArgumentNullException(nameof(change));
        CaretOffset = caretOffset;
        Warnings = warnings ?? [];
    }

    public Ra2TextChange Change { get; }

    public int CaretOffset { get; }

    public IReadOnlyList<string> Warnings { get; }
}
