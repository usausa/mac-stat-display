namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Ring gauge widget for memory usage with Active/Wired detail and Swap on the left.</summary>
internal sealed class MemoryUsageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "MEM", "Usage");

        var usage = (float)Math.Clamp(monitor.MemoryUsagePercent, 0, 100);

        // Content area below title
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentH = rect.Bottom - WidgetTheme.PadY - contentTop;
        var sideMargin = 70f;
        var maxRadiusH = contentH / 1.707f;
        var maxRadiusW = (rect.Width - (2 * sideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        // Visually center the 270° arc
        var cy = contentTop + (contentH / 2f) + (radius * 0.147f);

        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, WidgetTheme.MemoryAccent);
        DrawHelper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (WidgetTheme.GaugeValueFontSize * 0.35f), WidgetTheme.MemoryAccent);

        // Left: Swap
        var leftX = rect.Left + WidgetTheme.PadX;
        var sideTop = cy - radius + 8;
        DrawHelper.DrawStackedLabelValue(canvas, "Swap", $"{monitor.SwapUsagePercent:0.0}%", leftX, sideTop, WidgetTheme.MemoryAccent);

        // Right: Active, Wired
        var rightX = rect.Right - WidgetTheme.PadX;
        DrawHelper.DrawStackedLabelValueRight(canvas, "Active", $"{monitor.MemoryActivePercent:0.0}%", rightX, sideTop, WidgetTheme.MemoryAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "Wired", $"{monitor.MemoryWiredPercent:0.0}%", rightX, sideTop + 44, WidgetTheme.MemoryAccent);
    }
}
