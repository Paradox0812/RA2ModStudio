using System.Windows;

namespace RA2IniEditor.IDE.ViewModels.Language;

internal static class Ra2FloatingInspectorPlacement
{
    public const double DefaultGap = 6;
    public const double DefaultMargin = 8;

    public static Size MeasureInspector(Window inspector)
    {
        ArgumentNullException.ThrowIfNull(inspector);

        Size contentSize = default;
        if (inspector.Content is FrameworkElement content)
        {
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            content.UpdateLayout();
            contentSize = content.DesiredSize;
        }

        inspector.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        inspector.UpdateLayout();

        double width = Math.Max(
            inspector.ActualWidth > 0 ? inspector.ActualWidth : inspector.DesiredSize.Width,
            contentSize.Width);
        double height = Math.Max(
            inspector.ActualHeight > 0 ? inspector.ActualHeight : inspector.DesiredSize.Height,
            contentSize.Height);
        return new Size(NormalizeExtent(width), NormalizeExtent(height));
    }

    public static Point PlaceNearCaret(
        Point caretBottomScreenDip,
        Size inspectorSizeDip,
        Rect workAreaDip,
        double gap = DefaultGap,
        double margin = DefaultMargin)
    {
        double width = NormalizeExtent(inspectorSizeDip.Width);
        double height = NormalizeExtent(inspectorSizeDip.Height);
        double safeGap = Math.Max(0, gap);
        double safeMargin = Math.Max(0, margin);

        double left = ClampToRange(
            caretBottomScreenDip.X,
            workAreaDip.Left + safeMargin,
            workAreaDip.Right - safeMargin - width);

        double belowTop = caretBottomScreenDip.Y + safeGap;
        double aboveTop = caretBottomScreenDip.Y - safeGap - height;
        double minTop = workAreaDip.Top + safeMargin;
        double maxTop = workAreaDip.Bottom - safeMargin - height;

        double top = belowTop;
        if (belowTop + height > workAreaDip.Bottom - safeMargin &&
            aboveTop >= minTop)
        {
            top = aboveTop;
        }
        else
        {
            top = ClampToRange(belowTop, minTop, maxTop);
        }

        return new Point(left, top);
    }

    private static double NormalizeExtent(double value)
        => double.IsNaN(value) || double.IsInfinity(value) || value < 0 ? 0 : value;

    private static double ClampToRange(double value, double minimum, double maximum)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return minimum;

        if (maximum < minimum)
            return minimum;

        return Math.Clamp(value, minimum, maximum);
    }
}
