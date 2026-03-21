namespace MacStatDisplay.Widgets;

using MacStatDisplay.Helpers;
using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Network traffic widget with sparkline graphs using display entries. Separate TX/RX sparklines per entry.
internal sealed class NetworkWidget : IWidget
{
    private readonly Dictionary<string, RingBuffer> rxHistory = [];
    private readonly Dictionary<string, RingBuffer> txHistory = [];

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "NET Traffic");

        var entries = monitor.NetworkInterfaces;
        var contentTop = rect.Top + Layout.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - Layout.PaddingY;
        var contentH = contentBottom - contentTop;
        var leftX = rect.Left + Layout.PaddingX;
        var rightX = rect.Right - Layout.PaddingX;

        if (entries.Count == 0)
        {
            return;
        }

        var entryH = contentH / entries.Count;
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var displayName = e.DisplayName ?? e.Name;
            PushHistory(rxHistory, displayName, (float)e.RxBytesPerSec);
            PushHistory(txHistory, displayName, (float)e.TxBytesPerSec);
            DrawIfEntry(canvas, displayName, e.RxBytesPerSec, e.TxBytesPerSec,
                leftX, rightX, contentTop + (i * entryH), entryH,
                rxHistory[displayName], txHistory[displayName]);
        }
    }

    private static void DrawIfEntry(
        SKCanvas canvas, string name, double rxBps, double txBps,
        float leftX, float rightX, float entryTop, float entryH,
        RingBuffer rHist, RingBuffer tHist)
    {
        // Name label at entry top
        using var nameFont = DrawHelper.MakeFont(FontSize.SubLabel);
        using var namePaint = DrawHelper.Fill(Colors.TextSecondary);
        canvas.DrawText(name, leftX, entryTop + 14, nameFont, namePaint);

        var labelH = 18f;
        var graphAreaTop = entryTop + labelH;
        var graphAreaBottom = entryTop + entryH - 2;
        var centerY = graphAreaTop + ((graphAreaBottom - graphAreaTop) / 2f);
        var valueWidth = 85f;
        var graphRight = rightX - valueWidth;

        using var labelFont = DrawHelper.MakeFont(FontSize.SubLabel);
        using var valFont = DrawHelper.MakeFont(FontSize.SubValue, true);
        using var statLabelPaint = DrawHelper.Fill(Colors.TextSecondary);

        // Use shared max so Upload and Download graphs share the same scale
        var sharedMax = Math.Max(Math.Max(tHist.Max(), rHist.Max()), 1f);

        // Upper half: TX (upload) sparkline (upward from center)
        var txGraphRect = new SKRect(leftX, graphAreaTop, graphRight - 4, centerY - 1);
        DrawHelper.DrawSparkline(canvas, txGraphRect, tHist, sharedMax, Colors.NetworkUploadAccent);

        canvas.DrawText("Upload", rightX - labelFont.MeasureText("Upload"), centerY - 20, labelFont, statLabelPaint);
        var txText = DrawHelper.FormatSpeed(txBps);
        using var txValPaint = DrawHelper.Fill(Colors.NetworkUploadAccent);
        canvas.DrawText(txText, rightX - valFont.MeasureText(txText), centerY - 4, valFont, txValPaint);

        // Lower half: RX (download) sparkline (inverted, downward from center)
        var rxGraphRect = new SKRect(leftX, centerY + 1, graphRight - 4, graphAreaBottom);
        DrawHelper.DrawSparklineInverted(canvas, rxGraphRect, rHist, sharedMax, Colors.NetworkDownloadAccent);

        canvas.DrawText("Download", rightX - labelFont.MeasureText("Download"), centerY + 14, labelFont, statLabelPaint);
        var rxText = DrawHelper.FormatSpeed(rxBps);
        using var rxValPaint = DrawHelper.Fill(Colors.NetworkDownloadAccent);
        canvas.DrawText(rxText, rightX - valFont.MeasureText(rxText), centerY + 30, valFont, rxValPaint);
    }

    private static void PushHistory(Dictionary<string, RingBuffer> dict, string key, float value)
    {
        if (!dict.TryGetValue(key, out var buf))
        {
            buf = new RingBuffer(Layout.SparklineCapacity);
            dict[key] = buf;
        }

        buf.Push(value);
    }
}
