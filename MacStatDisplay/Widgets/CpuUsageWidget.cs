namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class CpuUsageWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitle(canvas, rect, "CPU Usage");

        var usage = (float)Math.Clamp(monitor.CpuUsageTotal, 0, 100);

        // Calculate
        var contentTop = rect.Top + Layout.TitleOffsetY + Layout.ContentTopGap;
        var contentH = rect.Bottom - Layout.PaddingY - contentTop;
        var maxRadiusH = contentH / Layout.RingHeightRatio;
        var maxRadiusW = (rect.Width - (2 * Layout.RingSideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = contentTop + (contentH / 2f) + (radius * Layout.RingCenterOffsetRatio);

        // Gauge
        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, Colors.CpuUsageAccent);
        DrawHelper.DrawCenterValue(canvas, $"{usage:0}%", cx, cy + (FontSize.GaugeValue * Layout.BaselineRatio), Colors.CpuUsageAccent);

        // Temperature
        var cpuTemp = monitor.CpuTemperature;
        if (cpuTemp.HasValue)
        {
            using var tempFont = DrawHelper.MakeFont(FontSize.Temperature);
            using var tempPaint = DrawHelper.MakeFillPaint(Colors.TemperatureAccent);
            var tempText = $"{cpuTemp.Value:0}\u00b0C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (FontSize.GaugeValue * Layout.BaselineRatio) + (radius * Layout.TemperatureOffsetRatio), tempFont, tempPaint);
        }

        // Left
        var leftX = rect.Left + Layout.PaddingX;
        var sideStartY = cy - radius + (radius * Layout.RingSideStartRatio);
        var itemSpacing = radius * Layout.RingSideItemSpacingRatio;
        DrawHelper.DrawStackedValue(canvas, "E-Core", $"{monitor.CpuUsageEfficiency:0}%", leftX, sideStartY, Colors.CpuUsageAccent);
        DrawHelper.DrawStackedValue(canvas, "P-Core", $"{monitor.CpuUsagePerformance:0}%", leftX, sideStartY + itemSpacing, Colors.CpuUsageAccent);

        // Right
        var rightX = rect.Right - Layout.PaddingX;
        DrawHelper.DrawStackedValueRight(canvas, "System", $"{monitor.CpuSystemPercent:0.0}%", rightX, sideStartY, Colors.CpuUsageAccent);
        DrawHelper.DrawStackedValueRight(canvas, "User", $"{monitor.CpuUserPercent:0.0}%", rightX, sideStartY + itemSpacing, Colors.CpuUsageAccent);
        DrawHelper.DrawStackedValueRight(canvas, "Idle", $"{100 - monitor.CpuUsageTotal:0.0}%", rightX, sideStartY + (itemSpacing * 2), Colors.CpuUsageAccent);
    }
}
