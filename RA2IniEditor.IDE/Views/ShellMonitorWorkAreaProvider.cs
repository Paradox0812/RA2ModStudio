using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RA2IniEditor.IDE.Views;

internal readonly record struct ShellDockGeometryRecoveryResult(Rect Bounds, bool UsedFallback);

internal readonly record struct ShellMonitorWorkAreaSnapshot(
    IReadOnlyList<Rect> WorkAreas,
    Rect ShellWorkArea,
    bool HasReliableCrossMonitorCoordinates);

/// <summary>提供 Dock 浮动窗口恢复所需的当前显示器工作区和纯几何收敛规则。</summary>
internal sealed class ShellMonitorWorkAreaProvider
{
    private const double StandardDpi = 96.0;
    private const double SafetyInset = 16.0;
    private const double ReachableTitleWidth = 64.0;
    private const double ReachableTitleHeight = 32.0;
    private const double MaximumSafeCoordinate = 1_000_000.0;
    private readonly Window _shell;

    public ShellMonitorWorkAreaProvider(Window shell)
    {
        ArgumentNullException.ThrowIfNull(shell);
        _shell = shell;
    }

    public ShellMonitorWorkAreaSnapshot GetCurrentSnapshot()
    {
        Rect fallback = GetShellBoundsFallback();
        try
        {
            nint shellHandle = new WindowInteropHelper(_shell).Handle;
            if (shellHandle == 0)
                return new ShellMonitorWorkAreaSnapshot([fallback], fallback, false);

            nint shellMonitor = MonitorFromWindow(shellHandle, MonitorDefaultToNearest);
            if (shellMonitor == 0 || !TryGetMonitorDpi(shellMonitor, out uint shellDpi))
                return new ShellMonitorWorkAreaSnapshot([fallback], fallback, false);

            List<(nint Handle, NativeRect WorkArea, uint Dpi)> monitors = [];
            bool enumerationSucceeded = EnumDisplayMonitors(
                0,
                0,
                (nint monitor, nint _, ref NativeRect __, nint ___) =>
                {
                    MonitorInfo info = new() { Size = Marshal.SizeOf<MonitorInfo>() };
                    if (!GetMonitorInfo(monitor, ref info) || !TryGetMonitorDpi(monitor, out uint dpi))
                        return false;
                    monitors.Add((monitor, info.WorkArea, dpi));
                    return true;
                },
                0);
            if (!enumerationSucceeded || monitors.Count == 0)
                return new ShellMonitorWorkAreaSnapshot([fallback], fallback, false);

            (nint Handle, NativeRect WorkArea, uint Dpi)? shellEntry = monitors
                .Cast<(nint Handle, NativeRect WorkArea, uint Dpi)?>()
                .FirstOrDefault(entry => entry!.Value.Handle == shellMonitor);
            if (shellEntry is null)
                return new ShellMonitorWorkAreaSnapshot([fallback], fallback, false);

            double scale = StandardDpi / shellDpi;
            Rect shellWorkArea = ToDipRect(shellEntry.Value.WorkArea, scale);
            bool sameDpi = monitors.All(monitor => monitor.Dpi == shellDpi);
            if (!sameDpi)
                return new ShellMonitorWorkAreaSnapshot([shellWorkArea], shellWorkArea, false);

            Rect[] workAreas = monitors.Select(monitor => ToDipRect(monitor.WorkArea, scale)).ToArray();
            return new ShellMonitorWorkAreaSnapshot(workAreas, shellWorkArea, true);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or SEHException)
        {
            return new ShellMonitorWorkAreaSnapshot([fallback], fallback, false);
        }
    }

    internal static ShellDockGeometryRecoveryResult RecoverGeometry(
        Rect savedBounds,
        Size preferredSize,
        ShellMonitorWorkAreaSnapshot snapshot)
    {
        Rect shellWorkArea = IsUsableWorkArea(snapshot.ShellWorkArea)
            ? snapshot.ShellWorkArea
            : new Rect(0, 0, Math.Max(320, preferredSize.Width), Math.Max(240, preferredSize.Height));
        if (!snapshot.HasReliableCrossMonitorCoordinates || !IsSafeFloatingRect(savedBounds))
            return new ShellDockGeometryRecoveryResult(CenterPreferred(preferredSize, shellWorkArea), true);

        Rect? target = snapshot.WorkAreas
            .Where(IsUsableWorkArea)
            .Select(workArea => new { WorkArea = workArea, Area = IntersectionArea(savedBounds, workArea) })
            .Where(candidate => candidate.Area > 0)
            .OrderByDescending(candidate => candidate.Area)
            .Select(candidate => (Rect?)candidate.WorkArea)
            .FirstOrDefault();
        if (target is null)
            return new ShellDockGeometryRecoveryResult(CenterPreferred(preferredSize, shellWorkArea), true);

        Rect work = target.Value;
        double availableWidth = Math.Max(ReachableTitleWidth, work.Width - (SafetyInset * 2));
        double availableHeight = Math.Max(ReachableTitleHeight, work.Height - (SafetyInset * 2));
        double width = Math.Min(savedBounds.Width, availableWidth);
        double height = Math.Min(savedBounds.Height, availableHeight);
        double left = savedBounds.Width > availableWidth
            ? work.Left + SafetyInset
            : Math.Clamp(savedBounds.Left, work.Left - width + ReachableTitleWidth, work.Right - ReachableTitleWidth);
        double top = savedBounds.Height > availableHeight
            ? work.Top + SafetyInset
            : Math.Clamp(savedBounds.Top, work.Top, work.Bottom - ReachableTitleHeight);
        return new ShellDockGeometryRecoveryResult(new Rect(left, top, width, height), false);
    }

    private Rect GetShellBoundsFallback()
    {
        double width = double.IsFinite(_shell.ActualWidth) && _shell.ActualWidth > 0 ? _shell.ActualWidth : 1280;
        double height = double.IsFinite(_shell.ActualHeight) && _shell.ActualHeight > 0 ? _shell.ActualHeight : 800;
        double left = double.IsFinite(_shell.Left) ? _shell.Left : 0;
        double top = double.IsFinite(_shell.Top) ? _shell.Top : 0;
        return new Rect(left, top, width, height);
    }

    private static Rect CenterPreferred(Size preferredSize, Rect workArea)
    {
        double width = Math.Min(NormalizePreferred(preferredSize.Width, 800), Math.Max(ReachableTitleWidth, workArea.Width - (SafetyInset * 2)));
        double height = Math.Min(NormalizePreferred(preferredSize.Height, 420), Math.Max(ReachableTitleHeight, workArea.Height - (SafetyInset * 2)));
        return new Rect(
            workArea.Left + ((workArea.Width - width) / 2),
            workArea.Top + ((workArea.Height - height) / 2),
            width,
            height);
    }

    private static double NormalizePreferred(double value, double fallback)
        => double.IsFinite(value) && value > 0 ? value : fallback;

    private static bool IsSafeFloatingRect(Rect bounds)
        => IsFiniteAndBounded(bounds.Left) &&
           IsFiniteAndBounded(bounds.Top) &&
           double.IsFinite(bounds.Width) && bounds.Width > 0 && bounds.Width <= MaximumSafeCoordinate &&
           double.IsFinite(bounds.Height) && bounds.Height > 0 && bounds.Height <= MaximumSafeCoordinate;

    private static bool IsUsableWorkArea(Rect area)
        => IsFiniteAndBounded(area.Left) && IsFiniteAndBounded(area.Top) &&
           double.IsFinite(area.Width) && area.Width >= ReachableTitleWidth &&
           double.IsFinite(area.Height) && area.Height >= ReachableTitleHeight;

    private static bool IsFiniteAndBounded(double value)
        => double.IsFinite(value) && Math.Abs(value) <= MaximumSafeCoordinate;

    private static double IntersectionArea(Rect first, Rect second)
    {
        Rect intersection = Rect.Intersect(first, second);
        return intersection.IsEmpty ? 0 : intersection.Width * intersection.Height;
    }

    private static Rect ToDipRect(NativeRect area, double scale)
        => new(
            area.Left * scale,
            area.Top * scale,
            (area.Right - area.Left) * scale,
            (area.Bottom - area.Top) * scale);

    private static bool TryGetMonitorDpi(nint monitor, out uint dpi)
    {
        int result = GetDpiForMonitor(monitor, MonitorDpiType.Effective, out uint dpiX, out uint dpiY);
        dpi = dpiX;
        return result == 0 && dpiX > 0 && dpiX == dpiY;
    }

    private const uint MonitorDefaultToNearest = 2;

    private delegate bool MonitorEnumProcedure(nint monitor, nint deviceContext, ref NativeRect monitorRect, nint data);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayMonitors(nint deviceContext, nint clipRect, MonitorEnumProcedure callback, nint data);

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint windowHandle, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo monitorInfo);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(nint monitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

    private enum MonitorDpiType
    {
        Effective = 0
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect MonitorArea;
        public NativeRect WorkArea;
        public uint Flags;
    }
}
