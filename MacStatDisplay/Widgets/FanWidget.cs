namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;

using SkiaSharp;

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

        var fan = monitor.Fans.Count > 0 ? monitor.Fans[0] : null;

        // Fan speed percentage bottom-right
        if (fan is not null)
        {
            var speedPercent = fan.ActualRpm / fan.MaxRpm * 100.0;
            DrawHelper.DrawValue(canvas, $"{speedPercent:0}%", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.FanAccent);
        }

        // RPM sub-item at bottom, aligned with main value
        if (fan is not null)
        {
            var leftX = rect.Left + WidgetTheme.PadX;
            var mainBottom = rect.Bottom - WidgetTheme.PadY;
            DrawHelper.DrawStackedLabelValue(canvas, "Speed", $"{fan.ActualRpm:0} rpm", leftX, mainBottom - 18, WidgetTheme.FanAccent);
        }
    }
}
