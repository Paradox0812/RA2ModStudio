using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;

namespace RA2IniEditor.IDE.Views;

/// <summary>
/// 保留 Windows 原生非客户区行为的 Shell 窗口 Chrome 生命周期控制器。
/// </summary>
internal sealed class ShellWindowChromeController : IDisposable
{
    private const int WmGetMinMaxInfo = 0x0024;
    private const int WmNcHitTest = 0x0084;
    private const int WmNcLButtonDown = 0x00A1;
    private const int HtMaxButton = 9;
    private const uint MonitorDefaultToNearest = 2;

    private readonly Window _window;
    private readonly FrameworkElement _dragRegion;
    private readonly FrameworkElement? _maximizeRegion;
    private HwndSource? _source;
    private bool _isAttached;

    internal ShellWindowChromeController(
        Window window,
        FrameworkElement dragRegion,
        FrameworkElement? maximizeRegion)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _dragRegion = dragRegion ?? throw new ArgumentNullException(nameof(dragRegion));
        _maximizeRegion = maximizeRegion;
    }

    internal void Attach()
    {
        if (_isAttached)
            return;

        _isAttached = true;
        if (_maximizeRegion is not null)
        {
            _window.SourceInitialized += Window_OnSourceInitialized;
            _window.StateChanged += Window_OnStateChanged;
            AttachSourceHook();
        }

        SynchronizeMaximizeState();
    }

    internal void ShowSystemMenu(Point screenPoint)
        => SystemCommands.ShowSystemMenu(_window, screenPoint);

    public void Dispose()
    {
        if (!_isAttached)
            return;

        if (_maximizeRegion is not null)
        {
            _window.SourceInitialized -= Window_OnSourceInitialized;
            _window.StateChanged -= Window_OnStateChanged;
        }

        _source?.RemoveHook(WindowProc);
        _source = null;
        _isAttached = false;
    }

    private void Window_OnSourceInitialized(object? sender, EventArgs e)
        => AttachSourceHook();

    private void Window_OnStateChanged(object? sender, EventArgs e)
        => SynchronizeMaximizeState();

    private void AttachSourceHook()
    {
        if (_source is not null)
            return;

        _source = PresentationSource.FromVisual(_window) as HwndSource;
        _source?.AddHook(WindowProc);
    }

    private IntPtr WindowProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WmGetMinMaxInfo && _maximizeRegion is not null)
        {
            handled = TryApplyMaximizedWorkArea(hwnd, lParam);
            return IntPtr.Zero;
        }

        if (message == WmNcLButtonDown && wParam.ToInt32() == HtMaxButton)
        {
            handled = true;
            _window.Dispatcher.BeginInvoke(() =>
            {
                if (_window.WindowState == WindowState.Maximized)
                    SystemCommands.RestoreWindow(_window);
                else
                    SystemCommands.MaximizeWindow(_window);
            });
            return IntPtr.Zero;
        }

        if (message != WmNcHitTest || _maximizeRegion is null ||
            !_maximizeRegion.IsVisible || !_maximizeRegion.IsEnabled)
            return IntPtr.Zero;

        long packedPoint = lParam.ToInt64();
        Point screenPoint = new(
            unchecked((short)(packedPoint & 0xFFFF)),
            unchecked((short)((packedPoint >> 16) & 0xFFFF)));
        Point localPoint = _maximizeRegion.PointFromScreen(screenPoint);
        if (localPoint.X < 0 || localPoint.Y < 0 ||
            localPoint.X > _maximizeRegion.ActualWidth || localPoint.Y > _maximizeRegion.ActualHeight)
        {
            return IntPtr.Zero;
        }

        handled = true;
        return new IntPtr(HtMaxButton);
    }

    internal static bool TryCalculateMaximizedBounds(
        Int32Rect monitorArea,
        Int32Rect workArea,
        out Int32Rect maximizedBounds)
    {
        maximizedBounds = Int32Rect.Empty;
        if (monitorArea.IsEmpty || monitorArea.Width <= 0 || monitorArea.Height <= 0 ||
            workArea.IsEmpty || workArea.Width <= 0 || workArea.Height <= 0)
        {
            return false;
        }

        long monitorRight = (long)monitorArea.X + monitorArea.Width;
        long monitorBottom = (long)monitorArea.Y + monitorArea.Height;
        long workRight = (long)workArea.X + workArea.Width;
        long workBottom = (long)workArea.Y + workArea.Height;
        if (workArea.X < monitorArea.X || workArea.Y < monitorArea.Y ||
            workRight > monitorRight || workBottom > monitorBottom)
        {
            return false;
        }

        maximizedBounds = new Int32Rect(
            workArea.X - monitorArea.X,
            workArea.Y - monitorArea.Y,
            workArea.Width,
            workArea.Height);
        return true;
    }

    private static bool TryApplyMaximizedWorkArea(IntPtr windowHandle, IntPtr minMaxInfoPointer)
    {
        if (windowHandle == IntPtr.Zero || minMaxInfoPointer == IntPtr.Zero)
            return false;

        try
        {
            nint monitor = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
            if (monitor == 0)
                return false;

            MonitorInfo monitorInfo = new() { Size = Marshal.SizeOf<MonitorInfo>() };
            if (!GetMonitorInfo(monitor, ref monitorInfo) ||
                !TryCreateRect(monitorInfo.MonitorArea, out Int32Rect monitorArea) ||
                !TryCreateRect(monitorInfo.WorkArea, out Int32Rect workArea) ||
                !TryCalculateMaximizedBounds(monitorArea, workArea, out Int32Rect maximizedBounds))
            {
                return false;
            }

            MinMaxInfo minMaxInfo = Marshal.PtrToStructure<MinMaxInfo>(minMaxInfoPointer);
            minMaxInfo.MaxPosition = new NativePoint(maximizedBounds.X, maximizedBounds.Y);
            minMaxInfo.MaxSize = new NativePoint(maximizedBounds.Width, maximizedBounds.Height);
            Marshal.StructureToPtr(minMaxInfo, minMaxInfoPointer, false);
            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or SEHException)
        {
            return false;
        }
    }

    private static bool TryCreateRect(NativeRect nativeRect, out Int32Rect rect)
    {
        long width = (long)nativeRect.Right - nativeRect.Left;
        long height = (long)nativeRect.Bottom - nativeRect.Top;
        if (width <= 0 || width > int.MaxValue || height <= 0 || height > int.MaxValue)
        {
            rect = Int32Rect.Empty;
            return false;
        }

        rect = new Int32Rect(nativeRect.Left, nativeRect.Top, (int)width, (int)height);
        return true;
    }

    private void SynchronizeMaximizeState()
    {
        AutomationProperties.SetHelpText(_dragRegion, "拖动窗口；双击可最大化或还原");
        if (_maximizeRegion is null)
            return;

        bool isMaximized = _window.WindowState == WindowState.Maximized;
        string accessibleName = isMaximized ? "还原" : "最大化";
        AutomationProperties.SetName(_maximizeRegion, accessibleName);
        _maximizeRegion.ToolTip = accessibleName;
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint windowHandle, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo monitorInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        internal NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public NativePoint Reserved;
        public NativePoint MaxSize;
        public NativePoint MaxPosition;
        public NativePoint MinTrackSize;
        public NativePoint MaxTrackSize;
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
