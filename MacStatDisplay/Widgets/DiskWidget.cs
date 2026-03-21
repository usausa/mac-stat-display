namespace MacStatDisplay.Widgets;

using MacStatDisplay.Helpers;
using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Bar gauge widget for filesystem disk usage using display entries.
internal sealed class FileSystemWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "FS Disk Usage");

        var entries = monitor.FileSystems;
        var contentTop = rect.Top + Layout.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - Layout.PaddingY;
        var contentH = contentBottom - contentTop;

        var entryH = contentH / Math.Max(entries.Count, 1);
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
        var leftX = rect.Left + Layout.PaddingX;
        var rightX = rect.Right - Layout.PaddingX;
        var usedGb = totalGb - freeGb;
        var centerY = entryTop + (entryH / 2f);

        // Bar at center going down
        var barTop = centerY + 2;
        var barBottom = Math.Min(barTop + Layout.BarHeight, entryTop + entryH - 2);
        var barRect = new SKRect(leftX, barTop, rightX, barBottom);

        using var trackPaint = DrawHelper.Fill(Colors.TrackColor);
        canvas.DrawRoundRect(barRect, Layout.BarRadius, Layout.BarRadius, trackPaint);

        var fillWidth = barRect.Width * (float)Math.Clamp(usagePct, 0, 100) / 100f;
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        using var fillPaint = DrawHelper.Fill(Colors.FileSystemAccent);
        canvas.DrawRoundRect(fillRect, Layout.BarRadius, Layout.BarRadius, fillPaint);

        // Left side: mount point (line 1) and GB (line 2)
        using var mountFont = DrawHelper.MakeFont(FontSize.SubLabel);
        using var subPaint = DrawHelper.Fill(Colors.TextSecondary);
        using var accentPaint = DrawHelper.Fill(Colors.FileSystemAccent);
        canvas.DrawText(mount, leftX, centerY - 18, mountFont, subPaint);

        var gbText = $"{usedGb:0.0} / {totalGb:0.0} GB";
        canvas.DrawText(gbText, leftX, centerY - 2, mountFont, accentPaint);

        // Right side: usage percentage, large font, vertically centered with the two text lines
        using var pctFont = DrawHelper.MakeFont(FontSize.PrimaryValue, true);
        var pctText = $"{usagePct:0}%";
        canvas.DrawText(pctText, rightX - pctFont.MeasureText(pctText), centerY - 2, pctFont, accentPaint);
    }
}

// Disk I/O widget with sparkline graphs using display entries. Separate R/W sparklines per entry.
internal sealed class DiskIoWidget : IWidget
{
    private readonly Dictionary<string, RingBuffer> readHistory = [];
    private readonly Dictionary<string, RingBuffer> writeHistory = [];

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "DISK I/O");

        var entries = monitor.DiskDevices;
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
        RingBuffer rHist, RingBuffer wHist)
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

        // Use shared max so Write and Read graphs share the same scale
        var sharedMax = Math.Max(Math.Max(wHist.Max(), rHist.Max()), 1f);

        // Upper half: Write sparkline (upward from center)
        var wGraphRect = new SKRect(leftX, graphAreaTop, graphRight - 4, centerY - 1);
        DrawHelper.DrawSparkline(canvas, wGraphRect, wHist, sharedMax, Colors.DiskWriteAccent);

        canvas.DrawText("Write", rightX - labelFont.MeasureText("Write"), centerY - 20, labelFont, statLabelPaint);
        var wText = DrawHelper.FormatSpeed(writeBps);
        using var wValPaint = DrawHelper.Fill(Colors.DiskWriteAccent);
        canvas.DrawText(wText, rightX - valFont.MeasureText(wText), centerY - 4, valFont, wValPaint);

        // Lower half: Read sparkline (inverted, downward from center)
        var rGraphRect = new SKRect(leftX, centerY + 1, graphRight - 4, graphAreaBottom);
        DrawHelper.DrawSparklineInverted(canvas, rGraphRect, rHist, sharedMax, Colors.DiskReadAccent);

        canvas.DrawText("Read", rightX - labelFont.MeasureText("Read"), centerY + 14, labelFont, statLabelPaint);
        var rText = DrawHelper.FormatSpeed(readBps);
        using var rValPaint = DrawHelper.Fill(Colors.DiskReadAccent);
        canvas.DrawText(rText, rightX - valFont.MeasureText(rText), centerY + 30, valFont, rValPaint);
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
