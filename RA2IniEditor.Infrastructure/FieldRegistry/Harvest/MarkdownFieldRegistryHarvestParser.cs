namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class MarkdownFieldRegistryHarvestParser : IFieldRegistryHarvestParser
{
    public FieldRegistryHarvestParseResult Parse(FieldRegistryHarvestDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        List<FieldRegistryHarvestCandidate> candidates = new();
        List<FieldRegistryHarvestWarning> warnings = new();
        Dictionary<string, int> candidateIndexes = new(StringComparer.OrdinalIgnoreCase);
        TableHeader? currentTableHeader = null;

        int lineNumber = 1;
        foreach (string line in EnumerateLines(document.Text))
        {
            string trimmed = line.Trim();
            if (ShouldIgnoreLine(trimmed))
            {
                if (!IsTableLine(trimmed))
                    currentTableHeader = null;

                lineNumber++;
                continue;
            }

            if (TryParseTableLine(document, line, trimmed, lineNumber, currentTableHeader, candidates, warnings, candidateIndexes, out TableHeader? newHeader))
            {
                currentTableHeader = newHeader ?? currentTableHeader;
                lineNumber++;
                continue;
            }

            currentTableHeader = null;

            if (TryParseIniLikeLine(document, line, trimmed, lineNumber, out FieldRegistryHarvestCandidate? iniCandidate, warnings))
            {
                if (iniCandidate is not null)
                    AddCandidate(iniCandidate, candidates, warnings, candidateIndexes);

                lineNumber++;
                continue;
            }

            if (TryParseBulletLine(document, line, trimmed, lineNumber, out FieldRegistryHarvestCandidate? bulletCandidate, warnings))
            {
                if (bulletCandidate is not null)
                    AddCandidate(bulletCandidate, candidates, warnings, candidateIndexes);
            }

            lineNumber++;
        }

        return new FieldRegistryHarvestParseResult(
            Array.AsReadOnly(candidates.ToArray()),
            Array.AsReadOnly(warnings.ToArray()));
    }

    private static bool TryParseIniLikeLine(
        FieldRegistryHarvestDocument document,
        string rawLine,
        string trimmed,
        int lineNumber,
        out FieldRegistryHarvestCandidate? candidate,
        List<FieldRegistryHarvestWarning> warnings)
    {
        candidate = null;
        int equalsIndex = trimmed.IndexOf('=');
        if (equalsIndex < 0)
            return false;

        string key = trimmed[..equalsIndex].Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            warnings.Add(new FieldRegistryHarvestWarning(document.SourceName, lineNumber, "Skipped INI-like field because key is empty."));
            return true;
        }

        if (!IsValidKey(key))
        {
            warnings.Add(new FieldRegistryHarvestWarning(document.SourceName, lineNumber, $"Skipped INI-like field because key '{key}' is invalid."));
            return true;
        }

        candidate = new FieldRegistryHarvestCandidate(
            key,
            appliesToRaw: null,
            editorKindRaw: null,
            description: null,
            document.SourceName,
            lineNumber,
            rawLine,
            FieldRegistryHarvestConfidence.High);
        return true;
    }

    private static bool TryParseBulletLine(
        FieldRegistryHarvestDocument document,
        string rawLine,
        string trimmed,
        int lineNumber,
        out FieldRegistryHarvestCandidate? candidate,
        List<FieldRegistryHarvestWarning> warnings)
    {
        candidate = null;
        if (!trimmed.StartsWith("- ", StringComparison.Ordinal) &&
            !trimmed.StartsWith("* ", StringComparison.Ordinal))
        {
            return false;
        }

        string content = trimmed[2..].Trim();
        int separatorIndex = content.IndexOf(':');
        int separatorLength = 1;
        int dashIndex = content.IndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex < 0 || dashIndex >= 0 && dashIndex < separatorIndex)
        {
            separatorIndex = dashIndex;
            separatorLength = 3;
        }

        if (separatorIndex < 0)
            return false;

        string key = content[..separatorIndex].Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            warnings.Add(new FieldRegistryHarvestWarning(document.SourceName, lineNumber, "Skipped bullet field because key is empty."));
            return true;
        }

        if (!IsValidKey(key))
        {
            warnings.Add(new FieldRegistryHarvestWarning(document.SourceName, lineNumber, $"Skipped bullet field because key '{key}' is invalid."));
            return true;
        }

        string description = content[(separatorIndex + separatorLength)..].Trim();
        candidate = new FieldRegistryHarvestCandidate(
            key,
            appliesToRaw: null,
            editorKindRaw: null,
            description,
            document.SourceName,
            lineNumber,
            rawLine,
            FieldRegistryHarvestConfidence.Medium);
        return true;
    }

    private static bool TryParseTableLine(
        FieldRegistryHarvestDocument document,
        string rawLine,
        string trimmed,
        int lineNumber,
        TableHeader? currentHeader,
        List<FieldRegistryHarvestCandidate> candidates,
        List<FieldRegistryHarvestWarning> warnings,
        Dictionary<string, int> candidateIndexes,
        out TableHeader? newHeader)
    {
        newHeader = null;
        if (!IsTableLine(trimmed))
            return false;

        string[] cells = SplitTableCells(trimmed);
        if (cells.Length == 0 || IsTableSeparator(cells))
            return true;

        TableHeader? parsedHeader = TableHeader.TryCreate(cells);
        if (parsedHeader is not null)
        {
            newHeader = parsedHeader;
            return true;
        }

        if (currentHeader is null)
            return true;

        if (cells.Length < currentHeader.ColumnCount)
        {
            warnings.Add(new FieldRegistryHarvestWarning(document.SourceName, lineNumber, "Skipped table row because column count is lower than the header."));
            return true;
        }

        string key = cells[currentHeader.KeyIndex].Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            warnings.Add(new FieldRegistryHarvestWarning(document.SourceName, lineNumber, "Skipped table row because key is empty."));
            return true;
        }

        if (!IsValidKey(key))
        {
            warnings.Add(new FieldRegistryHarvestWarning(document.SourceName, lineNumber, $"Skipped table row because key '{key}' is invalid."));
            return true;
        }

        FieldRegistryHarvestCandidate candidate = new(
            key,
            currentHeader.AppliesToIndex >= 0 ? cells[currentHeader.AppliesToIndex] : null,
            currentHeader.EditorKindIndex >= 0 ? cells[currentHeader.EditorKindIndex] : null,
            currentHeader.DescriptionIndex >= 0 ? cells[currentHeader.DescriptionIndex] : null,
            document.SourceName,
            lineNumber,
            rawLine,
            FieldRegistryHarvestConfidence.High);
        AddCandidate(candidate, candidates, warnings, candidateIndexes);
        return true;
    }

    private static void AddCandidate(
        FieldRegistryHarvestCandidate candidate,
        List<FieldRegistryHarvestCandidate> candidates,
        List<FieldRegistryHarvestWarning> warnings,
        Dictionary<string, int> candidateIndexes)
    {
        if (!candidateIndexes.TryGetValue(candidate.Key, out int existingIndex))
        {
            candidateIndexes[candidate.Key] = candidates.Count;
            candidates.Add(candidate);
            return;
        }

        FieldRegistryHarvestCandidate existing = candidates[existingIndex];
        if (candidate.Confidence > existing.Confidence)
            candidates[existingIndex] = candidate;

        warnings.Add(new FieldRegistryHarvestWarning(
            candidate.SourceName,
            candidate.LineNumber,
            $"Skipped duplicate field candidate '{candidate.Key}'."));
    }

    private static bool ShouldIgnoreLine(string trimmed)
    {
        if (trimmed.Length == 0)
            return true;

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
            return true;

        if (trimmed.StartsWith('#'))
            return true;

        return trimmed.All(character => character == '-') ||
               trimmed.All(character => character == '*');
    }

    private static bool IsTableLine(string trimmed)
        => trimmed.StartsWith('|') && trimmed.EndsWith('|') && trimmed.Length >= 2;

    private static string[] SplitTableCells(string trimmed)
    {
        string content = trimmed.Trim('|');
        return content.Split('|')
            .Select(cell => cell.Trim())
            .ToArray();
    }

    private static bool IsTableSeparator(IReadOnlyList<string> cells)
    {
        return cells.Count > 0 &&
               cells.All(cell => cell.Length > 0 && cell.All(character => character == '-' || character == ':' || char.IsWhiteSpace(character)));
    }

    private static bool IsValidKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        foreach (char character in key)
        {
            if (char.IsLetterOrDigit(character) || character is '_' or '.' or '-')
                continue;

            return false;
        }

        return true;
    }

    private static IEnumerable<string> EnumerateLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        int lineStart = 0;
        while (lineStart < text.Length)
        {
            int lineEnd = lineStart;
            while (lineEnd < text.Length && text[lineEnd] != '\r' && text[lineEnd] != '\n')
                lineEnd++;

            yield return text[lineStart..lineEnd];

            if (lineEnd >= text.Length)
                yield break;

            if (text[lineEnd] == '\r' && lineEnd + 1 < text.Length && text[lineEnd + 1] == '\n')
                lineStart = lineEnd + 2;
            else
                lineStart = lineEnd + 1;
        }
    }

    private sealed class TableHeader
    {
        private TableHeader(int keyIndex, int appliesToIndex, int editorKindIndex, int descriptionIndex, int columnCount)
        {
            KeyIndex = keyIndex;
            AppliesToIndex = appliesToIndex;
            EditorKindIndex = editorKindIndex;
            DescriptionIndex = descriptionIndex;
            ColumnCount = columnCount;
        }

        public int KeyIndex { get; }

        public int AppliesToIndex { get; }

        public int EditorKindIndex { get; }

        public int DescriptionIndex { get; }

        public int ColumnCount { get; }

        public static TableHeader? TryCreate(IReadOnlyList<string> cells)
        {
            int keyIndex = -1;
            int appliesToIndex = -1;
            int editorKindIndex = -1;
            int descriptionIndex = -1;

            for (int index = 0; index < cells.Count; index++)
            {
                string normalized = NormalizeHeader(cells[index]);
                switch (normalized)
                {
                    case "key":
                        keyIndex = index;
                        break;
                    case "appliesto":
                        appliesToIndex = index;
                        break;
                    case "type":
                    case "editorkind":
                        editorKindIndex = index;
                        break;
                    case "description":
                        descriptionIndex = index;
                        break;
                }
            }

            return keyIndex < 0
                ? null
                : new TableHeader(keyIndex, appliesToIndex, editorKindIndex, descriptionIndex, cells.Count);
        }

        private static string NormalizeHeader(string header)
            => new(header
                .Where(character => !char.IsWhiteSpace(character) && character != '-' && character != '_')
                .Select(char.ToLowerInvariant)
                .ToArray());
    }
}
