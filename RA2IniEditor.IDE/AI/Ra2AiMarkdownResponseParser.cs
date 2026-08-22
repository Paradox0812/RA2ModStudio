namespace RA2IniEditor.IDE.AI;

internal static class Ra2AiMarkdownResponseParser
{
    public static IReadOnlyList<Ra2AiMarkdownBlock> Parse(string? markdown)
    {
        string source = markdown ?? string.Empty;
        if (source.Length == 0)
            return [new Ra2AiMarkdownBlock { Text = string.Empty }];

        List<Ra2AiMarkdownBlock> blocks = [];
        int position = 0;
        while (position < source.Length)
        {
            int openFenceStart = FindFenceLineStart(source, position);
            if (openFenceStart < 0)
            {
                AddTextBlocks(blocks, source[position..]);
                break;
            }

            if (openFenceStart > position)
                AddTextBlocks(blocks, source[position..openFenceStart]);

            int openFenceLineEnd = FindLineEnd(source, openFenceStart);
            int contentStart = SkipLineEnding(source, openFenceLineEnd);
            int closeFenceStart = FindFenceLineStart(source, contentStart);
            if (closeFenceStart < 0)
            {
                blocks.Clear();
                blocks.Add(new Ra2AiMarkdownBlock { Text = source });
                return blocks;
            }

            string language = source[(openFenceStart + 3)..openFenceLineEnd].Trim();
            string code = source[contentStart..closeFenceStart];
            blocks.Add(new Ra2AiMarkdownBlock
            {
                Kind = Ra2AiMarkdownBlockKind.Code,
                Language = string.IsNullOrWhiteSpace(language) ? null : language,
                Text = code
            });

            int closeFenceLineEnd = FindLineEnd(source, closeFenceStart);
            position = SkipLineEnding(source, closeFenceLineEnd);
        }

        return blocks;
    }

    private static void AddTextBlocks(List<Ra2AiMarkdownBlock> blocks, string text)
    {
        if (text.Length == 0)
            return;

        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        List<string> paragraphLines = [];
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index];
            string trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                FlushParagraph(blocks, paragraphLines);
                continue;
            }

            if (TryParseTable(lines, index, out Ra2AiMarkdownBlock? tableBlock, out int consumedLines))
            {
                FlushParagraph(blocks, paragraphLines);
                blocks.Add(tableBlock);
                index += consumedLines - 1;
                continue;
            }

            if (TryParseHeading(trimmed, out int headingLevel, out string headingText))
            {
                FlushParagraph(blocks, paragraphLines);
                blocks.Add(new Ra2AiMarkdownBlock
                {
                    Kind = Ra2AiMarkdownBlockKind.Heading,
                    HeadingLevel = headingLevel,
                    Text = headingText
                });
                continue;
            }

            if (TryParseBullet(trimmed, out string bulletText))
            {
                FlushParagraph(blocks, paragraphLines);
                blocks.Add(new Ra2AiMarkdownBlock
                {
                    Kind = Ra2AiMarkdownBlockKind.Bullet,
                    Text = bulletText
                });
                continue;
            }

            if (TryParseNumbered(trimmed, out string numberedText))
            {
                FlushParagraph(blocks, paragraphLines);
                blocks.Add(new Ra2AiMarkdownBlock
                {
                    Kind = Ra2AiMarkdownBlockKind.Numbered,
                    Text = numberedText
                });
                continue;
            }

            paragraphLines.Add(trimmed);
        }

        FlushParagraph(blocks, paragraphLines);
    }

    private static void FlushParagraph(List<Ra2AiMarkdownBlock> blocks, List<string> paragraphLines)
    {
        if (paragraphLines.Count == 0)
            return;

        blocks.Add(new Ra2AiMarkdownBlock
        {
            Kind = Ra2AiMarkdownBlockKind.Paragraph,
            Text = string.Join(Environment.NewLine, paragraphLines)
        });
        paragraphLines.Clear();
    }

    private static bool TryParseHeading(string line, out int level, out string text)
    {
        level = 0;
        text = string.Empty;
        while (level < line.Length && line[level] == '#')
            level++;

        if (level is < 1 or > 3 || level >= line.Length || line[level] != ' ')
            return false;

        text = line[(level + 1)..].Trim();
        return text.Length > 0;
    }

    private static bool TryParseBullet(string line, out string text)
    {
        text = string.Empty;
        if (line.Length < 3 || (line[0] != '-' && line[0] != '*') || line[1] != ' ')
            return false;

        text = line[2..].Trim();
        return text.Length > 0;
    }

    private static bool TryParseNumbered(string line, out string text)
    {
        text = string.Empty;
        int index = 0;
        while (index < line.Length && char.IsDigit(line[index]))
            index++;

        if (index == 0 || index + 1 >= line.Length || line[index] != '.' || line[index + 1] != ' ')
            return false;

        text = line[(index + 2)..].Trim();
        return text.Length > 0;
    }

    private static bool TryParseTable(
        string[] lines,
        int startIndex,
        out Ra2AiMarkdownBlock tableBlock,
        out int consumedLines)
    {
        tableBlock = new Ra2AiMarkdownBlock();
        consumedLines = 0;

        if (startIndex + 1 >= lines.Length)
            return false;

        if (!TryParseTableCells(lines[startIndex], out IReadOnlyList<string>? headers) || headers.Count < 2)
            return false;

        if (!TryParseTableCells(lines[startIndex + 1], out IReadOnlyList<string>? separatorCells)
            || separatorCells.Count != headers.Count
            || !separatorCells.All(IsTableSeparatorCell))
        {
            return false;
        }

        List<IReadOnlyList<string>> rows = [];
        int index = startIndex + 2;
        while (index < lines.Length
            && TryParseTableCells(lines[index], out IReadOnlyList<string>? rowCells)
            && rowCells.Count >= 2)
        {
            rows.Add(NormalizeTableRow(rowCells, headers.Count));
            index++;
        }

        tableBlock = new Ra2AiMarkdownBlock
        {
            Kind = Ra2AiMarkdownBlockKind.Table,
            TableHeaders = headers,
            TableRows = rows
        };
        consumedLines = index - startIndex;
        return true;
    }

    private static bool TryParseTableCells(string line, out IReadOnlyList<string> cells)
    {
        cells = [];
        string trimmed = line.Trim();
        if (!trimmed.Contains('|', StringComparison.Ordinal))
            return false;

        if (trimmed.StartsWith("|", StringComparison.Ordinal))
            trimmed = trimmed[1..];

        if (trimmed.EndsWith("|", StringComparison.Ordinal))
            trimmed = trimmed[..^1];

        string[] parts = trimmed.Split('|');
        if (parts.Length < 2)
            return false;

        cells = parts.Select(static part => part.Trim()).ToArray();
        return cells.Count > 1;
    }

    private static bool IsTableSeparatorCell(string cell)
    {
        string normalized = cell.Trim();
        if (normalized.Length < 3)
            return false;

        normalized = normalized.Trim(':');
        return normalized.Length >= 3 && normalized.All(static c => c == '-');
    }

    private static IReadOnlyList<string> NormalizeTableRow(IReadOnlyList<string> cells, int expectedCount)
    {
        string[] normalized = new string[expectedCount];
        for (int index = 0; index < normalized.Length; index++)
            normalized[index] = index < cells.Count ? cells[index] : string.Empty;

        return normalized;
    }

    private static int FindFenceLineStart(string source, int startIndex)
    {
        int index = startIndex;
        while (index < source.Length)
        {
            int candidate = source.IndexOf("```", index, StringComparison.Ordinal);
            if (candidate < 0)
                return -1;

            if (candidate == 0 || source[candidate - 1] == '\n' || source[candidate - 1] == '\r')
                return candidate;

            index = candidate + 3;
        }

        return -1;
    }

    private static int FindLineEnd(string source, int startIndex)
    {
        int index = startIndex;
        while (index < source.Length && source[index] is not '\r' and not '\n')
            index++;

        return index;
    }

    private static int SkipLineEnding(string source, int lineEndIndex)
    {
        if (lineEndIndex >= source.Length)
            return lineEndIndex;

        if (source[lineEndIndex] == '\r'
            && lineEndIndex + 1 < source.Length
            && source[lineEndIndex + 1] == '\n')
        {
            return lineEndIndex + 2;
        }

        return lineEndIndex + 1;
    }
}
