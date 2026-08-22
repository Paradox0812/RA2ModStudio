using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2AddPropertyInsertPlanner
{
    public const string AddPropertyReason = "AddProperty";
    public const string ReplaceExistingReason = "AddPropertyReplaceExisting";

    public Ra2AddPropertyInsertPlan PlanInsert(
        Ra2IniTextDocument document,
        int caretOffset,
        string option,
        string? value)
    {
        ArgumentNullException.ThrowIfNull(document);

        string key = NormalizeOption(option);
        string lineText = $"{key}={value ?? string.Empty}";
        string newLine = ResolveNewLine(document);
        int normalizedCaret = Math.Clamp(caretOffset, 0, document.Text.Length);
        IReadOnlyList<string> warnings = BuildWarnings(document, normalizedCaret, key);
        if (document.Lines.Count == 0)
        {
            return new Ra2AddPropertyInsertPlan(
                new Ra2TextChange(new Ra2TextSpan(0, 0), lineText, AddPropertyReason),
                lineText.Length,
                warnings);
        }

        Ra2IniDocumentLine currentLine = FindCurrentLine(document, normalizedCaret);
        int insertOffset;
        string insertText;
        if (!string.IsNullOrEmpty(currentLine.LineBreak))
        {
            insertOffset = currentLine.Span.End + currentLine.LineBreak.Length;
            insertText = lineText + currentLine.LineBreak;
        }
        else
        {
            insertOffset = currentLine.Span.End;
            insertText = newLine + lineText;
        }

        return new Ra2AddPropertyInsertPlan(
            new Ra2TextChange(new Ra2TextSpan(insertOffset, 0), insertText, AddPropertyReason),
            insertOffset + insertText.Length - (insertText.EndsWith(lineText, StringComparison.Ordinal) ? 0 : currentLine.LineBreak.Length),
            warnings);
    }

    public Ra2AddPropertyInsertPlan PlanInsertDuplicate(
        Ra2IniTextDocument document,
        int caretOffset,
        string option,
        string? value)
        => PlanInsert(document, caretOffset, option, value);

    public Ra2AddPropertyInsertPlan PlanReplaceExisting(
        Ra2DuplicateKeyMatch match,
        string option,
        string? value)
    {
        ArgumentNullException.ThrowIfNull(match);

        NormalizeOption(option);
        string newValue = value ?? string.Empty;
        return new Ra2AddPropertyInsertPlan(
            new Ra2TextChange(match.ValueSpan, newValue, ReplaceExistingReason),
            match.ValueSpan.Start + newValue.Length,
            []);
    }

    private static string NormalizeOption(string option)
    {
        if (string.IsNullOrWhiteSpace(option))
            throw new ArgumentException("Option cannot be empty.", nameof(option));

        string key = option.Trim();
        if (key.Contains('='))
            throw new ArgumentException("Option cannot contain '='.", nameof(option));

        return key;
    }

    private static string ResolveNewLine(Ra2IniTextDocument document)
    {
        return document.NewLineKind switch
        {
            Ra2IniNewLineKind.CrLf => "\r\n",
            Ra2IniNewLineKind.Cr => "\r",
            _ => "\n"
        };
    }

    private static Ra2IniDocumentLine FindCurrentLine(Ra2IniTextDocument document, int caretOffset)
    {
        return document.Lines.FirstOrDefault(line => ContainsCaretOffset(line, caretOffset)) ??
               document.Lines.Last();
    }

    private static IReadOnlyList<string> BuildWarnings(Ra2IniTextDocument document, int caretOffset, string key)
    {
        if (document.Lines.Count == 0)
            return [];

        Ra2IniDocumentLine currentLine = FindCurrentLine(document, caretOffset);
        string? currentSection = FindSectionNameBefore(document, currentLine.LineNumber);
        if (currentSection is null)
            return [];

        bool duplicate = document.Lines.Any(line =>
            line.Kind == Ra2IniDocumentLineKind.KeyValue &&
            string.Equals(line.Key, key, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(FindSectionNameBefore(document, line.LineNumber), currentSection, StringComparison.OrdinalIgnoreCase));
        return duplicate
            ? [$"Current section may already contain key '{key}'."]
            : [];
    }

    private static bool ContainsCaretOffset(Ra2IniDocumentLine line, int caretOffset)
    {
        if (caretOffset >= line.Span.Start && caretOffset <= line.Span.End)
            return true;

        return caretOffset > line.Span.End &&
               caretOffset < line.Span.End + line.LineBreak.Length;
    }

    private static string? FindSectionNameBefore(Ra2IniTextDocument document, int lineNumber)
    {
        return document.Lines
            .Where(line => line.LineNumber <= lineNumber && line.Kind == Ra2IniDocumentLineKind.SectionHeader)
            .OrderByDescending(line => line.LineNumber)
            .Select(line => line.SectionName)
            .FirstOrDefault(sectionName => !string.IsNullOrWhiteSpace(sectionName));
    }
}
