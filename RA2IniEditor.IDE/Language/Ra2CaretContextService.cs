namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2CaretContextService : IRa2CaretContextService
{
    public Ra2CaretContext GetContext(Ra2DocumentSemanticModel model, int offset)
    {
        ArgumentNullException.ThrowIfNull(model);
        int normalizedOffset = Math.Clamp(offset, 0, model.Snapshot.Text.Length);
        Ra2SectionSymbol? section = model.FindSectionAtOffset(normalizedOffset);

        if (section is not null && section.HeaderSpan.Contains(normalizedOffset))
            return CreateContext(model, normalizedOffset, Ra2CaretRegion.SectionHeader, section, null, section.HeaderSpan);

        Ra2KeyValueSymbol? keyValue = model.FindKeyValueAtOffset(normalizedOffset);
        if (keyValue is not null)
        {
            if (keyValue.KeySpan.Contains(normalizedOffset))
                return CreateContext(model, normalizedOffset, Ra2CaretRegion.Key, section, keyValue, keyValue.KeySpan);

            if (keyValue.ValueSpan is Ra2TextSpan valueSpan && valueSpan.Contains(normalizedOffset))
                return CreateContext(model, normalizedOffset, Ra2CaretRegion.Value, section, keyValue, valueSpan);
        }

        if (IsCommentLine(model.Snapshot.Text, normalizedOffset, out Ra2TextSpan commentSpan))
            return CreateContext(model, normalizedOffset, Ra2CaretRegion.Comment, section, keyValue, commentSpan);

        if (IsWhitespaceAt(model.Snapshot.Text, normalizedOffset))
            return new Ra2CaretContext(normalizedOffset, Ra2CaretRegion.Whitespace, section, keyValue, null, null);

        return new Ra2CaretContext(normalizedOffset, Ra2CaretRegion.Unknown, section, keyValue, null, null);
    }

    private static Ra2CaretContext CreateContext(
        Ra2DocumentSemanticModel model,
        int offset,
        Ra2CaretRegion region,
        Ra2SectionSymbol? section,
        Ra2KeyValueSymbol? keyValue,
        Ra2TextSpan tokenSpan)
    {
        string tokenText = model.Snapshot.Text.Substring(tokenSpan.Start, tokenSpan.Length);
        return new Ra2CaretContext(offset, region, section, keyValue, tokenText, tokenSpan);
    }

    private static bool IsCommentLine(string text, int offset, out Ra2TextSpan commentSpan)
    {
        commentSpan = default;
        if (text.Length == 0)
            return false;

        int effectiveOffset = offset >= text.Length ? text.Length - 1 : offset;
        int lineStart = FindLineStart(text, effectiveOffset);
        int lineEnd = FindLineEnd(text, effectiveOffset);
        int firstNonWhite = lineStart;
        while (firstNonWhite < lineEnd && char.IsWhiteSpace(text[firstNonWhite]))
            firstNonWhite++;

        if (firstNonWhite >= lineEnd || text[firstNonWhite] is not (';' or '#'))
            return false;

        commentSpan = new Ra2TextSpan(firstNonWhite, lineEnd - firstNonWhite);
        return commentSpan.Contains(offset) || offset == lineEnd;
    }

    private static bool IsWhitespaceAt(string text, int offset)
    {
        if (text.Length == 0)
            return true;

        if (offset >= text.Length)
            return true;

        return char.IsWhiteSpace(text[offset]);
    }

    private static int FindLineStart(string text, int offset)
    {
        int index = Math.Min(offset, text.Length);
        while (index > 0 && text[index - 1] is not ('\r' or '\n'))
            index--;

        return index;
    }

    private static int FindLineEnd(string text, int offset)
    {
        int index = Math.Min(offset, text.Length);
        while (index < text.Length && text[index] is not ('\r' or '\n'))
            index++;

        return index;
    }
}
