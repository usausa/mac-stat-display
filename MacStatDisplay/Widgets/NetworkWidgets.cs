namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Text widget for network download speed (with upload as detail).</summary>
internal sealed class NetworkDownloadWidget : IWidget
{
    private static readonly SKColor Accent = new(94, 234, 212);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "NET", "Download");

        var dlMbps = monitor.NetworkRxBytesPerSec / (1024.0 * 1024.0);
        var ulMbps = monitor.NetworkTxBytesPerSec / (1024.0 * 1024.0);

        helper.DrawValue(canvas, $"{dlMbps:0.00} MB/s", rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"Upload {ulMbps:0.00} MB/s", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
    }
}
