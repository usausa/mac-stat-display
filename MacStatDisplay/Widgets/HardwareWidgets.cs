namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

// Text widget for total system power. CPU/GPU stacked vertically, bottom-aligned with main value.
internal sealed class PowerWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "POWER", "System");

        // Total power bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.PowerTotalW:0.0} W", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.PowerAccent);

        // CPU and GPU stacked vertically, bottom-aligned with main value
        var leftX = rect.Left + WidgetTheme.PadX;
        var mainBottom = rect.Bottom - WidgetTheme.PadY;
        var y2 = mainBottom - 18;
        var y1 = y2 - 36;
        DrawHelper.DrawStackedLabelValue(canvas, "CPU", $"{monitor.PowerCpuW:0.0}W", leftX, y1, WidgetTheme.PowerAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "GPU", $"{monitor.PowerGpuW:0.0}W", leftX, y2, WidgetTheme.PowerAccent);
    }
}

// Text widget for fan speed percentage and RPM. Speed sub-item bottom-aligned with main value.
internal sealed class FanWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, string.Empty, "FAN");

        // Fan speed percentage bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.FanSpeedPercent:0}%", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.FanAccent);

        // RPM sub-item at bottom, aligned with main value
        var leftX = rect.Left + WidgetTheme.PadX;
        var mainBottom = rect.Bottom - WidgetTheme.PadY;
        DrawHelper.DrawStackedLabelValue(canvas, "Speed", $"{monitor.FanSpeedRpm:0} rpm", leftX, mainBottom - 18, WidgetTheme.FanAccent);
    }
}
