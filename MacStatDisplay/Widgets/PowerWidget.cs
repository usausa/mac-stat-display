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
        DrawHelper.DrawTitle(canvas, rect, "Power Consumption");

        // Total
        DrawHelper.DrawValue(canvas, FormatWatt(monitor.TotalSystemPower, " W"), rect.Right - Layout.PaddingX, rect.Bottom - Layout.PaddingY, Colors.PowerAccent);

        // CPU / GPU
        var leftX = rect.Left + Layout.PaddingX;
        var y = rect.Bottom - Layout.PaddingY;
        DrawHelper.DrawStackedValue(canvas, "CPU", FormatWatt(monitor.PowerCpuW), leftX, y, Colors.PowerAccent);
        DrawHelper.DrawStackedValue(canvas, "GPU", FormatWatt(monitor.PowerGpuW), leftX + subValueColumnWidth, y, Colors.PowerAccent);
    }

    private static string FormatWatt(double? value, string unit = "") => value.HasValue ? $"{value.Value:0.0}{unit}" : "-";
}
