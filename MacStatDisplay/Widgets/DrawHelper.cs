namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Provides shared rendering utilities for dashboard widgets.</summary>
internal sealed class DrawHelper : IDisposable
{
    internal static readonly SKColor BgColor = new(8, 10, 20);
    internal static readonly SKColor HeaderBg = new(14, 16, 30);
    internal static readonly SKColor CardBg = new(19, 22, 39);
    internal static readonly SKColor CardBorder = new(36, 40, 62);
    internal static readonly SKColor TrackColor = new(44, 47, 69);
    internal static readonly SKColor TextSub = new(126, 132, 156);

    private const float CardRadius = 3f;

    private readonly SKTypeface typeface;

    internal DrawHelper()
    {
        typeface = ResolveTypeface();
    }

    /// <summary>Creates a font with the resolved typeface.</summary>
    internal SKFont MakeFont(float size) => new(typeface, size) { Edging = SKFontEdging.SubpixelAntialias };

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

    /// <summary>Draws a card background with rounded corners and border.</summary>
    internal void DrawCard(SKCanvas canvas, SKRect rect)
    {
        using var cardPaint = Fill(CardBg);
        canvas.DrawRoundRect(rect, CardRadius, CardRadius, cardPaint);

        using var borderPaint = Stroke(CardBorder, 1f);
        canvas.DrawRoundRect(rect, CardRadius, CardRadius, borderPaint);
    }

    /// <summary>Draws a colored section badge.</summary>
    internal void DrawBadge(SKCanvas canvas, string text, SKColor accentColor, float x, float y)
    {
        using var font = MakeFont(8.5f);
        var textWidth = font.MeasureText(text);
        var badgeWidth = Math.Max(textWidth + 10, 30);

        using var bgPaint = Fill(accentColor.WithAlpha(34));
        var rect = new SKRect(x, y, x + badgeWidth, y + 15);
        canvas.DrawRoundRect(rect, 7, 7, bgPaint);

        using var textPaint = Fill(accentColor);
        canvas.DrawText(text, x + 5, y + 11.5f, font, textPaint);
    }

    /// <summary>Draws a label that may wrap to two lines.</summary>
    internal void DrawLabel(SKCanvas canvas, string label, float x, float y, float maxWidth)
    {
        using var font = MakeFont(11f);
        using var paint = Fill(TextSub);
        var lines = SplitLabel(label, maxWidth, font);
        for (var i = 0; i < lines.Length; i++)
        {
            canvas.DrawText(lines[i], x, y + (i * 13), font, paint);
        }
    }

    /// <summary>Draws a large right-aligned value text.</summary>
    internal void DrawLargeValue(SKCanvas canvas, string text, float rightX, float y, SKColor color, float fontSize = 34f)
    {
        using var font = MakeFont(fontSize);
        using var paint = Fill(color);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    /// <summary>Draws a small detail text.</summary>
    internal void DrawDetail(SKCanvas canvas, string text, float x, float y)
    {
        using var font = MakeFont(9.5f);
        using var paint = Fill(TextSub);
        canvas.DrawText(text, x, y, font, paint);
    }

    /// <summary>Draws a circular gauge with track, arc, and glow.</summary>
    internal void DrawCircularGauge(SKCanvas canvas, float centerX, float centerY, float radius, float percentage, SKColor color)
    {
        using var trackPaint = new SKPaint
        {
            Color = TrackColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
        };
        canvas.DrawCircle(centerX, centerY, radius, trackPaint);

        using var arcPath = new SKPath();
        var arcRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        arcPath.AddArc(arcRect, -90, 360f * percentage / 100f);

        using var glowPaint = new SKPaint
        {
            Color = color.WithAlpha(36),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 14,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5),
        };
        canvas.DrawPath(arcPath, glowPaint);

        using var arcPaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = 10,
        };
        canvas.DrawPath(arcPath, arcPaint);
    }

    /// <summary>Draws a horizontal bar gauge.</summary>
    internal void DrawBarGauge(SKCanvas canvas, SKRect trackRect, float percentage, SKColor color)
    {
        using var trackPaint = Fill(TrackColor);
        canvas.DrawRoundRect(trackRect, 4, 4, trackPaint);

        var fillWidth = trackRect.Width * (percentage / 100f);
        var fillRect = new SKRect(trackRect.Left, trackRect.Top, trackRect.Left + fillWidth, trackRect.Bottom);
        using var fillPaint = Fill(color);
        canvas.DrawRoundRect(fillRect, 4, 4, fillPaint);
    }

    /// <summary>Resolves the value display color for percentage metrics.</summary>
    internal static SKColor ResolvePercentColor(double value, SKColor accent) => value switch
    {
        >= 85 => new SKColor(255, 120, 120),
        >= 65 => new SKColor(255, 196, 92),
        _ => accent,
    };

    /// <summary>Resolves the value display color for temperature metrics.</summary>
    internal static SKColor ResolveTempColor(double value) => value switch
    {
        >= 80 => new SKColor(255, 96, 96),
        >= 65 => new SKColor(255, 168, 72),
        _ => new SKColor(255, 210, 120),
    };

    private string[] SplitLabel(string label, float maxWidth, SKFont font)
    {
        if (font.MeasureText(label) <= maxWidth)
        {
            return [label];
        }

        for (var i = 1; i < label.Length; i++)
        {
            var left = label[..i];
            var right = label[i..];
            if (font.MeasureText(left) <= maxWidth && font.MeasureText(right) <= maxWidth)
            {
                return [left, right];
            }
        }

        var mid = label.Length / 2;
        return [label[..mid], label[mid..]];
    }

    private static SKTypeface ResolveTypeface()
    {
        string[] families =
        [
            "Yu Gothic UI", "Meiryo", "MS Gothic",
            "Hiragino Sans", "Hiragino Kaku Gothic ProN",
            "Noto Sans CJK JP", "Noto Sans JP",
        ];

        foreach (var name in families)
        {
            var tf = SKTypeface.FromFamilyName(name);
            if (tf is not null && tf.FamilyName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return tf;
            }

            tf?.Dispose();
        }

        return SKTypeface.Default;
    }

    public void Dispose() => typeface.Dispose();
}
