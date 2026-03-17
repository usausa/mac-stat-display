namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Circular gauge widget for CPU usage.</summary>
internal sealed class CpuUsageWidget : IWidget
{
    private static readonly SKColor Accent = new(0, 200, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "CPU", Accent, rect.Left + 5, rect.Top + 4);

        var usage = monitor.CpuUsageTotal;
        var valueColor = DrawHelper.ResolvePercentColor(usage, Accent);

        helper.DrawLabel(canvas, "CPU使用率", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{usage:0}%", rect.Right - 8, rect.Top + 42, valueColor);

        var percentage = (float)Math.Clamp(usage, 0, 100);
        helper.DrawCircularGauge(canvas, rect.MidX, rect.MidY + 18, Math.Min(rect.Width, rect.Height) * 0.27f, percentage, valueColor);

        helper.DrawDetail(canvas, $"P {monitor.CpuUsagePerformance:0}% / E {monitor.CpuUsageEfficiency:0}%", rect.Left + 8, rect.Bottom - 8);
    }
}

/// <summary>Stat widget for CPU user percentage.</summary>
internal sealed class CpuUserWidget : IWidget
{
    private static readonly SKColor Accent = new(0, 200, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "CPU", Accent, rect.Left + 5, rect.Top + 4);

        var value = monitor.CpuUserPercent;
        var valueColor = DrawHelper.ResolvePercentColor(value, Accent);

        helper.DrawLabel(canvas, "ユーザー", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{value:0.0}%", rect.Right - 8, rect.MidY + 8, valueColor, 36f);
    }
}

/// <summary>Stat widget for CPU system percentage.</summary>
internal sealed class CpuSystemWidget : IWidget
{
    private static readonly SKColor Accent = new(0, 200, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "CPU", Accent, rect.Left + 5, rect.Top + 4);

        var value = monitor.CpuSystemPercent;
        var valueColor = DrawHelper.ResolvePercentColor(value, Accent);

        helper.DrawLabel(canvas, "システム", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{value:0.0}%", rect.Right - 8, rect.MidY + 8, valueColor, 36f);
    }
}

/// <summary>Stat widget for load average.</summary>
internal sealed class LoadAverageWidget : IWidget
{
    private static readonly SKColor Accent = new(0, 200, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "CPU", Accent, rect.Left + 5, rect.Top + 4);

        helper.DrawLabel(canvas, "Load 1m", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{monitor.LoadAverage1:0.00}", rect.Right - 8, rect.MidY + 8, Accent, 36f);

        helper.DrawDetail(canvas, $"5m {monitor.LoadAverage5:0.00}  15m {monitor.LoadAverage15:0.00}", rect.Left + 8, rect.Bottom - 7);
    }
}
