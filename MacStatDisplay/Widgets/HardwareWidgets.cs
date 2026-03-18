namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Text widget for total system power with CPU/GPU breakdown side by side.</summary>
internal sealed class PowerWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "POWER", "System");

        // Total power bottom-right
        helper.DrawValue(canvas, $"{monitor.PowerTotalW:0.0} W", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.PowerAccent);

        // CPU and GPU side by side near the top
        var leftX = rect.Left + WidgetTheme.PadX;
        var midX = rect.MidX;
        var topY = rect.Top + WidgetTheme.TitleOffsetY + 10;
        helper.DrawStackedLabelValue(canvas, "CPU", $"{monitor.PowerCpuW:0.0}W", leftX, topY, WidgetTheme.PowerAccent);
        helper.DrawStackedLabelValue(canvas, "GPU", $"{monitor.PowerGpuW:0.0}W", midX, topY, WidgetTheme.PowerAccent);
    }
}

/// <summary>Text widget for fan speed percentage and RPM.</summary>
internal sealed class FanWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, string.Empty, "FAN");

        // Fan speed percentage bottom-right
        helper.DrawValue(canvas, $"{monitor.FanSpeedPercent:0}%", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.FanAccent);

        // RPM detail on left
        var leftX = rect.Left + WidgetTheme.PadX;
        var topY = rect.Top + WidgetTheme.TitleOffsetY + 10;
        helper.DrawStackedLabelValue(canvas, "Speed", $"{monitor.FanSpeedRpm:0} rpm", leftX, topY, WidgetTheme.FanAccent);
    }
}
