namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Network traffic widget with sparkline graphs and multi-interface support using real NetworkIfEntry types.</summary>
internal sealed class NetworkWidget : IWidget
{
    private readonly Dictionary<string, List<float>> rxHistory = [];
    private readonly Dictionary<string, List<float>> txHistory = [];

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "NET", "Traffic");

        var entries = monitor.NetworkInterfaces;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;

        if (entries.Count == 0)
        {
            // Fallback to aggregate
            PushHistory(rxHistory, "agg", (float)monitor.NetworkRxBytesPerSec);
            PushHistory(txHistory, "agg", (float)monitor.NetworkTxBytesPerSec);
            DrawIfEntry(canvas, "aggregate", monitor.NetworkRxBytesPerSec, monitor.NetworkTxBytesPerSec,
                leftX, rightX, contentTop, contentBottom,
                rxHistory["agg"], txHistory["agg"]);
            return;
        }

        var entryHeight = (contentBottom - contentTop) / entries.Count;
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var name = e.DisplayName ?? e.Name;
            PushHistory(rxHistory, name, (float)e.RxBytesPerSec);
            PushHistory(txHistory, name, (float)e.TxBytesPerSec);
            DrawIfEntry(canvas, name, e.RxBytesPerSec, e.TxBytesPerSec,
                leftX, rightX, contentTop + (i * entryHeight), contentTop + ((i + 1) * entryHeight),
                rxHistory[name], txHistory[name]);
        }
    }

    private static void DrawIfEntry(
        SKCanvas canvas, string name, double rxBps, double txBps,
        float leftX, float rightX, float top, float bottom,
        List<float> rHist, List<float> tHist)
    {
        using var nameFont = DrawHelper.MakeFont(WidgetTheme.SmallFontSize);
        using var namePaint = DrawHelper.Fill(WidgetTheme.TextSub);
        canvas.DrawText(name, leftX, top + 14, nameFont, namePaint);

        var valueWidth = 130f;
        var graphRight = rightX - valueWidth;
        var rowH = (bottom - top - 18) / 2f;

        // DL row
        var dlTop = top + 18;
        var dlGraphRect = new SKRect(leftX, dlTop, graphRight - 4, dlTop + rowH - 2);
        var dlMax = rHist.Count > 0 ? Math.Max(rHist.Max(), 1f) : 1f;
        DrawHelper.DrawSparkline(canvas, dlGraphRect, rHist, dlMax, WidgetTheme.NetworkAccent);

        using var valFont = DrawHelper.MakeFont(WidgetTheme.DetailFontSize, true);
        using var valPaint = DrawHelper.Fill(WidgetTheme.NetworkAccent);
        var dlText = $"\u2193 {DrawHelper.FormatSpeed(rxBps)}";
        canvas.DrawText(dlText, rightX - valFont.MeasureText(dlText), dlTop + rowH - 4, valFont, valPaint);

        // UL row
        var ulTop = dlTop + rowH;
        var ulGraphRect = new SKRect(leftX, ulTop, graphRight - 4, ulTop + rowH - 2);
        var ulMax = tHist.Count > 0 ? Math.Max(tHist.Max(), 1f) : 1f;
        DrawHelper.DrawSparkline(canvas, ulGraphRect, tHist, ulMax, WidgetTheme.NetworkAccent);

        var ulText = $"\u2191 {DrawHelper.FormatSpeed(txBps)}";
        canvas.DrawText(ulText, rightX - valFont.MeasureText(ulText), ulTop + rowH - 4, valFont, valPaint);
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
