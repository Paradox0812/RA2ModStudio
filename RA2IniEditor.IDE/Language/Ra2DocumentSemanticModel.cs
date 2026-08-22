using RA2IniEditor.IDE.Classification;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2DocumentSemanticModel
{
    private readonly Dictionary<string, Ra2SectionSymbol> _sectionsByName;

    public Ra2DocumentSemanticModel(
        Ra2DocumentSnapshot snapshot,
        Ra2SectionClassificationResult classification,
        IReadOnlyList<Ra2SectionSymbol> sections,
        IReadOnlyList<Ra2KeyValueSymbol> keyValues,
        IReadOnlyList<Ra2ValueReferenceSymbol> references)
    {
        Snapshot = snapshot;
        Classification = classification;
        Sections = sections;
        KeyValues = keyValues;
        References = references;
        _sectionsByName = sections
            .GroupBy(section => section.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public Ra2DocumentSnapshot Snapshot { get; }

    public Ra2SectionClassificationResult Classification { get; }

    public IReadOnlyList<Ra2SectionSymbol> Sections { get; }

    public IReadOnlyList<Ra2KeyValueSymbol> KeyValues { get; }

    public IReadOnlyList<Ra2ValueReferenceSymbol> References { get; }

    public Ra2SectionSymbol? FindSectionAtOffset(int offset)
        => Sections.FirstOrDefault(section => section.HeaderSpan.Contains(offset) || section.BodySpan.Contains(offset));

    public Ra2KeyValueSymbol? FindKeyValueAtOffset(int offset)
        => KeyValues.FirstOrDefault(keyValue => keyValue.LineSpan.Contains(offset));

    public Ra2SectionSymbol? FindSectionByName(string sectionName)
        => _sectionsByName.TryGetValue(sectionName, out Ra2SectionSymbol? section) ? section : null;
}
