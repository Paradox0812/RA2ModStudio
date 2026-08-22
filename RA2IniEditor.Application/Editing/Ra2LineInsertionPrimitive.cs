using RA2IniEditor.Application.Language;
using RA2IniEditor.Application.TextModel;

namespace RA2IniEditor.Application.Editing;

internal static class Ra2LineInsertionPrimitive
{
    public static (Ra2TextChange Change, int CaretOffset) PlanAfterAnchor(
        Ra2IniTextDocument document,
        Ra2IniDocumentLine? anchor,
        string lineText,
        string reason)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(lineText);

        if (anchor is null)
        {
            Ra2TextChange emptyChange = new(new Ra2TextSpan(0, 0), lineText, reason);
            return (emptyChange, lineText.Length);
        }

        int insertOffset;
        string insertText;
        int caretOffset;
        if (!string.IsNullOrEmpty(anchor.LineBreak))
        {
            insertOffset = anchor.Span.End + anchor.LineBreak.Length;
            insertText = lineText + anchor.LineBreak;
            caretOffset = insertOffset + lineText.Length;
        }
        else
        {
            insertOffset = anchor.Span.End;
            insertText = ResolveNewLine(document) + lineText;
            caretOffset = insertOffset + insertText.Length;
        }

        return (
            new Ra2TextChange(new Ra2TextSpan(insertOffset, 0), insertText, reason),
            caretOffset);
    }

    private static string ResolveNewLine(Ra2IniTextDocument document)
        => document.NewLineKind switch
        {
            Ra2IniNewLineKind.CrLf => "\r\n",
            Ra2IniNewLineKind.Cr => "\r",
            _ => "\n"
        };
}
