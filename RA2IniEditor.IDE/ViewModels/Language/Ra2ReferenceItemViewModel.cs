using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal sealed class Ra2ReferenceItemViewModel
{
    public Ra2ReferenceItemViewModel(Ra2ReferenceItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Section = item.SourceSectionName;
        Key = item.SourceKey;
        Value = item.Value;
        Line = item.LineNumber;
        LineText = item.LineNumber.ToString();
        ValueSpanStart = item.ValueSpan.Start;
    }

    public string Section { get; }

    public string Key { get; }

    public string Value { get; }

    public int Line { get; }

    public string LineText { get; }

    public int ValueSpanStart { get; }
}
