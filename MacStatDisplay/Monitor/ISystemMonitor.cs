namespace MacStatDisplay.Monitor;

internal interface ISystemMonitor
{
    // ── CPU ──────────────────────────────────────────────────────────────

    double CpuUsageTotal { get; }
    double CpuUsageEfficiency { get; }
    double CpuUsagePerformance { get; }
    double CpuUserPercent { get; }
    double CpuSystemPercent { get; }

    // ── CPU Frequency ────────────────────────────────────────────────────

    double CpuFrequencyAllHz { get; }
    double CpuFrequencyEfficiencyHz { get; }
    double CpuFrequencyPerformanceHz { get; }

    // ── Uptime ───────────────────────────────────────────────────────────

    TimeSpan Uptime { get; }

    // ── Load ─────────────────────────────────────────────────────────────

    double LoadAverage1 { get; }
    double LoadAverage5 { get; }
    double LoadAverage15 { get; }

    // ── Process ──────────────────────────────────────────────────────────

    int ProcessCount { get; }
    int ThreadCount { get; }

    // ── Memory ───────────────────────────────────────────────────────────

    double MemoryUsagePercent { get; }
    double MemoryActivePercent { get; }
    double MemoryWiredPercent { get; }
    double SwapUsagePercent { get; }

    // ── Temperature ──────────────────────────────────────────────────────

    double? CpuTemperature { get; }

    // ── Power ────────────────────────────────────────────────────────────

    double PowerCpuW { get; }
    double PowerGpuW { get; }
    double PowerTotalW { get; }

    // ── Device collections ───────────────────────────────────────────────

    IReadOnlyList<IFileSystemEntry> FileSystems { get; }
    IReadOnlyList<IDiskDeviceEntry> DiskDevices { get; }
    IReadOnlyList<INetworkIfEntry> NetworkInterfaces { get; }
    IReadOnlyList<IGpuEntry> GpuDevices { get; }
    IReadOnlyList<IFanEntry> Fans { get; }

    // ── Disk aggregate (default: computed from FileSystems / DiskDevices) ─

    double DiskUsagePercent
    {
        get
        {
            var fss = FileSystems;
            for (var i = 0; i < fss.Count; i++)
            {
                if (fss[i].MountPoint != "/") continue;
                var e = fss[i];
                return e.TotalSize > 0 ? (double)(e.TotalSize - e.FreeSize) / e.TotalSize * 100.0 : 0;
            }
            return 0;
        }
    }

    double DiskTotalGb
    {
        get
        {
            var fss = FileSystems;
            for (var i = 0; i < fss.Count; i++)
            {
                if (fss[i].MountPoint == "/") return fss[i].TotalSize / (1024.0 * 1024.0 * 1024.0);
            }
            return 0;
        }
    }

    double DiskFreeGb
    {
        get
        {
            var fss = FileSystems;
            for (var i = 0; i < fss.Count; i++)
            {
                if (fss[i].MountPoint == "/") return fss[i].FreeSize / (1024.0 * 1024.0 * 1024.0);
            }
            return 0;
        }
    }

    double DiskReadBytesPerSec
    {
        get
        {
            var devices = DiskDevices;
            var sum = 0.0;
            for (var i = 0; i < devices.Count; i++) sum += devices[i].ReadBytesPerSec;
            return sum;
        }
    }

    double DiskWriteBytesPerSec
    {
        get
        {
            var devices = DiskDevices;
            var sum = 0.0;
            for (var i = 0; i < devices.Count; i++) sum += devices[i].WriteBytesPerSec;
            return sum;
        }
    }

    // ── Network aggregate (default: computed from NetworkInterfaces) ───────

    double NetworkRxBytesPerSec
    {
        get
        {
            var ifaces = NetworkInterfaces;
            var sum = 0.0;
            for (var i = 0; i < ifaces.Count; i++) sum += ifaces[i].RxBytesPerSec;
            return sum;
        }
    }

    double NetworkTxBytesPerSec
    {
        get
        {
            var ifaces = NetworkInterfaces;
            var sum = 0.0;
            for (var i = 0; i < ifaces.Count; i++) sum += ifaces[i].TxBytesPerSec;
            return sum;
        }
    }

    // ── GPU aggregate (default: computed from GpuDevices) ─────────────────

    double GpuUsagePercent => GpuDevices.Count > 0 ? GpuDevices[0].DeviceUtilization : 0;

    double? GpuTemperature => GpuDevices.Count > 0 ? GpuDevices[0].Temperature : null;

    // ── Fan aggregate (default: computed from Fans) ───────────────────────

    double FanSpeedPercent => Fans.Count > 0 && Fans[0].MaxRpm > 0
        ? Fans[0].ActualRpm / Fans[0].MaxRpm * 100.0 : 0;

    double FanSpeedRpm => Fans.Count > 0 ? Fans[0].ActualRpm : 0;

    // ── Update ────────────────────────────────────────────────────────────

    void Update();
}

