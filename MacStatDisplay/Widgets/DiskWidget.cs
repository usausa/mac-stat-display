namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Bar gauge widget for filesystem disk usage using display entries.</summary>
internal sealed class FileSystemWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "FS", "Disk Usage");

        var entries = monitor.FileSystems;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;
        var contentH = contentBottom - contentTop;

        if (entries.Count == 0)
        {
            DrawEntry(canvas, rect, "/", monitor.DiskTotalGb, monitor.DiskFreeGb, monitor.DiskUsagePercent,
                contentTop, contentH);
            return;
        }

        var entryH = contentH / entries.Count;
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var totalGb = e.TotalSize / (1024.0 * 1024.0 * 1024.0);
            var freeGb = e.FreeSize / (1024.0 * 1024.0 * 1024.0);
            var usagePct = e.TotalSize > 0 ? (double)(e.TotalSize - e.FreeSize) / e.TotalSize * 100.0 : 0;
            DrawEntry(canvas, rect, e.MountPoint, totalGb, freeGb, usagePct,
                contentTop + (i * entryH), entryH);
        }
    }

    private static void DrawEntry(
        SKCanvas canvas, SKRect rect,
        string mount, double totalGb, double freeGb, double usagePct,
        float entryTop, float entryH)
    {
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;
        var usedGb = totalGb - freeGb;
        var centerY = entryTop + (entryH / 2f);

        // Bar at center going down
        var barTop = centerY + 2;
        var barBottom = Math.Min(barTop + WidgetTheme.BarHeight, entryTop + entryH - 2);
        var barRect = new SKRect(leftX, barTop, rightX, barBottom);

        using var trackPaint = DrawHelper.Fill(WidgetTheme.TrackColor);
        canvas.DrawRoundRect(barRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, trackPaint);

        var fillWidth = barRect.Width * (float)Math.Clamp(usagePct, 0, 100) / 100f;
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        using var fillPaint = DrawHelper.Fill(WidgetTheme.FileSystemAccent);
        canvas.DrawRoundRect(fillRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, fillPaint);

        // Left side: mount point (line 1) and GB (line 2)
        using var mountFont = DrawHelper.MakeFont(WidgetTheme.SubLabelFontSize);
        using var subPaint = DrawHelper.Fill(WidgetTheme.TextSub);
        using var accentPaint = DrawHelper.Fill(WidgetTheme.FileSystemAccent);
        canvas.DrawText(mount, leftX, centerY - 18, mountFont, subPaint);

        var gbText = $"{usedGb:0.0} / {totalGb:0.0} GB";
        canvas.DrawText(gbText, leftX, centerY - 2, mountFont, accentPaint);

        // Right side: usage percentage, large font, vertically centered with the two text lines
        using var pctFont = DrawHelper.MakeFont(WidgetTheme.PrimaryValueFontSize, true);
        var pctText = $"{usagePct:0}%";
        canvas.DrawText(pctText, rightX - pctFont.MeasureText(pctText), centerY - 2, pctFont, accentPaint);
    }
}

/// <summary>Disk I/O widget with sparkline graphs using display entries. Separate R/W sparklines per entry.</summary>
internal sealed class DiskIoWidget : IWidget
{
    private readonly Dictionary<string, SparklineBuffer> readHistory = [];
    private readonly Dictionary<string, SparklineBuffer> writeHistory = [];

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "DISK", "I/O");

        var entries = monitor.DiskDevices;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;
        var contentH = contentBottom - contentTop;
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;

        if (entries.Count == 0)
        {
            PushHistory(readHistory, "agg", (float)monitor.DiskReadBytesPerSec);
            PushHistory(writeHistory, "agg", (float)monitor.DiskWriteBytesPerSec);
            DrawIoEntry(canvas, "aggregate", monitor.DiskReadBytesPerSec, monitor.DiskWriteBytesPerSec,
                leftX, rightX, contentTop, contentH,
                readHistory["agg"], writeHistory["agg"]);
            return;
        }

        var entryH = contentH / entries.Count;
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            PushHistory(readHistory, e.Name, (float)e.ReadBytesPerSec);
            PushHistory(writeHistory, e.Name, (float)e.WriteBytesPerSec);
            DrawIoEntry(canvas, e.Name, e.ReadBytesPerSec, e.WriteBytesPerSec,
                leftX, rightX, contentTop + (i * entryH), entryH,
                readHistory[e.Name], writeHistory[e.Name]);
        }
    }

    private static void DrawIoEntry(
        SKCanvas canvas, string name, double readBps, double writeBps,
        float leftX, float rightX, float entryTop, float entryH,
        SparklineBuffer rHist, SparklineBuffer wHist)
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

        // Use shared max so Write and Read graphs share the same scale
        var sharedMax = Math.Max(Math.Max(wHist.Max(), rHist.Max()), 1f);

        // Upper half: Write sparkline (upward from center)
        var wGraphRect = new SKRect(leftX, graphAreaTop, graphRight - 4, centerY - 1);
        DrawHelper.DrawSparkline(canvas, wGraphRect, wHist, sharedMax, WidgetTheme.DiskWriteAccent);

        canvas.DrawText("Write", rightX - labelFont.MeasureText("Write"), centerY - 20, labelFont, statLabelPaint);
        var wText = DrawHelper.FormatSpeed(writeBps);
        using var wValPaint = DrawHelper.Fill(WidgetTheme.DiskWriteAccent);
        canvas.DrawText(wText, rightX - valFont.MeasureText(wText), centerY - 4, valFont, wValPaint);

        // Lower half: Read sparkline (inverted, downward from center)
        var rGraphRect = new SKRect(leftX, centerY + 1, graphRight - 4, graphAreaBottom);
        DrawHelper.DrawSparklineInverted(canvas, rGraphRect, rHist, sharedMax, WidgetTheme.DiskReadAccent);

        canvas.DrawText("Read", rightX - labelFont.MeasureText("Read"), centerY + 14, labelFont, statLabelPaint);
        var rText = DrawHelper.FormatSpeed(readBps);
        using var rValPaint = DrawHelper.Fill(WidgetTheme.DiskReadAccent);
        canvas.DrawText(rText, rightX - valFont.MeasureText(rText), centerY + 30, valFont, rValPaint);
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
