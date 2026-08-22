using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Diagnostics;

internal sealed class Ra2ReferenceDiagnosticCatalog
{
    private readonly Dictionary<string, Ra2ReferenceDiagnosticCatalogEntry> _entriesByName;

    public Ra2ReferenceDiagnosticCatalog(IEnumerable<Ra2ReferenceDiagnosticCatalogEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _entriesByName = entries
            .GroupBy(entry => entry.SectionName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public bool ContainsSection(string sectionName)
        => !string.IsNullOrWhiteSpace(sectionName) &&
           _entriesByName.ContainsKey(sectionName.Trim());

    public bool TryGetSection(string sectionName, out Ra2ReferenceDiagnosticCatalogEntry entry)
    {
        entry = null!;
        return !string.IsNullOrWhiteSpace(sectionName) &&
               _entriesByName.TryGetValue(sectionName.Trim(), out entry!);
    }
}

internal sealed class Ra2ReferenceDiagnosticCatalogEntry
{
    public Ra2ReferenceDiagnosticCatalogEntry(
        string sectionName,
        Ra2SectionKind sectionKind,
        string filePath,
        int lineNumber)
    {
        SectionName = sectionName;
        SectionKind = sectionKind;
        FilePath = filePath;
        LineNumber = lineNumber;
    }

    public string SectionName { get; }

    public Ra2SectionKind SectionKind { get; }

    public string FilePath { get; }

    public int LineNumber { get; }
}
