using System.Windows;
using RA2IniEditor.IDE.ViewModels.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FloatingInspectorPlacementTests
{
    [Fact]
    public void PlaceNearCaret_UsesCaretBelowPositionWhenSpaceAllows()
    {
        Point result = Ra2FloatingInspectorPlacement.PlaceNearCaret(
            new Point(120, 140),
            new Size(300, 180),
            new Rect(0, 0, 1000, 800));

        Assert.Equal(120, result.X);
        Assert.Equal(146, result.Y);
    }

    [Fact]
    public void PlaceNearCaret_ClampsRightEdgeInsideWorkArea()
    {
        Point result = Ra2FloatingInspectorPlacement.PlaceNearCaret(
            new Point(960, 140),
            new Size(300, 180),
            new Rect(0, 0, 1000, 800));

        Assert.Equal(692, result.X);
        Assert.Equal(146, result.Y);
    }

    [Fact]
    public void PlaceNearCaret_PlacesAboveCaretWhenBelowWouldOverflow()
    {
        Point result = Ra2FloatingInspectorPlacement.PlaceNearCaret(
            new Point(120, 740),
            new Size(300, 180),
            new Rect(0, 0, 1000, 800));

        Assert.Equal(120, result.X);
        Assert.Equal(554, result.Y);
    }
}
