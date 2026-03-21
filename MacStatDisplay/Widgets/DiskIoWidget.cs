namespace MacStatDisplay.Widgets;

using MacStatDisplay.Helpers;
using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

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
        DrawHelper.DrawTitleBlock(canvas, rect, "Disk I/O");

        // TODO
        var entries = monitor.DiskDevices;
        var contentTop = rect.Top + Layout.TitleOffsetY + Layout.ContentTopGap;
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
        canvas.DrawText(name, leftX, entryTop + Layout.SparklineEntryNameBaseline, nameFont, namePaint);

        var graphAreaTop = entryTop + Layout.SparklineLabelHeight;
        var graphAreaBottom = entryTop + entryH - Layout.SparklineGraphMargin;
        var centerY = graphAreaTop + ((graphAreaBottom - graphAreaTop) / 2f);
        var graphRight = rightX - Layout.SparklineValueColumnWidth;

        using var labelFont = DrawHelper.MakeFont(FontSize.SubLabel);
        using var valFont = DrawHelper.MakeFont(FontSize.SubValue, true);
        using var statLabelPaint = DrawHelper.Fill(Colors.TextSecondary);

        // Use shared max so Write and Read graphs share the same scale
        var sharedMax = Math.Max(Math.Max(wHist.Max(), rHist.Max()), 1f);

        // Upper half: Write sparkline (upward from center)
        var wGraphRect = new SKRect(leftX, graphAreaTop, graphRight - Layout.SparklineGraphGap, centerY - Layout.SparklineCenterGap);
        DrawHelper.DrawSparkline(canvas, wGraphRect, wHist, sharedMax, Colors.DiskWriteAccent);

        canvas.DrawText("Write", rightX - labelFont.MeasureText("Write"), centerY - Layout.SparklineUpperLabelOffsetY, labelFont, statLabelPaint);
        var wText = DrawHelper.FormatSpeed(writeBps);
        using var wValPaint = DrawHelper.Fill(Colors.DiskWriteAccent);
        canvas.DrawText(wText, rightX - valFont.MeasureText(wText), centerY - Layout.SparklineUpperValueOffsetY, valFont, wValPaint);

        // Lower half: Read sparkline (inverted, downward from center)
        var rGraphRect = new SKRect(leftX, centerY + Layout.SparklineCenterGap, graphRight - Layout.SparklineGraphGap, graphAreaBottom);
        DrawHelper.DrawSparklineInverted(canvas, rGraphRect, rHist, sharedMax, Colors.DiskReadAccent);

        canvas.DrawText("Read", rightX - labelFont.MeasureText("Read"), centerY + Layout.SparklineLowerLabelOffsetY, labelFont, statLabelPaint);
        var rText = DrawHelper.FormatSpeed(readBps);
        using var rValPaint = DrawHelper.Fill(Colors.DiskReadAccent);
        canvas.DrawText(rText, rightX - valFont.MeasureText(rText), centerY + Layout.SparklineLowerValueOffsetY, valFont, rValPaint);
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
