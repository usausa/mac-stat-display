namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

// Ring gauge widget for CPU usage with E/P-core and System/User/Idle breakdown, plus CPU temperature.
internal sealed class CpuUsageWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU", "Usage");

        var usage = (float)Math.Clamp(monitor.CpuUsageTotal, 0, 100);

        // Content area below title
        var contentTop = rect.Top + Layout.TitleOffsetY + 4;
        var contentH = rect.Bottom - Layout.PaddingY - contentTop;
        var sideMargin = 70f;
        var maxRadiusH = contentH / 1.707f;
        var maxRadiusW = (rect.Width - (2 * sideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = contentTop + (contentH / 2f) + (radius * 0.147f);

        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, Colors.CpuUsageAccent);
        DrawHelper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (FontSize.GaugeValue * 0.35f), Colors.CpuUsageAccent);

        // CPU temperature below center value inside the ring
        var cpuTemp = monitor.CpuTemperature;
        if (cpuTemp.HasValue)
        {
            using var tempFont = DrawHelper.MakeFont(FontSize.Temperature);
            using var tempPaint = DrawHelper.Fill(Colors.TemperatureAccent);
            var tempText = $"{cpuTemp.Value:0} C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (FontSize.GaugeValue * 0.35f) + 22, tempFont, tempPaint);
        }

        // Left: E-Core / P-Core usage
        var leftX = rect.Left + Layout.PaddingX;
        var sideTop = cy - radius + 8;
        DrawHelper.DrawStackedLabelValue(canvas, "E-Core", $"{monitor.CpuUsageEfficiency:0}%", leftX, sideTop, Colors.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "P-Core", $"{monitor.CpuUsagePerformance:0}%", leftX, sideTop + 44, Colors.CpuUsageAccent);

        // Right: System / User / Idle
        var rightX = rect.Right - Layout.PaddingX;
        DrawHelper.DrawStackedLabelValueRight(canvas, "System", $"{monitor.CpuSystemPercent:0.0}%", rightX, sideTop, Colors.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "User", $"{monitor.CpuUserPercent:0.0}%", rightX, sideTop + 40, Colors.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "Idle", $"{100 - monitor.CpuUsageTotal:0.0}%", rightX, sideTop + 80, Colors.CpuUsageAccent);
    }
}
