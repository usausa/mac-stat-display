namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Bar gauge widget for filesystem disk usage with dynamic multi-disk support.</summary>
internal sealed class FileSystemWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "FS", "Disk Usage");

        var entries = monitor.FileSystemSnapshots;
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;

        if (entries.Count == 0)
        {
            DrawEntry(canvas, rect, helper, "/", monitor.DiskTotalGb, monitor.DiskFreeGb, monitor.DiskUsagePercent, contentTop, contentBottom);
            return;
        }

        var entryHeight = (contentBottom - contentTop) / Math.Max(entries.Count, 1);
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            DrawEntry(canvas, rect, helper, e.MountPoint, e.TotalGb, e.FreeGb, e.UsagePercent,
                contentTop + (i * entryHeight), contentTop + ((i + 1) * entryHeight));
        }
    }

    private static void DrawEntry(
        SKCanvas canvas, SKRect rect, DrawHelper helper,
        string mount, double totalGb, double freeGb, double usagePct,
        float top, float bottom)
    {
        var leftX = rect.Left + WidgetTheme.PadX;
        var rightX = rect.Right - WidgetTheme.PadX;
        var usedGb = totalGb - freeGb;

        using var detailFont = helper.MakeFont(WidgetTheme.DetailFontSize);
        using var subPaint = DrawHelper.Fill(WidgetTheme.TextSub);
        using var valPaint = DrawHelper.Fill(WidgetTheme.TextPrimary);

        var textY = top + 14;
        canvas.DrawText(mount, leftX, textY, detailFont, subPaint);

        var gbText = $"{usedGb:0.0} / {totalGb:0.0} GB";
        canvas.DrawText(gbText, rect.MidX - (detailFont.MeasureText(gbText) / 2f), textY, detailFont, subPaint);

        using var pctFont = helper.MakeFont(WidgetTheme.DetailFontSize, true);
        var pctText = $"{usagePct:0}%";
        canvas.DrawText(pctText, rightX - pctFont.MeasureText(pctText), textY, pctFont, valPaint);

        var barTop = textY + 4;
        var barBottom = Math.Min(barTop + WidgetTheme.BarHeight, bottom - 2);
        var barRect = new SKRect(leftX, barTop, rightX, barBottom);

        using var trackPaint = DrawHelper.Fill(WidgetTheme.TrackColor);
        canvas.DrawRoundRect(barRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, trackPaint);

        var fillWidth = barRect.Width * (float)Math.Clamp(usagePct, 0, 100) / 100f;
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        using var fillPaint = DrawHelper.Fill(WidgetTheme.FileSystemAccent);
        canvas.DrawRoundRect(fillRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, fillPaint);
    }
}

/// <summary>Text widget for disk I/O speeds (read/write) with dynamic multi-disk support for vertical layout.</summary>
internal sealed class DiskIoWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "DISK", "I/O");

        var rightX = rect.Right - WidgetTheme.PadX;
        var leftX = rect.Left + WidgetTheme.PadX;
        var entries = monitor.DiskIoSnapshots;

        if (entries.Count == 0)
        {
            // Aggregate: R and W values bottom-right
            var rText = $"R {DrawHelper.FormatSpeed(monitor.DiskReadBytesPerSec)}";
            var wText = $"W {DrawHelper.FormatSpeed(monitor.DiskWriteBytesPerSec)}";
            var bottomY = rect.Bottom - WidgetTheme.PadY;
            helper.DrawValue(canvas, wText, rightX, bottomY, WidgetTheme.DiskIoAccent);
            helper.DrawValue(canvas, rText, rightX, bottomY - WidgetTheme.ValueLargeFontSize - 2, WidgetTheme.DiskIoAccent);
            return;
        }

        // Multi-disk: stack entries vertically with name, R, W per entry
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentBottom = rect.Bottom - WidgetTheme.PadY;
        var entryHeight = (contentBottom - contentTop) / entries.Count;

        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var eTop = contentTop + (i * entryHeight);

            using var nameFont = helper.MakeFont(WidgetTheme.SmallFontSize);
            using var namePaint = DrawHelper.Fill(WidgetTheme.TextSub);
            canvas.DrawText(e.Name, leftX, eTop + 14, nameFont, namePaint);

            using var valFont = helper.MakeFont(WidgetTheme.DetailFontSize, true);
            using var valPaint = DrawHelper.Fill(WidgetTheme.DiskIoAccent);
            var rText = $"R {DrawHelper.FormatSpeed(e.ReadBytesPerSec)}";
            var wText = $"W {DrawHelper.FormatSpeed(e.WriteBytesPerSec)}";
            canvas.DrawText(rText, rightX - valFont.MeasureText(rText), eTop + 34, valFont, valPaint);
            canvas.DrawText(wText, rightX - valFont.MeasureText(wText), eTop + 52, valFont, valPaint);
        }
    }
}
