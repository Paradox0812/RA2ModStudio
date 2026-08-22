using System.Windows;
using RA2IniEditor.IDE.Views;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ShellMonitorWorkAreaProviderTests
{
    [Fact]
    public void ValidNegativeCoordinatesOnConnectedLeftMonitor_ArePreserved()
    {
        ShellMonitorWorkAreaSnapshot snapshot = new(
            [new Rect(-1920, 0, 1920, 1040), new Rect(0, 0, 1920, 1040)],
            new Rect(0, 0, 1920, 1040),
            true);

        ShellDockGeometryRecoveryResult result = ShellMonitorWorkAreaProvider.RecoverGeometry(
            new Rect(-1600, 120, 800, 420),
            new Size(800, 420),
            snapshot);

        Assert.False(result.UsedFallback);
        Assert.Equal(new Rect(-1600, 120, 800, 420), result.Bounds);
    }

    [Fact]
    public void PartiallyVisibleWindow_KeepsReachableTitleRegion()
    {
        ShellMonitorWorkAreaSnapshot snapshot = new(
            [new Rect(0, 0, 1920, 1040)],
            new Rect(0, 0, 1920, 1040),
            true);

        ShellDockGeometryRecoveryResult result = ShellMonitorWorkAreaProvider.RecoverGeometry(
            new Rect(1900, 1025, 800, 420),
            new Size(800, 420),
            snapshot);

        Assert.False(result.UsedFallback);
        Assert.True(result.Bounds.Left <= 1920 - 64);
        Assert.True(result.Bounds.Top <= 1040 - 32);
    }

    [Fact]
    public void DisconnectedMonitorWindow_IsCenteredOnShellMonitor()
    {
        Rect shell = new(0, 0, 1920, 1040);
        ShellDockGeometryRecoveryResult result = ShellMonitorWorkAreaProvider.RecoverGeometry(
            new Rect(-1800, 100, 800, 420),
            new Size(800, 420),
            new ShellMonitorWorkAreaSnapshot([shell], shell, true));

        Assert.True(result.UsedFallback);
        Assert.Equal(new Rect(560, 310, 800, 420), result.Bounds);
    }

    [Fact]
    public void OversizedWindow_IsClampedWithSafetyInset()
    {
        Rect shell = new(0, 0, 1280, 800);
        ShellDockGeometryRecoveryResult result = ShellMonitorWorkAreaProvider.RecoverGeometry(
            new Rect(0, 0, 3000, 2000),
            new Size(800, 420),
            new ShellMonitorWorkAreaSnapshot([shell], shell, true));

        Assert.False(result.UsedFallback);
        Assert.Equal(new Rect(16, 16, 1248, 768), result.Bounds);
    }

    [Theory]
    [InlineData(double.NaN, 0, 800, 420)]
    [InlineData(double.PositiveInfinity, 0, 800, 420)]
    [InlineData(0, 0, 0, 420)]
    [InlineData(0, 0, 800, 0)]
    public void InvalidGeometry_UsesPreferredShellFallback(double left, double top, double width, double height)
    {
        Rect shell = new(0, 0, 1920, 1040);
        ShellDockGeometryRecoveryResult result = ShellMonitorWorkAreaProvider.RecoverGeometry(
            new Rect(left, top, width, height),
            new Size(700, 460),
            new ShellMonitorWorkAreaSnapshot([shell], shell, true));

        Assert.True(result.UsedFallback);
        Assert.Equal(new Size(700, 460), result.Bounds.Size);
    }

    [Fact]
    public void UnreliableMixedDpiCoordinates_UseShellFallbackInsteadOfGuessing()
    {
        Rect shell = new(0, 0, 1536, 832);
        ShellDockGeometryRecoveryResult result = ShellMonitorWorkAreaProvider.RecoverGeometry(
            new Rect(100, 100, 800, 420),
            new Size(800, 420),
            new ShellMonitorWorkAreaSnapshot([shell], shell, false));

        Assert.True(result.UsedFallback);
        Assert.Equal(new Rect(368, 206, 800, 420), result.Bounds);
    }
}
