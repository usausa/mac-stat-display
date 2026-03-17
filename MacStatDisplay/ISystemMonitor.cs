namespace MacStatDisplay;

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

    // Update

    void Update();
}
