using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Language;

internal sealed class Ra2ValueReferenceSymbol
{
    public Ra2ValueReferenceSymbol(
        string sourceSectionName,
        string sourceKey,
        string targetSectionName,
        Ra2SectionKind targetSectionKind,
        Ra2ValueReferenceKind referenceKind,
        int lineNumber,
        Ra2TextSpan valueSpan,
        string? inlineComment = null)
    {
        SourceSectionName = sourceSectionName;
        SourceKey = sourceKey;
        TargetSectionName = targetSectionName;
        TargetSectionKind = targetSectionKind;
        ReferenceKind = referenceKind;
        LineNumber = lineNumber;
        ValueSpan = valueSpan;
        InlineComment = inlineComment;
    }

    public string SourceSectionName { get; }

    public string SourceKey { get; }

    public string TargetSectionName { get; }

    public Ra2SectionKind TargetSectionKind { get; }

    public Ra2ValueReferenceKind ReferenceKind { get; }

    public int LineNumber { get; }

    public Ra2TextSpan ValueSpan { get; }

    public string? InlineComment { get; }
}
