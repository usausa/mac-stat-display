namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Stat widget for CPU temperature.</summary>
internal sealed class CpuTemperatureWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 150, 70);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "HWモニター", Accent, rect.Left + 5, rect.Top + 4);

        var temp = monitor.CpuTemperature ?? 0;
        var valueColor = DrawHelper.ResolveTempColor(temp);

        helper.DrawLabel(canvas, "CPU温度", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{temp:0}℃", rect.Right - 8, rect.MidY + 8, valueColor, 36f);

        helper.DrawDetail(canvas, $"Power {monitor.PowerCpuW:0.0} W", rect.Left + 8, rect.Bottom - 7, 9f);
    }
}

/// <summary>Stat widget for GPU temperature.</summary>
internal sealed class GpuTemperatureWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 150, 70);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "HWモニター", Accent, rect.Left + 5, rect.Top + 4);

        var temp = monitor.GpuTemperature ?? 0;
        var valueColor = DrawHelper.ResolveTempColor(temp);

        helper.DrawLabel(canvas, "GPU温度", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{temp:0}℃", rect.Right - 8, rect.MidY + 8, valueColor, 36f);

        helper.DrawDetail(canvas, $"Power {monitor.PowerGpuW:0.0} W", rect.Left + 8, rect.Bottom - 7, 9f);
    }
}

/// <summary>Stat widget for total system power.</summary>
internal sealed class PowerTotalWidget : IWidget
{
    private static readonly SKColor Accent = new(255, 150, 70);
    private static readonly SKColor ValueColor = new(255, 186, 106);

    public void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper)
    {
        helper.DrawCard(canvas, rect);
        helper.DrawBadge(canvas, "HWモニター", Accent, rect.Left + 5, rect.Top + 4);

        helper.DrawLabel(canvas, "電力合計", rect.Left + 8, rect.Top + 32, rect.Width - 132);
        helper.DrawLargeValue(canvas, $"{monitor.PowerTotalW:0.0} W", rect.Right - 8, rect.MidY + 8, ValueColor, 36f);
    }
}
