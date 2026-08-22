namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Resolves readonly section navigation targets from the current source text.
/// </summary>
public sealed class ReadonlySourceSectionNavigationResolver
{
    /// <summary>
    /// Resolves the real section header location from the current source text.
    /// </summary>
    public ReadonlySectionNavigationTarget? Resolve(
        string sourceText,
        string sectionId,
        int? preferredOneBasedLineNumber = null)
    {
        if (string.IsNullOrWhiteSpace(sourceText) || string.IsNullOrWhiteSpace(sectionId))
            return null;

        if (preferredOneBasedLineNumber is > 0 &&
            TryResolvePreferredLine(sourceText, sectionId, preferredOneBasedLineNumber.Value, out ReadonlySectionNavigationTarget? preferredTarget))
        {
            return preferredTarget;
        }

        return ScanSourceText(sourceText, sectionId);
    }

    private static bool TryResolvePreferredLine(
        string sourceText,
        string sectionId,
        int preferredOneBasedLineNumber,
        out ReadonlySectionNavigationTarget? target)
    {
        target = null;
        foreach (SourceLine line in EnumerateLines(sourceText))
        {
            if (line.OneBasedLineNumber != preferredOneBasedLineNumber)
                continue;

            if (TryCreateTarget(sourceText, line, sectionId, out target))
                return true;

            return false;
        }

        return false;
    }

    private static ReadonlySectionNavigationTarget? ScanSourceText(string sourceText, string sectionId)
    {
        foreach (SourceLine line in EnumerateLines(sourceText))
        {
            if (TryCreateTarget(sourceText, line, sectionId, out ReadonlySectionNavigationTarget? target))
                return target;
        }

        return null;
    }

    private static bool TryCreateTarget(
        string sourceText,
        SourceLine line,
        string sectionId,
        out ReadonlySectionNavigationTarget? target)
    {
        target = null;
        ReadOnlySpan<char> lineSpan = sourceText.AsSpan(line.StartIndex, line.Length);
        int leadingWhitespaceLength = CountLeadingWhitespace(lineSpan);
        ReadOnlySpan<char> trimmedStart = lineSpan[leadingWhitespaceLength..];
        if (!TryReadSectionId(trimmedStart, out ReadOnlySpan<char> candidate))
            return false;

        if (!candidate.Trim().Equals(sectionId.AsSpan().Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        target = new ReadonlySectionNavigationTarget(
            sectionId,
            line.OneBasedLineNumber,
            line.StartIndex + leadingWhitespaceLength);
        return true;
    }

    private static bool TryReadSectionId(ReadOnlySpan<char> trimmedStartLine, out ReadOnlySpan<char> sectionId)
    {
        sectionId = default;
        if (trimmedStartLine.IsEmpty || trimmedStartLine[0] != '[')
            return false;

        int closeBracketIndex = trimmedStartLine.IndexOf(']');
        if (closeBracketIndex <= 1)
            return false;

        ReadOnlySpan<char> suffix = trimmedStartLine[(closeBracketIndex + 1)..].TrimStart();
        if (!suffix.IsEmpty && suffix[0] != ';' && suffix[0] != '#')
            return false;

        sectionId = trimmedStartLine[1..closeBracketIndex];
        return true;
    }

    private static int CountLeadingWhitespace(ReadOnlySpan<char> value)
    {
        int count = 0;
        while (count < value.Length && char.IsWhiteSpace(value[count]))
            count++;

        return count;
    }

    private static IEnumerable<SourceLine> EnumerateLines(string sourceText)
    {
        int lineStart = 0;
        int oneBasedLineNumber = 1;

        while (lineStart <= sourceText.Length)
        {
            int lineEnd = FindLineEnd(sourceText, lineStart);
            yield return new SourceLine(lineStart, lineEnd - lineStart, oneBasedLineNumber);

            if (lineEnd >= sourceText.Length)
                yield break;

            lineStart = GetNextLineStart(sourceText, lineEnd);
            oneBasedLineNumber++;
        }
    }

    private static int FindLineEnd(string sourceText, int lineStart)
    {
        int index = lineStart;
        while (index < sourceText.Length && sourceText[index] != '\r' && sourceText[index] != '\n')
            index++;

        return index;
    }

    private static int GetNextLineStart(string sourceText, int lineEnd)
    {
        if (sourceText[lineEnd] == '\r' &&
            lineEnd + 1 < sourceText.Length &&
            sourceText[lineEnd + 1] == '\n')
        {
            return lineEnd + 2;
        }

        return lineEnd + 1;
    }

    private readonly record struct SourceLine(int StartIndex, int Length, int OneBasedLineNumber);
}
