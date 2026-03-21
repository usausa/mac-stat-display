namespace MacStatDisplay.Widgets;

using System.Globalization;
using System.Runtime.InteropServices;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Header widget: title on the left, then Process / Thread / UPTIME / TIME placed right-to-left.
// Each section uses a fixed max-value width so the label anchor never shifts with changing values.
// Section-to-section gaps are expressed as a ratio of the total title bar width.
internal sealed class TitleBarWidget : IWidget
{
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

        // Machine info (left side)
        using var titleFont = DrawHelper.MakeFont(FontSize.HeaderTitle, true);
        using var titlePaint = DrawHelper.MakeFillPaint(Colors.TextPrimary);
        var titleBaseline = cy - ((titleFont.Metrics.Ascent + titleFont.Metrics.Descent) / 2f);
        canvas.DrawText(machineInfo, rect.Left + 16, titleBaseline, titleFont, titlePaint);

        using var labelFont = DrawHelper.MakeFont(FontSize.HeaderLabel);
        using var labelPaint = DrawHelper.MakeFillPaint(Colors.HeaderLabel);
        using var valFont = DrawHelper.MakeFont(FontSize.HeaderValue, true);
        using var valPaint = DrawHelper.MakeFillPaint(Colors.TextPrimary);

        var gap = Layout.TitleBarLabelValueGap;

        // TIME – max value width is fixed to "00:00" (5 chars, always same length)
        var timeLabelRight = DrawLabelValue(
            canvas, "TIME", DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture),
            rightX: rect.Right - Layout.TitleBarRightPad,
            maxValueWidth: valFont.MeasureText("00:00"),
            labelValueGap: gap, cy, labelFont, labelPaint, valFont, valPaint);

        // UPTIME – max value width covers 3-digit days: "999d 23h 59m"
        var uptime = monitor.Uptime;
        var uptimeLabelRight = DrawLabelValue(
            canvas, "UPTIME", $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m",
            rightX: timeLabelRight - rect.Width * Layout.TitleBarTimeUptimeMarginRatio,
            maxValueWidth: valFont.MeasureText("999d 23h 59m"),
            labelValueGap: gap, cy, labelFont, labelPaint, valFont, valPaint);

        // Thread – max value width covers 5 digits: "99999"
        var threadLabelRight = DrawLabelValue(
            canvas, "Thread", $"{monitor.ThreadCount}",
            rightX: uptimeLabelRight - rect.Width * Layout.TitleBarUptimeThreadMarginRatio,
            maxValueWidth: valFont.MeasureText("99999"),
            labelValueGap: gap, cy, labelFont, labelPaint, valFont, valPaint);

        // Process – max value width covers 5 digits: "99999"
        DrawLabelValue(
            canvas, "Process", $"{monitor.ProcessCount}",
            rightX: threadLabelRight - rect.Width * Layout.TitleBarThreadProcessMarginRatio,
            maxValueWidth: valFont.MeasureText("99999"),
            labelValueGap: gap, cy, labelFont, labelPaint, valFont, valPaint);
    }

    /// <summary>
    /// Draws a label+value pair anchored at <paramref name="rightX"/>.
    /// The value is right-aligned to <paramref name="rightX"/>; the label is right-aligned to the
    /// fixed slot at <paramref name="rightX"/> - <paramref name="maxValueWidth"/> - <paramref name="labelValueGap"/>.
    /// </summary>
    /// <returns>The right edge of the label slot, which serves as the margin anchor for the next section.</returns>
    private static float DrawLabelValue(
        SKCanvas canvas, string label, string value,
        float rightX, float maxValueWidth, float labelValueGap, float cy,
        SKFont labelFont, SKPaint labelPaint,
        SKFont valFont, SKPaint valPaint)
    {
        var valBaseline = cy - ((valFont.Metrics.Ascent + valFont.Metrics.Descent) / 2f);
        canvas.DrawText(value, rightX - valFont.MeasureText(value), valBaseline, valFont, valPaint);

        var labelRightX = rightX - maxValueWidth - labelValueGap;
        var labelBaseline = cy - ((labelFont.Metrics.Ascent + labelFont.Metrics.Descent) / 2f);
        canvas.DrawText(label, labelRightX - labelFont.MeasureText(label), labelBaseline, labelFont, labelPaint);

        return labelRightX;
    }
}
