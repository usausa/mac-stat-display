namespace MacStatDisplay.Theme;

using SkiaSharp;

internal static class Colors
{
    // Background & Panel

    internal static readonly SKColor GradientStart = new(15, 20, 28);
    internal static readonly SKColor GradientEnd = new(8, 10, 16);
    internal static readonly SKColor PanelBackground = new(18, 24, 34, 235);
    internal static readonly SKColor PanelBorder = new(255, 255, 255, 20);
    internal static readonly SKColor TrackColor = new(42, 48, 61);

    // Text

    internal static readonly SKColor TextPrimary = SKColors.White;
    internal static readonly SKColor TextSecondary = new(148, 163, 184);
    internal static readonly SKColor HeaderLabel = new(127, 225, 255);

    // Widget

    internal static readonly SKColor CpuUsageAccent = new(88, 166, 255); // Sky blue
    internal static readonly SKColor CpuLoadAccent = new(127, 225, 255); // Lighter cyan
    internal static readonly SKColor CpuClockAccent = new(196, 181, 253); // Lavender
    internal static readonly SKColor MemoryAccent = new(180, 120, 255); // Purple
    internal static readonly SKColor GpuAccent = new(94, 234, 212); // Teal
    internal static readonly SKColor DiskWriteAccent = new(255, 154, 162); // Coral pink
    internal static readonly SKColor DiskReadAccent = new(100, 200, 255); // Light blue
    internal static readonly SKColor FileSystemAccent = new(255, 99, 132); // Rose red
    internal static readonly SKColor NetworkUploadAccent = new(56, 189, 248); // Bright cyan
    internal static readonly SKColor NetworkDownloadAccent = new(74, 222, 128); // Emerald green
    internal static readonly SKColor TemperatureAccent = new(251, 146, 60); // Orange
    internal static readonly SKColor PowerAccent = new(250, 204, 21); // Gold
    internal static readonly SKColor FanAccent = new(96, 165, 250); // Blue
}
