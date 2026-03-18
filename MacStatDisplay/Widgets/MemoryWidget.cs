namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Ring gauge widget for memory usage with Active/Wired detail and Swap on the left.</summary>
internal sealed class MemoryUsageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "MEM", "Usage");

        var usage = (float)Math.Clamp(monitor.MemoryUsagePercent, 0, 100);

        // Larger ring – push center down so the 270° arc bottom aligns with the widget bottom
        var contentH = rect.Height - WidgetTheme.TitleOffsetY - WidgetTheme.PadY;
        var maxRadiusH = contentH / 1.71f;
        var maxRadiusW = (rect.Width - 160f) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = rect.Bottom - WidgetTheme.PadY - (radius * 0.71f);

        helper.DrawRingGauge(canvas, cx, cy, radius, usage, WidgetTheme.MemoryAccent);
        helper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (WidgetTheme.CenterValueFontSize * 0.35f), WidgetTheme.MemoryAccent);

        // Left: Swap
        var leftX = rect.Left + WidgetTheme.PadX;
        var sideTop = cy - radius + 8;
        helper.DrawStackedLabelValue(canvas, "Swap", $"{monitor.SwapUsagePercent:0.0}%", leftX, sideTop, WidgetTheme.MemoryAccent);

        // Right: Active, Wired
        var rightX = rect.Right - WidgetTheme.PadX;
        helper.DrawStackedLabelValueRight(canvas, "Active", $"{monitor.MemoryActivePercent:0.0}%", rightX, sideTop, WidgetTheme.MemoryAccent);
        helper.DrawStackedLabelValueRight(canvas, "Wired", $"{monitor.MemoryWiredPercent:0.0}%", rightX, sideTop + 40, WidgetTheme.MemoryAccent);
    }
}
