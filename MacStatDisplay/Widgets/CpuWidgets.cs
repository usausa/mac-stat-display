namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Ring gauge widget for CPU usage with E/P-core and System/User/Idle breakdown, plus CPU temperature.</summary>
internal sealed class CpuUsageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU", "Usage");

        var usage = (float)Math.Clamp(monitor.CpuUsageTotal, 0, 100);

        // Content area below title
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentH = rect.Bottom - WidgetTheme.PadY - contentTop;
        var sideMargin = 70f;
        var maxRadiusH = contentH / 1.707f;
        var maxRadiusW = (rect.Width - (2 * sideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = contentTop + (contentH / 2f) + (radius * 0.147f);

        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (WidgetTheme.GaugeValueFontSize * 0.35f), WidgetTheme.CpuUsageAccent);

        // CPU temperature below center value inside the ring
        var cpuTemp = monitor.CpuTemperature;
        if (cpuTemp.HasValue)
        {
            using var tempFont = DrawHelper.MakeFont(WidgetTheme.TemperatureFontSize);
            using var tempPaint = DrawHelper.Fill(WidgetTheme.TemperatureAccent);
            var tempText = $"{cpuTemp.Value:0}\u00b0C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (WidgetTheme.GaugeValueFontSize * 0.35f) + 22, tempFont, tempPaint);
        }

        // Left: E-Core / P-Core usage
        var leftX = rect.Left + WidgetTheme.PadX;
        var sideTop = cy - radius + 8;
        DrawHelper.DrawStackedLabelValue(canvas, "E-Core", $"{monitor.CpuUsageEfficiency:0}%", leftX, sideTop, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "P-Core", $"{monitor.CpuUsagePerformance:0}%", leftX, sideTop + 44, WidgetTheme.CpuUsageAccent);

        // Right: System / User / Idle
        var rightX = rect.Right - WidgetTheme.PadX;
        DrawHelper.DrawStackedLabelValueRight(canvas, "System", $"{monitor.CpuSystemPercent:0.0}%", rightX, sideTop, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "User", $"{monitor.CpuUserPercent:0.0}%", rightX, sideTop + 40, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "Idle", $"{100 - monitor.CpuUsageTotal:0.0}%", rightX, sideTop + 80, WidgetTheme.CpuUsageAccent);
    }
}

/// <summary>Text widget for CPU clock frequency. P-Core/E-Core stacked vertically, bottom-aligned with main value.</summary>
internal sealed class CpuClockWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU", "Clock");

        var mhz = monitor.CpuFrequencyAllHz / 1_000_000.0;
        var pMhz = monitor.CpuFrequencyPerformanceHz / 1_000_000.0;
        var eMhz = monitor.CpuFrequencyEfficiencyHz / 1_000_000.0;

        // Main value bottom-right
        DrawHelper.DrawValue(canvas, $"{mhz:0} MHz", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.CpuClockAccent);

        // P-Core / E-Core stacked vertically, bottom-aligned with main value
        var leftX = rect.Left + WidgetTheme.PadX;
        var mainBottom = rect.Bottom - WidgetTheme.PadY;
        var y2 = mainBottom - 18;
        var y1 = y2 - 36;
        DrawHelper.DrawStackedLabelValue(canvas, "P-Core", $"{pMhz:0}", leftX, y1, WidgetTheme.CpuClockAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "E-Core", $"{eMhz:0}", leftX, y2, WidgetTheme.CpuClockAccent);
    }
}

/// <summary>Text widget for load average. 5m/15m stacked vertically, bottom-aligned with main value.</summary>
internal sealed class LoadAverageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU", "Load");

        // Main value (1m) bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.LoadAverage1:0.00}", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.CpuLoadAccent);

        // 5m / 15m stacked vertically, bottom-aligned with main value
        var leftX = rect.Left + WidgetTheme.PadX;
        var mainBottom = rect.Bottom - WidgetTheme.PadY;
        var y2 = mainBottom - 18;
        var y1 = y2 - 36;
        DrawHelper.DrawStackedLabelValue(canvas, "5m", $"{monitor.LoadAverage5:0.00}", leftX, y1, WidgetTheme.CpuLoadAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "15m", $"{monitor.LoadAverage15:0.00}", leftX, y2, WidgetTheme.CpuLoadAccent);
    }
}
