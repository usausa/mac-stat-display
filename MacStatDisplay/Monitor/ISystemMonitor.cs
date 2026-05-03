namespace MacStatDisplay.Monitor;

#pragma warning disable IDE0051
internal interface IDiskDeviceEntry
{
    string Name { get; }
    double ReadBytesPerSec { get; }
    double WriteBytesPerSec { get; }
}

internal interface IFileSystemEntry
{
    string MountPoint { get; }
    string FileSystem { get; }
    ulong TotalSize { get; }
    ulong FreeSize { get; }
    ulong AvailableSize { get; }
}

internal interface INetworkIfEntry
{
    string Name { get; }
    string? DisplayName { get; }
    double RxBytesPerSec { get; }
    double TxBytesPerSec { get; }
}

internal interface IFanEntry
{
    int Index { get; }
    double ActualRpm { get; }
    double MinRpm { get; }
    double MaxRpm { get; }
}

internal interface ISystemMonitor
{
    // CPU Usage

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

    // Memory

    double MemoryUsagePercent { get; }
    double MemoryActivePercent { get; }
    double MemoryWiredPercent { get; }
    double MemoryCompressorPercent { get; }

    // Swap

    double SwapUsagePercent { get; }

    // Disk

    IReadOnlyList<IDiskDeviceEntry> DiskDevices { get; }

    IReadOnlyList<IFileSystemEntry> FileSystems { get; }

    // Network

    IReadOnlyList<INetworkIfEntry> NetworkInterfaces { get; }

    // Process

    int ProcessCount { get; }
    int ThreadCount { get; }

    // Handle

    int HandleOpenFiles { get; }
    int HandleOpenVnodes { get; }

    // GPU

    ulong? GpuDeviceUtilization { get; }
    ulong? GpuRendererUtilization { get; }
    ulong? GpuTilerUtilization { get; }
    double? GpuTemperature { get; }

    // Temperature

    double? CpuTemperature { get; }
    double? NandTemperature { get; }
    double? SsdTemperature { get; }
    double? MainboardTemperature { get; }

    // Voltage

    double? DcInVoltage { get; }

    // Current

    double? DcInCurrent { get; }

    // Power

    double? DcInPower { get; }

    double? TotalSystemPower { get; }

    // Fan

    IReadOnlyList<IFanEntry> Fans { get; }

    // Power Consumption

    double PowerCpuW { get; }
    double PowerGpuW { get; }
    double PowerAneW { get; }
    double PowerRamW { get; }
    double PowerPciW { get; }

    // Update

    void Update();
}
#pragma warning restore IDE0051
