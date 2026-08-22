using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2TextChangeSetTests
{
    [Fact]
    public void Apply_UsesOriginalCoordinatesFromEndToStart()
    {
        Ra2TextChangeSet changeSet = new(
        [
            new Ra2TextChange(new Ra2TextSpan(0, 3), "Primary", "Replace all"),
            new Ra2TextChange(new Ra2TextSpan(8, 3), "Secondary", "Replace all")
        ]);

        string result = changeSet.Apply("Gun and Gun");

        Assert.Equal("Primary and Secondary", result);
    }

    [Fact]
    public void Constructor_SortsNonOverlappingChanges()
    {
        Ra2TextChangeSet changeSet = new(
        [
            new Ra2TextChange(new Ra2TextSpan(4, 1), "B", "Second"),
            new Ra2TextChange(new Ra2TextSpan(0, 1), "A", "First")
        ]);

        Assert.Equal([0, 4], changeSet.Changes.Select(change => change.Span.Start));
    }

    [Fact]
    public void Constructor_RejectsOverlappingChanges()
    {
        Assert.Throws<ArgumentException>(() => new Ra2TextChangeSet(
        [
            new Ra2TextChange(new Ra2TextSpan(0, 4), "A", "First"),
            new Ra2TextChange(new Ra2TextSpan(3, 2), "B", "Second")
        ]));
    }

    [Fact]
    public void Apply_RejectsSpanOutsideSourceText()
    {
        Ra2TextChangeSet changeSet = new(
        [
            new Ra2TextChange(new Ra2TextSpan(4, 2), "B", "Out of range")
        ]);

        Assert.Throws<ArgumentOutOfRangeException>(() => changeSet.Apply("abcd"));
    }
}
