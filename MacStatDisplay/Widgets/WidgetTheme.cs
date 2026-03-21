namespace MacStatDisplay.Widgets;

using SkiaSharp;

internal static class WidgetTheme
{
    // Dashboard Layout

    internal const int OuterPadding = 8;
    internal const int HeaderHeight = 40;
    internal const int ContentGap = 8;

    // Widget Layout

    internal const float PaddingX = 8f;
    internal const float PaddingY = 8f;
    internal const float TitleOffsetY = 20f;
    internal const float BarHeight = 16f;
    internal const float BarRadius = 5f;

    internal const int SparklineCapacity = 100;

    // ── Panel Radius ────────────────────────────────────────────────────

    // TODO
    internal const float PanelRadius = 14f;
    internal const float HeaderRadius = 16f;

    // ── Font Sizes ──────────────────────────────────────────────────────

    // TODO
    internal const float HeaderTitleFontSize = 22f;
    internal const float HeaderValueFontSize = 20f;
    internal const float HeaderLabelFontSize = 14f;

    // Widget title
    internal const float WidgetTitleFontSize = 18f;

    // TODO
    // Primary metric value in 1×1 widgets (bottom-right large text).
    internal const float PrimaryValueFontSize = 34f;

    // TODO
    // Ring gauge center value (CPU/MEM/GPU %).
    internal const float GaugeValueFontSize = 40f;

    // TODO
    // Temperature display inside ring gauges.
    internal const float TemperatureFontSize = 20f;

    // TODO
    // Value part of stacked label-value pairs; stat speed values.
    internal const float SubValueFontSize = 16f;

    // TODO
    // Label part of stacked label-value pairs; device/entry names in stat widgets.
    internal const float SubLabelFontSize = 13f;

    // Ring Gauge

    internal const float RingStrokeWidth = 16f;
    internal const float RingArcDegrees = 270f;
    internal const float RingStartAngle = 135f;

    // Background & Panel

    internal static readonly SKColor GradientStart = new(15, 20, 28);
    internal static readonly SKColor GradientEnd = new(8, 10, 16);
    internal static readonly SKColor PanelBackground = new(18, 24, 34, 235);
    internal static readonly SKColor PanelBorder = new(255, 255, 255, 20);
    internal static readonly SKColor TrackColor = new(42, 48, 61);

    // Text Colors

    // TODO
    internal static readonly SKColor TextPrimary = SKColors.White;
    internal static readonly SKColor TextSecondary = new(148, 163, 184);
    internal static readonly SKColor HeaderLabel = new(127, 225, 255);

    // Widget Accent Colors

    internal static readonly SKColor CpuUsageAccent = new(88, 166, 255);
    internal static readonly SKColor CpuLoadAccent = new(127, 225, 255);
    internal static readonly SKColor CpuClockAccent = new(196, 181, 253);
    internal static readonly SKColor MemoryAccent = new(180, 120, 255);
    internal static readonly SKColor GpuAccent = new(94, 234, 212);
    internal static readonly SKColor DiskWriteAccent = new(255, 154, 162);
    internal static readonly SKColor DiskReadAccent = new(100, 200, 255);
    internal static readonly SKColor FileSystemAccent = new(255, 99, 132);
    internal static readonly SKColor NetworkUploadAccent = new(56, 189, 248);
    internal static readonly SKColor NetworkDownloadAccent = new(74, 222, 128);
    internal static readonly SKColor TemperatureAccent = new(251, 146, 60);
    internal static readonly SKColor PowerAccent = new(250, 204, 21);
    internal static readonly SKColor FanAccent = new(96, 165, 250);
}
