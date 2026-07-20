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

        var valFont = DrawHelper.GetFont(FontSize.HeaderValue, true);
        timeValueWidth = valFont.MeasureText("00:00");
        uptimeValueWidth = valFont.MeasureText("999d 23h 59m");
        countValueWidth = valFont.MeasureText("99999");

        var labelFont = DrawHelper.GetFont(FontSize.HeaderLabel);
        timeLabelWidth = labelFont.MeasureText("Time");
        uptimeLabelWidth = labelFont.MeasureText("Uptime");
        threadLabelWidth = labelFont.MeasureText("Thread");
        processLabelWidth = labelFont.MeasureText("Process");
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        // Panel
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, DrawHelper.GetFillPaint(Colors.PanelBackground));
        canvas.DrawRoundRect(rect, Layout.HeaderRadius, Layout.HeaderRadius, DrawHelper.GetStrokePaint(Colors.PanelBorder, 1));

        var cy = rect.MidY;

        // Title
        var titleFont = DrawHelper.GetFont(FontSize.HeaderTitle, true);
        var titleBaseline = cy - ((titleFont.Metrics.Ascent + titleFont.Metrics.Descent) / 2f);
        canvas.DrawText(labelText, rect.Left + Layout.TitleBarSidePad, titleBaseline, SKTextAlign.Left, titleFont, DrawHelper.GetFillPaint(Colors.TextPrimary));

        // Values
        var labelFont = DrawHelper.GetFont(FontSize.HeaderLabel);
        var valFont = DrawHelper.GetFont(FontSize.HeaderValue, true);

        // Time
        var timeLabelLeft = DrawLabelValue(
            canvas, "Time", timeLabelWidth, DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture),
            rect.Right - Layout.TitleBarSidePad, timeValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, Colors.HeaderLabel, valFont, Colors.TextPrimary);

        // Uptime
        var uptime = monitor.Uptime;
        var uptimeLabelLeft = DrawLabelValue(
            canvas, "Uptime", uptimeLabelWidth, $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m",
            timeLabelLeft - (rect.Width * Layout.TitleBarTimeUptimeMarginRatio), uptimeValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, Colors.HeaderLabel, valFont, Colors.TextPrimary);

        // Thread
        var threadLabelLeft = DrawLabelValue(
            canvas, "Thread", threadLabelWidth, $"{monitor.ThreadCount}",
            uptimeLabelLeft - (rect.Width * Layout.TitleBarUptimeThreadMarginRatio), countValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, Colors.HeaderLabel, valFont, Colors.TextPrimary);

        // Process
        DrawLabelValue(
            canvas, "Process", processLabelWidth, $"{monitor.ProcessCount}",
            threadLabelLeft - (rect.Width * Layout.TitleBarThreadProcessMarginRatio), countValueWidth, Layout.TitleBarLabelValueGap, cy,
            labelFont, Colors.HeaderLabel, valFont, Colors.TextPrimary);
    }

    private static float DrawLabelValue(
        SKCanvas canvas, string label, float labelWidth, string value,
        float rightX, float maxValueWidth, float labelValueGap, float cy,
        SKFont labelFont, SKColor labelColor, SKFont valFont, SKColor valColor)
    {
        // Value
        var valBaseline = cy - ((valFont.Metrics.Ascent + valFont.Metrics.Descent) / 2f);
        canvas.DrawText(value, rightX - maxValueWidth, valBaseline, SKTextAlign.Left, valFont, DrawHelper.GetFillPaint(valColor));

        // Label
        var labelRightX = rightX - maxValueWidth - labelValueGap;
        var labelLeftX = labelRightX - labelWidth;
        var labelBaseline = cy - ((labelFont.Metrics.Ascent + labelFont.Metrics.Descent) / 2f);
        canvas.DrawText(label, labelLeftX, labelBaseline, SKTextAlign.Left, labelFont, DrawHelper.GetFillPaint(labelColor));

        return labelLeftX;
    }
}
