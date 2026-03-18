namespace MacStatDisplay.Widgets;

using System.Globalization;
using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Text widget for process count.</summary>
internal sealed class ProcessCountWidget : IWidget
{
    private static readonly SKColor Accent = new(244, 114, 182);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "PROC", "Processes");

        helper.DrawValue(canvas, monitor.ProcessCount.ToString(CultureInfo.InvariantCulture), rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"Threads {monitor.ThreadCount}", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
    }
}
