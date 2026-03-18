namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Network traffic widget with sparkline graphs using display entries. Separate TX/RX sparklines per entry.</summary>
internal sealed class NetworkWidget : IWidget
{
    private readonly Dictionary<string, List<float>> rxHistory = [];
    private readonly Dictionary<string, List<float>> txHistory = [];

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "NET", "Traffic");

        var entries = monitor.NetworkIfDisplayEntries;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;
        var contentH = contentBottom - contentTop;
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;

        if (entries.Count == 0)
        {
            PushHistory(rxHistory, "agg", (float)monitor.NetworkRxBytesPerSec);
            PushHistory(txHistory, "agg", (float)monitor.NetworkTxBytesPerSec);
            DrawIfEntry(canvas, "aggregate", monitor.NetworkRxBytesPerSec, monitor.NetworkTxBytesPerSec,
                leftX, rightX, contentTop, contentH,
                rxHistory["agg"], txHistory["agg"]);
            return;
        }

        var entryH = contentH / entries.Count;
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            PushHistory(rxHistory, e.Name, (float)e.RxBytesPerSec);
            PushHistory(txHistory, e.Name, (float)e.TxBytesPerSec);
            DrawIfEntry(canvas, e.Name, e.RxBytesPerSec, e.TxBytesPerSec,
                leftX, rightX, contentTop + (i * entryH), entryH,
                rxHistory[e.Name], txHistory[e.Name]);
        }
    }

    private static void DrawIfEntry(
        SKCanvas canvas, string name, double rxBps, double txBps,
        float leftX, float rightX, float entryTop, float entryH,
        List<float> rHist, List<float> tHist)
    {
        // Name label at entry top
        using var nameFont = DrawHelper.MakeFont(WidgetTheme.SmallFontSize);
        using var namePaint = DrawHelper.Fill(WidgetTheme.TextSub);
        canvas.DrawText(name, leftX, entryTop + 14, nameFont, namePaint);

        var labelH = 18f;
        var graphAreaTop = entryTop + labelH;
        var graphAreaBottom = entryTop + entryH - 2;
        var centerY = graphAreaTop + ((graphAreaBottom - graphAreaTop) / 2f);
        var valueWidth = 85f;
        var graphRight = rightX - valueWidth;

        using var labelFont = DrawHelper.MakeFont(WidgetTheme.SmallFontSize);
        using var valFont = DrawHelper.MakeFont(WidgetTheme.DetailFontSize, true);
        using var statLabelPaint = DrawHelper.Fill(WidgetTheme.TextSub);

        // Upper half: TX (upload) sparkline (upward from center)
        var txGraphRect = new SKRect(leftX, graphAreaTop, graphRight - 4, centerY - 1);
        var txMax = tHist.Count > 0 ? Math.Max(tHist.Max(), 1f) : 1f;
        DrawHelper.DrawSparkline(canvas, txGraphRect, tHist, txMax, WidgetTheme.NetworkAccent);

        canvas.DrawText("Upload", rightX - labelFont.MeasureText("Upload"), centerY - 20, labelFont, statLabelPaint);
        var txText = DrawHelper.FormatSpeed(txBps);
        using var txValPaint = DrawHelper.Fill(WidgetTheme.NetworkAccent);
        canvas.DrawText(txText, rightX - valFont.MeasureText(txText), centerY - 4, valFont, txValPaint);

        // Lower half: RX (download) sparkline (inverted, downward from center)
        var rxGraphRect = new SKRect(leftX, centerY + 1, graphRight - 4, graphAreaBottom);
        var rxMax = rHist.Count > 0 ? Math.Max(rHist.Max(), 1f) : 1f;
        DrawHelper.DrawSparklineInverted(canvas, rxGraphRect, rHist, rxMax, WidgetTheme.NetworkRxAccent);

        canvas.DrawText("Download", rightX - labelFont.MeasureText("Download"), centerY + 14, labelFont, statLabelPaint);
        var rxText = DrawHelper.FormatSpeed(rxBps);
        using var rxValPaint = DrawHelper.Fill(WidgetTheme.NetworkRxAccent);
        canvas.DrawText(rxText, rightX - valFont.MeasureText(rxText), centerY + 30, valFont, rxValPaint);
    }

    private static void PushHistory(Dictionary<string, List<float>> dict, string key, float value)
    {
        if (!dict.TryGetValue(key, out var list))
        {
            list = new List<float>(WidgetTheme.SparklineCapacity);
            dict[key] = list;
        }

        if (list.Count >= WidgetTheme.SparklineCapacity)
        {
            list.RemoveAt(0);
        }

        list.Add(value);
    }
}
