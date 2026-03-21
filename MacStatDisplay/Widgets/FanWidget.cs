namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class FanWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "FAN Speed");

        var fan = monitor.Fans.Count > 0 ? monitor.Fans[0] : null;

        // Fan speed
        if (fan is not null)
        {
            var speedPercent = fan.ActualRpm / fan.MaxRpm * 100.0;
            DrawHelper.DrawValue(canvas, $"{speedPercent:0}%", rect.Right - Layout.PaddingX, rect.Bottom - Layout.PaddingY, Colors.FanAccent);
        }

        // RPM
        if (fan is not null)
        {
            var leftX = rect.Left + Layout.PaddingX;
            var mainBottom = rect.Bottom - Layout.PaddingY;
            DrawHelper.DrawStackedLabelValue(canvas, "Speed", $"{fan.ActualRpm:0} rpm", leftX, mainBottom, Colors.FanAccent);
        }
    }
}
