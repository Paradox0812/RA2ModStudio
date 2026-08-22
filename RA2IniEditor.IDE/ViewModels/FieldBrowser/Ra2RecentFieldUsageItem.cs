using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2RecentFieldUsageItem
{
    public Ra2RecentFieldUsageItem(Ra2SectionKind sectionKind, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Recent field key cannot be empty.", nameof(key));

        SectionKind = sectionKind;
        Key = key.Trim();
    }

    public Ra2SectionKind SectionKind { get; }

    public string Key { get; }
}
