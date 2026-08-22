using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2RecentFieldUsageTracker
{
    private readonly List<Ra2RecentFieldUsageItem> _items = new();

    public void Record(Ra2SectionKind sectionKind, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        string normalizedKey = key.Trim();
        _items.RemoveAll(item =>
            item.SectionKind == sectionKind &&
            string.Equals(item.Key, normalizedKey, StringComparison.OrdinalIgnoreCase));
        _items.Insert(0, new Ra2RecentFieldUsageItem(sectionKind, normalizedKey));
    }

    public IReadOnlyList<Ra2RecentFieldUsageItem> GetRecent(Ra2SectionKind sectionKind, int maxCount)
    {
        if (maxCount <= 0)
            return [];

        return _items
            .Where(item => item.SectionKind == sectionKind)
            .Take(maxCount)
            .ToArray();
    }
}
