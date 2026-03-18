namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Text widget for total system power. CPU/GPU at bottom with even spacing from left.</summary>
internal sealed class PowerWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "POWER", "System");

        // Total power bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.PowerTotalW:0.0} W", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.PowerAccent);

        // CPU and GPU at bottom, evenly spaced from left
        var leftX = rect.Left + WidgetTheme.PadX;
        var bottomY = rect.Bottom - WidgetTheme.PadY - WidgetTheme.ValueLargeFontSize - 6;
        DrawHelper.DrawStackedLabelValue(canvas, "CPU", $"{monitor.PowerCpuW:0.0}W", leftX, bottomY, WidgetTheme.PowerAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "GPU", $"{monitor.PowerGpuW:0.0}W", leftX + WidgetTheme.SubItemSpacing, bottomY, WidgetTheme.PowerAccent);
    }
}

/// <summary>Text widget for fan speed percentage and RPM.</summary>
internal sealed class FanWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, string.Empty, "FAN");

        // Fan speed percentage bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.FanSpeedPercent:0}%", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.FanAccent);

        // RPM detail at bottom, left
        var leftX = rect.Left + WidgetTheme.PadX;
        var bottomY = rect.Bottom - WidgetTheme.PadY - WidgetTheme.ValueLargeFontSize - 6;
        DrawHelper.DrawStackedLabelValue(canvas, "Speed", $"{monitor.FanSpeedRpm:0} rpm", leftX, bottomY, WidgetTheme.FanAccent);
    }
}
