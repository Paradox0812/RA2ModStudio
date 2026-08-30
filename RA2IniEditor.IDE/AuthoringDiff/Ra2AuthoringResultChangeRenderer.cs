using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace RA2IniEditor.IDE.AuthoringDiff;

internal sealed class Ra2AuthoringResultChangeRenderer : IBackgroundRenderer
{
    private static readonly Brush ChangedBackground = Freeze(new SolidColorBrush(Color.FromArgb(28, 16, 185, 129)));
    private static readonly Brush ChangedStripe = Freeze(new SolidColorBrush(Color.FromRgb(0, 120, 212)));
    private static readonly Brush DeletionBackground = Freeze(new SolidColorBrush(Color.FromRgb(254, 226, 226)));
    private static readonly Brush DeletionText = Freeze(new SolidColorBrush(Color.FromRgb(185, 28, 28)));
    private IReadOnlyList<Ra2AuthoringReviewChangeLocation> _locations = [];

    public KnownLayer Layer => KnownLayer.Background;

    public void SetLocations(IReadOnlyList<Ra2AuthoringReviewChangeLocation>? locations)
        => _locations = locations ?? [];

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_locations.Count == 0 || !textView.VisualLinesValid)
            return;

        foreach (VisualLine visualLine in textView.VisualLines)
        {
            int lineNumber = visualLine.FirstDocumentLine.LineNumber;
            Ra2AuthoringReviewChangeLocation[] matches = _locations
                .Where(location => lineNumber >= location.CandidateLineNumber && lineNumber <= location.CandidateEndLineNumber)
                .ToArray();
            if (matches.Length == 0)
                continue;

            double y = visualLine.VisualTop - textView.VerticalOffset;
            double height = visualLine.Height;
            Rect row = new(0, y, Math.Max(0, textView.ActualWidth), height);
            drawingContext.DrawRectangle(ChangedBackground, null, row);
            drawingContext.DrawRectangle(ChangedStripe, null, new Rect(0, y, 3, height));

            int removed = matches.Where(location => location.CandidateLineNumber == lineNumber).Select(location => location.RemovedLineCount).DefaultIfEmpty(0).Max();
            if (removed <= 0)
                continue;
            string label = $"−{removed} 行";
            FormattedText text = new(
                label,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                11,
                DeletionText,
                VisualTreeHelper.GetDpi(textView).PixelsPerDip);
            double width = text.Width + 12;
            Rect badge = new(Math.Max(4, textView.ActualWidth - width - 12), y + 2, width, Math.Max(16, height - 4));
            drawingContext.DrawRoundedRectangle(DeletionBackground, null, badge, 2, 2);
            drawingContext.DrawText(text, new Point(badge.X + 6, badge.Y + Math.Max(0, (badge.Height - text.Height) / 2)));
        }
    }

    private static Brush Freeze(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }
}
