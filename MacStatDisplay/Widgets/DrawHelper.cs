namespace MacStatDisplay.Widgets;

using SkiaSharp;

/// <summary>Provides shared rendering utilities for Draw4-style dashboard widgets.</summary>
internal sealed class DrawHelper : IDisposable
{
    // Draw4 color palette
    internal static readonly SKColor GradientStart = new(15, 20, 28);
    internal static readonly SKColor GradientEnd = new(8, 10, 16);
    internal static readonly SKColor PanelBg = new(18, 24, 34, 235);
    internal static readonly SKColor PanelBorder = new(255, 255, 255, 20);
    internal static readonly SKColor TrackColor = new(42, 48, 61);
    internal static readonly SKColor TextSub = new(148, 163, 184);
    internal static readonly SKColor HeaderSubtitle = new(151, 161, 176);
    internal static readonly SKColor AccentCyan = new(127, 225, 255);

    private const float PanelRadius = 16f;
    internal const float HeaderRadius = 18f;

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

    /// <summary>Draws the full-screen gradient background.</summary>
    internal static void DrawBackground(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [GradientStart, GradientEnd],
                null,
                SKShaderTileMode.Clamp),
            IsAntialias = true,
        };
        canvas.DrawRect(0, 0, width, height, paint);
    }

    /// <summary>Draws a card panel with rounded corners and border.</summary>
    internal void DrawPanel(SKCanvas canvas, SKRect rect)
    {
        using var bg = Fill(PanelBg);
        canvas.DrawRoundRect(rect, PanelRadius, PanelRadius, bg);

        using var border = Stroke(PanelBorder, 1);
        canvas.DrawRoundRect(rect, PanelRadius, PanelRadius, border);
    }

    /// <summary>Draws "CATEGORY Title" text at the top-left of a widget.</summary>
    internal void DrawTitleBlock(SKCanvas canvas, SKRect rect, string category, string title)
    {
        using var font = MakeFont(16f, true);
        using var paint = Fill(SKColors.White);
        var label = string.IsNullOrWhiteSpace(category) ? title : $"{category} {title}";
        canvas.DrawText(label, rect.Left + 14, rect.Top + 24, font, paint);
    }

    /// <summary>Draws a right-aligned value in 32pt bold.</summary>
    internal void DrawValue(SKCanvas canvas, string text, float rightX, float y, SKColor color)
    {
        using var font = MakeFont(32f, true);
        using var paint = Fill(color);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    /// <summary>Draws a centered value in 38pt bold (for ring gauge interior).</summary>
    internal void DrawCenteredValue(SKCanvas canvas, string text, float centerX, float y, SKColor color)
    {
        using var font = MakeFont(38f, true);
        using var paint = Fill(color);
        canvas.DrawText(text, centerX - (font.MeasureText(text) / 2f), y, font, paint);
    }

    /// <summary>Draws right-aligned detail text in 12pt.</summary>
    internal void DrawRightAlignedDetail(SKCanvas canvas, string text, float rightX, float y)
    {
        using var font = MakeFont(12f);
        using var paint = Fill(TextSub);
        canvas.DrawText(text, rightX - font.MeasureText(text), y, font, paint);
    }

    /// <summary>Draws word-wrapped detail text in 12pt (up to 2 lines).</summary>
    internal void DrawWrappedDetail(SKCanvas canvas, string text, float x, float y, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        using var font = MakeFont(12f);
        using var paint = Fill(TextSub);

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

    /// <summary>Draws a 270° ring gauge (open at bottom).</summary>
    internal void DrawRingGauge(SKCanvas canvas, float centerX, float centerY, float radius, float percentage, SKColor color)
    {
        using var trackPaint = new SKPaint
        {
            Color = TrackColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
        };

        using var valuePaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
        };

        var ringRect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(ringRect, 135, 270, false, trackPaint);
        canvas.DrawArc(ringRect, 135, 270f * percentage / 100f, false, valuePaint);
    }

    /// <summary>Draws a horizontal bar gauge at the bottom of a card.</summary>
    internal void DrawBarGauge(SKCanvas canvas, SKRect cardRect, float percentage, SKColor color)
    {
        var barRect = new SKRect(cardRect.Left + 16, cardRect.Bottom - 26, cardRect.Right - 16, cardRect.Bottom - 16);

        using var trackPaint = Fill(TrackColor);
        canvas.DrawRoundRect(barRect, 5, 5, trackPaint);

        var fillWidth = barRect.Width * percentage / 100f;
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        using var fillPaint = Fill(color);
        canvas.DrawRoundRect(fillRect, 5, 5, fillPaint);
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
