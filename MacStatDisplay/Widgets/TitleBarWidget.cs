namespace MacStatDisplay.Widgets;

using System.Globalization;
using System.Runtime.InteropServices;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Header widget with 6-column grid: title | Process | Thread | UPTIME | TIME (right-aligned per column).
internal sealed class TitleBarWidget : IWidget
{
    private const int GridColumns = 6;

    private string machineInfo = string.Empty;

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
        machineInfo = $"{Environment.MachineName} · {RuntimeInformation.FrameworkDescription}";
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        // Panel
        using var bg = DrawHelper.MakeFillPaint(Colors.PanelBackground);
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, bg);
        using var border = DrawHelper.MakeStrokePaint(Colors.PanelBorder, 1);
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, border);

        var cy = rect.MidY;
        var colWidth = rect.Width / GridColumns;

        // Column 0-1: Machine info
        using var titleFont = DrawHelper.MakeFont(FontSize.HeaderTitle, true);
        using var titlePaint = DrawHelper.MakeFillPaint(Colors.TextPrimary);
        var titleBaseline = cy - ((titleFont.Metrics.Ascent + titleFont.Metrics.Descent) / 2f);
        canvas.DrawText(machineInfo, rect.Left + 16, titleBaseline, titleFont, titlePaint);

        using var labelFont = DrawHelper.MakeFont(FontSize.HeaderLabel);
        using var labelPaint = DrawHelper.MakeFillPaint(Colors.HeaderLabel);
        using var valFont = DrawHelper.MakeFont(FontSize.HeaderValue, true);
        using var valPaint = DrawHelper.MakeFillPaint(Colors.TextPrimary);

        const float colPad = 8f;

        // Column 2: Process (right-aligned within column)
        var col2Right = rect.Left + (3 * colWidth) - colPad;
        DrawRightAlignedLabelValue(canvas, "Process ", $"{monitor.ProcessCount}",
            col2Right, cy, labelFont, labelPaint, valFont, valPaint);

        // Column 3: Thread (right-aligned within column)
        var col3Right = rect.Left + (4 * colWidth) - colPad;
        DrawRightAlignedLabelValue(canvas, "Thread ", $"{monitor.ThreadCount}",
            col3Right, cy, labelFont, labelPaint, valFont, valPaint);

        // Column 4: UPTIME (right-aligned within column)
        var col4Right = rect.Left + (5 * colWidth) - colPad;
        var uptime = monitor.Uptime;
        DrawRightAlignedLabelValue(canvas, "UPTIME ", $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m",
            col4Right, cy, labelFont, labelPaint, valFont, valPaint);

        // Column 5: TIME (right-aligned within column)
        var col5Right = rect.Right - colPad;
        DrawRightAlignedLabelValue(canvas, "TIME ", DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture),
            col5Right, cy, labelFont, labelPaint, valFont, valPaint);
    }

    private static void DrawRightAlignedLabelValue(
        SKCanvas canvas, string label, string value,
        float rightX, float cy,
        SKFont labelFont, SKPaint labelPaint,
        SKFont valFont, SKPaint valPaint)
    {
        var valBaseline = cy - ((valFont.Metrics.Ascent + valFont.Metrics.Descent) / 2f);
        var valW = valFont.MeasureText(value);
        var x = rightX - valW;
        canvas.DrawText(value, x, valBaseline, valFont, valPaint);

        var labelBaseline = cy - ((labelFont.Metrics.Ascent + labelFont.Metrics.Descent) / 2f);
        var lblW = labelFont.MeasureText(label);
        canvas.DrawText(label, x - lblW, labelBaseline, labelFont, labelPaint);
    }
}
