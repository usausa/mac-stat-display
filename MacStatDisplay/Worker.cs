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
    private const int OuterPadding = 18;
    private const int HeaderHeight = 76;
    private const int ContentGap = 12;
    private const int GridColumns = 4;
    private const int GridRows = 4;

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
        new(new LoadAverageWidget(), 2, 0),
        new(new CpuClockWidget(), 3, 0),
        new(new DiskCapacityWidget(), 2, 1),
        new(new NetworkDownloadWidget(), 3, 1),
        new(new MemoryAppWidget(), 0, 2),
        new(new MemorySwapWidget(), 1, 2),
        new(new DiskReadWidget(), 2, 2),
        new(new DiskWriteWidget(), 3, 2),
        new(new ProcessCountWidget(), 0, 3),
        new(new CpuTemperatureWidget(), 1, 3),
        new(new GpuTemperatureWidget(), 2, 3),
        new(new PowerTotalWidget(), 3, 3),
    ];

    private void RenderDashboard(SKCanvas canvas, WidgetPlacement[] placements, DrawHelper drawHelper)
    {
        DrawHelper.DrawBackground(canvas, ImageWidth, ImageHeight);
        DrawHeader(canvas, drawHelper);
        DrawWidgets(canvas, placements, drawHelper);
    }

    private void DrawHeader(SKCanvas canvas, DrawHelper drawHelper)
    {
        var headerRect = new SKRect(OuterPadding, OuterPadding, ImageWidth - OuterPadding, OuterPadding + HeaderHeight);

        using var panelBg = DrawHelper.Fill(DrawHelper.PanelBg);
        canvas.DrawRoundRect(headerRect, DrawHelper.HeaderRadius, DrawHelper.HeaderRadius, panelBg);

        using var panelBorder = DrawHelper.Stroke(DrawHelper.PanelBorder, 1);
        canvas.DrawRoundRect(headerRect, DrawHelper.HeaderRadius, DrawHelper.HeaderRadius, panelBorder);

        // Title
        using var titleFont = drawHelper.MakeFont(26f, true);
        using var titlePaint = DrawHelper.Fill(SKColors.White);
        canvas.DrawText("SYSTEM MONITOR", headerRect.Left + 20, headerRect.Top + 30, titleFont, titlePaint);

        // Subtitle
        using var subtitleFont = drawHelper.MakeFont(13f);
        using var subtitlePaint = DrawHelper.Fill(DrawHelper.HeaderSubtitle);
        canvas.DrawText("Realtime System Monitor", headerRect.Left + 20, headerRect.Top + 52, subtitleFont, subtitlePaint);

        // Header metrics
        using var labelFont = drawHelper.MakeFont(16f, true);
        using var labelPaint = DrawHelper.Fill(DrawHelper.AccentCyan);
        using var valueFont = drawHelper.MakeFont(18f, true);
        using var valuePaint = DrawHelper.Fill(SKColors.White);

        // Uptime
        var uptime = monitor.Uptime;
        var uptimeText = $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m";
        var uptimeX = ImageWidth - 420f;
        canvas.DrawText("UPTIME", uptimeX, headerRect.Top + 24, labelFont, labelPaint);
        canvas.DrawText(uptimeText, uptimeX, headerRect.Top + 48, valueFont, valuePaint);

        // Time
        var clock = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var timeX = ImageWidth - 250f;
        canvas.DrawText("TIME", timeX, headerRect.Top + 24, labelFont, labelPaint);
        canvas.DrawText(clock, timeX, headerRect.Top + 48, valueFont, valuePaint);
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
