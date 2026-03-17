namespace MacStatDisplay;

using System.Globalization;

using HidSharp;

using LcdDriver.TrofeoVision;

using MacStatDisplay.Widgets;

using SkiaSharp;

internal sealed class Worker(ILogger<Worker> logger, ISystemMonitor monitor, DisplaySettings settings) : BackgroundService
{
    private const int ImageWidth = 1280;
    private const int ImageHeight = 480;
    private const int Margin = 3;
    private const int HeaderHeight = 28;
    private const int GridColumns = 4;
    private const int GridRows = 4;
    private const int GridGap = 3;

    private record struct WidgetPlacement(IWidget Widget, int Column, int Row, int ColumnSpan = 1, int RowSpan = 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var drawHelper = new DrawHelper();
        using var surface = SKSurface.Create(new SKImageInfo(ImageWidth, ImageHeight));
        var canvas = surface.Canvas;
        var placements = BuildLayout();

        var hidDevice = DeviceList.Local
            .GetHidDevices(ScreenDevice.VendorId, ScreenDevice.ProductId)
            .FirstOrDefault();

        ScreenDevice? screen = null;
        if (hidDevice is not null)
        {
            screen = new ScreenDevice(hidDevice);
            logger.LogInformation("LCD device found");
        }
        else
        {
            logger.LogWarning("LCD device not found");
        }

        try
        {
            // Initial render
            monitor.Update();
            RenderDashboard(canvas, placements, drawHelper);
            var jpegBytes = EncodeToJpeg(surface);
            screen?.DrawJpeg(jpegBytes);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            var tickCount = 0;

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                tickCount++;

                if (tickCount >= settings.UpdatePeriod)
                {
                    tickCount = 0;
                    monitor.Update();
                    RenderDashboard(canvas, placements, drawHelper);
                    jpegBytes = EncodeToJpeg(surface);
                }

                screen?.DrawJpeg(jpegBytes);
            }
        }
        finally
        {
            screen?.Dispose();
        }
    }

    private static WidgetPlacement[] BuildLayout() =>
    [
        new(new CpuUsageWidget(), 0, 0, 1, 2),
        new(new MemoryUsageWidget(), 1, 0, 1, 2),
        new(new DiskUsageWidget(), 2, 0, 2, 1),
        new(new CpuTemperatureWidget(), 2, 1),
        new(new GpuTemperatureWidget(), 3, 1),
        new(new NetworkDownloadWidget(), 0, 2),
        new(new NetworkUploadWidget(), 1, 2),
        new(new LoadAverageWidget(), 2, 2),
        new(new ProcessCountWidget(), 3, 2),
        new(new CpuUserWidget(), 0, 3),
        new(new CpuSystemWidget(), 1, 3),
        new(new PowerTotalWidget(), 2, 3),
        new(new ThreadCountWidget(), 3, 3)
    ];

    private void RenderDashboard(SKCanvas canvas, WidgetPlacement[] placements, DrawHelper drawHelper)
    {
        canvas.Clear(DrawHelper.BgColor);
        DrawHeader(canvas, drawHelper);
        DrawWidgets(canvas, placements, drawHelper);
    }

    private void DrawHeader(SKCanvas canvas, DrawHelper drawHelper)
    {
        using var bgPaint = DrawHelper.Fill(DrawHelper.HeaderBg);
        canvas.DrawRect(0, 0, ImageWidth, HeaderHeight, bgPaint);

        using var separator = DrawHelper.Stroke(DrawHelper.CardBorder, 1);
        canvas.DrawLine(0, HeaderHeight, ImageWidth, HeaderHeight, separator);

        using var titleFont = drawHelper.MakeFont(14f);
        using var titlePaint = DrawHelper.Fill(new SKColor(112, 223, 255));
        canvas.DrawText("SYSTEM MONITOR", Margin + 2, 19, titleFont, titlePaint);

        using var infoFont = drawHelper.MakeFont(10f);
        using var infoPaint = DrawHelper.Fill(DrawHelper.TextSub);

        var uptime = monitor.Uptime;
        var uptimeText = $"Uptime {(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m";
        canvas.DrawText(uptimeText, (ImageWidth - infoFont.MeasureText(uptimeText)) / 2f, 19, infoFont, infoPaint);

        var clock = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        canvas.DrawText(clock, ImageWidth - Margin - infoFont.MeasureText(clock) - 2, 19, infoFont, infoPaint);
    }

    private void DrawWidgets(SKCanvas canvas, WidgetPlacement[] placements, DrawHelper drawHelper)
    {
        var gridTop = HeaderHeight + GridGap;
        var cellWidth = (ImageWidth - (Margin * 2f) - ((GridColumns - 1) * GridGap)) / GridColumns;
        var cellHeight = (ImageHeight - gridTop - Margin - ((GridRows - 1) * GridGap)) / GridRows;

        foreach (var placement in placements)
        {
            var x = Margin + (placement.Column * (cellWidth + GridGap));
            var y = gridTop + (placement.Row * (cellHeight + GridGap));
            var w = (cellWidth * placement.ColumnSpan) + (GridGap * (placement.ColumnSpan - 1));
            var h = (cellHeight * placement.RowSpan) + (GridGap * (placement.RowSpan - 1));
            var rect = new SKRect(x, y, x + w, y + h);

            placement.Widget.Draw(canvas, rect, monitor, drawHelper);
        }
    }

    private static byte[] EncodeToJpeg(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }
}
