namespace MacStatDisplay;

using HidSharp;

using LcdDriver.TrofeoVision;
using MacStatDisplay.Monitor;
using MacStatDisplay.Settings;
using MacStatDisplay.Widgets;

using SkiaSharp;

internal sealed class Worker(ILogger<Worker> logger, ISystemMonitor monitor, DisplaySettings settings) : BackgroundService
{
    private const int ImageWidth = 1280;
    private const int ImageHeight = 480;
    private const int OuterPadding = 10;
    private const int HeaderHeight = 48;
    private const int ContentGap = 8;
    private const int GridColumns = 4;
    private const int GridRows = 4;

    private readonly TitleBarWidget titleBarWidget = new();

    private record struct WidgetPlacement(IWidget Widget, int Column, int Row, int ColumnSpan = 1, int RowSpan = 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DrawHelper.Initialize();
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
            RenderDashboard(canvas, placements);
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
                    RenderDashboard(canvas, placements);
                    jpegBytes = EncodeToJpeg(surface);
                }

                screen?.DrawJpeg(jpegBytes);
            }
        }
        finally
        {
            DrawHelper.Shutdown();
            screen?.Dispose();
        }
    }

    private static WidgetPlacement[] BuildLayout() =>
    [
        // Row 0-1: CPU (1×2), Memory (1×2), Filesystem (1×2), Network (1×2)
        new(new CpuUsageWidget(), 0, 0, 1, 2),
        new(new MemoryUsageWidget(), 1, 0, 1, 2),
        new(new FileSystemWidget(), 2, 0, 1, 2),
        new(new NetworkWidget(), 3, 0, 1, 2),
        // Row 2-3: Load, GPU (1×2), DiskIO (1×2), FAN
        new(new LoadAverageWidget(), 0, 2),
        new(new GpuUsageWidget(), 1, 2, 1, 2),
        new(new DiskIoWidget(), 2, 2, 1, 2),
        new(new FanWidget(), 3, 2),
        // Row 3: Clock, Power
        new(new CpuClockWidget(), 0, 3),
        new(new PowerWidget(), 3, 3),
    ];

    private void RenderDashboard(SKCanvas canvas, WidgetPlacement[] placements)
    {
        DrawHelper.DrawBackground(canvas, ImageWidth, ImageHeight);

        var headerRect = new SKRect(OuterPadding, OuterPadding, ImageWidth - OuterPadding, OuterPadding + HeaderHeight);
        titleBarWidget.Draw(canvas, headerRect, monitor);

        DrawWidgets(canvas, placements);
    }

    private void DrawWidgets(SKCanvas canvas, WidgetPlacement[] placements)
    {
        var gridTop = OuterPadding + HeaderHeight + ContentGap;
        var gridHeight = ImageHeight - gridTop - OuterPadding;
        var cellWidth = (ImageWidth - (OuterPadding * 2) - (ContentGap * (GridColumns - 1))) / (float)GridColumns;
        var cellHeight = (gridHeight - (ContentGap * (GridRows - 1))) / (float)GridRows;

        foreach (var placement in placements)
        {
            var x = OuterPadding + (placement.Column * (cellWidth + ContentGap));
            var y = gridTop + (placement.Row * (cellHeight + ContentGap));
            var w = (cellWidth * placement.ColumnSpan) + (ContentGap * (placement.ColumnSpan - 1));
            var h = (cellHeight * placement.RowSpan) + (ContentGap * (placement.RowSpan - 1));
            var rect = new SKRect(x, y, x + w, y + h);

            placement.Widget.Draw(canvas, rect, monitor);
        }
    }

    private static byte[] EncodeToJpeg(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }
}
