namespace MacStatDisplay.Widgets;

using System.Globalization;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Header widget with 6-column grid: title | Process | Thread | UPTIME | TIME (right-aligned per column).
internal sealed class TitleBarWidget : IWidget
{
    private const int GridColumns = 6;

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        // Panel
        using var bg = DrawHelper.Fill(Colors.PanelBackground);
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, bg);
        using var border = DrawHelper.Stroke(Colors.PanelBorder, 1);
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, border);

        var cy = rect.MidY;
        var colWidth = rect.Width / GridColumns;

        // Column 0-1: Title
        using var titleFont = DrawHelper.MakeFont(FontSize.HeaderTitle, true);
        using var titlePaint = DrawHelper.Fill(Colors.TextPrimary);
        canvas.DrawText("SYSTEM MONITOR", rect.Left + 16, cy + (titleFont.Size * 0.35f), titleFont, titlePaint);

        using var labelFont = DrawHelper.MakeFont(FontSize.HeaderLabel);
        using var labelPaint = DrawHelper.Fill(Colors.HeaderLabel);
        using var valFont = DrawHelper.MakeFont(FontSize.HeaderValue, true);
        using var valPaint = DrawHelper.Fill(Colors.TextPrimary);

        var baseline = cy + (valFont.Size * 0.35f);
        const float colPad = 8f;

        // Column 2: Process (right-aligned within column)
        var col2Right = rect.Left + (3 * colWidth) - colPad;
        DrawRightAlignedLabelValue(canvas, "Process ", $"{monitor.ProcessCount}",
            col2Right, baseline, labelFont, labelPaint, valFont, valPaint);

        // Column 3: Thread (right-aligned within column)
        var col3Right = rect.Left + (4 * colWidth) - colPad;
        DrawRightAlignedLabelValue(canvas, "Thread ", $"{monitor.ThreadCount}",
            col3Right, baseline, labelFont, labelPaint, valFont, valPaint);

        // Column 4: UPTIME (right-aligned within column)
        var col4Right = rect.Left + (5 * colWidth) - colPad;
        var uptime = monitor.Uptime;
        DrawRightAlignedLabelValue(canvas, "UPTIME ", $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m",
            col4Right, baseline, labelFont, labelPaint, valFont, valPaint);

        // Column 5: TIME (right-aligned within column)
        var col5Right = rect.Right - colPad;
        DrawRightAlignedLabelValue(canvas, "TIME ", DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture),
            col5Right, baseline, labelFont, labelPaint, valFont, valPaint);
    }

    private static void DrawRightAlignedLabelValue(
        SKCanvas canvas, string label, string value,
        float rightX, float y,
        SKFont labelFont, SKPaint labelPaint,
        SKFont valFont, SKPaint valPaint)
    {
        var valW = valFont.MeasureText(value);
        var x = rightX - valW;
        canvas.DrawText(value, x, y, valFont, valPaint);

        var lblW = labelFont.MeasureText(label);
        canvas.DrawText(label, x - lblW, y, labelFont, labelPaint);
    }
}
