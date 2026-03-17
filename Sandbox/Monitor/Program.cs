using MacStatDisplay;

var lines = new List<(string Label, string Value)>();

var monitor = new SystemMonitor();

await Task.Delay(1000);
monitor.Update();

// CPU
lines.Add(("CPU Usage:", $"Total: {monitor.CpuUsageTotal:F1} %  (E: {monitor.CpuUsageEfficiency:F1} %  P: {monitor.CpuUsagePerformance:F1} %)"));
lines.Add(("CPU Usage Breakdown:", $"User: {monitor.CpuUserPercent:F1} %  System: {monitor.CpuSystemPercent:F1} %  Idle: {monitor.CpuIdlePercent:F1} %"));
lines.Add(("CPU Frequency All:", $"{monitor.CpuFrequencyAllHz / 1_000_000.0:F0} MHz  (E: {monitor.CpuFrequencyEfficiencyHz / 1_000_000.0:F0} MHz  P: {monitor.CpuFrequencyPerformanceHz / 1_000_000.0:F0} MHz)"));
// System
lines.Add(("Uptime:", $"{monitor.Uptime:d\\.hh\\:mm\\:ss}"));
lines.Add(("System:", $"Processes: {monitor.ProcessCount}  Threads: {monitor.ThreadCount}"));
lines.Add(("Load Average:", $"{monitor.LoadAverage1:F2}  {monitor.LoadAverage5:F2}  {monitor.LoadAverage15:F2}  (1/5/15 min)"));
// Memory
lines.Add(("Memory Usage:", $"{monitor.MemoryUsagePercent:F1} %  (Active: {monitor.MemoryActivePercent:F1} %  Wired: {monitor.MemoryWiredPercent:F1} %  Compressor: {monitor.MemoryCompressorPercent:F1} %)"));
lines.Add(("Swap Usage:", $"{monitor.SwapUsagePercent:F1} %"));
// GPU
foreach (var gpu in monitor.GpuDevices)
{
    lines.Add(($"GPU [{gpu.Name}]:", $"Device: {gpu.DeviceUtilization} %  Renderer: {gpu.RendererUtilization} %  Tiler: {gpu.TilerUtilization} %"));
}
// Disk
foreach (var disk in monitor.DiskDevices)
{
    lines.Add(($"Disk {disk.Name} ({disk.BusType}):", $"Read: {disk.ReadBytesPerSec / 1024.0:F1} KB/s  Write: {disk.WriteBytesPerSec / 1024.0:F1} KB/s"));
}
// File System
foreach (var fs in monitor.FileSystems)
{
    var usage = (double)(fs.TotalSize - fs.AvailableSize) / fs.TotalSize * 100.0;
    lines.Add(($"FS {fs.MountPoint} ({fs.FileSystem}):", $"{usage:F1} %  ({fs.TotalSize / 1024 / 1024 / 1024} GB total)"));
}
// Network
foreach (var net in monitor.NetworkInterfaces)
{
    var name = net.DisplayName is not null ? $"{net.Name} ({net.DisplayName})" : net.Name;
    lines.Add(($"Net {name}:", $"DL: {net.RxBytesPerSec / 1024.0:F1} KB/s  UL: {net.TxBytesPerSec / 1024.0:F1} KB/s  Total RX: {net.RxBytes / 1024 / 1024} MB  TX: {net.TxBytes / 1024 / 1024} MB"));
}
// Temperature
if (monitor.CpuTemperature is { } cpuTemp)
{
    lines.Add(("Temp CPU:", $"{cpuTemp:F2} C"));
}
if (monitor.MainboardTemperature is { } mbTemp)
{
    lines.Add(("Temp Mainboard:", $"{mbTemp:F2} C"));
}
if (monitor.NandTemperature is { } nandTemp)
{
    lines.Add(("Temp NAND:", $"{nandTemp:F2} C"));
}
if (monitor.SsdTemperature is { } ssdTemp)
{
    lines.Add(("Temp SSD:", $"{ssdTemp:F2} C"));
}
// Voltage
if (monitor.DcInVoltage is { } voltage)
{
    lines.Add(("Voltage DC-in:", $"{voltage:F3} V"));
}
// Current
if (monitor.DcInCurrent is { } current)
{
    lines.Add(("Current DC-in:", $"{current:F3} A"));
}
// Power
if (monitor.DcInPower is { } dcPower)
{
    lines.Add(("Power DC-in:", $"{dcPower:F2} W"));
}
if (monitor.TotalSystemPower is { } sysTotal)
{
    lines.Add(("Power Total System:", $"{sysTotal:F2} W"));
}
// Fan
foreach (var f in monitor.Fans)
{
    var usage = f.MaxRpm > 0 ? f.ActualRpm / f.MaxRpm * 100.0 : 0;
    lines.Add(($"Fan {f.Index}:", $"{f.ActualRpm:F0} rpm  ({usage:F1} %)  [min: {f.MinRpm:F0}  max: {f.MaxRpm:F0}]"));
}
// Power Consumption
lines.Add(("Power:", $"CPU: {monitor.PowerCpuW:F2} W  GPU: {monitor.PowerGpuW:F2} W  ANE: {monitor.PowerAneW:F2} W  RAM: {monitor.PowerRamW:F2} W  PCI: {monitor.PowerPciW:F2} W"));

var labelWidth = lines.Max(l => l.Label.Length);
foreach (var (label, value) in lines)
{
    Console.WriteLine($"{label.PadRight(labelWidth)}  {value}");
}
