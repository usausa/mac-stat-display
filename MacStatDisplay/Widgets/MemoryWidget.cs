namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Ring gauge widget for memory usage with Active/Wired detail and Swap on the left.
internal sealed class MemoryUsageWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "MEM Usage");

        var usage = (float)Math.Clamp(monitor.MemoryUsagePercent, 0, 100);

        // Content area below title
        var contentTop = rect.Top + Layout.TitleOffsetY + Layout.ContentTopGap;
        var contentH = rect.Bottom - Layout.PaddingY - contentTop;
        var sideMargin = Layout.RingSideMargin;
        var maxRadiusH = contentH / Layout.RingHeightRatio;
        var maxRadiusW = (rect.Width - (2 * sideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = contentTop + (contentH / 2f) + (radius * Layout.RingCenterOffsetRatio);

        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, Colors.MemoryAccent);
        DrawHelper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (FontSize.GaugeValue * Layout.BaselineRatio), Colors.MemoryAccent);

        // Left: Swap
        var leftX = rect.Left + Layout.PaddingX;
        var sideTop = cy - radius + Layout.RingSideTopOffset;
        DrawHelper.DrawStackedLabelValue(canvas, "Swap", $"{monitor.SwapUsagePercent:0.0}%", leftX, sideTop, Colors.MemoryAccent);

        // Right: Active, Wired
        var rightX = rect.Right - Layout.PaddingX;
        DrawHelper.DrawStackedLabelValueRight(canvas, "Active", $"{monitor.MemoryActivePercent:0.0}%", rightX, sideTop, Colors.MemoryAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "Wired", $"{monitor.MemoryWiredPercent:0.0}%", rightX, sideTop + Layout.StackedItemSpacing, Colors.MemoryAccent);
    }
}
