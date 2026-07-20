namespace MacStatDisplay.Widgets;

using MacStatDisplay.Helpers;
using MacStatDisplay.Theme;

using SkiaSharp;

internal static class DrawHelper
{
    private static readonly Dictionary<(float Size, bool Bold), SKFont> Fonts = [];

    private static SKTypeface typeface = null!;
    private static SKTypeface typefaceBold = null!;

    private static SKPaint fillPaint = null!;
    private static SKPaint strokePaint = null!;

    private static SKPaint? backgroundPaint;
    private static SKShader? backgroundShader;
    private static int backgroundWidth;
    private static int backgroundHeight;

    public static void Initialize()
    {
        typeface = ResolveTypeface(false);
        typefaceBold = ResolveTypeface(true);

        fillPaint = new SKPaint { IsAntialias = true };
        strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
    }

    public static void Shutdown()
    {
        foreach (var font in Fonts.Values)
        {
            font.Dispose();
        }

        Fonts.Clear();

        backgroundPaint?.Dispose();
        backgroundPaint = null;
        backgroundShader?.Dispose();
        backgroundShader = null;

        fillPaint.Dispose();
        strokePaint.Dispose();
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

    //--------------------------------------------------------------------------------
    // Resource
    //--------------------------------------------------------------------------------

    public static SKFont GetFont(float size, bool bold = false)
    {
        if (!Fonts.TryGetValue((size, bold), out var font))
        {
            font = new SKFont(bold ? typefaceBold : typeface, size)
            {
                Edging = SKFontEdging.SubpixelAntialias
            };
            Fonts[(size, bold)] = font;
        }

        return font;
    }

    public static SKPaint GetFillPaint(SKColor color)
    {
        fillPaint.Color = color;
        return fillPaint;
    }

    public static SKPaint GetStrokePaint(SKColor color, float width, SKStrokeCap cap = SKStrokeCap.Butt)
    {
        strokePaint.Color = color;
        strokePaint.StrokeWidth = width;
        strokePaint.StrokeCap = cap;
        return strokePaint;
    }

    //--------------------------------------------------------------------------------
    // Measurement
    //--------------------------------------------------------------------------------

    public static float MeasureSubValueWidth(string text)
    {
        return GetFont(FontSize.SubValue, true).MeasureText(text);
    }

    //--------------------------------------------------------------------------------
    // Background / Panel
    //--------------------------------------------------------------------------------

    public static void DrawBackground(SKCanvas canvas, int width, int height)
    {
        if ((backgroundPaint is null) || (backgroundWidth != width) || (backgroundHeight != height))
        {
            backgroundPaint?.Dispose();
            backgroundShader?.Dispose();

            backgroundShader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [Colors.GradientStart, Colors.GradientEnd],
                null,
                SKShaderTileMode.Clamp);
            backgroundPaint = new SKPaint
            {
                Shader = backgroundShader, IsAntialias = true
            };

            backgroundWidth = width;
            backgroundHeight = height;
        }

        canvas.DrawRect(0, 0, width, height, backgroundPaint);
    }

    // Draws a card panel with rounded corners and border.
    public static void DrawPanel(SKCanvas canvas, SKRect rect)
    {
        canvas.DrawRoundRect(rect, Layout.PanelRadius, Layout.PanelRadius, GetFillPaint(Colors.PanelBackground));
        canvas.DrawRoundRect(rect, Layout.PanelRadius, Layout.PanelRadius, GetStrokePaint(Colors.PanelBorder, 1));
    }

    //--------------------------------------------------------------------------------
    // Text
    //--------------------------------------------------------------------------------

    public static void DrawTitle(SKCanvas canvas, SKRect rect, string title)
    {
        var font = GetFont(FontSize.WidgetTitle, true);
        canvas.DrawText(title, rect.Left + Layout.PaddingX, rect.Top + Layout.TitleOffsetY, font, GetFillPaint(Colors.TextPrimary));
    }

    public static void DrawValue(SKCanvas canvas, string text, float rightX, float y, SKColor color)
    {
        var font = GetFont(FontSize.PrimaryValue, true);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, GetFillPaint(color));
    }

    public static void DrawCenterValue(SKCanvas canvas, string text, float centerX, float y, SKColor color)
    {
        var font = GetFont(FontSize.GaugeValue, true);
        canvas.DrawText(text, centerX - (font.MeasureText(text) / 2f), y, font, GetFillPaint(color));
    }

    public static void DrawStackedValue(SKCanvas canvas, string label, string value, float x, float bottomY, SKColor valueColor)
    {
        var valueFont = GetFont(FontSize.SubValue, true);
        var labelFont = GetFont(FontSize.SubLabel);

        canvas.DrawText(value, x, bottomY, valueFont, GetFillPaint(valueColor));

        var labelY = bottomY + valueFont.Metrics.Ascent - labelFont.Metrics.Descent;
        canvas.DrawText(label, x, labelY, labelFont, GetFillPaint(Colors.TextSecondary));
    }

    public static void DrawStackedValueRight(SKCanvas canvas, string label, string value, float rightX, float bottomY, SKColor valueColor)
    {
        var valueFont = GetFont(FontSize.SubValue, true);
        var labelFont = GetFont(FontSize.SubLabel);

        canvas.DrawText(value, rightX - valueFont.MeasureText(value), bottomY, valueFont, GetFillPaint(valueColor));

        var labelY = bottomY + valueFont.Metrics.Ascent - labelFont.Metrics.Descent;
        canvas.DrawText(label, rightX - labelFont.MeasureText(label), labelY, labelFont, GetFillPaint(Colors.TextSecondary));
    }

    //--------------------------------------------------------------------------------
    // Gauge
    //--------------------------------------------------------------------------------

    public static void DrawRingGauge(SKCanvas canvas, float centerX, float centerY, float radius, float percentage, SKColor color)
    {
        var ringRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

        canvas.DrawArc(
            ringRect,
            Layout.RingStartAngle,
            Layout.RingArcDegrees,
            false,
            GetStrokePaint(Colors.TrackColor, Layout.RingStrokeWidth, SKStrokeCap.Round));

        canvas.DrawArc(
            ringRect,
            Layout.RingStartAngle,
            Layout.RingArcDegrees * percentage / 100f,
            false,
            GetStrokePaint(color, Layout.RingStrokeWidth, SKStrokeCap.Round));
    }

    //--------------------------------------------------------------------------------
    // Sparkline
    //--------------------------------------------------------------------------------

    public static void DrawSparkline(SKCanvas canvas, SKRect rect, RingBuffer buffer, float maxValue, SKColor color)
    {
        if (maxValue <= 0)
        {
            maxValue = 1;
        }

        var cap = buffer.Capacity;
        var stepX = rect.Width / (cap - 1);

        // Fill area
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

        canvas.DrawPath(areaPath, GetFillPaint(color.WithAlpha(30)));

        // Draw line
        using var linePath = new SKPath();
        linePath.MoveTo(rect.Left, rect.Bottom - (Math.Clamp(buffer[0] / maxValue, 0, 1) * rect.Height));
        for (var i = 1; i < cap; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Bottom - (Math.Clamp(buffer[i] / maxValue, 0, 1) * rect.Height);
            linePath.LineTo(x, y);
        }

        canvas.DrawPath(linePath, GetStrokePaint(color.WithAlpha(100), Layout.SparklineStrokeWidth));
    }

    public static void DrawSparklineInverted(SKCanvas canvas, SKRect rect, RingBuffer buffer, float maxValue, SKColor color)
    {
        if (maxValue <= 0)
        {
            maxValue = 1;
        }

        var cap = buffer.Capacity;
        var stepX = rect.Width / (cap - 1);

        // Fill area
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

        canvas.DrawPath(areaPath, GetFillPaint(color.WithAlpha(30)));

        // Draw line
        using var linePath = new SKPath();
        linePath.MoveTo(rect.Left, rect.Top + (Math.Clamp(buffer[0] / maxValue, 0, 1) * rect.Height));
        for (var i = 1; i < cap; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Top + (Math.Clamp(buffer[i] / maxValue, 0, 1) * rect.Height);
            linePath.LineTo(x, y);
        }

        canvas.DrawPath(linePath, GetStrokePaint(color.WithAlpha(100), Layout.SparklineStrokeWidth));
    }

    public static void DrawSparklineValues(
        SKCanvas canvas, float rightX, float areaTop, float areaBottom,
        string upperLabel, string upperValue, SKColor upperColor,
        string lowerLabel, string lowerValue, SKColor lowerColor)
    {
        var valFont = GetFont(FontSize.SubValue, true);
        var labelFont = GetFont(FontSize.SubLabel);

        var areaHeight = areaBottom - areaTop;
        var halfContent = (1f - Layout.SparklineSideCenterMarginRatio) / 2f;

        // Upper
        var upperAnchor = areaTop + (areaHeight * halfContent);
        var upperValueY = upperAnchor - valFont.Metrics.Descent;
        var upperLabelY = upperValueY + valFont.Metrics.Ascent - labelFont.Metrics.Descent;

        // The fill paint is shared, so its colour is set immediately before each draw
        canvas.DrawText(upperLabel, rightX - labelFont.MeasureText(upperLabel), upperLabelY, labelFont, GetFillPaint(Colors.TextSecondary));
        canvas.DrawText(upperValue, rightX - valFont.MeasureText(upperValue), upperValueY, valFont, GetFillPaint(upperColor));

        // Lower
        var lowerAnchor = areaBottom - (areaHeight * halfContent);
        var lowerLabelY = lowerAnchor - labelFont.Metrics.Ascent;
        var lowerValueY = lowerLabelY + labelFont.Metrics.Descent - valFont.Metrics.Ascent;

        canvas.DrawText(lowerLabel, rightX - labelFont.MeasureText(lowerLabel), lowerLabelY, labelFont, GetFillPaint(Colors.TextSecondary));
        canvas.DrawText(lowerValue, rightX - valFont.MeasureText(lowerValue), lowerValueY, valFont, GetFillPaint(lowerColor));
    }

    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    public static string FormatSpeed(double bytesPerSec)
    {
        if (bytesPerSec >= 1024 * 1024)
        {
            return $"{bytesPerSec / (1024.0 * 1024.0):0.0} MB/s";
        }

        return $"{bytesPerSec / 1024.0:0} KB/s";
    }
}
