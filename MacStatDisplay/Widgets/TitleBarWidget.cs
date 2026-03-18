namespace MacStatDisplay.Widgets;

using System.Globalization;

using MacStatDisplay.Monitor;

using SkiaSharp;

/// <summary>Header widget that displays title, uptime, clock, and process/thread counts horizontally.</summary>
internal sealed class TitleBarWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        // Panel
        using var bg = DrawHelper.Fill(WidgetTheme.PanelBg);
        canvas.DrawRoundRect(rect, WidgetTheme.HeaderRadius, WidgetTheme.HeaderRadius, bg);
        using var border = DrawHelper.Stroke(WidgetTheme.PanelBorder, 1);
        canvas.DrawRoundRect(rect, WidgetTheme.HeaderRadius, WidgetTheme.HeaderRadius, border);

        var cy = rect.MidY;

        // Title
        using var titleFont = helper.MakeFont(WidgetTheme.HeaderTitleFontSize, true);
        using var titlePaint = DrawHelper.Fill(WidgetTheme.TextPrimary);
        canvas.DrawText("SYSTEM MONITOR", rect.Left + 16, cy + (titleFont.Size * 0.35f), titleFont, titlePaint);

        // Horizontal metrics laid out from right: PROC/THR, TIME, UPTIME
        using var labelFont = helper.MakeFont(WidgetTheme.HeaderLabelFontSize);
        using var labelPaint = DrawHelper.Fill(WidgetTheme.AccentCyan);
        using var valFont = helper.MakeFont(WidgetTheme.HeaderValueFontSize, true);
        using var valPaint = DrawHelper.Fill(WidgetTheme.TextPrimary);

        var uptime = monitor.Uptime;
        var uptimeText = $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m";
        var clockText = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var procText = $"{monitor.ProcessCount} / THR {monitor.ThreadCount}";

        var baseline = cy + (valFont.Size * 0.35f);
        var x = rect.Right - 16f;

        // PROC / THR (right-most)
        DrawLabelValue(canvas, "PROC ", procText, ref x, baseline, labelFont, labelPaint, valFont, valPaint);
        x -= 20f;

        // TIME
        DrawLabelValue(canvas, "TIME ", clockText, ref x, baseline, labelFont, labelPaint, valFont, valPaint);
        x -= 20f;

        // UPTIME
        DrawLabelValue(canvas, "UPTIME ", uptimeText, ref x, baseline, labelFont, labelPaint, valFont, valPaint);
    }

    private static void DrawLabelValue(
        SKCanvas canvas, string label, string value,
        ref float x, float y,
        SKFont labelFont, SKPaint labelPaint,
        SKFont valFont, SKPaint valPaint)
    {
        var valW = valFont.MeasureText(value);
        x -= valW;
        canvas.DrawText(value, x, y, valFont, valPaint);

        var lblW = labelFont.MeasureText(label);
        x -= lblW;
        canvas.DrawText(label, x, y, labelFont, labelPaint);
    }
}
