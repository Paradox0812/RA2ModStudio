using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.TextModel;

internal sealed class Ra2IniTextDocumentParser : IRa2IniTextDocumentParser
{
    public Ra2IniTextDocument Parse(string text)
    {
        text ??= string.Empty;

        List<Ra2IniDocumentLine> lines = [];
        List<Ra2IniNewLineKind> newlineKinds = [];
        int lineStart = 0;
        int lineNumber = 1;
        while (lineStart < text.Length)
        {
            int lineEnd = Ra2IniLineParser.FindLineEnd(text, lineStart);
            string lineBreak = GetLineBreak(text, lineEnd);
            if (!string.IsNullOrEmpty(lineBreak))
                newlineKinds.Add(GetNewLineKind(lineBreak));

            lines.Add(ParseLine(
                text,
                lineStart,
                lineEnd,
                lineBreak,
                lineNumber));

            lineStart = lineEnd + lineBreak.Length;
            lineNumber++;
        }

        return new Ra2IniTextDocument(
            text,
            lines,
            DetectDocumentNewLineKind(newlineKinds));
    }

    private static Ra2IniDocumentLine ParseLine(
        string text,
        int lineStart,
        int lineEnd,
        string lineBreak,
        int lineNumber)
    {
        string lineText = text.Substring(lineStart, lineEnd - lineStart);
        Ra2TextSpan lineSpan = new(lineStart, lineEnd - lineStart);

        if (string.IsNullOrWhiteSpace(lineText))
            return new Ra2IniDocumentLine(lineNumber, lineSpan, lineText, lineBreak, Ra2IniDocumentLineKind.Blank);

        int firstNonWhite = FindFirstNonWhite(text, lineStart, lineEnd);
        if (firstNonWhite < lineEnd && text[firstNonWhite] is ';' or '#')
        {
            return new Ra2IniDocumentLine(lineNumber, lineSpan, lineText, lineBreak, Ra2IniDocumentLineKind.Comment);
        }

        if (Ra2IniLineParser.TryParseSectionHeader(
            text,
            lineStart,
            lineEnd,
            out Ra2IniLineParser.ParsedSectionHeader header))
        {
            Ra2TextSpan sectionNameSpan = ResolveSectionNameSpan(text, header.HeaderSpan);
            Ra2TextSpan? inlineCommentSpan = FindInlineCommentSpan(text, header.HeaderSpan.End, lineEnd);
            return new Ra2IniDocumentLine(lineNumber, lineSpan, lineText, lineBreak, Ra2IniDocumentLineKind.SectionHeader)
            {
                SectionName = header.Name,
                SectionNameSpan = sectionNameSpan,
                InlineComment = inlineCommentSpan is Ra2TextSpan commentSpan
                    ? text.Substring(commentSpan.Start, commentSpan.Length)
                    : null,
                InlineCommentSpan = inlineCommentSpan
            };
        }

        if (Ra2IniLineParser.TryParseKeyValue(
            text,
            lineStart,
            lineEnd,
            out Ra2IniLineParser.ParsedKeyValueLine keyValue))
        {
            Ra2TextSpan? inlineCommentSpan = FindKeyValueInlineCommentSpan(text, keyValue.ValueSpan, lineStart, lineEnd);
            return new Ra2IniDocumentLine(lineNumber, lineSpan, lineText, lineBreak, Ra2IniDocumentLineKind.KeyValue)
            {
                Key = keyValue.Key,
                KeySpan = keyValue.KeySpan,
                Value = keyValue.Value,
                ValueSpan = keyValue.ValueSpan,
                InlineComment = inlineCommentSpan is Ra2TextSpan commentSpan
                    ? text.Substring(commentSpan.Start, commentSpan.Length)
                    : null,
                InlineCommentSpan = inlineCommentSpan
            };
        }

        return new Ra2IniDocumentLine(lineNumber, lineSpan, lineText, lineBreak, Ra2IniDocumentLineKind.Raw);
    }

    private static string GetLineBreak(string text, int lineEnd)
    {
        if (lineEnd >= text.Length)
            return string.Empty;

        if (text[lineEnd] == '\r' && lineEnd + 1 < text.Length && text[lineEnd + 1] == '\n')
            return "\r\n";

        return text[lineEnd] == '\r' ? "\r" : "\n";
    }

    private static Ra2IniNewLineKind DetectDocumentNewLineKind(IReadOnlyList<Ra2IniNewLineKind> newlineKinds)
    {
        if (newlineKinds.Count == 0)
            return Ra2IniNewLineKind.Unknown;

        Ra2IniNewLineKind first = newlineKinds[0];
        return newlineKinds.All(kind => kind == first)
            ? first
            : Ra2IniNewLineKind.Mixed;
    }

    private static Ra2IniNewLineKind GetNewLineKind(string lineBreak)
    {
        return lineBreak switch
        {
            "\n" => Ra2IniNewLineKind.Lf,
            "\r\n" => Ra2IniNewLineKind.CrLf,
            "\r" => Ra2IniNewLineKind.Cr,
            _ => Ra2IniNewLineKind.Unknown
        };
    }

    private static Ra2TextSpan ResolveSectionNameSpan(string text, Ra2TextSpan headerSpan)
    {
        int openBracket = headerSpan.Start;
        int closeBracket = headerSpan.End - 1;
        int nameStart = FindFirstNonWhite(text, openBracket + 1, closeBracket);
        int nameEnd = FindLastNonWhiteExclusive(text, nameStart, closeBracket);
        return new Ra2TextSpan(nameStart, Math.Max(0, nameEnd - nameStart));
    }

    private static Ra2TextSpan? FindKeyValueInlineCommentSpan(
        string text,
        Ra2TextSpan? valueSpan,
        int lineStart,
        int lineEnd)
    {
        int searchStart = valueSpan is Ra2TextSpan span
            ? span.End
            : IndexOf(text, '=', lineStart, lineEnd) + 1;
        if (searchStart <= 0)
            searchStart = lineStart;

        return FindInlineCommentSpan(text, searchStart, lineEnd);
    }

    private static Ra2TextSpan? FindInlineCommentSpan(string text, int start, int end)
    {
        for (int index = start; index < end; index++)
        {
            if (text[index] is (';' or '#') && IsInlineCommentStart(text, start, index))
            {
                int commentEnd = FindLastNonWhiteExclusive(text, index, end);
                return new Ra2TextSpan(index, commentEnd - index);
            }
        }

        return null;
    }

    private static bool IsInlineCommentStart(string text, int valueStart, int markerIndex)
    {
        if (markerIndex <= valueStart)
            return true;

        return char.IsWhiteSpace(text[markerIndex - 1]);
    }

    private static int FindFirstNonWhite(string text, int start, int end)
    {
        int index = start;
        while (index < end && char.IsWhiteSpace(text[index]))
            index++;

        return index;
    }

    private static int FindLastNonWhiteExclusive(string text, int start, int end)
    {
        int index = end;
        while (index > start && char.IsWhiteSpace(text[index - 1]))
            index--;

        return index;
    }

    private static int IndexOf(string text, char value, int start, int end)
    {
        for (int index = start; index < end; index++)
        {
            if (text[index] == value)
                return index;
        }

        return -1;
    }
}
