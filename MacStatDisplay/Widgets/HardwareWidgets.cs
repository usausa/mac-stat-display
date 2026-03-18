namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

/// <summary>Text widget for CPU temperature.</summary>
internal sealed class CpuTemperatureWidget : IWidget
{
    private static readonly SKColor Accent = new(251, 146, 60);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, string.Empty, "CPU Temp");

        var temp = monitor.CpuTemperature ?? 0;
        helper.DrawValue(canvas, $"{temp:0} C", rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"Power {monitor.PowerCpuW:0.0} W", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
    }
}

/// <summary>Text widget for GPU temperature.</summary>
internal sealed class GpuTemperatureWidget : IWidget
{
    private static readonly SKColor Accent = new(96, 165, 250);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, string.Empty, "GPU Temp");

        var temp = monitor.GpuTemperature ?? 0;
        helper.DrawValue(canvas, $"{temp:0} C", rect.Right - 16, rect.MidY + 8, Accent);
        helper.DrawWrappedDetail(canvas, $"Power {monitor.PowerGpuW:0.0} W", rect.Left + 16, rect.MidY + 8, rect.Width - 32);
    }
}

/// <summary>Text widget for total system power.</summary>
internal sealed class PowerTotalWidget : IWidget
{
    private static readonly SKColor Accent = new(250, 204, 21);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawPanel(canvas, rect);
        helper.DrawTitleBlock(canvas, rect, "POWER", "System");

        helper.DrawValue(canvas, $"{monitor.PowerTotalW:0.0} W", rect.Right - 16, rect.MidY + 8, Accent);
    }
}
