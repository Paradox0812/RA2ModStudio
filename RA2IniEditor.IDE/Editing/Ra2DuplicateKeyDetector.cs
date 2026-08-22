using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2DuplicateKeyDetector
{
    public bool ContainsKeyInCurrentSection(Ra2IniTextDocument document, int caretOffset, string key)
        => FindInCurrentSection(document, caretOffset, key) is not null;

    public Ra2DuplicateKeyMatch? FindInCurrentSection(Ra2IniTextDocument document, int caretOffset, string key)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (string.IsNullOrWhiteSpace(key) || document.Lines.Count == 0)
            return null;

        int normalizedCaret = Math.Clamp(caretOffset, 0, document.Text.Length);
        Ra2IniDocumentLine currentLine = FindCurrentLine(document, normalizedCaret);
        if (!TryFindCurrentSectionBounds(document, currentLine.LineNumber, out int sectionStartLine, out int sectionEndLine))
            return null;

        Ra2IniDocumentLine? match = document.Lines.FirstOrDefault(line =>
            line.Kind == Ra2IniDocumentLineKind.KeyValue &&
            line.LineNumber > sectionStartLine &&
            line.LineNumber < sectionEndLine &&
            string.Equals(line.Key, key.Trim(), StringComparison.OrdinalIgnoreCase) &&
            line.KeySpan is not null);
        if (match is null)
            return null;

        return new Ra2DuplicateKeyMatch(
            match.Key ?? key,
            match.LineNumber,
            match.Span,
            ResolveValueReplacementSpan(match),
            match.Value ?? string.Empty);
    }

    private static Ra2IniDocumentLine FindCurrentLine(Ra2IniTextDocument document, int caretOffset)
    {
        return document.Lines.FirstOrDefault(line =>
            caretOffset >= line.Span.Start && caretOffset <= line.Span.End ||
            caretOffset > line.Span.End && caretOffset < line.Span.End + line.LineBreak.Length) ??
            document.Lines.Last();
    }

    private static bool TryFindCurrentSectionBounds(
        Ra2IniTextDocument document,
        int lineNumber,
        out int sectionStartLine,
        out int sectionEndLine)
    {
        sectionStartLine = 0;
        sectionEndLine = int.MaxValue;
        Ra2IniDocumentLine? header = document.Lines
            .Where(line => line.LineNumber <= lineNumber &&
                           line.Kind == Ra2IniDocumentLineKind.SectionHeader)
            .OrderByDescending(line => line.LineNumber)
            .FirstOrDefault();
        if (header is null)
            return false;

        sectionStartLine = header.LineNumber;
        sectionEndLine = document.Lines
            .Where(line => line.LineNumber > header.LineNumber &&
                           line.Kind == Ra2IniDocumentLineKind.SectionHeader)
            .OrderBy(line => line.LineNumber)
            .Select(line => line.LineNumber)
            .FirstOrDefault();
        if (sectionEndLine == 0)
            sectionEndLine = int.MaxValue;

        return true;
    }

    private static Ra2TextSpan ResolveValueReplacementSpan(Ra2IniDocumentLine line)
    {
        if (line.ValueSpan is Ra2TextSpan valueSpan)
            return valueSpan;

        int equalsIndex = line.Text.IndexOf('=');
        int valueStart = equalsIndex < 0
            ? line.Span.End
            : line.Span.Start + equalsIndex + 1;
        return new Ra2TextSpan(valueStart, 0);
    }
}
