using System.Windows.Media.Media3D;
using RA2IniEditor.IDE.Views.AssetAuthoring;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2VoxelViewportCameraStateTests
{
    [Fact]
    public void CaptureAndRestore_MapsPoseAcrossCompatibleBounds()
    {
        Rect3D sourceBounds = new(10d, 20d, 30d, 20d, 40d, 10d);
        Point3D sourceTarget = new(15d, 50d, 35d);

        bool captured = Ra2VoxelViewportCameraState.TryCapture(
            sourceBounds,
            sourceTarget,
            distance: 70d,
            yaw: 0.75d,
            pitch: -0.25d,
            hasUserInteraction: true,
            out Ra2VoxelViewportCameraState state);
        bool restored = state.TryRestore(
            new Rect3D(100d, 200d, 300d, 40d, 80d, 20d),
            out Point3D target,
            out double distance,
            out double yaw,
            out double pitch);

        Assert.True(captured);
        Assert.True(restored);
        Assert.Equal(new Point3D(110d, 260d, 310d), target);
        Assert.Equal(140d, distance, precision: 8);
        Assert.Equal(0.75d, yaw);
        Assert.Equal(-0.25d, pitch);
        Assert.True(state.HasUserInteraction);
    }

    [Fact]
    public void Capture_UsesCentreForZeroSizedAxes()
    {
        bool captured = Ra2VoxelViewportCameraState.TryCapture(
            new Rect3D(0d, 0d, 0d, 0d, 4d, 3d),
            new Point3D(0d, 2d, 1.5d),
            distance: 8.5d,
            yaw: 0d,
            pitch: 0d,
            hasUserInteraction: false,
            out Ra2VoxelViewportCameraState state);

        Assert.True(captured);
        Assert.Equal(0.5d, state.NormalizedTargetX);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Capture_RejectsInvalidCameraNumbers(double invalid)
    {
        bool captured = Ra2VoxelViewportCameraState.TryCapture(
            new Rect3D(0d, 0d, 0d, 10d, 10d, 10d),
            new Point3D(5d, 5d, 5d),
            distance: 20d,
            yaw: invalid,
            pitch: 0d,
            hasUserInteraction: true,
            out _);

        Assert.False(captured);
    }

    [Fact]
    public void Restore_RejectsInvalidNormalizedState()
    {
        var state = new Ra2VoxelViewportCameraState(
            Yaw: 0d,
            Pitch: 0d,
            NormalizedTargetX: 1.25d,
            NormalizedTargetY: 0.5d,
            NormalizedTargetZ: 0.5d,
            DistanceRatio: 1.7d,
            HasUserInteraction: false);

        Assert.False(state.TryRestore(
            new Rect3D(0d, 0d, 0d, 10d, 10d, 10d),
            out _,
            out _,
            out _,
            out _));
    }
}
