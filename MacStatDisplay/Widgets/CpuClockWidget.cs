namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class CpuClockWidget : IWidget
{
    private float subValueColumnWidth;

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
        subValueColumnWidth = DrawHelper.MeasureSubValueWidth("000000");
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU Clock");

        var mhz = monitor.CpuFrequencyAllHz / 1_000_000.0;
        var pMhz = monitor.CpuFrequencyPerformanceHz / 1_000_000.0;
        var eMhz = monitor.CpuFrequencyEfficiencyHz / 1_000_000.0;

        // Main value
        DrawHelper.DrawValue(canvas, $"{mhz:0} MHz", rect.Right - Layout.PaddingX, rect.Bottom - Layout.PaddingY, Colors.CpuClockAccent);

        // E-Core / P-Core
        var leftX = rect.Left + Layout.PaddingX;
        var y = rect.Bottom - Layout.PaddingY - Layout.StackedValueOffsetY;
        DrawHelper.DrawStackedLabelValue(canvas, "E-Core", $"{eMhz:0}", leftX, y, Colors.CpuClockAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "P-Core", $"{pMhz:0}", leftX + subValueColumnWidth, y, Colors.CpuClockAccent);
    }
}
