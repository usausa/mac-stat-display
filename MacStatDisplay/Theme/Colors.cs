namespace MacStatDisplay.Theme;

using SkiaSharp;

internal static class Colors
{
    // Background & Panel

    public static readonly SKColor GradientStart = new(15, 20, 28);
    public static readonly SKColor GradientEnd = new(8, 10, 16);
    public static readonly SKColor PanelBackground = new(18, 24, 34, 235);
    public static readonly SKColor PanelBorder = new(255, 255, 255, 64);
    public static readonly SKColor TrackColor = new(42, 48, 61);

    // Text

    public static readonly SKColor TextPrimary = SKColors.White;
    public static readonly SKColor TextSecondary = new(148, 163, 184);
    public static readonly SKColor HeaderLabel = new(127, 225, 255);

    // Widget

    public static readonly SKColor CpuUsageAccent = new(88, 166, 255); // Sky blue
    public static readonly SKColor CpuLoadAccent = new(127, 225, 255); // Lighter cyan
    public static readonly SKColor CpuClockAccent = new(196, 181, 253); // Lavender
    public static readonly SKColor MemoryAccent = new(180, 120, 255); // Purple
    public static readonly SKColor GpuAccent = new(94, 234, 212); // Teal
    public static readonly SKColor DiskWriteAccent = new(255, 154, 162); // Coral pink
    public static readonly SKColor DiskReadAccent = new(100, 200, 255); // Light blue
    public static readonly SKColor FileSystemAccent = new(255, 99, 132); // Rose red
    public static readonly SKColor NetworkUploadAccent = new(56, 189, 248); // Bright cyan
    public static readonly SKColor NetworkDownloadAccent = new(74, 222, 128); // Emerald green
    public static readonly SKColor TemperatureAccent = new(251, 146, 60); // Orange
    public static readonly SKColor PowerAccent = new(250, 204, 21); // Gold
    public static readonly SKColor FanAccent = new(96, 165, 250); // Blue
}
