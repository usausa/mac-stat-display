namespace MacStatDisplay.Widgets;

using MacStatDisplay.Helpers;
using MacStatDisplay.Theme;

using SkiaSharp;

internal static class DrawHelper
{
    private static SKTypeface typeface = null!;
    private static SKTypeface typefaceBold = null!;

    internal static void Initialize()
    {
        typeface = ResolveTypeface(false);
        typefaceBold = ResolveTypeface(true);
    }

    internal static void Shutdown()
    {
        typeface.Dispose();
        typefaceBold.Dispose();
    }

    private static SKTypeface ResolveTypeface(bool bold)
    {
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

        var mediumPath = Path.Combine("Assets", "Roboto-Medium.ttf");
        if (File.Exists(mediumPath))
        {
            var tf = SKTypeface.FromFile(mediumPath);
            if (tf is not null)
            {
                return tf;
            }
        }

        return bold ? SKTypeface.FromFamilyName(SKTypeface.Default.FamilyName, SKFontStyle.Bold) : SKTypeface.Default;
    }

    // Measures the width of the given text using SubLabel font.
    internal static float MeasureSubLabelWidth(string text)
    {
        using var font = MakeFont(FontSize.SubLabel);
        return font.MeasureText(text);
    }

    // Measures the width of the given text using SubValue bold font.
    internal static float MeasureSubValueWidth(string text)
    {
        using var font = MakeFont(FontSize.SubValue, true);
        return font.MeasureText(text);
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
            [Colors.GradientStart, Colors.GradientEnd],
            null,
            SKShaderTileMode.Clamp);
        paint.IsAntialias = true;
        canvas.DrawRect(0, 0, width, height, paint);
    }

    // Draws a card panel with rounded corners and border.
    internal static void DrawPanel(SKCanvas canvas, SKRect rect)
    {
        using var bg = Fill(Colors.PanelBackground);
        canvas.DrawRoundRect(rect, Layout.PanelRadius, Layout.PanelRadius, bg);

        using var border = Stroke(Colors.PanelBorder, 1);
        canvas.DrawRoundRect(rect, Layout.PanelRadius, Layout.PanelRadius, border);
    }

    // Draws title text at the top-left of a widget.
    internal static void DrawTitleBlock(SKCanvas canvas, SKRect rect, string title)
    {
        using var font = MakeFont(FontSize.WidgetTitle, true);
        using var paint = Fill(Colors.TextPrimary);
        canvas.DrawText(title, rect.Left + Layout.PaddingX, rect.Top + Layout.TitleOffsetY, font, paint);
    }

    // Draws a right-aligned value in large bold font.
    internal static void DrawValue(SKCanvas canvas, string text, float rightX, float y, SKColor color)
    {
        using var font = MakeFont(FontSize.PrimaryValue, true);
        using var paint = Fill(color);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    // Draws a centered value in large bold font (for ring gauge interior).
    internal static void DrawCenteredValue(SKCanvas canvas, string text, float centerX, float y, SKColor color)
    {
        using var font = MakeFont(FontSize.GaugeValue, true);
        using var paint = Fill(color);
        canvas.DrawText(text, centerX - (font.MeasureText(text) / 2f), y, font, paint);
    }

    // Draws right-aligned detail text.
    internal static void DrawRightAlignedDetail(SKCanvas canvas, string text, float rightX, float y)
    {
        using var font = MakeFont(FontSize.SubValue);
        using var paint = Fill(Colors.TextSecondary);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    // Draws a left-aligned label above a colored value, bottom-aligned at the given y (value baseline).
    internal static void DrawStackedLabelValue(SKCanvas canvas, string label, string value, float x, float bottomY, SKColor valueColor)
    {
        using var valueFont = MakeFont(FontSize.SubValue, true);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(value, x, bottomY, valueFont, valuePaint);

        using var labelFont = MakeFont(FontSize.SubLabel);
        using var labelPaint = Fill(Colors.TextSecondary);
        canvas.DrawText(label, x, bottomY + valueFont.Metrics.Ascent - labelFont.Metrics.Descent, labelFont, labelPaint);
    }

    // Draws a right-aligned label above a colored value, bottom-aligned at the given y (value baseline).
    internal static void DrawStackedLabelValueRight(SKCanvas canvas, string label, string value, float rightX, float bottomY, SKColor valueColor)
    {
        using var valueFont = MakeFont(FontSize.SubValue, true);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(value, rightX - valueFont.MeasureText(value), bottomY, valueFont, valuePaint);

        using var labelFont = MakeFont(FontSize.SubLabel);
        using var labelPaint = Fill(Colors.TextSecondary);
        canvas.DrawText(label, rightX - labelFont.MeasureText(label), bottomY + valueFont.Metrics.Ascent - labelFont.Metrics.Descent, labelFont, labelPaint);
    }

    // Draws two right-aligned label+value pairs for sparkline side values.
    // Upper pair is drawn upward from the upper anchor, lower pair is drawn downward from the lower anchor.
    // The center margin (as a ratio of area height) controls the gap between upper and lower sections.
    internal static void DrawSparklineSideValues(
        SKCanvas canvas, float rightX, float areaTop, float areaBottom,
        string upperLabel, string upperValue, SKColor upperColor,
        string lowerLabel, string lowerValue, SKColor lowerColor)
    {
        using var valFont = MakeFont(FontSize.SubValue, true);
        using var labelFont = MakeFont(FontSize.SubLabel);
        using var labelPaint = Fill(Colors.TextSecondary);

        var areaHeight = areaBottom - areaTop;
        var halfContent = (1f - Layout.SparklineSideCenterMarginRatio) / 2f;

        // Upper: draw upward from upper anchor (value bottom at anchor, label above value)
        var upperAnchor = areaTop + (areaHeight * halfContent);
        var upperValueY = upperAnchor - valFont.Metrics.Descent;
        var upperLabelY = upperValueY + valFont.Metrics.Ascent - labelFont.Metrics.Descent;

        using var upperPaint = Fill(upperColor);
        canvas.DrawText(upperLabel, rightX - labelFont.MeasureText(upperLabel), upperLabelY, labelFont, labelPaint);
        canvas.DrawText(upperValue, rightX - valFont.MeasureText(upperValue), upperValueY, valFont, upperPaint);

        // Lower: draw downward from lower anchor (label top at anchor, value below label)
        var lowerAnchor = areaBottom - (areaHeight * halfContent);
        var lowerLabelY = lowerAnchor - labelFont.Metrics.Ascent;
        var lowerValueY = lowerLabelY + labelFont.Metrics.Descent - valFont.Metrics.Ascent;

        using var lowerPaint = Fill(lowerColor);
        canvas.DrawText(lowerLabel, rightX - labelFont.MeasureText(lowerLabel), lowerLabelY, labelFont, labelPaint);
        canvas.DrawText(lowerValue, rightX - valFont.MeasureText(lowerValue), lowerValueY, valFont, lowerPaint);
    }

    // Draws a 270° ring gauge (open at bottom).
    internal static void DrawRingGauge(SKCanvas canvas, float centerX, float centerY, float radius, float percentage, SKColor color)
    {
        using var trackPaint = new SKPaint();
        trackPaint.Color = Colors.TrackColor;
        trackPaint.IsAntialias = true;
        trackPaint.Style = SKPaintStyle.Stroke;
        trackPaint.StrokeWidth = Layout.RingStrokeWidth;
        trackPaint.StrokeCap = SKStrokeCap.Round;

        using var valuePaint = new SKPaint();
        valuePaint.Color = color;
        valuePaint.IsAntialias = true;
        valuePaint.Style = SKPaintStyle.Stroke;
        valuePaint.StrokeWidth = Layout.RingStrokeWidth;
        valuePaint.StrokeCap = SKStrokeCap.Round;

        var ringRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(ringRect, Layout.RingStartAngle, Layout.RingArcDegrees, false, trackPaint);
        canvas.DrawArc(ringRect, Layout.RingStartAngle, Layout.RingArcDegrees * percentage / 100f, false, valuePaint);
    }

    // Draws a sparkline area chart in the given rectangle (value 0 at bottom, growing upward).
    internal static void DrawSparkline(SKCanvas canvas, SKRect rect, RingBuffer buffer, float maxValue, SKColor color)
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
        linePaint.StrokeWidth = Layout.SparklineStrokeWidth;
        canvas.DrawPath(linePath, linePaint);
    }

    // Draws an inverted sparkline
    internal static void DrawSparklineInverted(SKCanvas canvas, SKRect rect, RingBuffer buffer, float maxValue, SKColor color)
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
        linePaint.StrokeWidth = Layout.SparklineStrokeWidth;
        canvas.DrawPath(linePath, linePaint);
    }
}
