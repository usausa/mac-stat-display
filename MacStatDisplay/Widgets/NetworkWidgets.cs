namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Text widget for network speeds (download/upload) showing the first interface.</summary>
internal sealed class NetworkWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "NET", "Traffic");

        var rightX = rect.Right - WidgetTheme.PadX;
        var leftX = rect.Left + WidgetTheme.PadX;
        var bottomY = rect.Bottom - WidgetTheme.PadY;

        // Use first entry if available, otherwise aggregate
        var entries = monitor.NetworkIfSnapshots;
        double rx, tx;
        if (entries.Count > 0)
        {
            var first = entries[0];
            rx = first.RxBytesPerSec;
            tx = first.TxBytesPerSec;

            using var nameFont = helper.MakeFont(WidgetTheme.SmallFontSize);
            using var namePaint = DrawHelper.Fill(WidgetTheme.TextSub);
            canvas.DrawText(first.Name, leftX, rect.Top + WidgetTheme.TitleOffsetY + 18, nameFont, namePaint);
        }
        else
        {
            rx = monitor.NetworkRxBytesPerSec;
            tx = monitor.NetworkTxBytesPerSec;
        }

        var dlText = $"\u2193 {DrawHelper.FormatSpeed(rx)}";
        var ulText = $"\u2191 {DrawHelper.FormatSpeed(tx)}";
        helper.DrawValue(canvas, ulText, rightX, bottomY, WidgetTheme.NetworkAccent);
        helper.DrawValue(canvas, dlText, rightX, bottomY - WidgetTheme.ValueLargeFontSize - 2, WidgetTheme.NetworkAccent);
    }
}
