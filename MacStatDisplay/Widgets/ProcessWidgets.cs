namespace MacStatDisplay.Widgets;

using System.Globalization;

using SkiaSharp;

/// <summary>Stat widget for process count.</summary>
internal sealed class ProcessCountWidget : IWidget
{
    private static readonly SKColor Accent = new(170, 130, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "プロセス", Accent, rect.Left + 5, rect.Top + 4);

        helper.DrawLabel(canvas, "プロセス数", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, monitor.ProcessCount.ToString(CultureInfo.InvariantCulture), rect.Right - 8, rect.MidY + 8, Accent, 36f);

        helper.DrawDetail(canvas, $"Threads {monitor.ThreadCount}", rect.Left + 8, rect.Bottom - 7, 9f);
    }
}

/// <summary>Stat widget for thread count.</summary>
internal sealed class ThreadCountWidget : IWidget
{
    private static readonly SKColor Accent = new(170, 130, 255);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "プロセス", Accent, rect.Left + 5, rect.Top + 4);

        helper.DrawLabel(canvas, "スレッド数", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, monitor.ThreadCount.ToString(CultureInfo.InvariantCulture), rect.Right - 8, rect.MidY + 8, Accent, 36f);
    }
}
