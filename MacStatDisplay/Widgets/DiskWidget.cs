namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Bar gauge widget for filesystem disk usage using real FileSystemMonitorEntry types.</summary>
internal sealed class FileSystemWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "FS", "Disk Usage");

        var entries = monitor.FileSystems;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;

        if (entries.Count == 0)
        {
            // Fallback to aggregate
            DrawEntry(canvas, rect, "/", monitor.DiskTotalGb, monitor.DiskFreeGb, monitor.DiskUsagePercent, contentTop, contentBottom);
            return;
        }

        var entryHeight = (contentBottom - contentTop) / Math.Max(entries.Count, 1);
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var totalGb = e.TotalSize / (1024.0 * 1024.0 * 1024.0);
            var freeGb = e.FreeSize / (1024.0 * 1024.0 * 1024.0);
            var usagePct = e.TotalSize > 0 ? (double)(e.TotalSize - e.FreeSize) / e.TotalSize * 100.0 : 0;
            DrawEntry(canvas, rect, e.MountPoint, totalGb, freeGb, usagePct,
                contentTop + (i * entryHeight), contentTop + ((i + 1) * entryHeight));
        }
    }

    private static void DrawEntry(
        SKCanvas canvas, SKRect rect,
        string mount, double totalGb, double freeGb, double usagePct,
        float top, float bottom)
    {
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;
        var usedGb = totalGb - freeGb;

        // Larger font for FS labels
        using var detailFont = DrawHelper.MakeFont(WidgetTheme.DetailFontSize);
        using var subPaint = DrawHelper.Fill(WidgetTheme.TextSub);
        using var valPaint = DrawHelper.Fill(WidgetTheme.TextPrimary);

        var textY = top + 16;
        canvas.DrawText(mount, leftX, textY, detailFont, subPaint);

        var gbText = $"{usedGb:0.0} / {totalGb:0.0} GB";
        canvas.DrawText(gbText, rect.MidX - (detailFont.MeasureText(gbText) / 2f), textY, detailFont, subPaint);

        using var pctFont = DrawHelper.MakeFont(WidgetTheme.DetailFontSize, true);
        var pctText = $"{usagePct:0}%";
        canvas.DrawText(pctText, rightX - pctFont.MeasureText(pctText), textY, pctFont, valPaint);

        // Thicker bar, tight to text
        var barTop = textY + 5;
        var barBottom = Math.Min(barTop + WidgetTheme.BarHeight, bottom - 4);
        var barRect = new SKRect(leftX, barTop, rightX, barBottom);

        using var trackPaint = DrawHelper.Fill(WidgetTheme.TrackColor);
        canvas.DrawRoundRect(barRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, trackPaint);

        var fillWidth = barRect.Width * (float)Math.Clamp(usagePct, 0, 100) / 100f;
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        using var fillPaint = DrawHelper.Fill(WidgetTheme.FileSystemAccent);
        canvas.DrawRoundRect(fillRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, fillPaint);
    }
}

/// <summary>Disk I/O widget with sparkline graphs and multi-disk support using real DiskDeviceEntry types.</summary>
internal sealed class DiskIoWidget : IWidget
{
    private readonly Dictionary<string, List<float>> readHistory = [];
    private readonly Dictionary<string, List<float>> writeHistory = [];

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "DISK", "I/O");

        var entries = monitor.DiskDevices;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;

        if (entries.Count == 0)
        {
            // Fallback to aggregate
            PushHistory(readHistory, "agg", (float)monitor.DiskReadBytesPerSec);
            PushHistory(writeHistory, "agg", (float)monitor.DiskWriteBytesPerSec);
            DrawIoEntry(canvas, "aggregate", monitor.DiskReadBytesPerSec, monitor.DiskWriteBytesPerSec,
                leftX, rightX, contentTop, contentBottom,
                readHistory["agg"], writeHistory["agg"]);
            return;
        }

        var entryHeight = (contentBottom - contentTop) / entries.Count;
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            PushHistory(readHistory, e.Name, (float)e.ReadBytesPerSec);
            PushHistory(writeHistory, e.Name, (float)e.WriteBytesPerSec);
            DrawIoEntry(canvas, e.Name, e.ReadBytesPerSec, e.WriteBytesPerSec,
                leftX, rightX, contentTop + (i * entryHeight), contentTop + ((i + 1) * entryHeight),
                readHistory[e.Name], writeHistory[e.Name]);
        }
    }

    private static void DrawIoEntry(
        SKCanvas canvas, string name, double readBps, double writeBps,
        float leftX, float rightX, float top, float bottom,
        List<float> rHist, List<float> wHist)
    {
        using var nameFont = DrawHelper.MakeFont(WidgetTheme.SmallFontSize);
        using var namePaint = DrawHelper.Fill(WidgetTheme.TextSub);
        canvas.DrawText(name, leftX, top + 14, nameFont, namePaint);

        var valueWidth = 130f;
        var graphRight = rightX - valueWidth;
        var rowH = (bottom - top - 18) / 2f;

        // Read row
        var rTop = top + 18;
        var rGraphRect = new SKRect(leftX, rTop, graphRight - 4, rTop + rowH - 2);
        var rMax = rHist.Count > 0 ? Math.Max(rHist.Max(), 1f) : 1f;
        DrawHelper.DrawSparkline(canvas, rGraphRect, rHist, rMax, WidgetTheme.DiskIoAccent);

        using var valFont = DrawHelper.MakeFont(WidgetTheme.DetailFontSize, true);
        using var valPaint = DrawHelper.Fill(WidgetTheme.DiskIoAccent);
        var rText = $"R {DrawHelper.FormatSpeed(readBps)}";
        canvas.DrawText(rText, rightX - valFont.MeasureText(rText), rTop + rowH - 4, valFont, valPaint);

        // Write row
        var wTop = rTop + rowH;
        var wGraphRect = new SKRect(leftX, wTop, graphRight - 4, wTop + rowH - 2);
        var wMax = wHist.Count > 0 ? Math.Max(wHist.Max(), 1f) : 1f;
        DrawHelper.DrawSparkline(canvas, wGraphRect, wHist, wMax, WidgetTheme.DiskIoAccent);

        var wText = $"W {DrawHelper.FormatSpeed(writeBps)}";
        canvas.DrawText(wText, rightX - valFont.MeasureText(wText), wTop + rowH - 4, valFont, valPaint);
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
