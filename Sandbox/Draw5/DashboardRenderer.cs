namespace Draw5;

using System.Globalization;
using SkiaSharp;

/// <summary>システムモニターダッシュボードを PNG 画像としてレンダリングする</summary>
internal sealed class DashboardRenderer : IDisposable
{
    const int ImgW = 1280;
    const int ImgH = 480;
    const int Margin = 3;
    const int HeaderH = 28;
    const int GridColumns = 4;
    const int GridRows = 4;
    const int GridGap = 3;
    const float CardRadius = 3f;

    static readonly SKColor BgColor = new(8, 10, 20);
    static readonly SKColor HeaderBg = new(14, 16, 30);
    static readonly SKColor CardBg = new(19, 22, 39);
    static readonly SKColor CardBorder = new(36, 40, 62);
    static readonly SKColor TrackColor = new(44, 47, 69);
    static readonly SKColor TextMain = new(230, 234, 244);
    static readonly SKColor TextSub = new(126, 132, 156);

    readonly SKTypeface _typeface = ResolveJapaneseTypeface();

    readonly record struct WidgetSpec(string Section, string Label, int Column, int Row, int ColumnSpan = 1, int RowSpan = 1);

    /// <summary>セクション一覧を描画し PNG として保存する</summary>
    public void Render(IReadOnlyList<MetricSection> sections, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(sections);
        ArgumentNullException.ThrowIfNull(outputPath);

        using var surface = SKSurface.Create(new SKImageInfo(ImgW, ImgH));
        var canvas = surface.Canvas;
        canvas.Clear(BgColor);

        DrawHeader(canvas);
        DrawWidgets(canvas, sections);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(outputPath);
        data.SaveTo(stream);
    }

    void DrawHeader(SKCanvas canvas)
    {
        using var bgPaint = Fill(HeaderBg);
        canvas.DrawRect(0, 0, ImgW, HeaderH, bgPaint);

        using var separator = Stroke(CardBorder, 1);
        canvas.DrawLine(0, HeaderH, ImgW, HeaderH, separator);

        using var titleFont = MakeFont(14f);
        using var titlePaint = Fill(new SKColor(112, 223, 255));
        canvas.DrawText("SYSTEM MONITOR", Margin + 2, 19, titleFont, titlePaint);

        using var infoFont = MakeFont(10f);
        using var infoPaint = Fill(TextSub);
        const string uptime = "Uptime 5d 12h 30m";
        canvas.DrawText(uptime, CenterX(infoFont, uptime, ImgW), 19, infoFont, infoPaint);

        var clock = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        canvas.DrawText(clock, ImgW - Margin - infoFont.MeasureText(clock) - 2, 19, infoFont, infoPaint);
    }

    void DrawWidgets(SKCanvas canvas, IReadOnlyList<MetricSection> sections)
    {
        int gridTop = HeaderH + GridGap;
        float cellWidth = (ImgW - (Margin * 2f) - ((GridColumns - 1) * GridGap)) / GridColumns;
        float cellHeight = (ImgH - gridTop - Margin - ((GridRows - 1) * GridGap)) / GridRows;

        foreach (var spec in BuildPriorityLayout())
        {
            if (!TryResolveMetric(sections, spec, out var section, out var item))
            {
                continue;
            }

            float x = Margin + (spec.Column * (cellWidth + GridGap));
            float y = gridTop + (spec.Row * (cellHeight + GridGap));
            float w = (cellWidth * spec.ColumnSpan) + (GridGap * (spec.ColumnSpan - 1));
            float h = (cellHeight * spec.RowSpan) + (GridGap * (spec.RowSpan - 1));

            DrawWidget(canvas, section, item, x, y, w, h);
        }
    }

    static IReadOnlyList<WidgetSpec> BuildPriorityLayout() =>
    [
        new("CPU", "CPU使用率", 0, 0, 1, 2),
        new("メモリ", "使用率", 1, 0, 1, 2),
        new("ディスク", "使用率", 2, 0, 2, 1),
        new("HWモニター", "CPU温度", 2, 1),
        new("HWモニター", "GPU温度", 3, 1),
        new("ネットワーク", "DL", 0, 2),
        new("ネットワーク", "UP", 1, 2),
        new("CPU", "Load 1m", 2, 2),
        new("プロセス", "プロセス数", 3, 2),
        new("CPU", "ユーザー", 0, 3),
        new("CPU", "システム", 1, 3),
        new("HWモニター", "電力合計", 2, 3),
        new("プロセス", "スレッド数", 3, 3),
    ];

    bool TryResolveMetric(
        IReadOnlyList<MetricSection> sections,
        WidgetSpec spec,
        out MetricSection section,
        out MetricItem item)
    {
        section = sections.FirstOrDefault(s => s.Name == spec.Section)
            ?? throw new InvalidOperationException($"Section '{spec.Section}' was not found.");
        item = section.Items.FirstOrDefault(i => i.Label == spec.Label)
            ?? throw new InvalidOperationException($"Metric '{spec.Label}' was not found in section '{spec.Section}'.");
        return true;
    }

    void DrawWidget(SKCanvas canvas, MetricSection section, MetricItem item, float x, float y, float w, float h)
    {
        var rect = new SKRect(x, y, x + w, y + h);
        using var cardPaint = Fill(CardBg);
        canvas.DrawRoundRect(rect, CardRadius, CardRadius, cardPaint);

        using var borderPaint = Stroke(CardBorder, 1f);
        canvas.DrawRoundRect(rect, CardRadius, CardRadius, borderPaint);

        DrawSectionBadge(canvas, section, x + 5, y + 4);

        if (item.Type == WidgetType.CircularGauge)
        {
            DrawGaugeWidget(canvas, section, item, rect);
            return;
        }

        if (item.Type == WidgetType.BarGauge)
        {
            DrawBarWidget(canvas, section, item, rect);
            return;
        }

        DrawStatWidget(canvas, section, item, rect);
    }

    void DrawSectionBadge(SKCanvas canvas, MetricSection section, float x, float y)
    {
        using var badgePaint = Fill(section.AccentColor.WithAlpha(34));
        var badgeRect = new SKRect(x, y, x + 52, y + 15);
        canvas.DrawRoundRect(badgeRect, 7, 7, badgePaint);

        using var badgeFont = MakeFont(8.5f);
        using var badgeTextPaint = Fill(section.AccentColor);
        canvas.DrawText(section.Name, x + 5, y + 11.5f, badgeFont, badgeTextPaint);
    }

    void DrawGaugeWidget(SKCanvas canvas, MetricSection section, MetricItem item, SKRect rect)
    {
        var valueColor = ResolveValueColor(section, item);
        var valueText = FormatValue(item);
        var labelLines = SplitLabel(item.Label, rect.Width - 132, 11f);
        float labelY = rect.Top + 32;

        using var labelFont = MakeFont(11f);
        using var labelPaint = Fill(TextSub);
        for (int i = 0; i < labelLines.Length; i++)
        {
            canvas.DrawText(labelLines[i], rect.Left + 8, labelY + (i * 13), labelFont, labelPaint);
        }

        using var valueFont = MakeFont(34f);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(valueText, rect.Right - 8 - valueFont.MeasureText(valueText), rect.Top + 42, valueFont, valuePaint);

        float percentage = TryParseNumber(item.Value, out var rawValue) ? Math.Clamp(rawValue, 0f, 100f) : 0f;
        float centerX = rect.MidX;
        float centerY = rect.MidY + 18;
        float radius = Math.Min(rect.Width, rect.Height) * 0.27f;

        using var trackPaint = new SKPaint
        {
            Color = TrackColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
        };
        canvas.DrawCircle(centerX, centerY, radius, trackPaint);

        using var arcPath = new SKPath();
        arcPath.AddArc(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius), -90, 360f * percentage / 100f);

        using var glowPaint = new SKPaint
        {
            Color = valueColor.WithAlpha(36),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 14,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5),
        };
        canvas.DrawPath(arcPath, glowPaint);

        using var arcPaint = new SKPaint
        {
            Color = valueColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = 10,
        };
        canvas.DrawPath(arcPath, arcPaint);

        if (!string.IsNullOrWhiteSpace(item.Detail))
        {
            using var detailFont = MakeFont(9.5f);
            using var detailPaint = Fill(TextSub);
            canvas.DrawText(item.Detail, rect.Left + 8, rect.Bottom - 8, detailFont, detailPaint);
        }
    }

    void DrawBarWidget(SKCanvas canvas, MetricSection section, MetricItem item, SKRect rect)
    {
        var valueColor = ResolveValueColor(section, item);
        var valueText = FormatValue(item);
        var labelLines = SplitLabel($"{section.Name} {item.Label}", rect.Width - 150, 11f);

        using var labelFont = MakeFont(11f);
        using var labelPaint = Fill(TextSub);
        for (int i = 0; i < labelLines.Length; i++)
        {
            canvas.DrawText(labelLines[i], rect.Left + 8, rect.Top + 28 + (i * 13), labelFont, labelPaint);
        }

        using var valueFont = MakeFont(34f);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(valueText, rect.Right - 8 - valueFont.MeasureText(valueText), rect.Top + 31, valueFont, valuePaint);

        float percentage = TryParseNumber(item.Value, out var rawValue) ? Math.Clamp(rawValue, 0f, 100f) : 0f;
        var trackRect = new SKRect(rect.Left + 8, rect.Top + 48, rect.Right - 8, rect.Top + 61);
        using var trackPaint = Fill(TrackColor);
        canvas.DrawRoundRect(trackRect, 4, 4, trackPaint);

        var fillRect = new SKRect(trackRect.Left, trackRect.Top, trackRect.Left + (trackRect.Width * (percentage / 100f)), trackRect.Bottom);
        using var fillPaint = Fill(valueColor);
        canvas.DrawRoundRect(fillRect, 4, 4, fillPaint);

        if (!string.IsNullOrWhiteSpace(item.Detail))
        {
            using var detailFont = MakeFont(9.5f);
            using var detailPaint = Fill(TextSub);
            canvas.DrawText(item.Detail, rect.Left + 8, rect.Bottom - 8, detailFont, detailPaint);
        }
    }

    void DrawStatWidget(SKCanvas canvas, MetricSection section, MetricItem item, SKRect rect)
    {
        var valueColor = ResolveValueColor(section, item);
        var valueText = FormatValue(item);
        var labelLines = SplitLabel(item.Label, rect.Width - 132, 11f);

        using var labelFont = MakeFont(11f);
        using var labelPaint = Fill(TextSub);
        for (int i = 0; i < labelLines.Length; i++)
        {
            canvas.DrawText(labelLines[i], rect.Left + 8, rect.Top + 32 + (i * 13), labelFont, labelPaint);
        }

        using var valueFont = MakeFont(36f);
        using var valuePaint = Fill(valueColor);
        canvas.DrawText(valueText, rect.Right - 8 - valueFont.MeasureText(valueText), rect.MidY + 8, valueFont, valuePaint);

        if (!string.IsNullOrWhiteSpace(item.Detail))
        {
            using var detailFont = MakeFont(9f);
            using var detailPaint = Fill(TextSub);
            canvas.DrawText(item.Detail, rect.Left + 8, rect.Bottom - 7, detailFont, detailPaint);
        }
    }

    string FormatValue(MetricItem item) => string.IsNullOrEmpty(item.Unit)
        ? item.Value
        : $"{item.Value}{(item.Unit == "%" || item.Unit == "℃" ? string.Empty : " ")}{item.Unit}";

    SKColor ResolveValueColor(MetricSection section, MetricItem item)
    {
        if (!TryParseNumber(item.Value, out var value))
        {
            return section.AccentColor;
        }

        if (item.Unit == "%")
        {
            return value switch
            {
                >= 85 => new SKColor(255, 120, 120),
                >= 65 => new SKColor(255, 196, 92),
                _ => section.AccentColor,
            };
        }

        if (item.Unit == "℃")
        {
            return value switch
            {
                >= 80 => new SKColor(255, 96, 96),
                >= 65 => new SKColor(255, 168, 72),
                _ => new SKColor(255, 210, 120),
            };
        }

        if (item.Unit == "W")
        {
            return new SKColor(255, 186, 106);
        }

        if (section.Name == "ネットワーク")
        {
            return new SKColor(100, 226, 180);
        }

        return section.AccentColor;
    }

    string[] SplitLabel(string label, float maxWidth, float fontSize)
    {
        using var font = MakeFont(fontSize);
        if (font.MeasureText(label) <= maxWidth)
        {
            return [label];
        }

        for (int i = 1; i < label.Length; i++)
        {
            var left = label[..i];
            var right = label[i..];
            if (font.MeasureText(left) <= maxWidth && font.MeasureText(right) <= maxWidth)
            {
                return [left, right];
            }
        }

        int mid = label.Length / 2;
        return [label[..mid], label[mid..]];
    }

    static bool TryParseNumber(string text, out float value)
    {
        var normalized = text.Replace(",", string.Empty, StringComparison.Ordinal);
        return float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    SKFont MakeFont(float size) => new(_typeface, size) { Edging = SKFontEdging.SubpixelAntialias };

    static SKPaint Fill(SKColor color) => new() { Color = color, IsAntialias = true };

    static SKPaint Stroke(SKColor color, float width) => new()
    {
        Color = color,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = width,
    };

    static float CenterX(SKFont font, string text, float areaWidth) => (areaWidth - font.MeasureText(text)) / 2f;

    static SKTypeface ResolveJapaneseTypeface()
    {
        string[] families =
        [
            "Yu Gothic UI", "Meiryo", "MS Gothic",
            "Hiragino Sans", "Hiragino Kaku Gothic ProN",
            "Noto Sans CJK JP", "Noto Sans JP",
        ];

        foreach (var name in families)
        {
            var typeface = SKTypeface.FromFamilyName(name);
            if (typeface is not null && typeface.FamilyName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return typeface;
            }

            typeface?.Dispose();
        }

        return SKTypeface.Default;
    }

    public void Dispose() => _typeface.Dispose();
}
