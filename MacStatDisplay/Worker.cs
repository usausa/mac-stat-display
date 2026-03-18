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
        // Row 0-1: CPU (1×2), Memory (1×2), Filesystem (1×2), Disk I/O (1×2)
        new(new CpuUsageWidget(), 0, 0, 1, 2),
        new(new MemoryUsageWidget(), 1, 0, 1, 2),
        new(new FileSystemWidget(), 2, 0, 1, 2),
        new(new DiskIoWidget(), 3, 0, 1, 2),
        // Row 2: GPU, Clock, Network, Power
        new(new GpuUsageWidget(), 0, 2),
        new(new CpuClockWidget(), 1, 2),
        new(new NetworkWidget(), 2, 2),
        new(new PowerWidget(), 3, 2),
        // Row 3: FAN (2×1), Load Average (2×1)
        new(new FanWidget(), 0, 3, 2, 1),
        new(new LoadAverageWidget(), 2, 3, 2, 1),
    ];

    private void RenderDashboard(SKCanvas canvas, WidgetPlacement[] placements, DrawHelper drawHelper)
    {
        DrawHelper.DrawBackground(canvas, ImageWidth, ImageHeight);

        var headerRect = new SKRect(OuterPadding, OuterPadding, ImageWidth - OuterPadding, OuterPadding + HeaderHeight);
        titleBarWidget.Draw(canvas, headerRect, monitor, drawHelper);

        DrawWidgets(canvas, placements, drawHelper);
    }

    private void DrawWidgets(SKCanvas canvas, WidgetPlacement[] placements, DrawHelper drawHelper)
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
