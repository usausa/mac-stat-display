namespace MacStatDisplay.Monitor;

internal sealed class MockSystemMonitor : ISystemMonitor
{
    private readonly Random random = new();
    private readonly DateTime startTime = DateTime.UtcNow;

    // CPU

    public double CpuUsageTotal { get; private set; } = 35;
    public double CpuUsageEfficiency { get; private set; } = 28;
    public double CpuUsagePerformance { get; private set; } = 42;
    public double CpuUserPercent { get; private set; } = 29.8;
    public double CpuSystemPercent { get; private set; } = 5.2;

    // CPU Frequency

    public double CpuFrequencyAllHz { get; private set; } = 3_200_000_000;
    public double CpuFrequencyEfficiencyHz { get; private set; } = 2_400_000_000;
    public double CpuFrequencyPerformanceHz { get; private set; } = 3_800_000_000;

    // Uptime

    public TimeSpan Uptime => DateTime.UtcNow - startTime + TimeSpan.FromDays(3);

    // Load

    public double LoadAverage1 { get; private set; } = 1.52;
    public double LoadAverage5 { get; private set; } = 1.28;
    public double LoadAverage15 { get; private set; } = 0.95;

    // Process

    public int ProcessCount { get; private set; } = 352;
    public int ThreadCount { get; private set; } = 1847;

    // Memory

    public double MemoryUsagePercent { get; private set; } = 72;
    public double MemoryActivePercent { get; private set; } = 48;
    public double MemoryWiredPercent { get; private set; } = 18;
    public double SwapUsagePercent { get; private set; } = 12;

    // GPU

    public double GpuUsagePercent { get; private set; } = 45;

    // Fan

    public double FanSpeedPercent { get; private set; } = 72;
    public double FanSpeedRpm { get; private set; } = 3200;

    // Disk

    public double DiskUsagePercent { get; private set; } = 58;
    public double DiskTotalGb { get; private set; } = 475;
    public double DiskFreeGb { get; private set; } = 198;
    public double DiskReadBytesPerSec { get; private set; } = 156_000;
    public double DiskWriteBytesPerSec { get; private set; } = 86_000;

    // Network

    public double NetworkRxBytesPerSec { get; private set; } = 5_500_000;
    public double NetworkTxBytesPerSec { get; private set; } = 1_200_000;

    // Temperature

    public double? CpuTemperature { get; private set; } = 62;
    public double? GpuTemperature { get; private set; } = 55;

    // Power

    public double PowerCpuW { get; private set; } = 28.2;
    public double PowerGpuW { get; private set; } = 15.8;
    public double PowerTotalW { get; private set; } = 48.5;

    // Device collections (empty – real types cannot be instantiated without platform APIs)

    public IReadOnlyList<FileSystemMonitorEntry> FileSystems { get; } = [];
    public IReadOnlyList<DiskDeviceEntry> DiskDevices { get; } = [];
    public IReadOnlyList<NetworkIfEntry> NetworkInterfaces { get; } = [];
    public IReadOnlyList<GpuEntry> GpuDevices { get; } = [];
    public IReadOnlyList<FanSensorEntry> Fans { get; } = [];

    // Snapshot collections (2 entries each for FS, Disk, Network)

    public IReadOnlyList<FileSystemSnapshot> FileSystemSnapshots { get; } =
    [
        new("/", 475, 198, 58),
        new("/Volumes/Data", 250, 120, 52),
    ];

    public IReadOnlyList<DiskIoSnapshot> DiskIoSnapshots { get; private set; } =
    [
        new("disk0s1", 156_000, 86_000),
        new("disk0s2", 42_000, 18_000),
    ];

    public IReadOnlyList<NetworkIfSnapshot> NetworkIfSnapshots { get; private set; } =
    [
        new("en0 (Wi-Fi)", 5_500_000, 1_200_000),
        new("en1 (Ethernet)", 320_000, 95_000),
    ];

    public void Update()
    {
        CpuUsageTotal = Vary(CpuUsageTotal, 5, 95);
        CpuUsageEfficiency = Vary(CpuUsageEfficiency, 5, 95);
        CpuUsagePerformance = Vary(CpuUsagePerformance, 5, 95);
        CpuUserPercent = Vary(CpuUserPercent, 1, 80);
        CpuSystemPercent = Vary(CpuSystemPercent, 1, 30);
        LoadAverage1 = Vary(LoadAverage1, 0.1, 10);
        LoadAverage5 = Vary(LoadAverage5, 0.1, 8);
        LoadAverage15 = Vary(LoadAverage15, 0.1, 6);
        ProcessCount = (int)Vary(ProcessCount, 300, 500);
        ThreadCount = (int)Vary(ThreadCount, 1500, 2500);
        MemoryUsagePercent = Vary(MemoryUsagePercent, 30, 95);
        GpuUsagePercent = Vary(GpuUsagePercent, 5, 95);
        FanSpeedPercent = Vary(FanSpeedPercent, 20, 100);
        FanSpeedRpm = FanSpeedPercent / 100.0 * 4500;
        DiskReadBytesPerSec = Vary(DiskReadBytesPerSec, 0, 500_000);
        DiskWriteBytesPerSec = Vary(DiskWriteBytesPerSec, 0, 300_000);
        NetworkRxBytesPerSec = Vary(NetworkRxBytesPerSec, 0, 20_000_000);
        NetworkTxBytesPerSec = Vary(NetworkTxBytesPerSec, 0, 10_000_000);
        CpuTemperature = Vary(CpuTemperature ?? 62, 40, 90);
        GpuTemperature = Vary(GpuTemperature ?? 55, 35, 85);
        PowerCpuW = Vary(PowerCpuW, 5, 50);
        PowerGpuW = Vary(PowerGpuW, 3, 40);
        PowerTotalW = PowerCpuW + PowerGpuW + 4.5;

        DiskIoSnapshots =
        [
            new("disk0s1", Vary(156_000, 0, 500_000), Vary(86_000, 0, 300_000)),
            new("disk0s2", Vary(42_000, 0, 200_000), Vary(18_000, 0, 100_000)),
        ];

        NetworkIfSnapshots =
        [
            new("en0 (Wi-Fi)", NetworkRxBytesPerSec, NetworkTxBytesPerSec),
            new("en1 (Ethernet)", Vary(320_000, 0, 2_000_000), Vary(95_000, 0, 500_000)),
        ];
    }

    private double Vary(double current, double min, double max)
    {
        var delta = (random.NextDouble() - 0.5) * (max - min) * 0.1;
        return Math.Clamp(current + delta, min, max);
    }
}
