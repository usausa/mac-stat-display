namespace MacStatDisplay.Widgets;

/// <summary>
/// Creates widget instances from configuration type names.
/// </summary>
internal static class WidgetFactory
{
    internal static IWidget Create(string type) =>
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
            _ => throw new InvalidOperationException($"Unknown widget type: '{type}'")
        };
}
