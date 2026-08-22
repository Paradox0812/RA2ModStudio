using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2RecentFieldUsageTrackerTests
{
    [Fact]
    public void Record_StoresRecentFieldsPerSectionKind()
    {
        Ra2RecentFieldUsageTracker tracker = new();

        tracker.Record(Ra2SectionKind.Vehicle, "Strength");
        tracker.Record(Ra2SectionKind.Infantry, "Primary");

        Assert.Equal("Strength", Assert.Single(tracker.GetRecent(Ra2SectionKind.Vehicle, 10)).Key);
        Assert.Equal("Primary", Assert.Single(tracker.GetRecent(Ra2SectionKind.Infantry, 10)).Key);
    }

    [Fact]
    public void Record_DeduplicatesAndMovesReusedFieldToFront()
    {
        Ra2RecentFieldUsageTracker tracker = new();

        tracker.Record(Ra2SectionKind.Vehicle, "Strength");
        tracker.Record(Ra2SectionKind.Vehicle, "Armor");
        tracker.Record(Ra2SectionKind.Vehicle, "strength");

        Assert.Equal(["strength", "Armor"], tracker.GetRecent(Ra2SectionKind.Vehicle, 10).Select(item => item.Key).ToArray());
    }

    [Fact]
    public void GetRecent_LimitsReturnedCount()
    {
        Ra2RecentFieldUsageTracker tracker = new();
        tracker.Record(Ra2SectionKind.Vehicle, "A");
        tracker.Record(Ra2SectionKind.Vehicle, "B");
        tracker.Record(Ra2SectionKind.Vehicle, "C");

        Assert.Equal(["C", "B"], tracker.GetRecent(Ra2SectionKind.Vehicle, 2).Select(item => item.Key).ToArray());
    }
}
