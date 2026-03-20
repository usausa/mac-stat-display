namespace MacStatDisplay.Widgets;

using SkiaSharp;

// Provides shared rendering utilities for dashboard widgets.
internal static class DrawHelper
{
    private static SKTypeface typeface = null!;
    private static SKTypeface typefaceBold = null!;

    // Initializes shared typefaces. Must be called once at startup.
    internal static void Initialize()
    {
        typeface = ResolveTypeface(false);
        typefaceBold = ResolveTypeface(true);
    }

    // Disposes shared typefaces.
    internal static void Shutdown()
    {
        typeface.Dispose();
        typefaceBold.Dispose();
    }

    // Creates a font with the resolved typeface.
    internal static SKFont MakeFont(float size, bool bold = false) =>
        new(bold ? typefaceBold : typeface, size) { Edging = SKFontEdging.SubpixelAntialias };

    // Creates a fill paint.
    internal static SKPaint Fill(SKColor color) => new() { Color = color, IsAntialias = true };

    // Creates a stroke paint.
    internal static SKPaint Stroke(SKColor color, float width) => new()
    {
        Color = color,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = width
    };

    // Formats bytes/sec into a human-readable speed string.
    internal static string FormatSpeed(double bytesPerSec)
    {
        if (bytesPerSec >= 1024 * 1024)
        {
            return $"{bytesPerSec / (1024.0 * 1024.0):0.0} MB/s";
        }

        return $"{bytesPerSec / 1024.0:0} KB/s";
    }

    // Draws the full-screen gradient background.
    internal static void DrawBackground(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint();
        paint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(width, height),
            [WidgetTheme.GradientStart, WidgetTheme.GradientEnd],
            null,
            SKShaderTileMode.Clamp);
        paint.IsAntialias = true;
        canvas.DrawRect(0, 0, width, height, paint);
    }

    // Draws a card panel with rounded corners and border.
    internal static void DrawPanel(SKCanvas canvas, SKRect rect)
    {
        using var bg = Fill(WidgetTheme.PanelBg);
        canvas.DrawRoundRect(rect, WidgetTheme.PanelRadius, WidgetTheme.PanelRadius, bg);

        using var border = Stroke(WidgetTheme.PanelBorder, 1);
        canvas.DrawRoundRect(rect, WidgetTheme.PanelRadius, WidgetTheme.PanelRadius, border);
    }

    // Draws "CATEGORY Title" text at the top-left of a widget.
    internal static void DrawTitleBlock(SKCanvas canvas, SKRect rect, string category, string title)
    {
        using var font = MakeFont(WidgetTheme.WidgetTitleFontSize, true);
        using var paint = Fill(WidgetTheme.TextPrimary);
        var label = string.IsNullOrWhiteSpace(category) ? title : $"{category} {title}";
        canvas.DrawText(label, rect.Left + WidgetTheme.PadX, rect.Top + WidgetTheme.TitleOffsetY, font, paint);
    }

    // Draws a right-aligned value in large bold font.
    internal static void DrawValue(SKCanvas canvas, string text, float rightX, float y, SKColor color)
    {
        using var font = MakeFont(WidgetTheme.PrimaryValueFontSize, true);
        using var paint = Fill(color);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    // Draws a centered value in large bold font (for ring gauge interior).
    internal static void DrawCenteredValue(SKCanvas canvas, string text, float centerX, float y, SKColor color)
    {
        using var font = MakeFont(WidgetTheme.GaugeValueFontSize, true);
        using var paint = Fill(color);
        canvas.DrawText(text, centerX - (font.MeasureText(text) / 2f), y, font, paint);
    }

    // Draws right-aligned detail text.
    internal static void DrawRightAlignedDetail(SKCanvas canvas, string text, float rightX, float y)
    {
        using var font = MakeFont(WidgetTheme.SubValueFontSize);
        using var paint = Fill(WidgetTheme.TextSub);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    // Draws a left-aligned label on one line and a colored value below it.
    internal static void DrawStackedLabelValue(SKCanvas canvas, string label, string value, float x, float y, SKColor valueColor)
    {
        using var labelFont = MakeFont(WidgetTheme.SubLabelFontSize);
        using var labelPaint = Fill(WidgetTheme.TextSub);
        canvas.DrawText(label, x, y, labelFont, labelPaint);

        using var valueFont = MakeFont(WidgetTheme.SubValueFontSize, true);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(value, x, y + 18, valueFont, valuePaint);
    }

    // Draws a right-aligned label on one line and a colored value below it.
    internal static void DrawStackedLabelValueRight(SKCanvas canvas, string label, string value, float rightX, float y, SKColor valueColor)
    {
        using var labelFont = MakeFont(WidgetTheme.SubLabelFontSize);
        using var labelPaint = Fill(WidgetTheme.TextSub);
        canvas.DrawText(label, rightX - labelFont.MeasureText(label), y, labelFont, labelPaint);

        using var valueFont = MakeFont(WidgetTheme.SubValueFontSize, true);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(value, rightX - valueFont.MeasureText(value), y + 18, valueFont, valuePaint);
    }

    // Draws a 270° ring gauge (open at bottom).
    internal static void DrawRingGauge(SKCanvas canvas, float centerX, float centerY, float radius, float percentage, SKColor color)
    {
        using var trackPaint = new SKPaint();
        trackPaint.Color = WidgetTheme.TrackColor;
        trackPaint.IsAntialias = true;
        trackPaint.Style = SKPaintStyle.Stroke;
        trackPaint.StrokeWidth = WidgetTheme.RingStrokeWidth;
        trackPaint.StrokeCap = SKStrokeCap.Round;

        using var valuePaint = new SKPaint();
        valuePaint.Color = color;
        valuePaint.IsAntialias = true;
        valuePaint.Style = SKPaintStyle.Stroke;
        valuePaint.StrokeWidth = WidgetTheme.RingStrokeWidth;
        valuePaint.StrokeCap = SKStrokeCap.Round;

        var ringRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(ringRect, WidgetTheme.RingStartAngle, WidgetTheme.RingArcDegrees, false, trackPaint);
        canvas.DrawArc(ringRect, WidgetTheme.RingStartAngle, WidgetTheme.RingArcDegrees * percentage / 100f, false, valuePaint);
    }

    // Draws a sparkline area chart in the given rectangle (value 0 at bottom, growing upward).
    internal static void DrawSparkline(SKCanvas canvas, SKRect rect, SparklineBuffer buffer, float maxValue, SKColor color)
    {
        if (maxValue <= 0)
        {
            maxValue = 1;
        }

        var cap = buffer.Capacity;
        var stepX = rect.Width / (cap - 1);

        // Filled area
        using var areaPath = new SKPath();
        areaPath.MoveTo(rect.Left, rect.Bottom);
        for (var i = 0; i < cap; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Bottom - (Math.Clamp(buffer[i] / maxValue, 0, 1) * rect.Height);
            areaPath.LineTo(x, y);
        }

        areaPath.LineTo(rect.Right, rect.Bottom);
        areaPath.Close();

        using var fillPaint = new SKPaint();
        fillPaint.Color = color.WithAlpha(30);
        fillPaint.IsAntialias = true;
        fillPaint.Style = SKPaintStyle.Fill;
        canvas.DrawPath(areaPath, fillPaint);

        // Line on top
        using var linePath = new SKPath();
        linePath.MoveTo(rect.Left, rect.Bottom - (Math.Clamp(buffer[0] / maxValue, 0, 1) * rect.Height));
        for (var i = 1; i < cap; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Bottom - (Math.Clamp(buffer[i] / maxValue, 0, 1) * rect.Height);
            linePath.LineTo(x, y);
        }

        using var linePaint = new SKPaint();
        linePaint.Color = color.WithAlpha(100);
        linePaint.IsAntialias = true;
        linePaint.Style = SKPaintStyle.Stroke;
        linePaint.StrokeWidth = 1.5f;
        canvas.DrawPath(linePath, linePaint);
    }

    // Draws an inverted sparkline area chart (value 0 at top, growing downward).
    internal static void DrawSparklineInverted(SKCanvas canvas, SKRect rect, SparklineBuffer buffer, float maxValue, SKColor color)
    {
        if (maxValue <= 0)
        {
            maxValue = 1;
        }

        var cap = buffer.Capacity;
        var stepX = rect.Width / (cap - 1);

        // Filled area (from top, growing downward)
        using var areaPath = new SKPath();
        areaPath.MoveTo(rect.Left, rect.Top);
        for (var i = 0; i < cap; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Top + (Math.Clamp(buffer[i] / maxValue, 0, 1) * rect.Height);
            areaPath.LineTo(x, y);
        }

        areaPath.LineTo(rect.Right, rect.Top);
        areaPath.Close();

        using var fillPaint = new SKPaint();
        fillPaint.Color = color.WithAlpha(30);
        fillPaint.IsAntialias = true;
        fillPaint.Style = SKPaintStyle.Fill;
        canvas.DrawPath(areaPath, fillPaint);

        // Line on edge
        using var linePath = new SKPath();
        linePath.MoveTo(rect.Left, rect.Top + (Math.Clamp(buffer[0] / maxValue, 0, 1) * rect.Height));
        for (var i = 1; i < cap; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Top + (Math.Clamp(buffer[i] / maxValue, 0, 1) * rect.Height);
            linePath.LineTo(x, y);
        }

        using var linePaint = new SKPaint();
        linePaint.Color = color.WithAlpha(100);
        linePaint.IsAntialias = true;
        linePaint.Style = SKPaintStyle.Stroke;
        linePaint.StrokeWidth = 1.5f;
        canvas.DrawPath(linePath, linePaint);
    }

    private static SKTypeface ResolveTypeface(bool bold)
    {
        // Bold: prefer Roboto-Bold.ttf, then fall back to Roboto-Medium.ttf
        if (bold)
        {
            var boldPath = Path.Combine("Assets", "Roboto-Bold.ttf");
            if (File.Exists(boldPath))
            {
                var tf = SKTypeface.FromFile(boldPath);
                if (tf is not null)
                {
                    return tf;
                }
            }
        }

        // Regular (and bold fallback): Roboto-Medium.ttf
        var mediumPath = Path.Combine("Assets", "Roboto-Medium.ttf");
        if (File.Exists(mediumPath))
        {
            var tf = SKTypeface.FromFile(mediumPath);
            if (tf is not null)
            {
                return tf;
            }
        }

        // System font fallback
        string[] families =
        [
            "Yu Gothic UI", "Meiryo", "MS Gothic",
            "Hiragino Sans", "Hiragino Kaku Gothic ProN",
            "Noto Sans CJK JP", "Noto Sans JP"
        ];

        var style = bold ? SKFontStyle.Bold : SKFontStyle.Normal;

        foreach (var name in families)
        {
            var tf = SKTypeface.FromFamilyName(name, style);
            if (tf is not null && tf.FamilyName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return tf;
            }

            tf?.Dispose();
        }

        return bold
            ? SKTypeface.FromFamilyName(SKTypeface.Default.FamilyName, SKFontStyle.Bold)
            : SKTypeface.Default;
    }
}
