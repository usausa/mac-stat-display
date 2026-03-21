namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;

using SkiaSharp;

// Text widget for total system power. CPU/GPU stacked vertically, bottom-aligned with main value.
internal sealed class PowerWidget : IWidget
{
    public void Initialize(IReadOnlyDictionary<string, string> parameters)
    {
    }

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor)
    {
        DrawHelper.DrawPanel(canvas, rect);
        DrawHelper.DrawTitleBlock(canvas, rect, "POWER", "System");

        // Total power bottom-right
        DrawHelper.DrawValue(canvas, $"{monitor.TotalSystemPower:0.0} W", rect.Right - WidgetTheme.PaddingX, rect.Bottom - WidgetTheme.PaddingY, WidgetTheme.PowerAccent);

        // CPU and GPU stacked vertically, bottom-aligned with main value
        var leftX = rect.Left + WidgetTheme.PaddingX;
        var mainBottom = rect.Bottom - WidgetTheme.PaddingY;
        var y2 = mainBottom - 18;
        var y1 = y2 - 36;
        DrawHelper.DrawStackedLabelValue(canvas, "CPU", $"{monitor.PowerCpuW:0.0}W", leftX, y1, WidgetTheme.PowerAccent);
        DrawHelper.DrawStackedLabelValue(canvas, "GPU", $"{monitor.PowerGpuW:0.0}W", leftX, y2, WidgetTheme.PowerAccent);
    }
}
