namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Circular gauge widget for memory usage.</summary>
internal sealed class MemoryUsageWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 100, 150);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "メモリ", Accent, rect.Left + 5, rect.Top + 4);

        var usage = monitor.MemoryUsagePercent;
        var valueColor = DrawHelper.ResolvePercentColor(usage, Accent);

        helper.DrawLabel(canvas, "使用率", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{usage:0}%", rect.Right - 8, rect.Top + 42, valueColor);

        var percentage = (float)Math.Clamp(usage, 0, 100);
        helper.DrawCircularGauge(canvas, rect.MidX, rect.MidY + 18, Math.Min(rect.Width, rect.Height) * 0.27f, percentage, valueColor);

        helper.DrawDetail(canvas, $"Wired {monitor.MemoryWiredPercent:0.0}% / Swap {monitor.SwapUsagePercent:0.0}%", rect.Left + 8, rect.Bottom - 8);
    }
}
