namespace MacStatDisplay.Widgets;

using System.Globalization;
using System.Runtime.InteropServices;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class TitleBarWidget : IWidget
{
    private string labelText = default!;

    private float timeValueWidth;
    private float uptimeValueWidth;
    private float countValueWidth;

    private float timeLabelWidth;
    private float uptimeLabelWidth;
    private float threadLabelWidth;
    private float processLabelWidth;

    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
        labelText = $"System Monitor  {Environment.MachineName} - {RuntimeInformation.OSDescription}";

        using var valFont = DrawHelper.MakeFont(FontSize.HeaderValue, true);
        timeValueWidth = valFont.MeasureText("00:00");
        uptimeValueWidth = valFont.MeasureText("999d 23h 59m");
        countValueWidth = valFont.MeasureText("99999");

        using var labelFont = DrawHelper.MakeFont(FontSize.HeaderLabel);
        timeLabelWidth = labelFont.MeasureText("Time");
        uptimeLabelWidth = labelFont.MeasureText("Uptime");
        threadLabelWidth = labelFont.MeasureText("Thread");
        processLabelWidth = labelFont.MeasureText("Process");
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        // Panel
        using var bg = DrawHelper.MakeFillPaint(Colors.PanelBackground);
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, bg);
        using var border = DrawHelper.MakeStrokePaint(Colors.PanelBorder, 1);
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, border);

        var cy = rect.MidY;

        // Title
        using var titleFont = DrawHelper.MakeFont(FontSize.HeaderTitle, true);
        using var titlePaint = DrawHelper.MakeFillPaint(Colors.TextPrimary);
        var titleBaseline = cy - ((titleFont.Metrics.Ascent + titleFont.Metrics.Descent) / 2f);
        canvas.DrawText(labelText, rect.Left + Layout.TitleBarSidePad, titleBaseline, titleFont, titlePaint);

        // Values
        using var labelFont = DrawHelper.MakeFont(FontSize.HeaderLabel);
        using var labelPaint = DrawHelper.MakeFillPaint(Colors.HeaderLabel);
        using var valFont = DrawHelper.MakeFont(FontSize.HeaderValue, true);
        using var valPaint = DrawHelper.MakeFillPaint(Colors.TextPrimary);

        // Time
        var timeLabelLeft = DrawLabelValue(
            canvas, "Time", timeLabelWidth, DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture),
            rect.Right - Layout.TitleBarSidePad, timeValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, labelPaint, valFont, valPaint);

        // Uptime
        var uptime = monitor.Uptime;
        var uptimeLabelLeft = DrawLabelValue(
            canvas, "Uptime", uptimeLabelWidth, $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m",
            timeLabelLeft - (rect.Width * Layout.TitleBarTimeUptimeMarginRatio), uptimeValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, labelPaint, valFont, valPaint);

        // Thread
        var threadLabelLeft = DrawLabelValue(
            canvas, "Thread", threadLabelWidth, $"{monitor.ThreadCount}",
            uptimeLabelLeft - (rect.Width * Layout.TitleBarUptimeThreadMarginRatio), countValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, labelPaint, valFont, valPaint);

        // Process
        DrawLabelValue(
            canvas, "Process", processLabelWidth, $"{monitor.ProcessCount}",
            threadLabelLeft - (rect.Width * Layout.TitleBarThreadProcessMarginRatio), countValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, labelPaint, valFont, valPaint);
    }

    private static float DrawLabelValue(
        SKCanvas canvas, string label, float labelWidth, string value,
        float rightX, float maxValueWidth, float labelValueGap, float cy,
        SKFont labelFont, SKPaint labelPaint, SKFont valFont, SKPaint valPaint)
    {
        // Value
        var valBaseline = cy - ((valFont.Metrics.Ascent + valFont.Metrics.Descent) / 2f);
        canvas.DrawText(value, rightX - maxValueWidth, valBaseline, valFont, valPaint);

        // Label
        var labelRightX = rightX - maxValueWidth - labelValueGap;
        var labelLeftX = labelRightX - labelWidth;
        var labelBaseline = cy - ((labelFont.Metrics.Ascent + labelFont.Metrics.Descent) / 2f);
        canvas.DrawText(label, labelLeftX, labelBaseline, labelFont, labelPaint);

        return labelLeftX;
    }
}
