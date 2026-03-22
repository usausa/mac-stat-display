namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using MacStatDisplay.Theme;

using SkiaSharp;

internal sealed class GpuUsageWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitle(canvas, rect, "GPU Usage");

        var usage = (float)Math.Clamp(monitor.GpuDeviceUtilization ?? 0, 0, 100);

        // Calculate
        var contentTop = rect.Top + Layout.TitleOffsetY + Layout.ContentTopGap;
        var contentH = rect.Bottom - Layout.PaddingY - contentTop;
        var maxRadiusH = contentH / Layout.RingHeightRatio;
        var maxRadiusW = (rect.Width - (2 * Layout.RingSideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = contentTop + (contentH / 2f) + (radius * Layout.RingCenterOffsetRatio);

        // Gauge
        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, Colors.GpuAccent);
        DrawHelper.DrawCenterValue(canvas, $"{usage:0}%", cx, cy + (FontSize.GaugeValue * Layout.BaselineRatio), Colors.GpuAccent);

        // Temperature
        if (monitor.GpuTemperature.HasValue)
        {
            using var tempFont = DrawHelper.MakeFont(FontSize.Temperature);
            using var tempPaint = DrawHelper.MakeFillPaint(Colors.TemperatureAccent);
            var tempText = $"{monitor.GpuTemperature.Value:0}\u00b0C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (FontSize.GaugeValue * Layout.BaselineRatio) + (radius * Layout.TemperatureOffsetRatio), tempFont, tempPaint);
        }

        // Left
        var leftX = rect.Left + Layout.PaddingX;
        var currentY = cy - radius + (radius * Layout.RingSideStartRatio);
        var itemSpacing = radius * Layout.RingSideItemSpacingRatio;
        if (monitor.GpuRendererUtilization.HasValue)
        {
            DrawHelper.DrawStackedValue(canvas, "Renderer", $"{monitor.GpuRendererUtilization.Value}%", leftX, currentY, Colors.GpuAccent);
            currentY += itemSpacing;
        }
        if (monitor.GpuTilerUtilization.HasValue)
        {
            DrawHelper.DrawStackedValue(canvas, "Tiler", $"{monitor.GpuTilerUtilization.Value}%", leftX, currentY, Colors.GpuAccent);
        }
    }
}
