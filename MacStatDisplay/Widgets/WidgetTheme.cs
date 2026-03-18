namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Centralized color, font-size and layout constants for all widgets.</summary>
internal static class WidgetTheme
{
    // ── Background & Panel ──────────────────────────────────────────────

    internal static readonly SKColor GradientStart = new(15, 20, 28);
    internal static readonly SKColor GradientEnd = new(8, 10, 16);
    internal static readonly SKColor PanelBg = new(18, 24, 34, 235);
    internal static readonly SKColor PanelBorder = new(255, 255, 255, 20);
    internal static readonly SKColor TrackColor = new(42, 48, 61);

    // ── Text Colors ─────────────────────────────────────────────────────

    internal static readonly SKColor TextPrimary = SKColors.White;
    internal static readonly SKColor TextSub = new(148, 163, 184);
    internal static readonly SKColor HeaderLabelColor = new(127, 225, 255);

    // ── Widget Accent Colors ────────────────────────────────────────────

    internal static readonly SKColor CpuUsageAccent = new(88, 166, 255);
    internal static readonly SKColor CpuLoadAccent = new(127, 225, 255);
    internal static readonly SKColor CpuClockAccent = new(196, 181, 253);
    internal static readonly SKColor MemoryAccent = new(180, 120, 255);
    internal static readonly SKColor GpuAccent = new(94, 234, 212);
    internal static readonly SKColor FileSystemAccent = new(255, 99, 132);
    internal static readonly SKColor DiskWriteAccent = new(255, 154, 162);
    internal static readonly SKColor DiskReadAccent = new(100, 200, 255);
    internal static readonly SKColor NetworkUploadAccent = new(56, 189, 248);
    internal static readonly SKColor NetworkDownloadAccent = new(74, 222, 128);
    internal static readonly SKColor TemperatureAccent = new(251, 146, 60);
    internal static readonly SKColor PowerAccent = new(250, 204, 21);
    internal static readonly SKColor FanAccent = new(96, 165, 250);

    // ── Panel Radius ────────────────────────────────────────────────────

    internal const float PanelRadius = 14f;
    internal const float HeaderRadius = 16f;

    // ── Font Sizes ──────────────────────────────────────────────────────

    /// <summary>Widget title block (e.g. "CPU Usage").</summary>
    internal const float WidgetTitleFontSize = 14f;

    /// <summary>Primary metric value in 1×1 widgets (bottom-right large text).</summary>
    internal const float PrimaryValueFontSize = 34f;

    /// <summary>Ring gauge center value (CPU/MEM/GPU %).</summary>
    internal const float GaugeValueFontSize = 40f;

    /// <summary>Temperature display inside ring gauges.</summary>
    internal const float TemperatureFontSize = 20f;

    /// <summary>Value part of stacked label-value pairs; stat speed values.</summary>
    internal const float SubValueFontSize = 16f;

    /// <summary>Label part of stacked label-value pairs; device/entry names in stat widgets.</summary>
    internal const float SubLabelFontSize = 13f;

    internal const float HeaderTitleFontSize = 22f;
    internal const float HeaderValueFontSize = 20f;
    internal const float HeaderLabelFontSize = 14f;

    // ── Ring Gauge ──────────────────────────────────────────────────────

    internal const float RingStrokeWidth = 14f;
    internal const float RingArcDegrees = 270f;
    internal const float RingStartAngle = 135f;

    // ── Layout ──────────────────────────────────────────────────────────

    internal const float PadX = 10f;
    internal const float PadY = 6f;
    internal const float TitleOffsetY = 20f;
    internal const float BarHeight = 14f;
    internal const float BarRadius = 5f;
    internal const int SparklineCapacity = 60;
}
