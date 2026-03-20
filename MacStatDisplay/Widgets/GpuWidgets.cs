namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;

using SkiaSharp;

// Ring gauge widget for GPU usage (1×2). Shows temperature and renderer/tiler utilization.
internal sealed class GpuUsageWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "GPU", "Usage");

        var gpu = monitor.GpuDevices.Count > 0 ? monitor.GpuDevices[0] : null;

        var usage = (float)Math.Clamp(gpu?.DeviceUtilization ?? 0, 0, 100);

        // Content area below title
        var contentTop = rect.Top + WidgetTheme.TitleOffsetY + 4;
        var contentH = rect.Bottom - WidgetTheme.PadY - contentTop;
        var sideMargin = 70f;
        var maxRadiusH = contentH / 1.707f;
        var maxRadiusW = (rect.Width - (2 * sideMargin)) / 2f;
        var radius = Math.Min(maxRadiusH, maxRadiusW);
        var cx = rect.MidX;
        var cy = contentTop + (contentH / 2f) + (radius * 0.147f);

        DrawHelper.DrawRingGauge(canvas, cx, cy, radius, usage, WidgetTheme.GpuAccent);
        DrawHelper.DrawCenteredValue(canvas, $"{usage:0}%", cx, cy + (WidgetTheme.GaugeValueFontSize * 0.35f), WidgetTheme.GpuAccent);

        // GPU temperature below center value
        var temp = gpu?.Temperature;
        if (temp.HasValue)
        {
            using var tempFont = DrawHelper.MakeFont(WidgetTheme.TemperatureFontSize);
            using var tempPaint = DrawHelper.Fill(WidgetTheme.TemperatureAccent);
            var tempText = $"{temp.Value:0} C";
            canvas.DrawText(tempText, cx - (tempFont.MeasureText(tempText) / 2f), cy + (WidgetTheme.GaugeValueFontSize * 0.35f) + 22, tempFont, tempPaint);
        }

        // Left side: Renderer / Tiler utilization from first GPU entry
        var leftX = rect.Left + WidgetTheme.PadX;
        var sideTop = cy - radius + 8;
        if (gpu is not null)
        {
            DrawHelper.DrawStackedLabelValue(canvas, "Renderer", $"{gpu.RendererUtilization}%", leftX, sideTop, WidgetTheme.GpuAccent);
            DrawHelper.DrawStackedLabelValue(canvas, "Tiler", $"{gpu.TilerUtilization}%", leftX, sideTop + 44, WidgetTheme.GpuAccent);
        }
    }
}
