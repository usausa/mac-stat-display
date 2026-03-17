namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Stat widget for network download speed.</summary>
internal sealed class NetworkDownloadWidget : IWidget
{
    private static readonly SKColor Accent = new(100, 210, 120);
    private static readonly SKColor ValueColor = new(100, 226, 180);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "ネットワーク", Accent, rect.Left + 5, rect.Top + 4);

        var mbps = monitor.NetworkRxBytesPerSec / (1024.0 * 1024.0);

        helper.DrawLabel(canvas, "DL", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{mbps:0.00} MB/s", rect.Right - 8, rect.MidY + 8, ValueColor, 36f);
    }
}

/// <summary>Stat widget for network upload speed.</summary>
internal sealed class NetworkUploadWidget : IWidget
{
    private static readonly SKColor Accent = new(100, 210, 120);
    private static readonly SKColor ValueColor = new(100, 226, 180);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "ネットワーク", Accent, rect.Left + 5, rect.Top + 4);

        var mbps = monitor.NetworkTxBytesPerSec / (1024.0 * 1024.0);

        helper.DrawLabel(canvas, "UP", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{mbps:0.00} MB/s", rect.Right - 8, rect.MidY + 8, ValueColor, 36f);
    }
}
