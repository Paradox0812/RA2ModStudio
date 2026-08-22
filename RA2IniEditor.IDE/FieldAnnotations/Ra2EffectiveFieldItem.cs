using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2EffectiveFieldItem
{
    public Ra2EffectiveFieldItem(
        Ra2SectionKind sectionKind,
        Ra2FieldApplicabilityKind applicability,
        Ra2FieldDisplayInfo displayInfo)
    {
        SectionKind = sectionKind;
        Applicability = applicability;
        DisplayInfo = displayInfo ?? throw new ArgumentNullException(nameof(displayInfo));
    }

    public Ra2SectionKind SectionKind { get; }

    public Ra2FieldApplicabilityKind Applicability { get; }

    public Ra2FieldDisplayInfo DisplayInfo { get; }

    public string Key => DisplayInfo.Key;
}
