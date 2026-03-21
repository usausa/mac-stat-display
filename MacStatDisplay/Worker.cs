namespace MacStatDisplay;

using MacStatDisplay.Display;
using MacStatDisplay.Monitor;
using MacStatDisplay.Settings;
using MacStatDisplay.Theme;
using MacStatDisplay.Widgets;

using SkiaSharp;

internal sealed class Worker(ILogger<Worker> log, DisplaySettings settings, ISystemMonitor monitor, IDisplayDriver displayDriver) : BackgroundService
{
    private record struct WidgetPlacement(IWidget Widget, SKRect Rect);

    //--------------------------------------------------------------------------------
    // Core
    //--------------------------------------------------------------------------------

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DrawHelper.Initialize();

        var width = displayDriver.Width;
        var height = displayDriver.Height;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var placements = BuildLayout(width, height);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
#pragma warning disable CA1031
                try
                {
                    await RunDisplayLoopAsync(surface, width, height, placements, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.ErrorUnknownException(ex);
                }
#pragma warning restore CA1031

                if (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(settings.DeviceRetrySeconds), stoppingToken);
                }
            }
        }
        finally
        {
            DrawHelper.Shutdown();
        }
    }

    private async Task RunDisplayLoopAsync(SKSurface surface, int width, int height, WidgetPlacement[] placements, CancellationToken stoppingToken)
    {
        if (!displayDriver.Initialize())
        {
            return;
        }

        var canvas = surface.Canvas;

        // Initial render immediately after monitor update.
        monitor.Update();
        RenderDashboard(canvas, width, height, placements);
        displayDriver.Draw(surface);

        var refreshInterval = displayDriver.RefreshIntervalSeconds;

        if (refreshInterval <= 0)
        {
            // No periodic refresh — only redraw on monitor updates.
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(settings.UpdatePeriod));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                monitor.Update();
                RenderDashboard(canvas, width, height, placements);
                displayDriver.Draw(surface);
            }
        }
        else
        {
            // Periodic refresh with monitor updates at the configured period.
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(refreshInterval));
            var tickCount = 0;

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                tickCount += refreshInterval;

                if (tickCount >= settings.UpdatePeriod)
                {
                    tickCount = 0;
                    monitor.Update();
                    RenderDashboard(canvas, width, height, placements);
                }

                displayDriver.Draw(surface);
            }
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private WidgetPlacement[] BuildLayout(int width, int height)
    {
        var placements = new List<WidgetPlacement>();

        // Header
        var headerWidget = WidgetFactory.Create(settings.Header.Type);
        headerWidget.Initialize(settings.Header.Parameters);
        var headerRect = new SKRect(Layout.OuterPadding, Layout.OuterPadding, width - Layout.OuterPadding, Layout.OuterPadding + Layout.HeaderHeight);

        placements.Add(new WidgetPlacement(headerWidget, headerRect));

        // Grid
        var gridColumns = settings.Grid.Columns;
        var gridRows = settings.Grid.Rows;

        var gridTop = Layout.OuterPadding + Layout.HeaderHeight + Layout.ContentGap;
        var gridHeight = height - gridTop - Layout.OuterPadding;
        var cellWidth = (float)(width - (Layout.OuterPadding * 2) - (Layout.ContentGap * (gridColumns - 1))) / gridColumns;
        var cellHeight = (float)(gridHeight - (Layout.ContentGap * (gridRows - 1))) / gridRows;

        for (var i = 0; i < settings.Widgets.Count; i++)
        {
            var entry = settings.Widgets[i];

            var widget = WidgetFactory.Create(entry.Type);
            widget.Initialize(entry.Parameters);

            var x = Layout.OuterPadding + (entry.Column * (cellWidth + Layout.ContentGap));
            var y = gridTop + (entry.Row * (cellHeight + Layout.ContentGap));
            var w = (cellWidth * entry.ColumnSpan) + (Layout.ContentGap * (entry.ColumnSpan - 1));
            var h = (cellHeight * entry.RowSpan) + (Layout.ContentGap * (entry.RowSpan - 1));

            placements.Add(new WidgetPlacement(widget, new SKRect(x, y, x + w, y + h)));
        }

        return placements.ToArray();
    }

    private void RenderDashboard(SKCanvas canvas, int width, int height, IEnumerable<WidgetPlacement> placements)
    {
        DrawHelper.DrawBackground(canvas, width, height);

        foreach (var placement in placements)
        {
            placement.Widget.Draw(canvas, placement.Rect, monitor);
        }
    }
}
