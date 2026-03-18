namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Ring gauge widget for CPU usage with E/P-core and System/User/Idle breakdown, plus CPU temperature.</summary>
internal sealed class CpuUsageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "CPU", "Usage");

        var usage = (float)Math.Clamp(monitor.CpuUsageTotal, 0, 100);

        // Larger ring – push center down so the 270° arc bottom aligns with the widget bottom
        var contentH = rect.Height - WidgetTheme.TitleOffsetY - WidgetTheme.PadY;
        var maxRadiusH = contentH / 1.71f;
        var maxRadiusW = (rect.Width - 160f) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = rect.Bottom - WidgetTheme.PadY - (radius * 0.71f);

        helper.DrawRingGauge(canvas, cx, cy, radius, usage, WidgetTheme.CpuUsageAccent);
        helper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (WidgetTheme.CenterValueFontSize * 0.35f), WidgetTheme.CpuUsageAccent);

        // CPU temperature below center value inside the ring
        var cpuTemp = monitor.CpuTemperature;
        if (cpuTemp.HasValue)
        {
            using var tempFont = helper.MakeFont(WidgetTheme.DetailFontSize);
            using var tempPaint = DrawHelper.Fill(WidgetTheme.TemperatureAccent);
            var tempText = $"{cpuTemp.Value:0}\u00b0C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (WidgetTheme.CenterValueFontSize * 0.35f) + 18, tempFont, tempPaint);
        }

        // Left: E-Core / P-Core usage (stacked label + value)
        var leftX = rect.Left + WidgetTheme.PadX;
        var sideTop = cy - radius + 8;
        helper.DrawStackedLabelValue(canvas, "E-Core", $"{monitor.CpuUsageEfficiency:0}%", leftX, sideTop, WidgetTheme.CpuUsageAccent);
        helper.DrawStackedLabelValue(canvas, "P-Core", $"{monitor.CpuUsagePerformance:0}%", leftX, sideTop + 40, WidgetTheme.CpuUsageAccent);

        // Right: System / User / Idle (stacked label + value)
        var rightX = rect.Right - WidgetTheme.PadX;
        helper.DrawStackedLabelValueRight(canvas, "System", $"{monitor.CpuSystemPercent:0.0}%", rightX, sideTop, WidgetTheme.CpuUsageAccent);
        helper.DrawStackedLabelValueRight(canvas, "User", $"{monitor.CpuUserPercent:0.0}%", rightX, sideTop + 36, WidgetTheme.CpuUsageAccent);
        helper.DrawStackedLabelValueRight(canvas, "Idle", $"{100 - monitor.CpuUsageTotal:0.0}%", rightX, sideTop + 72, WidgetTheme.CpuUsageAccent);
    }
}

/// <summary>Text widget for CPU clock frequency with E/P-core breakdown.</summary>
internal sealed class CpuClockWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "CPU", "Clock");

        var mhz = monitor.CpuFrequencyAllHz / 1_000_000.0;
        var pMhz = monitor.CpuFrequencyPerformanceHz / 1_000_000.0;
        var eMhz = monitor.CpuFrequencyEfficiencyHz / 1_000_000.0;

        // Main value bottom-right
        helper.DrawValue(canvas, $"{mhz:0} MHz", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.CpuClockAccent);

        // E/P core clocks on the left
        var leftX = rect.Left + WidgetTheme.PadX;
        var topY = rect.Top + WidgetTheme.TitleOffsetY + 10;
        helper.DrawStackedLabelValue(canvas, "P-Core", $"{pMhz:0} MHz", leftX, topY, WidgetTheme.CpuClockAccent);
        helper.DrawStackedLabelValue(canvas, "E-Core", $"{eMhz:0} MHz", leftX, topY + 28, WidgetTheme.CpuClockAccent);
    }
}

/// <summary>Text widget for load average (1m main, 5m/15m secondary).</summary>
internal sealed class LoadAverageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "CPU", "Load");

        // Main value (1m) bottom-right
        helper.DrawValue(canvas, $"{monitor.LoadAverage1:0.00}", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.CpuLoadAccent);

        // 5m / 15m on left
        var leftX = rect.Left + WidgetTheme.PadX;
        var topY = rect.Top + WidgetTheme.TitleOffsetY + 10;
        helper.DrawStackedLabelValue(canvas, "5m", $"{monitor.LoadAverage5:0.00}", leftX, topY, WidgetTheme.CpuLoadAccent);
        helper.DrawStackedLabelValue(canvas, "15m", $"{monitor.LoadAverage15:0.00}", leftX, topY + 28, WidgetTheme.CpuLoadAccent);
    }
}
