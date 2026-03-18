namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Network traffic widget with sparkline graphs using display entries. Separate TX/RX sparklines per entry.</summary>
internal sealed class NetworkWidget : IWidget
{
    private readonly Dictionary<string, SparklineBuffer> rxHistory = [];
    private readonly Dictionary<string, SparklineBuffer> txHistory = [];

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "NET", "Traffic");

        var entries = monitor.NetworkInterfaces;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;
        var contentH = contentBottom - contentTop;
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;

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
        SparklineBuffer rHist, SparklineBuffer tHist)
    {
        // Name label at entry top
        using var nameFont = DrawHelper.MakeFont(WidgetTheme.SubLabelFontSize);
        using var namePaint = DrawHelper.Fill(WidgetTheme.TextSub);
        canvas.DrawText(name, leftX, entryTop + 14, nameFont, namePaint);

        var labelH = 18f;
        var graphAreaTop = entryTop + labelH;
        var graphAreaBottom = entryTop + entryH - 2;
        var centerY = graphAreaTop + ((graphAreaBottom - graphAreaTop) / 2f);
        var valueWidth = 85f;
        var graphRight = rightX - valueWidth;

        using var labelFont = DrawHelper.MakeFont(WidgetTheme.SubLabelFontSize);
        using var valFont = DrawHelper.MakeFont(WidgetTheme.SubValueFontSize, true);
        using var statLabelPaint = DrawHelper.Fill(WidgetTheme.TextSub);

        // Use shared max so Upload and Download graphs share the same scale
        var sharedMax = Math.Max(Math.Max(tHist.Max(), rHist.Max()), 1f);

        // Upper half: TX (upload) sparkline (upward from center)
        var txGraphRect = new SKRect(leftX, graphAreaTop, graphRight - 4, centerY - 1);
        DrawHelper.DrawSparkline(canvas, txGraphRect, tHist, sharedMax, WidgetTheme.NetworkUploadAccent);

        canvas.DrawText("Upload", rightX - labelFont.MeasureText("Upload"), centerY - 20, labelFont, statLabelPaint);
        var txText = DrawHelper.FormatSpeed(txBps);
        using var txValPaint = DrawHelper.Fill(WidgetTheme.NetworkUploadAccent);
        canvas.DrawText(txText, rightX - valFont.MeasureText(txText), centerY - 4, valFont, txValPaint);

        // Lower half: RX (download) sparkline (inverted, downward from center)
        var rxGraphRect = new SKRect(leftX, centerY + 1, graphRight - 4, graphAreaBottom);
        DrawHelper.DrawSparklineInverted(canvas, rxGraphRect, rHist, sharedMax, WidgetTheme.NetworkDownloadAccent);

        canvas.DrawText("Download", rightX - labelFont.MeasureText("Download"), centerY + 14, labelFont, statLabelPaint);
        var rxText = DrawHelper.FormatSpeed(rxBps);
        using var rxValPaint = DrawHelper.Fill(WidgetTheme.NetworkDownloadAccent);
        canvas.DrawText(rxText, rightX - valFont.MeasureText(rxText), centerY + 30, valFont, rxValPaint);
    }

    private static void PushHistory(Dictionary<string, SparklineBuffer> dict, string key, float value)
    {
        if (!dict.TryGetValue(key, out var buf))
        {
            buf = new SparklineBuffer(WidgetTheme.SparklineCapacity);
            dict[key] = buf;
        }

        buf.Push(value);
    }
}
