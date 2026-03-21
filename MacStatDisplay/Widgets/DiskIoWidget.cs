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

        var entries = monitor.DiskDevices;
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
            PushHistory(readHistory, e.Name, (float)e.ReadBytesPerSec);
            PushHistory(writeHistory, e.Name, (float)e.WriteBytesPerSec);
            DrawIoEntry(
                canvas, e.Name, e.ReadBytesPerSec, e.WriteBytesPerSec,
                leftX, rightX, contentTop + (i * entryH), entryH,
                readHistory[e.Name], writeHistory[e.Name]);
        }
    }

    private static void DrawIoEntry(
        SKCanvas canvas, string name, double readBps, double writeBps,
        float leftX, float rightX, float entryTop, float entryH,
        RingBuffer rHist, RingBuffer wHist)
    {
        // Name
        using var nameFont = DrawHelper.MakeFont(FontSize.SubLabel);
        using var namePaint = DrawHelper.Fill(Colors.TextSecondary);
        canvas.DrawText(name, leftX, entryTop + Layout.SparklineEntryNameBaseline, nameFont, namePaint);

        // Calculate
        var graphAreaTop = entryTop + Layout.SparklineLabelHeight;
        var graphAreaBottom = entryTop + entryH - Layout.SparklineGraphMargin;
        var centerY = graphAreaTop + ((graphAreaBottom - graphAreaTop) / 2f);
        var graphRight = rightX - Layout.SparklineValueColumnWidth;

        var sharedMax = Math.Max(Math.Max(wHist.Max(), rHist.Max()), 1f);

        // Upper
        var wGraphRect = new SKRect(leftX, graphAreaTop, graphRight - Layout.SparklineGraphGap, centerY - Layout.SparklineCenterGap);
        DrawHelper.DrawSparkline(canvas, wGraphRect, wHist, sharedMax, Colors.DiskWriteAccent);

        // Lower
        var rGraphRect = new SKRect(leftX, centerY + Layout.SparklineCenterGap, graphRight - Layout.SparklineGraphGap, graphAreaBottom);
        DrawHelper.DrawSparklineInverted(canvas, rGraphRect, rHist, sharedMax, Colors.DiskReadAccent);

        // Side
        var wText = DrawHelper.FormatSpeed(writeBps);
        var rText = DrawHelper.FormatSpeed(readBps);
        DrawHelper.DrawSparklineSideValues(
            canvas, rightX, graphAreaTop, graphAreaBottom,
            "Write", wText, Colors.DiskWriteAccent,
            "Read", rText, Colors.DiskReadAccent);
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
