namespace MacStatDisplay;

public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
#pragma warning disable CA1727
#pragma warning disable CA1848
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
#pragma warning restore CA1848
#pragma warning restore CA1727
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
