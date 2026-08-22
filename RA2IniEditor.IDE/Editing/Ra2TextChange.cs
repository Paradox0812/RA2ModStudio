using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2TextChange
{
    public Ra2TextChange(Ra2TextSpan span, string newText, string reason)
    {
        Span = span;
        NewText = newText ?? throw new ArgumentNullException(nameof(newText));
        Reason = string.IsNullOrWhiteSpace(reason)
            ? throw new ArgumentException("Text change reason cannot be empty.", nameof(reason))
            : reason;
    }

    public Ra2TextSpan Span { get; }

    public string NewText { get; }

    public string Reason { get; }
}
