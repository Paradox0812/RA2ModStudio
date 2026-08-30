using System.Windows;
using RA2IniEditor.IDE.Views.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelSemanticStrokePointerTests
{
    [Fact]
    public void Interpolation_IncludesEndpointAndKeepsMaximumFourDipSpacing()
    {
        IReadOnlyList<Point> points = Ra2VoxelViewport3D.InterpolateStrokePoints(new(0d, 0d), new(10d, 0d));

        Assert.Equal(3, points.Count);
        Assert.Equal(new Point(10d, 0d), points[^1]);
        Point previous = new(0d, 0d);
        foreach (Point point in points)
        {
            Assert.InRange((point - previous).Length, 0d, Ra2VoxelViewport3D.StrokeSampleSpacing);
            previous = point;
        }
    }

    [Fact]
    public void Interpolation_UsesOneEndpointForStationaryPointerAndRejectsExcessiveMove()
    {
        Assert.Equal([new Point(5d, 7d)],
            Ra2VoxelViewport3D.InterpolateStrokePoints(new(5d, 7d), new(5d, 7d)));
        Assert.Throws<InvalidOperationException>(() => Ra2VoxelViewport3D.InterpolateStrokePoints(
            new(0d, 0d),
            new((Ra2VoxelViewport3D.MaximumStrokeSamplesPerMove + 1) * Ra2VoxelViewport3D.StrokeSampleSpacing, 0d)));
    }
}
