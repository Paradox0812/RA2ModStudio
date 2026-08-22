using System.IO;
using RA2IniEditor.IDE.Models;

namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Builds a lightweight readonly section index from the currently loaded INI source text.
/// </summary>
public sealed class ReadonlyNavigatorIndexService
{
    /// <summary>
    /// Builds a lightweight section index without reading files or using the full INI parser.
    /// </summary>
    public IReadOnlyList<ReadonlySectionIndexItem> BuildSectionIndex(string sourceText)
    {
        List<ReadonlySectionIndexItem> items = [];
        PendingSection? current = null;
        using StringReader reader = new(sourceText);

        int lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            string trimmed = line.Trim();

            if (TryReadSectionId(trimmed, out string? sectionId))
            {
                AddCurrentSection(items, current);
                current = new PendingSection(sectionId!, lineNumber);
                continue;
            }

            if (current is not null)
                TryCaptureDisplayField(current, trimmed);
        }

        AddCurrentSection(items, current);
        return items;
    }

    private static bool TryReadSectionId(string trimmedLine, out string? sectionId)
    {
        sectionId = null;
        if (trimmedLine.StartsWith(';') || trimmedLine.StartsWith('#'))
            return false;

        if (!trimmedLine.StartsWith('['))
            return false;

        int closeBracketIndex = trimmedLine.IndexOf(']');
        if (closeBracketIndex <= 1)
            return false;

        string suffix = trimmedLine[(closeBracketIndex + 1)..].TrimStart();
        if (suffix.Length > 0 && suffix[0] is not ';' and not '#')
            return false;

        string candidate = trimmedLine[1..closeBracketIndex].Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        sectionId = candidate;
        return true;
    }

    private static void TryCaptureDisplayField(PendingSection section, string trimmedLine)
    {
        if (trimmedLine.Length == 0 || trimmedLine.StartsWith(';') || trimmedLine.StartsWith('#'))
            return;

        int separatorIndex = trimmedLine.IndexOf('=');
        if (separatorIndex <= 0)
            return;

        string key = trimmedLine[..separatorIndex].Trim();
        string value = trimmedLine[(separatorIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (section.Name is null && string.Equals(key, "Name", StringComparison.OrdinalIgnoreCase))
        {
            section.Name = value;
            return;
        }

        if (section.UiName is null && string.Equals(key, "UIName", StringComparison.OrdinalIgnoreCase))
        {
            section.UiName = value;
            return;
        }

        if (section.Image is null && string.Equals(key, "Image", StringComparison.OrdinalIgnoreCase))
            section.Image = value;
    }

    private static void AddCurrentSection(List<ReadonlySectionIndexItem> items, PendingSection? section)
    {
        if (section is null)
            return;

        items.Add(new ReadonlySectionIndexItem(
            section.SectionId,
            section.LineNumber,
            section.Name ?? section.UiName ?? section.Image));
    }

    private sealed class PendingSection
    {
        public PendingSection(string sectionId, int lineNumber)
        {
            SectionId = sectionId;
            LineNumber = lineNumber;
        }

        public string SectionId { get; }

        public int LineNumber { get; }

        public string? Name { get; set; }

        public string? UiName { get; set; }

        public string? Image { get; set; }
    }
}
