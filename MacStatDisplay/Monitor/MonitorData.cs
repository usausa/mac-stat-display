namespace MacStatDisplay.Monitor;

/// <summary>Snapshot of a filesystem entry for widget rendering.</summary>
internal sealed record FileSystemSnapshot(string MountPoint, double TotalGb, double FreeGb, double UsagePercent);

/// <summary>Snapshot of disk I/O for widget rendering.</summary>
internal sealed record DiskIoSnapshot(string Name, double ReadBytesPerSec, double WriteBytesPerSec);

/// <summary>Snapshot of a network interface for widget rendering.</summary>
internal sealed record NetworkIfSnapshot(string Name, double RxBytesPerSec, double TxBytesPerSec);
