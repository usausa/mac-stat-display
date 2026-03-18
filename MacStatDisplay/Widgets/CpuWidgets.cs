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
        var sideMargin = 80f;
        var maxRadiusH = contentH / 1.707f;
        var maxRadiusW = (rect.Width - (2 * sideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        // Visually center the 270° arc (accounts for the bottom gap)
        var cy = contentTop + (contentH / 2f) + (radius * 0.147f);

        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (WidgetTheme.CenterValueFontSize * 0.35f), WidgetTheme.CpuUsageAccent);

        // CPU temperature below center value inside the ring
        var cpuTemp = monitor.CpuTemperature;
        if (cpuTemp.HasValue)
        {
            using var tempFont = DrawHelper.MakeFont(WidgetTheme.DetailFontSize);
            using var tempPaint = DrawHelper.Fill(WidgetTheme.TemperatureAccent);
            var tempText = $"{cpuTemp.Value:0}\u00b0C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (WidgetTheme.CenterValueFontSize * 0.35f) + 18, tempFont, tempPaint);
        }

        // Left: E-Core / P-Core usage (stacked label + value)
        var leftX = rect.Left + WidgetTheme.PadX;
        var sideTop = cy - radius + 8;
        DrawHelper.DrawStackedLabelValue(canvas, "E-Core", $"{monitor.CpuUsageEfficiency:0}%", leftX, sideTop, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "P-Core", $"{monitor.CpuUsagePerformance:0}%", leftX, sideTop + 40, WidgetTheme.CpuUsageAccent);

        // Right: System / User / Idle (stacked label + value)
        var rightX = rect.Right - WidgetTheme.PadX;
        DrawHelper.DrawStackedLabelValueRight(canvas, "System", $"{monitor.CpuSystemPercent:0.0}%", rightX, sideTop, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "User", $"{monitor.CpuUserPercent:0.0}%", rightX, sideTop + 36, WidgetTheme.CpuUsageAccent);
        DrawHelper.DrawStackedLabelValueRight(canvas, "Idle", $"{100 - monitor.CpuUsageTotal:0.0}%", rightX, sideTop + 72, WidgetTheme.CpuUsageAccent);
    }
}

/// <summary>Text widget for CPU clock frequency. P-Core/E-Core at bottom with even spacing.</summary>
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

        // P-Core / E-Core at bottom, evenly spaced from left
        var leftX = rect.Left + WidgetTheme.PadX;
        var bottomY = rect.Bottom - WidgetTheme.PadY - WidgetTheme.ValueLargeFontSize - 6;
        DrawHelper.DrawStackedLabelValue(canvas, "P-Core", $"{pMhz:0} MHz", leftX, bottomY, WidgetTheme.CpuClockAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "E-Core", $"{eMhz:0} MHz", leftX + WidgetTheme.SubItemSpacing, bottomY, WidgetTheme.CpuClockAccent);
    }
}

/// <summary>Text widget for load average. 5m/15m at bottom with even spacing.</summary>
internal sealed class LoadAverageWidget : IWidget
{
    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "CPU", "Load");

        // Main value (1m) bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.LoadAverage1:0.00}", rect.Right - WidgetTheme.PadX, rect.Bottom - WidgetTheme.PadY, WidgetTheme.CpuLoadAccent);

        // 5m / 15m at bottom, evenly spaced from left
        var leftX = rect.Left + WidgetTheme.PadX;
        var bottomY = rect.Bottom - WidgetTheme.PadY - WidgetTheme.ValueLargeFontSize - 6;
        DrawHelper.DrawStackedLabelValue(canvas, "5m", $"{monitor.LoadAverage5:0.00}", leftX, bottomY, WidgetTheme.CpuLoadAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "15m", $"{monitor.LoadAverage15:0.00}", leftX + WidgetTheme.SubItemSpacing, bottomY, WidgetTheme.CpuLoadAccent);
    }
}
