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
        int normalizedCaret = Math.Clamp(caretOffset, 0, document.Text.Length);
        IReadOnlyList<string> warnings = BuildWarnings(document, normalizedCaret, key);
        if (document.Lines.Count == 0)
        {
            (Ra2TextChange change, int resultCaret) = Ra2LineInsertionPrimitive.PlanAfterAnchor(
                document,
                anchor: null,
                lineText,
                AddPropertyReason);
            return new Ra2AddPropertyInsertPlan(
                change,
                resultCaret,
                warnings);
        }

        Ra2IniDocumentLine currentLine = FindCurrentLine(document, normalizedCaret);
        (Ra2TextChange insertChange, int insertCaret) = Ra2LineInsertionPrimitive.PlanAfterAnchor(
            document,
            currentLine,
            lineText,
            AddPropertyReason);

        return new Ra2AddPropertyInsertPlan(
            insertChange,
            insertCaret,
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
