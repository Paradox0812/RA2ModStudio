using System.Globalization;

namespace RA2IniEditor.IDE.Language;

internal static class Ra2IniLineParser
{
    public static bool TryParseSectionHeader(
        string text,
        int lineStart,
        int lineEnd,
        out ParsedSectionHeader header)
    {
        header = default;
        int firstNonWhite = FindFirstNonWhite(text, lineStart, lineEnd);
        if (firstNonWhite >= lineEnd || text[firstNonWhite] is ';' or '#' || text[firstNonWhite] != '[')
            return false;

        int closeBracket = IndexOf(text, ']', firstNonWhite + 1, lineEnd);
        if (closeBracket < 0)
            return false;

        string sectionName = text.Substring(firstNonWhite + 1, closeBracket - firstNonWhite - 1).Trim();
        if (string.IsNullOrWhiteSpace(sectionName))
            return false;

        header = new ParsedSectionHeader(
            sectionName,
            new Ra2TextSpan(firstNonWhite, closeBracket - firstNonWhite + 1));
        return true;
    }

    public static bool TryParseKeyValue(
        string text,
        int lineStart,
        int lineEnd,
        out ParsedKeyValueLine keyValue)
    {
        keyValue = default;
        int firstNonWhite = FindFirstNonWhite(text, lineStart, lineEnd);
        if (firstNonWhite >= lineEnd || text[firstNonWhite] is ';' or '#')
            return false;

        int equalsIndex = IndexOf(text, '=', lineStart, lineEnd);
        if (equalsIndex < 0)
            return false;

        int keyStart = FindFirstNonWhite(text, lineStart, equalsIndex);
        int keyEnd = FindLastNonWhiteExclusive(text, keyStart, equalsIndex);
        if (keyStart >= keyEnd)
            return false;

        int valueRawEnd = FindValueEndBeforeInlineComment(text, equalsIndex + 1, lineEnd);
        int valueStart = FindFirstNonWhite(text, equalsIndex + 1, valueRawEnd);
        int valueEnd = FindLastNonWhiteExclusive(text, valueStart, valueRawEnd);
        Ra2TextSpan? valueSpan = valueStart < valueEnd
            ? new Ra2TextSpan(valueStart, valueEnd - valueStart)
            : null;
        string value = valueSpan is null
            ? string.Empty
            : text.Substring(valueSpan.Value.Start, valueSpan.Value.Length);

        keyValue = new ParsedKeyValueLine(
            text.Substring(keyStart, keyEnd - keyStart),
            value,
            new Ra2TextSpan(keyStart, keyEnd - keyStart),
            valueSpan);
        return true;
    }

    public static bool TryGetFirstValueToken(
        string? value,
        Ra2TextSpan valueSpan,
        out string token,
        out Ra2TextSpan tokenSpan)
    {
        token = string.Empty;
        tokenSpan = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        int effectiveEnd = IndexOf(value, ';', 0, value.Length);
        if (effectiveEnd < 0)
            effectiveEnd = value.Length;

        int tokenStart = FindFirstNonWhite(value, 0, effectiveEnd);
        int tokenRawEnd = IndexOf(value, ',', tokenStart, effectiveEnd);
        if (tokenRawEnd < 0)
            tokenRawEnd = effectiveEnd;

        int tokenEnd = FindLastNonWhiteExclusive(value, tokenStart, tokenRawEnd);
        if (tokenStart >= tokenEnd)
            return false;

        token = value.Substring(tokenStart, tokenEnd - tokenStart);
        tokenSpan = new Ra2TextSpan(valueSpan.Start + tokenStart, tokenEnd - tokenStart);
        return true;
    }

    public static string GetEffectiveValue(string? rawValue)
    {
        if (string.IsNullOrEmpty(rawValue))
            return string.Empty;

        int commentIndex = rawValue.IndexOf(';');
        string value = commentIndex >= 0
            ? rawValue[..commentIndex]
            : rawValue;
        return value.Trim();
    }

    public static bool IsNumericLiteral(string value)
        => double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out _);

    public static string NormalizeSectionHeadersForClassification(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string[] lines = text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        string newline = text.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : text.Contains('\r')
                ? "\r"
                : "\n";
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];
            if (TryParseSectionHeader(line, 0, line.Length, out ParsedSectionHeader header))
                lines[index] = line.Substring(header.HeaderSpan.Start, header.HeaderSpan.Length);
        }

        return string.Join(newline, lines);
    }

    internal static int FindInlineCommentStart(string text, int start, int end, bool includeHash)
    {
        for (int index = start; index < end; index++)
        {
            if (text[index] == ';')
                return index;

            if (includeHash && text[index] == '#' && IsInlineCommentStart(text, start, index))
                return index;
        }

        return -1;
    }

    internal static string? ExtractInlineCommentText(string text, int markerIndex, int lineEnd)
    {
        if (markerIndex < 0 || markerIndex >= lineEnd)
            return null;

        string comment = text[(markerIndex + 1)..lineEnd].Trim();
        return string.IsNullOrWhiteSpace(comment) ? null : comment;
    }

    private static int FindValueEndBeforeInlineComment(string text, int start, int end)
    {
        int commentStart = FindInlineCommentStart(text, start, end, includeHash: true);
        return commentStart < 0 ? end : commentStart;
    }

    private static bool IsInlineCommentStart(string text, int valueStart, int markerIndex)
    {
        if (markerIndex <= valueStart)
            return true;

        return char.IsWhiteSpace(text[markerIndex - 1]);
    }

    internal static int FindLineEnd(string text, int start)
    {
        int index = start;
        while (index < text.Length && text[index] != '\r' && text[index] != '\n')
            index++;

        return index;
    }

    internal static int MoveToNextLine(string text, int lineEnd)
    {
        if (lineEnd >= text.Length)
            return text.Length;

        if (text[lineEnd] == '\r' && lineEnd + 1 < text.Length && text[lineEnd + 1] == '\n')
            return lineEnd + 2;

        return lineEnd + 1;
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

    internal readonly struct ParsedSectionHeader
    {
        public ParsedSectionHeader(string name, Ra2TextSpan headerSpan)
        {
            Name = name;
            HeaderSpan = headerSpan;
        }

        public string Name { get; }

        public Ra2TextSpan HeaderSpan { get; }
    }

    internal readonly struct ParsedKeyValueLine
    {
        public ParsedKeyValueLine(
            string key,
            string value,
            Ra2TextSpan keySpan,
            Ra2TextSpan? valueSpan)
        {
            Key = key;
            Value = value;
            KeySpan = keySpan;
            ValueSpan = valueSpan;
        }

        public string Key { get; }

        public string Value { get; }

        public Ra2TextSpan KeySpan { get; }

        public Ra2TextSpan? ValueSpan { get; }
    }
}
