namespace MacStatDisplay.Widgets;

using System.Globalization;

using MacStatDisplay.Monitor;

using SkiaSharp;

/// <summary>Header widget with fixed-position grid: title, PROC/THR, UPTIME, TIME.</summary>
internal sealed class TitleBarWidget : IWidget
{
    private const int GridColumns = 5;

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        // Panel
        using var bg = DrawHelper.Fill(WidgetTheme.PanelBg);
        canvas.DrawRoundRect(rect, WidgetTheme.HeaderRadius, WidgetTheme.HeaderRadius, bg);
        using var border = DrawHelper.Stroke(WidgetTheme.PanelBorder, 1);
        canvas.DrawRoundRect(rect, WidgetTheme.HeaderRadius, WidgetTheme.HeaderRadius, border);

        var cy = rect.MidY;
        var colWidth = rect.Width / GridColumns;

        // Column 0-1: Title
        using var titleFont = DrawHelper.MakeFont(WidgetTheme.HeaderTitleFontSize, true);
        using var titlePaint = DrawHelper.Fill(WidgetTheme.TextPrimary);
        canvas.DrawText("SYSTEM MONITOR", rect.Left + 16, cy + (titleFont.Size * 0.35f), titleFont, titlePaint);

        using var labelFont = DrawHelper.MakeFont(WidgetTheme.HeaderLabelFontSize);
        using var labelPaint = DrawHelper.Fill(WidgetTheme.AccentCyan);
        using var valFont = DrawHelper.MakeFont(WidgetTheme.HeaderValueFontSize, true);
        using var valPaint = DrawHelper.Fill(WidgetTheme.TextPrimary);

        var baseline = cy + (valFont.Size * 0.35f);
        var labelOffsetX = 60f;

        // Column 2: PROC / THR
        var col2X = rect.Left + (2 * colWidth);
        canvas.DrawText("PROC", col2X, baseline, labelFont, labelPaint);
        canvas.DrawText($"{monitor.ProcessCount} / THR {monitor.ThreadCount}", col2X + labelOffsetX, baseline, valFont, valPaint);

        // Column 3: UPTIME
        var col3X = rect.Left + (3 * colWidth);
        var uptime = monitor.Uptime;
        canvas.DrawText("UPTIME", col3X, baseline, labelFont, labelPaint);
        canvas.DrawText($"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m", col3X + labelOffsetX, baseline, valFont, valPaint);

        // Column 4: TIME
        var col4X = rect.Left + (4 * colWidth);
        canvas.DrawText("TIME", col4X, baseline, labelFont, labelPaint);
        canvas.DrawText(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture), col4X + labelOffsetX, baseline, valFont, valPaint);
    }
}
