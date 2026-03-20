namespace MacStatDisplay.Monitor;

internal sealed class MockSystemMonitor : ISystemMonitor
{
    // Entry

    private sealed record MockDiskDeviceEntry(string Name, double ReadBytesPerSec, double WriteBytesPerSec)
        : IDiskDeviceEntry;

    private sealed record MockFileSystemEntry(
        string MountPoint, string FileSystem, ulong TotalSize, ulong FreeSize, ulong AvailableSize)
        : IFileSystemEntry;

    private sealed record MockNetworkIfEntry(string Name, string? DisplayName, double RxBytesPerSec, double TxBytesPerSec)
        : INetworkIfEntry;

    private sealed record MockGpuEntry(string Name, long DeviceUtilization, long RendererUtilization, long TilerUtilization, int Temperature)
        : IGpuEntry;

    private sealed record MockFanEntry(int Index, double ActualRpm, double MinRpm, double MaxRpm)
        : IFanEntry;

    // Filed

    private readonly Random random = new();
    private readonly DateTime startTime = DateTime.UtcNow;

    private double diskRead1 = 156_000;
    private double diskWrite1 = 86_000;
    private double diskRead2 = 42_000;
    private double diskWrite2 = 18_000;
    private double netRx1 = 5_500_000;
    private double netTx1 = 1_200_000;
    private double netRx2 = 320_000;
    private double netTx2 = 95_000;
    private double gpuUtil = 45;
    private double gpuTemp = 55;
    private double fanRpm = 3200;

    // CPU Usage

    public double CpuUsageTotal { get; private set; } = 35;
    public double CpuUsageEfficiency { get; private set; } = 28;
    public double CpuUsagePerformance { get; private set; } = 42;
    public double CpuUserPercent { get; private set; } = 29.8;
    public double CpuSystemPercent { get; private set; } = 5.2;

    // CPU Frequency

    public double CpuFrequencyAllHz { get; private set; } = 3_200_000_000;
    public double CpuFrequencyEfficiencyHz { get; private set; } = 2_400_000_000;
    public double CpuFrequencyPerformanceHz { get; private set; } = 3_500_000_000;

    // Uptime

    public TimeSpan Uptime => DateTime.UtcNow - startTime + TimeSpan.FromDays(3);

    // Load

    public double LoadAverage1 { get; private set; } = 1.52;
    public double LoadAverage5 { get; private set; } = 1.28;
    public double LoadAverage15 { get; private set; } = 0.95;

    // Memory

    public double MemoryUsagePercent { get; private set; } = 72;
    public double MemoryActivePercent { get; } = 48;
    public double MemoryWiredPercent { get; } = 18;

    // Swap

    public double SwapUsagePercent { get; } = 12;

    // Disk

    public IReadOnlyList<IDiskDeviceEntry> DiskDevices { get; private set; } =
    [
        new MockDiskDeviceEntry("disk0s1", 156_000, 86_000),
        new MockDiskDeviceEntry("disk0s2", 42_000,  18_000)
    ];

    public IReadOnlyList<IFileSystemEntry> FileSystems { get; } =
    [
        new MockFileSystemEntry("/",             "apfs", 475UL * 1024 * 1024 * 1024, 198UL * 1024 * 1024 * 1024, 198UL * 1024 * 1024 * 1024),
        new MockFileSystemEntry("/Volumes/Data", "apfs", 250UL * 1024 * 1024 * 1024, 120UL * 1024 * 1024 * 1024, 120UL * 1024 * 1024 * 1024)
    ];

    // Network

    public IReadOnlyList<INetworkIfEntry> NetworkInterfaces { get; private set; } =
    [
        new MockNetworkIfEntry("en0", "en0 (Wi-Fi)",    5_500_000, 1_200_000),
        new MockNetworkIfEntry("en1", "en1 (Ethernet)",   320_000,    95_000)
    ];

    // Process

    public int ProcessCount { get; private set; } = 352;
    public int ThreadCount { get; private set; } = 1847;

    // Handle

    public int HandleOpenFiles { get; } = 512;
    public int HandleOpenVnodes { get; } = 824;

    // GPU

    public IReadOnlyList<IGpuEntry> GpuDevices { get; private set; } =
    [
        new MockGpuEntry("Apple M2 GPU", 45, 36, 50, 55)
    ];

    // Temperature

    public double? CpuTemperature { get; private set; } = 62;
    public double? NandTemperature { get; private set; } = 52.0;
    public double? SsdTemperature { get; private set; } = 48.0;
    public double? MainboardTemperature { get; private set; } = 45.0;

    // Voltage

    public double? DcInVoltage { get; } = 20.0;

    // Current

    public double? DcInCurrent { get; } = 2.4;

    // Power

    public double? DcInPower { get; } = 48.0;
    public double? TotalSystemPower { get; private set; } = 48.5;

    // Fan

    public IReadOnlyList<IFanEntry> Fans { get; private set; } =
    [
        new MockFanEntry(0, 3200, 1200, 4500)
    ];

    // Power Consumption

    public double PowerCpuW { get; private set; } = 28.2;
    public double PowerGpuW { get; private set; } = 15.8;
    public double PowerAneW { get; private set; } = 2.1;
    public double PowerRamW { get; private set; } = 1.8;
    public double PowerPciW { get; private set; } = 0.8;

    // Update

    public void Update()
    {
        CpuUsageTotal            = Vary(CpuUsageTotal, 5, 95);
        CpuUsageEfficiency        = Vary(CpuUsageEfficiency, 5, 95);
        CpuUsagePerformance       = Vary(CpuUsagePerformance, 5, 95);
        CpuFrequencyEfficiencyHz  = Vary(CpuFrequencyEfficiencyHz,  1_000_000_000, 2_600_000_000);
        CpuFrequencyPerformanceHz = Vary(CpuFrequencyPerformanceHz, 1_000_000_000, 3_500_000_000);
        CpuFrequencyAllHz         = (CpuFrequencyEfficiencyHz + CpuFrequencyPerformanceHz) / 2.0;
        CpuUserPercent      = Vary(CpuUserPercent, 1, 80);
        CpuSystemPercent    = Vary(CpuSystemPercent, 1, 30);
        LoadAverage1        = Vary(LoadAverage1, 0.1, 10);
        LoadAverage5        = Vary(LoadAverage5, 0.1, 8);
        LoadAverage15       = Vary(LoadAverage15, 0.1, 6);
        ProcessCount        = (int)Vary(ProcessCount, 300, 500);
        ThreadCount         = (int)Vary(ThreadCount, 1500, 2500);
        MemoryUsagePercent  = Vary(MemoryUsagePercent, 30, 95);
        CpuTemperature         = Vary(CpuTemperature ?? 62, 40, 90);
        NandTemperature         = Vary(NandTemperature ?? 52, 40, 80);
        SsdTemperature          = Vary(SsdTemperature ?? 48, 35, 70);
        MainboardTemperature    = Vary(MainboardTemperature ?? 45, 30, 65);
        PowerCpuW           = Vary(PowerCpuW, 5, 50);
        PowerGpuW           = Vary(PowerGpuW, 3, 40);
        PowerAneW           = Vary(PowerAneW, 0.5, 8);
        PowerRamW           = Vary(PowerRamW, 0.5, 5);
        PowerPciW           = Vary(PowerPciW, 0.1, 3);
        TotalSystemPower    = PowerCpuW + PowerGpuW + PowerAneW + PowerRamW + PowerPciW;

        diskRead1  = Vary(diskRead1,  0, 500_000);
        diskWrite1 = Vary(diskWrite1, 0, 300_000);
        diskRead2  = Vary(diskRead2,  0, 200_000);
        diskWrite2 = Vary(diskWrite2, 0, 100_000);
        DiskDevices =
        [
            new MockDiskDeviceEntry("disk0s1", diskRead1, diskWrite1),
            new MockDiskDeviceEntry("disk0s2", diskRead2, diskWrite2)
        ];

        netRx1 = Vary(netRx1, 0, 20_000_000);
        netTx1 = Vary(netTx1, 0, 10_000_000);
        netRx2 = Vary(netRx2, 0,  2_000_000);
        netTx2 = Vary(netTx2, 0,    500_000);
        NetworkInterfaces =
        [
            new MockNetworkIfEntry("en0", "en0 (Wi-Fi)",    netRx1, netTx1),
            new MockNetworkIfEntry("en1", "en1 (Ethernet)", netRx2, netTx2)
        ];

        gpuUtil = Vary(gpuUtil, 5, 95);
        gpuTemp = Vary(gpuTemp, 35, 85);
        GpuDevices =
        [
            new MockGpuEntry("Apple M2 GPU", (long)gpuUtil, (long)(gpuUtil * 0.8), (long)(gpuUtil * 1.1), (int)gpuTemp)
        ];

        fanRpm = Vary(fanRpm, 1200, 4500);
        Fans =
        [
            new MockFanEntry(0, fanRpm, 1200, 4500)
        ];
    }

#pragma warning disable CA5394
    private double Vary(double current, double min, double max)
    {
        var delta = (random.NextDouble() - 0.5) * (max - min) * 0.1;
        return Math.Clamp(current + delta, min, max);
    }
#pragma warning restore CA5394
}
