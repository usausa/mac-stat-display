namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Bar gauge widget for disk usage.</summary>
internal sealed class DiskUsageWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 210, 60);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "ディスク", Accent, rect.Left + 5, rect.Top + 4);

        var usage = monitor.DiskUsagePercent;
        var valueColor = DrawHelper.ResolvePercentColor(usage, Accent);

        helper.DrawLabel(canvas, "ディスク 使用率", rect.Left + 8, rect.Top + 28, rect.Width - 150);
        helper.DrawLargeValue(canvas, $"{usage:0}%", rect.Right - 8, rect.Top + 31, valueColor);

        var percentage = (float)Math.Clamp(usage, 0, 100);
        var trackRect = new SKRect(rect.Left + 8, rect.Top + 48, rect.Right - 8, rect.Top + 61);
        helper.DrawBarGauge(canvas, trackRect, percentage, valueColor);

        helper.DrawDetail(canvas, $"{monitor.DiskFreeGb:0} GB free / {monitor.DiskTotalGb:0} GB", rect.Left + 8, rect.Bottom - 8);
    }
}
