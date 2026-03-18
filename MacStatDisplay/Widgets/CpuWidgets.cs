namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Ring gauge widget for CPU usage.</summary>
internal sealed class CpuUsageWidget : IWidget
{
    private static readonly SKColor Accent = new(88, 166, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "CPU", "Usage");

        var usage = monitor.CpuUsageTotal;
        var center = new SKPoint(rect.MidX, rect.MidY + 12);
        const float radius = 50;

        helper.DrawRingGauge(canvas, center.X, center.Y, radius, (float)Math.Clamp(usage, 0, 100), Accent);
        helper.DrawCenteredValue(canvas, $"{usage:0}%", center.X, center.Y + 12, Accent);

        var detailX = rect.Right - 18;
        var detailTop = rect.Top + 54;
        helper.DrawRightAlignedDetail(canvas, $"System {monitor.CpuSystemPercent:0}%", detailX, detailTop);
        helper.DrawRightAlignedDetail(canvas, $"User {monitor.CpuUserPercent:0}%", detailX, detailTop + 18);
        helper.DrawRightAlignedDetail(canvas, $"Idle {100 - monitor.CpuUsageTotal:0}%", detailX, detailTop + 36);
    }
}

/// <summary>Text widget for load average.</summary>
internal sealed class LoadAverageWidget : IWidget
{
    private static readonly SKColor Accent = new(127, 225, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "CPU", "Load Avg");

        helper.DrawValue(canvas, $"1m {monitor.LoadAverage1:0.00}", rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"5m {monitor.LoadAverage5:0.00}  15m {monitor.LoadAverage15:0.00}", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
    }
}

/// <summary>Text widget for CPU clock frequency.</summary>
internal sealed class CpuClockWidget : IWidget
{
    private static readonly SKColor Accent = new(196, 181, 253);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "CPU", "Clock");

        var mhz = monitor.CpuFrequencyAllHz / 1_000_000.0;
        var pMhz = monitor.CpuFrequencyPerformanceHz / 1_000_000.0;
        var eMhz = monitor.CpuFrequencyEfficiencyHz / 1_000_000.0;

        helper.DrawValue(canvas, $"{mhz:0} MHz", rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"P {pMhz:0} / E {eMhz:0}", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
    }
}
