namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class FileSystemWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "Disk Usage");

        var entries = monitor.FileSystems;
        if (entries.Count == 0)
        {
            return;
        }

        // Calculate
        var contentTop = rect.Top + Layout.TitleOffsetY + Layout.ContentTopGap;
        var contentBottom = rect.Bottom - Layout.PaddingY;
        var contentH = contentBottom - contentTop;

        var entryH = contentH / Math.Max(entries.Count, 1);
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var totalGb = e.TotalSize / (1024.0 * 1024.0 * 1024.0);
            var freeGb = e.FreeSize / (1024.0 * 1024.0 * 1024.0);
            var usage = e.TotalSize > 0 ? (double)(e.TotalSize - e.FreeSize) / e.TotalSize * 100.0 : 0;
            DrawEntry(canvas, rect, e.MountPoint, totalGb, freeGb, usage, contentTop + (i * entryH), entryH);
        }
    }

    private static void DrawEntry(
        SKCanvas canvas, SKRect rect,
        string mount, double totalGb, double freeGb, double usage,
        float entryTop, float entryH)
    {
        var leftX = rect.Left + Layout.PaddingX;
        var rightX = rect.Right - Layout.PaddingX;
        var usedGb = totalGb - freeGb;
        var centerY = entryTop + (entryH / 2f);

        // Bar
        var barTop = centerY + Layout.BarGaugeMargin;
        var barBottom = Math.Min(barTop + Layout.BarHeight, entryTop + entryH - Layout.BarGaugeMargin);
        var barRect = new SKRect(leftX, barTop, rightX, barBottom);

        using var trackPaint = DrawHelper.Fill(Colors.TrackColor);
        canvas.DrawRoundRect(barRect, Layout.BarRadius, Layout.BarRadius, trackPaint);

        var fillWidth = barRect.Width * (float)Math.Clamp(usage, 0, 100) / 100f;
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        using var fillPaint = DrawHelper.Fill(Colors.FileSystemAccent);
        canvas.DrawRoundRect(fillRect, Layout.BarRadius, Layout.BarRadius, fillPaint);

        // Left
        using var mountFont = DrawHelper.MakeFont(FontSize.SubLabel);
        using var subPaint = DrawHelper.Fill(Colors.TextSecondary);
        using var accentPaint = DrawHelper.Fill(Colors.FileSystemAccent);
        canvas.DrawText(mount, leftX, centerY + mountFont.Metrics.Ascent - mountFont.Metrics.Descent, mountFont, subPaint);

        var gbText = $"{usedGb:0.0} / {totalGb:0.0} GB";
        canvas.DrawText(gbText, leftX, centerY, mountFont, accentPaint);

        // Right
        using var pctFont = DrawHelper.MakeFont(FontSize.PrimaryValue, true);
        var pctText = $"{usage:0}%";
        canvas.DrawText(pctText, rightX - pctFont.MeasureText(pctText), centerY, pctFont, accentPaint);
    }
}
