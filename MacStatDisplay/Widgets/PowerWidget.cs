namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class PowerWidget : IWidget
{
    private float subValueColumnWidth;

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
        subValueColumnWidth = DrawHelper.MeasureSubValueWidth("000000");
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "Power Consumption");

        // Total
        DrawHelper.DrawValue(canvas, $"{monitor.TotalSystemPower:0.0} W", rect.Right - Layout.PaddingX, rect.Bottom - Layout.PaddingY, Colors.PowerAccent);

        // CPU / GPU
        var leftX = rect.Left + Layout.PaddingX;
        var y = rect.Bottom - Layout.PaddingY;
        DrawHelper.DrawStackedLabelValue(canvas, "CPU", $"{monitor.PowerCpuW:0.0}", leftX, y, Colors.PowerAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "GPU", $"{monitor.PowerGpuW:0.0}", leftX + subValueColumnWidth, y, Colors.PowerAccent);
    }
}
