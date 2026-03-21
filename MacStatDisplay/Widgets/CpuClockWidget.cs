namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Text widget for CPU clock frequency. P-Core/E-Core stacked vertically, bottom-aligned with main value.
internal sealed class CpuClockWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU", "Clock");

        var mhz = monitor.CpuFrequencyAllHz / 1_000_000.0;
        var pMhz = monitor.CpuFrequencyPerformanceHz / 1_000_000.0;
        var eMhz = monitor.CpuFrequencyEfficiencyHz / 1_000_000.0;

        // Main value bottom-right
        DrawHelper.DrawValue(canvas, $"{mhz:0} MHz", rect.Right - Layout.PaddingX, rect.Bottom - Layout.PaddingY, Colors.CpuClockAccent);

        // P-Core / E-Core stacked vertically, bottom-aligned with main value
        var leftX = rect.Left + Layout.PaddingX;
        var mainBottom = rect.Bottom - Layout.PaddingY;
        var y2 = mainBottom - 18;
        var y1 = y2 - 36;
        DrawHelper.DrawStackedLabelValue(canvas, "P-Core", $"{pMhz:0}", leftX, y1, Colors.CpuClockAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "E-Core", $"{eMhz:0}", leftX, y2, Colors.CpuClockAccent);
    }
}
