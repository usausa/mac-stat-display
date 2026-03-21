namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class LoadAverageWidget : IWidget
{
    private float subValueColumnWidth;

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
        subValueColumnWidth = DrawHelper.MeasureSubValueWidth("000000");
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "Load Average");

        // Main value
        DrawHelper.DrawValue(canvas, $"{monitor.LoadAverage1:0.00}", rect.Right - Layout.PaddingX, rect.Bottom - Layout.PaddingY, Colors.CpuLoadAccent);

        // 5m / 15m
        var leftX = rect.Left + Layout.PaddingX;
        var y = rect.Bottom - Layout.PaddingY;
        DrawHelper.DrawStackedLabelValue(canvas, "5m", $"{monitor.LoadAverage5:0.00}", leftX, y, Colors.CpuLoadAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "15m", $"{monitor.LoadAverage15:0.00}", leftX + subValueColumnWidth, y, Colors.CpuLoadAccent);
    }
}
