namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;

using SkiaSharp;

// Text widget for load average. 5m/15m stacked vertically, bottom-aligned with main value.
internal sealed class LoadAverageWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU", "Load");

        // Main value (1m) bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.LoadAverage1:0.00}", rect.Right - WidgetTheme.PaddingX, rect.Bottom - WidgetTheme.PaddingY, WidgetTheme.CpuLoadAccent);

        // 5m / 15m stacked vertically, bottom-aligned with main value
        var leftX = rect.Left + WidgetTheme.PaddingX;
        var mainBottom = rect.Bottom - WidgetTheme.PaddingY;
        var y2 = mainBottom - 18;
        var y1 = y2 - 36;
        DrawHelper.DrawStackedLabelValue(canvas, "5m", $"{monitor.LoadAverage5:0.00}", leftX, y1, WidgetTheme.CpuLoadAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "15m", $"{monitor.LoadAverage15:0.00}", leftX, y2, WidgetTheme.CpuLoadAccent);
    }
}
