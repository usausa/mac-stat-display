namespace MacStatDisplay.Widgets;

using MacStatDisplay.Helpers;
using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

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
        DrawHelper.DrawTitle(canvas, rect, "Network Traffic");

        var entries = monitor.NetworkInterfaces;
        if (entries.Count == 0)
        {
            return;
        }

        // Calculate
        var contentTop = rect.Top + Layout.TitleOffsetY + Layout.ContentTopGap;
        var contentBottom = rect.Bottom - Layout.PaddingY;
        var contentH = contentBottom - contentTop;
        var leftX = rect.Left + Layout.PaddingX;
        var rightX = rect.Right - Layout.PaddingX;

        var entryH = contentH / entries.Count;
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var displayName = e.DisplayName ?? e.Name;
            var rxBuffer = rxHistory.GetOrAdd(displayName, () => new RingBuffer(Layout.SparklineCapacity));
            var txBuffer = txHistory.GetOrAdd(displayName, () => new RingBuffer(Layout.SparklineCapacity));
            rxBuffer.Push((float)e.RxBytesPerSec);
            txBuffer.Push((float)e.TxBytesPerSec);
            DrawIfEntry(
                canvas, displayName, e.RxBytesPerSec, e.TxBytesPerSec,
                leftX, rightX, contentTop + (i * entryH), entryH,
                rxBuffer, txBuffer);
        }
    }

    private static void DrawIfEntry(
        SKCanvas canvas, string name, double rxBps, double txBps,
        float leftX, float rightX, float entryTop, float entryH,
        RingBuffer rHist, RingBuffer tHist)
    {
        // Name
        var nameFont = DrawHelper.GetFont(FontSize.SubLabel);
        canvas.DrawText(name, leftX, entryTop + Layout.SparklineEntryNameBaseline, nameFont, DrawHelper.GetFillPaint(Colors.TextSecondary));

        // Calculate
        var graphAreaTop = entryTop + Layout.SparklineLabelHeight;
        var graphAreaBottom = entryTop + entryH - Layout.SparklineGraphMargin;
        var centerY = graphAreaTop + ((graphAreaBottom - graphAreaTop) / 2f);
        var graphRight = rightX - Layout.SparklineValueColumnWidth;

        var sharedMax = Math.Max(Math.Max(tHist.Max(), rHist.Max()), 1f);

        // Upper
        var txGraphRect = new SKRect(leftX, graphAreaTop, graphRight - Layout.SparklineGraphGap, centerY - Layout.SparklineCenterGap);
        DrawHelper.DrawSparkline(canvas, txGraphRect, tHist, sharedMax, Colors.NetworkUploadAccent);

        // Lower
        var rxGraphRect = new SKRect(leftX, centerY + Layout.SparklineCenterGap, graphRight - Layout.SparklineGraphGap, graphAreaBottom);
        DrawHelper.DrawSparklineInverted(canvas, rxGraphRect, rHist, sharedMax, Colors.NetworkDownloadAccent);

        // Side
        var txText = DrawHelper.FormatSpeed(txBps);
        var rxText = DrawHelper.FormatSpeed(rxBps);
        DrawHelper.DrawSparklineValues(
            canvas, rightX, graphAreaTop, graphAreaBottom,
            "Upload", txText, Colors.NetworkUploadAccent,
            "Download", rxText, Colors.NetworkDownloadAccent);
    }
}
