namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;

using SkiaSharp;

/// <summary>Ring gauge widget for GPU usage.</summary>
internal sealed class GpuUsageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "GPU", "Usage");

        var usage = (float)Math.Clamp(monitor.GpuUsagePercent, 0, 100);
        var gaugeArea = rect.Height - WidgetTheme.TitleOffsetY - WidgetTheme.PadY;
        var radius = Math.Min(rect.Width * 0.35f, gaugeArea * 0.42f);
        var cx = rect.MidX;
        var cy = rect.Top + WidgetTheme.TitleOffsetY + (gaugeArea / 2f) + WidgetTheme.PadY;

        helper.DrawRingGauge(canvas, cx, cy, radius, usage, WidgetTheme.GpuAccent);
        helper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (WidgetTheme.CenterValueFontSize * 0.35f), WidgetTheme.GpuAccent);

        // GPU temperature on right if available
        var temp = monitor.GpuTemperature;
        if (temp.HasValue)
        {
            helper.DrawRightAlignedDetail(canvas, $"{temp.Value:0}°C", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY - 2);
        }
    }
}
