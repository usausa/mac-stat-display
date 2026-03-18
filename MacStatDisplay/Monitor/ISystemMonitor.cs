namespace MacStatDisplay.Monitor;

/// <summary>Lightweight display record for filesystem widget rendering.</summary>
internal sealed record FileSystemDisplayEntry(string MountPoint, double TotalGb, double FreeGb, double UsagePercent);

/// <summary>Lightweight display record for disk I/O widget rendering.</summary>
internal sealed record DiskIoDisplayEntry(string Name, double ReadBytesPerSec, double WriteBytesPerSec);

/// <summary>Lightweight display record for network interface widget rendering.</summary>
internal sealed record NetworkIfDisplayEntry(string Name, double RxBytesPerSec, double TxBytesPerSec);

internal interface ISystemMonitor
{
    // CPU

    double CpuUsageTotal { get; }
    double CpuUsageEfficiency { get; }
    double CpuUsagePerformance { get; }
    double CpuUserPercent { get; }
    double CpuSystemPercent { get; }

    // CPU Frequency

    double CpuFrequencyAllHz { get; }
    double CpuFrequencyEfficiencyHz { get; }
    double CpuFrequencyPerformanceHz { get; }

    // Uptime

    TimeSpan Uptime { get; }

    // Load

    double LoadAverage1 { get; }
    double LoadAverage5 { get; }
    double LoadAverage15 { get; }

    // Process

    int ProcessCount { get; }
    int ThreadCount { get; }

    // Memory

    double MemoryUsagePercent { get; }
    double MemoryActivePercent { get; }
    double MemoryWiredPercent { get; }
    double SwapUsagePercent { get; }

    // Disk (aggregate)

    double DiskUsagePercent { get; }
    double DiskTotalGb { get; }
    double DiskFreeGb { get; }
    double DiskReadBytesPerSec { get; }
    double DiskWriteBytesPerSec { get; }

    // Network (aggregate)

    double NetworkRxBytesPerSec { get; }
    double NetworkTxBytesPerSec { get; }

    // Temperature

    double? CpuTemperature { get; }
    double? GpuTemperature { get; }

    // Power

    double PowerCpuW { get; }
    double PowerGpuW { get; }
    double PowerTotalW { get; }

    // Device collections

    IReadOnlyList<FileSystemMonitorEntry> FileSystems { get; }
    IReadOnlyList<DiskDeviceEntry> DiskDevices { get; }
    IReadOnlyList<NetworkIfEntry> NetworkInterfaces { get; }
    IReadOnlyList<GpuEntry> GpuDevices { get; }
    IReadOnlyList<FanSensorEntry> Fans { get; }

    // GPU aggregate (default: computed from GpuDevices)

    double GpuUsagePercent => GpuDevices.Count > 0 ? GpuDevices[0].DeviceUtilization : 0;

    // Fan aggregate (default: computed from Fans)

    double FanSpeedPercent => Fans.Count > 0 && Fans[0].MaxRpm > 0
        ? Fans[0].ActualRpm / Fans[0].MaxRpm * 100.0 : 0;

    double FanSpeedRpm => Fans.Count > 0 ? Fans[0].ActualRpm : 0;

    // Display entry lists (default: computed from device collections)

    IReadOnlyList<FileSystemDisplayEntry> FileSystemDisplayEntries =>
        FileSystems.Select(static e => new FileSystemDisplayEntry(
            e.MountPoint,
            e.TotalSize / (1024.0 * 1024.0 * 1024.0),
            e.FreeSize / (1024.0 * 1024.0 * 1024.0),
            e.TotalSize > 0 ? (double)(e.TotalSize - e.FreeSize) / e.TotalSize * 100.0 : 0)).ToList();

    IReadOnlyList<DiskIoDisplayEntry> DiskIoDisplayEntries =>
        DiskDevices.Select(static e => new DiskIoDisplayEntry(e.Name, e.ReadBytesPerSec, e.WriteBytesPerSec)).ToList();

    IReadOnlyList<NetworkIfDisplayEntry> NetworkIfDisplayEntries =>
        NetworkInterfaces.Select(static e => new NetworkIfDisplayEntry(
            e.DisplayName ?? e.Name, e.RxBytesPerSec, e.TxBytesPerSec)).ToList();

    // Update

    void Update();
}
