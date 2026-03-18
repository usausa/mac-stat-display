namespace MacStatDisplay.Monitor;

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

    // Update

    void Update();
}
