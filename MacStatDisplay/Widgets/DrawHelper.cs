namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Provides shared rendering utilities for dashboard widgets.</summary>
internal sealed class DrawHelper : IDisposable
{
    private readonly SKTypeface typeface;
    private readonly SKTypeface typefaceBold;

    internal DrawHelper()
    {
        typeface = ResolveTypeface(false);
        typefaceBold = ResolveTypeface(true);
    }

    /// <summary>Creates a font with the resolved typeface.</summary>
    internal SKFont MakeFont(float size, bool bold = false) =>
        new(bold ? typefaceBold : typeface, size) { Edging = SKFontEdging.SubpixelAntialias };

    /// <summary>Creates a fill paint.</summary>
    internal static SKPaint Fill(SKColor color) => new() { Color = color, IsAntialias = true };

    /// <summary>Creates a stroke paint.</summary>
    internal static SKPaint Stroke(SKColor color, float width) => new()
    {
        Color = color,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = width,
    };

    /// <summary>Formats bytes/sec into a human-readable speed string.</summary>
    internal static string FormatSpeed(double bytesPerSec)
    {
        if (bytesPerSec >= 1024 * 1024)
        {
            return $"{bytesPerSec / (1024.0 * 1024.0):0.0} MB/s";
        }

        return $"{bytesPerSec / 1024.0:0} KB/s";
    }

    /// <summary>Draws the full-screen gradient background.</summary>
    internal static void DrawBackground(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [WidgetTheme.GradientStart, WidgetTheme.GradientEnd],
                null,
                SKShaderTileMode.Clamp),
            IsAntialias = true,
        };
        canvas.DrawRect(0, 0, width, height, paint);
    }

    /// <summary>Draws a card panel with rounded corners and border.</summary>
    internal void DrawPanel(SKCanvas canvas, SKRect rect)
    {
        using var bg = Fill(WidgetTheme.PanelBg);
        canvas.DrawRoundRect(rect, WidgetTheme.PanelRadius, WidgetTheme.PanelRadius, bg);

        using var border = Stroke(WidgetTheme.PanelBorder, 1);
        canvas.DrawRoundRect(rect, WidgetTheme.PanelRadius, WidgetTheme.PanelRadius, border);
    }

    /// <summary>Draws "CATEGORY Title" text at the top-left of a widget.</summary>
    internal void DrawTitleBlock(SKCanvas canvas, SKRect rect, string category, string title)
    {
        using var font = MakeFont(WidgetTheme.TitleFontSize, true);
        using var paint = Fill(WidgetTheme.TextPrimary);
        var label = string.IsNullOrWhiteSpace(category) ? title : $"{category} {title}";
        canvas.DrawText(label, rect.Left + WidgetTheme.PadX, rect.Top + WidgetTheme.TitleOffsetY, font, paint);
    }

    /// <summary>Draws a right-aligned value in large bold font.</summary>
    internal void DrawValue(SKCanvas canvas, string text, float rightX, float y, SKColor color)
    {
        using var font = MakeFont(WidgetTheme.ValueLargeFontSize, true);
        using var paint = Fill(color);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    /// <summary>Draws a centered value in large bold font (for ring gauge interior).</summary>
    internal void DrawCenteredValue(SKCanvas canvas, string text, float centerX, float y, SKColor color)
    {
        using var font = MakeFont(WidgetTheme.CenterValueFontSize, true);
        using var paint = Fill(color);
        canvas.DrawText(text, centerX - (font.MeasureText(text) / 2f), y, font, paint);
    }

    /// <summary>Draws right-aligned detail text.</summary>
    internal void DrawRightAlignedDetail(SKCanvas canvas, string text, float rightX, float y)
    {
        using var font = MakeFont(WidgetTheme.DetailFontSize);
        using var paint = Fill(WidgetTheme.TextSub);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    /// <summary>Draws word-wrapped detail text (up to 2 lines).</summary>
    internal void DrawWrappedDetail(SKCanvas canvas, string text, float x, float y, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        using var font = MakeFont(WidgetTheme.DetailFontSize);
        using var paint = Fill(WidgetTheme.TextSub);

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var line = string.Empty;
        var lineIndex = 0;

        foreach (var word in words)
        {
            var candidate = string.IsNullOrEmpty(line) ? word : $"{line} {word}";
            if (font.MeasureText(candidate) <= maxWidth)
            {
                line = candidate;
                continue;
            }

            canvas.DrawText(line, x, y + (lineIndex * 14), font, paint);
            line = word;
            lineIndex++;
            if (lineIndex >= 2)
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(line) && lineIndex < 2)
        {
            canvas.DrawText(line, x, y + (lineIndex * 14), font, paint);
        }
    }

    /// <summary>Draws a left-aligned label on one line and a colored value below it.</summary>
    internal void DrawStackedLabelValue(SKCanvas canvas, string label, string value, float x, float y, SKColor valueColor)
    {
        using var labelFont = MakeFont(WidgetTheme.SmallFontSize);
        using var labelPaint = Fill(WidgetTheme.TextSub);
        canvas.DrawText(label, x, y, labelFont, labelPaint);

        using var valueFont = MakeFont(WidgetTheme.DetailFontSize, true);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(value, x, y + 16, valueFont, valuePaint);
    }

    /// <summary>Draws a right-aligned label on one line and a colored value below it.</summary>
    internal void DrawStackedLabelValueRight(SKCanvas canvas, string label, string value, float rightX, float y, SKColor valueColor)
    {
        using var labelFont = MakeFont(WidgetTheme.SmallFontSize);
        using var labelPaint = Fill(WidgetTheme.TextSub);
        canvas.DrawText(label, rightX - labelFont.MeasureText(label), y, labelFont, labelPaint);

        using var valueFont = MakeFont(WidgetTheme.DetailFontSize, true);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(value, rightX - valueFont.MeasureText(value), y + 16, valueFont, valuePaint);
    }

    /// <summary>Draws a 270° ring gauge (open at bottom).</summary>
    internal void DrawRingGauge(SKCanvas canvas, float centerX, float centerY, float radius, float percentage, SKColor color)
    {
        using var trackPaint = new SKPaint
        {
            Color = WidgetTheme.TrackColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = WidgetTheme.RingStrokeWidth,
            StrokeCap = SKStrokeCap.Round,
        };

        using var valuePaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = WidgetTheme.RingStrokeWidth,
            StrokeCap = SKStrokeCap.Round,
        };

        var ringRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(ringRect, WidgetTheme.RingStartAngle, WidgetTheme.RingArcDegrees, false, trackPaint);
        canvas.DrawArc(ringRect, WidgetTheme.RingStartAngle, WidgetTheme.RingArcDegrees * percentage / 100f, false, valuePaint);
    }

    /// <summary>Draws a horizontal bar gauge at the bottom of a card.</summary>
    internal void DrawBarGauge(SKCanvas canvas, SKRect cardRect, float percentage, SKColor color)
    {
        var barRect = new SKRect(
            cardRect.Left + WidgetTheme.PadX,
            cardRect.Bottom - WidgetTheme.BarHeight - WidgetTheme.PadY,
            cardRect.Right - WidgetTheme.PadX,
            cardRect.Bottom - WidgetTheme.PadY);

        using var trackPaint = Fill(WidgetTheme.TrackColor);
        canvas.DrawRoundRect(barRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, trackPaint);

        var fillWidth = barRect.Width * percentage / 100f;
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        using var fillPaint = Fill(color);
        canvas.DrawRoundRect(fillRect, WidgetTheme.BarRadius, WidgetTheme.BarRadius, fillPaint);
    }

    private static SKTypeface ResolveTypeface(bool bold)
    {
        string[] families =
        [
            "Yu Gothic UI", "Meiryo", "MS Gothic",
            "Hiragino Sans", "Hiragino Kaku Gothic ProN",
            "Noto Sans CJK JP", "Noto Sans JP",
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

    public void Dispose()
    {
        typeface.Dispose();
        typefaceBold.Dispose();
    }
}
