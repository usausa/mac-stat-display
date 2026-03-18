namespace MacStatDisplay.Monitor;

internal interface IFileSystemEntry
{
    string MountPoint { get; }
    string FileSystem { get; }
    ulong TotalSize { get; }
    ulong FreeSize { get; }
    ulong AvailableSize { get; }
}

internal interface IDiskDeviceEntry
{
    string Name { get; }
    double ReadBytesPerSec { get; }
    double WriteBytesPerSec { get; }
}

internal interface INetworkIfEntry
{
    string Name { get; }
    string? DisplayName { get; }
    double RxBytesPerSec { get; }
    double TxBytesPerSec { get; }
}

internal interface IGpuEntry
{
    string Name { get; }
    long DeviceUtilization { get; }
    long RendererUtilization { get; }
    long TilerUtilization { get; }
    int Temperature { get; }
}

internal interface IFanEntry
{
    int Index { get; }
    double ActualRpm { get; }
    double MinRpm { get; }
    double MaxRpm { get; }
}
