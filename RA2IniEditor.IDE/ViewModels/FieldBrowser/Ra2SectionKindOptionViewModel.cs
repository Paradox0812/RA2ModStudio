using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels.FieldAnnotations;

namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2SectionKindOptionViewModel
{
    public Ra2SectionKindOptionViewModel(
        Ra2SectionKind? value,
        Ra2SectionKindDisplayNameProvider displayNameProvider)
    {
        Value = value;
        DisplayName = displayNameProvider.GetDisplayName(value);
    }

    public Ra2SectionKind? Value { get; }

    public string DisplayName { get; }
}
