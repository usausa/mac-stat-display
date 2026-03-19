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

    private readonly TitleBarWidget titleBarWidget = new();

    // A widget paired with its pre-computed drawing rectangle.
    private record struct WidgetPlacement(IWidget Widget, SKRect Rect);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DrawHelper.Initialize();
        using var surface = SKSurface.Create(new SKImageInfo(ImageWidth, ImageHeight));
        var canvas = surface.Canvas;
        var placements = BuildLayout();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunDeviceSessionAsync(canvas, surface, placements, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Device session failed");
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Retrying device connection in {Delay}s", settings.DeviceRetrySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(settings.DeviceRetrySeconds), stoppingToken);
                }
            }
        }
        finally
        {
            DrawHelper.Shutdown();
        }
    }

    // Acquires the LCD device and runs the display loop until error or cancellation.
    private async Task RunDeviceSessionAsync(
        SKCanvas canvas, SKSurface surface, WidgetPlacement[] placements, CancellationToken stoppingToken)
    {
        var hidDevice = DeviceList.Local
            .GetHidDevices(ScreenDevice.VendorId, ScreenDevice.ProductId)
            .FirstOrDefault();

        if (hidDevice is null)
        {
            logger.LogWarning("LCD device not found");
            return;
        }

        logger.LogInformation("LCD device found");
        using var screen = new ScreenDevice(hidDevice);

        monitor.Update();
        RenderDashboard(canvas, placements);
        var jpegBytes = EncodeToJpeg(surface);
        screen.DrawJpeg(jpegBytes);

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

            screen.DrawJpeg(jpegBytes);
        }
    }

    // Builds the widget layout from settings, pre-computing each widget's drawing rectangle.
    private WidgetPlacement[] BuildLayout()
    {
        if (settings.Widgets.Count == 0)
        {
            throw new InvalidOperationException(
                "No widgets configured. Add entries to Display.Widgets in appsettings.json.");
        }

        var gridColumns = settings.Grid.Columns;
        var gridRows = settings.Grid.Rows;

        var gridTop = OuterPadding + HeaderHeight + ContentGap;
        var gridHeight = ImageHeight - gridTop - OuterPadding;
        var cellWidth = (ImageWidth - (OuterPadding * 2) - (ContentGap * (gridColumns - 1))) / (float)gridColumns;
        var cellHeight = (gridHeight - (ContentGap * (gridRows - 1))) / (float)gridRows;

        var placements = new WidgetPlacement[settings.Widgets.Count];
        for (var i = 0; i < settings.Widgets.Count; i++)
        {
            var entry = settings.Widgets[i];
            var widget = CreateWidget(entry.Type);
            widget.Initialize(entry.Parameters);

            var x = OuterPadding + (entry.Column * (cellWidth + ContentGap));
            var y = gridTop + (entry.Row * (cellHeight + ContentGap));
            var w = (cellWidth * entry.ColumnSpan) + (ContentGap * (entry.ColumnSpan - 1));
            var h = (cellHeight * entry.RowSpan) + (ContentGap * (entry.RowSpan - 1));

            placements[i] = new WidgetPlacement(widget, new SKRect(x, y, x + w, y + h));
        }

        logger.LogInformation("Layout built: {Count} widgets on {Cols}×{Rows} grid", placements.Length, gridColumns, gridRows);
        return placements;
    }

    private static IWidget CreateWidget(string type) =>
        type switch
        {
            "CpuUsage"    => new CpuUsageWidget(),
            "CpuClock"    => new CpuClockWidget(),
            "LoadAverage" => new LoadAverageWidget(),
            "MemoryUsage" => new MemoryUsageWidget(),
            "GpuUsage"    => new GpuUsageWidget(),
            "FileSystem"  => new FileSystemWidget(),
            "DiskIo"      => new DiskIoWidget(),
            "Network"     => new NetworkWidget(),
            "Fan"         => new FanWidget(),
            "Power"       => new PowerWidget(),
            _ => throw new InvalidOperationException($"Unknown widget type: '{type}'"),
        };

    private void RenderDashboard(SKCanvas canvas, WidgetPlacement[] placements)
    {
        DrawHelper.DrawBackground(canvas, ImageWidth, ImageHeight);

        var headerRect = new SKRect(OuterPadding, OuterPadding, ImageWidth - OuterPadding, OuterPadding + HeaderHeight);
        titleBarWidget.Draw(canvas, headerRect, monitor);

        foreach (var placement in placements)
        {
            placement.Widget.Draw(canvas, placement.Rect, monitor);
        }
    }

    private static byte[] EncodeToJpeg(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }
}
