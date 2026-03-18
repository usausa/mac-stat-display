namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Bar gauge widget for disk capacity.</summary>
internal sealed class DiskCapacityWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 99, 132);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "DISK", "Capacity");

        var usage = monitor.DiskUsagePercent;

        helper.DrawValue(canvas, $"{usage:0}%", rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"{monitor.DiskFreeGb:0} GB free / {monitor.DiskTotalGb:0} GB", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
        helper.DrawBarGauge(canvas, rect, (float)Math.Clamp(usage, 0, 100), Accent);
    }
}

/// <summary>Text widget for disk read speed.</summary>
internal sealed class DiskReadWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 154, 162);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "DISK", "Read");

        var kbps = monitor.DiskReadBytesPerSec / 1024.0;
        helper.DrawValue(canvas, $"{kbps:0} KB/s", rect.Right - 16, rect.MidY + 8, Accent);
    }
}

/// <summary>Text widget for disk write speed.</summary>
internal sealed class DiskWriteWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 154, 162);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "DISK", "Write");

        var kbps = monitor.DiskWriteBytesPerSec / 1024.0;
        helper.DrawValue(canvas, $"{kbps:0} KB/s", rect.Right - 16, rect.MidY + 8, Accent);
    }
}
