using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Classification;

internal sealed class Ra2SectionClassificationResult
{
    public Ra2SectionClassificationResult(
        IReadOnlyDictionary<string, Ra2SectionKind> sectionKindsByName,
        IReadOnlyList<Ra2SectionClassificationWarning> warnings)
    {
        SectionKindsByName = sectionKindsByName;
        Warnings = warnings;
    }

    public IReadOnlyDictionary<string, Ra2SectionKind> SectionKindsByName { get; }

    public IReadOnlyList<Ra2SectionClassificationWarning> Warnings { get; }
}
