namespace MacStatDisplay.Monitor;

internal static class SystemMonitorFactory
{
    public static ISystemMonitor Create(string monitor) =>
        monitor switch
        {
            "System" => new SystemMonitor(),
            "Mock" => new MockSystemMonitor(),
            _ => throw new InvalidOperationException($"Unsupported system monitor: {monitor}")
        };
}
