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
        DrawHelper.DrawTitleBlock(canvas, rect, "GPU Usage");

        var gpu = monitor.GpuDevices.Count > 0 ? monitor.GpuDevices[0] : null;
        var usage = (float)Math.Clamp(gpu?.DeviceUtilization ?? 0, 0, 100);

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
        DrawHelper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (FontSize.GaugeValue * Layout.BaselineRatio), Colors.GpuAccent);

        // Temperature
        if (gpu is not null)
        {
            using var tempFont = DrawHelper.MakeFont(FontSize.Temperature);
            using var tempPaint = DrawHelper.Fill(Colors.TemperatureAccent);
            var tempText = $"{gpu.Temperature:0}\u00b0C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (FontSize.GaugeValue * Layout.BaselineRatio) + (radius * Layout.TemperatureOffsetRatio), tempFont, tempPaint);
        }

        // Left
        if (gpu is not null)
        {
            var leftX = rect.Left + Layout.PaddingX;
            var sideStartY = cy - radius + (radius * Layout.RingSideStartRatio);
            var itemSpacing = radius * Layout.RingSideItemSpacingRatio;
            DrawHelper.DrawStackedLabelValue(canvas, "Renderer", $"{gpu.RendererUtilization}%", leftX, sideStartY, Colors.GpuAccent);
            DrawHelper.DrawStackedLabelValue(canvas, "Tiler", $"{gpu.TilerUtilization}%", leftX, sideStartY + itemSpacing, Colors.GpuAccent);
        }
    }
}
