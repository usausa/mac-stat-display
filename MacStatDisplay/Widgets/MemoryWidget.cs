namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Ring gauge widget for memory usage.</summary>
internal sealed class MemoryUsageWidget : IWidget
{
    private static readonly SKColor Accent = new(180, 120, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "MEM", "Usage");

        var usage = monitor.MemoryUsagePercent;
        var center = new SKPoint(rect.MidX, rect.MidY + 12);
        const float radius = 50;

        helper.DrawRingGauge(canvas, center.X, center.Y, radius, (float)Math.Clamp(usage, 0, 100), Accent);
        helper.DrawCenteredValue(canvas, $"{usage:0}%", center.X, center.Y + 12, Accent);

        var detailX = rect.Right - 18;
        var detailTop = rect.Top + 54;
        helper.DrawRightAlignedDetail(canvas, $"Active {monitor.MemoryActivePercent:0.0}%", detailX, detailTop);
        helper.DrawRightAlignedDetail(canvas, $"Wired {monitor.MemoryWiredPercent:0.0}%", detailX, detailTop + 18);
        helper.DrawRightAlignedDetail(canvas, $"Swap {monitor.SwapUsagePercent:0.0}%", detailX, detailTop + 36);
    }
}

/// <summary>Text widget for application memory.</summary>
internal sealed class MemoryAppWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 214, 102);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "MEM", "App Memory");

        helper.DrawValue(canvas, $"{monitor.MemoryActivePercent:0.0}%", rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"Wired {monitor.MemoryWiredPercent:0.0}%", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
    }
}

/// <summary>Text widget for swap usage.</summary>
internal sealed class MemorySwapWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 214, 102);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "MEM", "Swap");

        helper.DrawValue(canvas, $"{monitor.SwapUsagePercent:0.0}%", rect.Right - 16, rect.MidY + 8, Accent);
    }
}
