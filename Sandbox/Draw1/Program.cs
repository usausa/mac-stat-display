using SkiaSharp;

const int width = 1280;
const int height = 480;
const string outputPath = "output.png";

var dashboard = new DashboardRenderer(width, height);
dashboard.Render(outputPath);

return;

file sealed class DashboardRenderer(int width, int height)
{
    private static readonly SKColor Background = new(3, 10, 22);
    private static readonly SKColor PanelFill = new(10, 23, 38, 220);
    private static readonly SKColor LabelFill = new(12, 38, 58, 235);
    private static readonly SKColor PanelStroke = new(76, 233, 255, 180);
    private static readonly SKColor Accent = new(0, 245, 255);
    private static readonly SKColor AccentSoft = new(120, 255, 247);
    private static readonly SKColor TextPrimary = new(225, 250, 255);
    private static readonly SKColor TextSecondary = new(125, 187, 208);
    private static readonly SKColor Warning = new(255, 176, 72);
    private static readonly SKColor Danger = new(255, 94, 122);
    private static readonly SKColor Good = new(110, 255, 184);

    private readonly IReadOnlyList<WidgetData> _widgets =
    [
        new(WidgetKind.Title, new SKRect(16, 12, 224, 62), "MAC STATUS", "Variable widget layout", Accent, 0f),
        new(WidgetKind.Info, new SKRect(16, 68, 224, 108), "UPTIME", "03d 14h 22m", Good, 0f),
        new(WidgetKind.Info, new SKRect(16, 112, 224, 152), "LOCAL TIME", "20:41:36", TextPrimary, 0f),
        new(WidgetKind.Info, new SKRect(16, 156, 224, 196), "PROCESSES", "412", TextPrimary, 0f),
        new(WidgetKind.Info, new SKRect(16, 200, 224, 240), "THREADS", "6489", TextPrimary, 0f),
        new(WidgetKind.Info, new SKRect(16, 244, 224, 284), "CPU TEMP", "63 C", Warning, 0f),
        new(WidgetKind.Info, new SKRect(16, 288, 224, 328), "GPU TEMP", "57 C", AccentSoft, 0f),

        new(WidgetKind.Label, new SKRect(236, 12, 390, 46), "CPU", "CORE TELEMETRY", TextPrimary, 0f),
        new(WidgetKind.Label, new SKRect(398, 12, 552, 46), "MEMORY", "PRESSURE / CACHE", TextPrimary, 0f),
        new(WidgetKind.Label, new SKRect(560, 12, 714, 46), "NETWORK", "TRANSFER RATE", TextPrimary, 0f),

        new(WidgetKind.Gauge, new SKRect(236, 52, 390, 164), "CPU Usage", "42%", Accent, 0.42f),
        new(WidgetKind.Gauge, new SKRect(398, 52, 552, 164), "Memory Usage", "68%", Warning, 0.68f),
        new(WidgetKind.Gauge, new SKRect(560, 52, 714, 164), "Network Load", "51%", AccentSoft, 0.51f),

        new(WidgetKind.Timeline, new SKRect(236, 170, 390, 282), "CPU History", "60 sec", Accent, 0.42f),
        new(WidgetKind.Timeline, new SKRect(398, 170, 552, 282), "Memory History", "60 sec", Warning, 0.68f),
        new(WidgetKind.NetworkTimeline, new SKRect(560, 170, 714, 282), "Traffic", "DL / UP", Accent, 0.51f),

        new(WidgetKind.Label, new SKRect(722, 12, 872, 46), "STORAGE", "IO / CAPACITY", TextPrimary, 0f),
        new(WidgetKind.Label, new SKRect(880, 12, 1048, 46), "POWER", "SUPPLY / LOAD", TextPrimary, 0f),
        new(WidgetKind.Label, new SKRect(1056, 12, 1256, 46), "CPU DETAIL", "LOAD COMPONENTS", TextPrimary, 0f),

        new(WidgetKind.Metric, new SKRect(236, 292, 390, 334), "CPU System", "11%", TextSecondary, 0f),
        new(WidgetKind.Metric, new SKRect(236, 338, 390, 380), "CPU User", "24%", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(236, 384, 390, 426), "CPU Idle", "58%", Good, 0f),
        new(WidgetKind.Metric, new SKRect(236, 430, 390, 472), "CPU E-Core", "37%", AccentSoft, 0f),

        new(WidgetKind.Metric, new SKRect(398, 292, 552, 334), "CPU P-Core", "61%", Warning, 0f),
        new(WidgetKind.Metric, new SKRect(398, 338, 552, 380), "CPU Freq", "3560 MHz", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(398, 384, 552, 426), "CPU E/P Freq", "2840 / 4012 MHz", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(398, 430, 552, 472), "Load 1/5", "3.12 / 2.84", TextPrimary, 0f),

        new(WidgetKind.Metric, new SKRect(560, 292, 714, 334), "Download", "18.4 MB/s", Accent, 0f),
        new(WidgetKind.Metric, new SKRect(560, 338, 714, 380), "Upload", "2.6 MB/s", AccentSoft, 0f),
        new(WidgetKind.Metric, new SKRect(560, 384, 714, 426), "Used", "21.8 GB", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(560, 430, 714, 472), "Application", "14.2 GB", AccentSoft, 0f),

        new(WidgetKind.BarGauge, new SKRect(722, 52, 872, 102), "Disk Usage", "74%", Warning, 0.74f),
        new(WidgetKind.Metric, new SKRect(722, 106, 872, 148), "Disk Free", "468 GB", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(722, 152, 872, 194), "Disk Total", "1.8 TB", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(722, 198, 872, 240), "Disk Read", "824 KB/s", AccentSoft, 0f),
        new(WidgetKind.Metric, new SKRect(722, 244, 872, 286), "Disk Write", "391 KB/s", Accent, 0f),
        new(WidgetKind.Metric, new SKRect(722, 292, 872, 334), "Wired", "4.7 GB", TextSecondary, 0f),
        new(WidgetKind.Metric, new SKRect(722, 338, 872, 380), "Swap", "512 MB", Danger, 0f),

        new(WidgetKind.Metric, new SKRect(880, 52, 1048, 94), "System Power", "214 W", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(880, 98, 1048, 140), "CPU Power", "88 W", Accent, 0f),
        new(WidgetKind.Metric, new SKRect(880, 144, 1048, 186), "GPU Power", "126 W", Danger, 0f),
        new(WidgetKind.Metric, new SKRect(880, 190, 1048, 232), "Load 15", "2.41", TextPrimary, 0f),

        new(WidgetKind.Metric, new SKRect(1056, 52, 1256, 94), "Load Avg", "3.12 / 2.84 / 2.41", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(1056, 98, 1256, 140), "CPU System/User", "11 / 24 %", TextPrimary, 0f),
        new(WidgetKind.Metric, new SKRect(1056, 144, 1256, 186), "CPU Idle", "58%", Good, 0f),
        new(WidgetKind.Metric, new SKRect(1056, 190, 1256, 232), "CPU E/P", "37 / 61 %", Warning, 0f)
    ];

    public void Render(string outputPath)
    {
        ArgumentNullException.ThrowIfNull(outputPath);

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(Background);
        DrawBackground(canvas);

        foreach (var widget in _widgets)
        {
            DrawWidget(canvas, widget);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        data.SaveTo(stream);
    }

    private void DrawBackground(SKCanvas canvas)
    {
        using var gradient = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [new SKColor(4, 11, 27), new SKColor(8, 24, 38), new SKColor(2, 7, 18)],
                [0f, 0.55f, 1f],
                SKShaderTileMode.Clamp),
            IsAntialias = true
        };

        canvas.DrawRect(new SKRect(0, 0, width, height), gradient);

        using var grid = new SKPaint
        {
            Color = new SKColor(90, 220, 255, 18),
            StrokeWidth = 1,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        for (var x = 0; x <= width; x += 32)
        {
            canvas.DrawLine(x, 0, x, height, grid);
        }

        for (var y = 0; y <= height; y += 32)
        {
            canvas.DrawLine(0, y, width, y, grid);
        }
    }

    private void DrawWidget(SKCanvas canvas, WidgetData widget)
    {
        var rect = widget.Bounds;

        if (widget.Kind == WidgetKind.Label)
        {
            DrawLabelWidget(canvas, widget);
            return;
        }

        using var shadowPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 90),
            IsAntialias = true,
            ImageFilter = SKImageFilter.CreateDropShadow(0, 8, 18, 18, new SKColor(0, 0, 0, 80))
        };

        using var fillPaint = new SKPaint
        {
            Color = PanelFill,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var strokePaint = new SKPaint
        {
            Color = PanelStroke,
            StrokeWidth = 1.6f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        var rounded = new SKRoundRect(rect, 8, 8);
        canvas.DrawRoundRect(rounded, shadowPaint);
        canvas.DrawRoundRect(rounded, fillPaint);
        canvas.DrawRoundRect(rounded, strokePaint);

        switch (widget.Kind)
        {
            case WidgetKind.Title:
                DrawTitleWidget(canvas, widget);
                break;
            case WidgetKind.Label:
                break;
            case WidgetKind.Info:
                DrawInfoWidget(canvas, widget);
                break;
            case WidgetKind.Metric:
                DrawMetricWidget(canvas, widget);
                break;
            case WidgetKind.Gauge:
                DrawGaugeWidget(canvas, widget);
                break;
            case WidgetKind.BarGauge:
                DrawBarGaugeWidget(canvas, widget);
                break;
            case WidgetKind.Timeline:
                DrawTimelineWidget(canvas, widget);
                break;
            case WidgetKind.NetworkTimeline:
                DrawNetworkTimelineWidget(canvas, widget);
                break;
        }
    }

    private void DrawTitleWidget(SKCanvas canvas, WidgetData widget)
    {
        using var titlePaint = CreateTextPaint(24, widget.Color, true);
        using var subtitlePaint = CreateTextPaint(11, TextSecondary, false);
        canvas.DrawText(widget.Title, widget.Bounds.Left + 12, widget.Bounds.Top + 34, titlePaint);
        canvas.DrawText(widget.Value, widget.Bounds.Left + 12, widget.Bounds.Top + 52, subtitlePaint);
    }

    private void DrawLabelWidget(SKCanvas canvas, WidgetData widget)
    {
        using var fillPaint = new SKPaint
        {
            Color = LabelFill,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var linePaint = new SKPaint
        {
            Color = widget.Color.WithAlpha(90),
            StrokeWidth = 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var titlePaint = CreateTextPaint(18, TextPrimary, true);
        using var subtitlePaint = CreateTextPaint(11, TextSecondary, false);

        canvas.DrawRoundRect(new SKRoundRect(widget.Bounds, 6, 6), fillPaint);
        canvas.DrawLine(widget.Bounds.Left + 10, widget.Bounds.MidY, widget.Bounds.Left + 34, widget.Bounds.MidY, linePaint);
        canvas.DrawText(widget.Title, widget.Bounds.Left + 40, widget.Bounds.Top + 29, titlePaint);
        canvas.DrawText(widget.Value, widget.Bounds.Left + 40, widget.Bounds.Top + 44, subtitlePaint);
    }

    private void DrawInfoWidget(SKCanvas canvas, WidgetData widget)
    {
        using var labelPaint = CreateTextPaint(11, TextSecondary, false);
        using var valuePaint = CreateTextPaint(20, widget.Color, true);
        DrawTitleValueWidget(canvas, widget, labelPaint, valuePaint);
    }

    private void DrawMetricWidget(SKCanvas canvas, WidgetData widget)
    {
        using var labelPaint = CreateTextPaint(11, TextSecondary, false);
        using var valuePaint = CreateTextPaint(17, widget.Color, true);
        DrawTitleValueWidget(canvas, widget, labelPaint, valuePaint);
    }

    private void DrawGaugeWidget(SKCanvas canvas, WidgetData widget)
    {
        DrawGauge(canvas, new SKRect(widget.Bounds.Left + 6, widget.Bounds.Top + 6, widget.Bounds.Right - 6, widget.Bounds.Bottom - 6), widget.Ratio, widget.Value);
    }

    private void DrawBarGaugeWidget(SKCanvas canvas, WidgetData widget)
    {
        using var labelPaint = CreateTextPaint(10, TextSecondary, false);
        using var valuePaint = CreateTextPaint(16, widget.Color, true);
        var titleWidth = labelPaint.MeasureText(widget.Title);
        var valueWidth = valuePaint.MeasureText(widget.Value);
        canvas.DrawText(widget.Title, widget.Bounds.Left + 8, widget.Bounds.Top + 18, labelPaint);
        canvas.DrawText(widget.Value, widget.Bounds.Right - valueWidth - 8, widget.Bounds.Top + 18, valuePaint);

        var barRect = new SKRect(widget.Bounds.Left + 8, widget.Bounds.Top + 26, widget.Bounds.Right - 8, widget.Bounds.Bottom - 10);
        DrawBarGauge(canvas, barRect, widget.Ratio, widget.Color);
    }

    private void DrawGauge(SKCanvas canvas, SKRect rect, float ratio, string valueText)
    {
        using var trackPaint = new SKPaint
        {
            Color = new SKColor(80, 188, 210, 55),
            StrokeWidth = 8,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round
        };

        using var valuePaint = new SKPaint
        {
            Shader = SKShader.CreateSweepGradient(
                new SKPoint(rect.MidX, rect.MidY),
                [AccentSoft, Accent, Warning, Danger],
                [0f, 0.35f, 0.75f, 1f]),
            StrokeWidth = 8,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round
        };

        using var textPaint = CreateTextPaint(19, TextPrimary, true);
        using var percentPaint = CreateTextPaint(10, TextSecondary, false);

        canvas.DrawArc(rect, 135, 270, false, trackPaint);
        canvas.DrawArc(rect, 135, 270 * Math.Clamp(ratio, 0f, 1f), false, valuePaint);
        canvas.DrawText(valueText, rect.Left + 12, rect.MidY + 6, textPaint);
        canvas.DrawText("LOAD", rect.Left + 18, rect.MidY + 22, percentPaint);
    }

    private void DrawBarGauge(SKCanvas canvas, SKRect rect, float ratio, SKColor color)
    {
        using var trackPaint = new SKPaint
        {
            Color = new SKColor(80, 188, 210, 42),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var fillPaint = new SKPaint
        {
            Color = color.WithAlpha(180),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var borderPaint = new SKPaint
        {
            Color = color.WithAlpha(120),
            StrokeWidth = 1,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        var fillRect = new SKRect(rect.Left, rect.Top, rect.Left + rect.Width * Math.Clamp(ratio, 0f, 1f), rect.Bottom);
        canvas.DrawRoundRect(new SKRoundRect(rect, 4, 4), trackPaint);
        canvas.DrawRoundRect(new SKRoundRect(fillRect, 4, 4), fillPaint);
        canvas.DrawRoundRect(new SKRoundRect(rect, 4, 4), borderPaint);
    }

    private void DrawTimelineWidget(SKCanvas canvas, WidgetData widget)
    {
        using var labelPaint = CreateTextPaint(11, TextSecondary, false);
        DrawTimelineHeader(canvas, widget, labelPaint);
        DrawTimeline(canvas, new SKRect(widget.Bounds.Left + 8, widget.Bounds.Top + 26, widget.Bounds.Right - 8, widget.Bounds.Bottom - 8), widget.Ratio, widget.Color);
    }

    private void DrawTimeline(SKCanvas canvas, SKRect rect, float seed, SKColor color)
    {
        DrawTimelineFrame(canvas, rect);

        using var linePaint = new SKPaint
        {
            Color = color,
            StrokeWidth = 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var fillPaint = new SKPaint
        {
            Color = color.WithAlpha(28),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var path = new SKPath();
        using var fillPath = new SKPath();

        const int points = 36;
        for (var i = 0; i < points; i++)
        {
            var t = i / (float)(points - 1);
            var x = rect.Left + rect.Width * t;
            var wave = MathF.Sin((t * 5.6f) + seed * 4.2f) * 0.34f + MathF.Cos((t * 11.2f) + seed * 3.1f) * 0.12f;
            var normalized = Math.Clamp(0.5f + wave, 0.08f, 0.92f);
            var y = rect.Bottom - rect.Height * normalized;

            if (i == 0)
            {
                path.MoveTo(x, y);
                fillPath.MoveTo(x, rect.Bottom);
                fillPath.LineTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
                fillPath.LineTo(x, y);
            }
        }

        fillPath.LineTo(rect.Right, rect.Bottom);
        fillPath.Close();

        canvas.DrawPath(fillPath, fillPaint);
        canvas.DrawPath(path, linePaint);
    }

    private void DrawNetworkTimelineWidget(SKCanvas canvas, WidgetData widget)
    {
        using var labelPaint = CreateTextPaint(11, TextSecondary, false);
        DrawTimelineHeader(canvas, widget, labelPaint);
        DrawNetworkTimeline(canvas, new SKRect(widget.Bounds.Left + 8, widget.Bounds.Top + 26, widget.Bounds.Right - 8, widget.Bounds.Bottom - 8), widget.Ratio);
    }

    private void DrawTimelineHeader(SKCanvas canvas, WidgetData widget, SKPaint labelPaint)
    {
        var titleLines = WrapTitle(widget.Title, labelPaint, widget.Bounds.Width * 0.55f);
        var headerTop = widget.Bounds.Top + 12;

        for (var index = 0; index < titleLines.Count; index++)
        {
            canvas.DrawText(titleLines[index], widget.Bounds.Left + 10, headerTop + index * 10, labelPaint);
        }

        var valueWidth = labelPaint.MeasureText(widget.Value);
        canvas.DrawText(widget.Value, widget.Bounds.Right - valueWidth - 10, widget.Bounds.Top + 18, labelPaint);
    }

    private void DrawTitleValueWidget(SKCanvas canvas, WidgetData widget, SKPaint titlePaint, SKPaint valuePaint)
    {
        var titleLines = WrapTitle(widget.Title, titlePaint, widget.Bounds.Width * 0.46f);
        var firstLineY = widget.Bounds.Top + 18;

        for (var index = 0; index < titleLines.Count; index++)
        {
            canvas.DrawText(titleLines[index], widget.Bounds.Left + 10, firstLineY + index * 10, titlePaint);
        }

        var valueWidth = valuePaint.MeasureText(widget.Value);
        var valueX = widget.Bounds.Right - valueWidth - 10;
        var valueY = widget.Bounds.Top + (widget.Bounds.Height / 2f) + 4;
        canvas.DrawText(widget.Value, valueX, valueY, valuePaint);
    }

    private static IReadOnlyList<string> WrapTitle(string title, SKPaint paint, float maxWidth)
    {
        if (paint.MeasureText(title) <= maxWidth)
        {
            return [title];
        }

        var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2)
        {
            return [title];
        }

        var firstLine = words[0];
        var secondLine = string.Join(' ', words.Skip(1));
        for (var i = 1; i < words.Length; i++)
        {
            var candidate = string.Join(' ', words.Take(i));
            if (paint.MeasureText(candidate) <= maxWidth)
            {
                firstLine = candidate;
                secondLine = string.Join(' ', words.Skip(i));
            }
            else
            {
                break;
            }
        }

        return string.IsNullOrWhiteSpace(secondLine) ? [firstLine] : [firstLine, secondLine];
    }

    private void DrawNetworkTimeline(SKCanvas canvas, SKRect rect, float seed)
    {
        DrawTimelineFrame(canvas, rect);

        using var centerPaint = new SKPaint
        {
            Color = new SKColor(160, 220, 235, 50),
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash([6, 6], 0),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var downloadPaint = new SKPaint
        {
            Color = Accent,
            StrokeWidth = 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var uploadPaint = new SKPaint
        {
            Color = AccentSoft,
            StrokeWidth = 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var downloadFill = new SKPaint
        {
            Color = new SKColor(0, 245, 255, 22),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var uploadFill = new SKPaint
        {
            Color = new SKColor(120, 255, 247, 20),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        var midY = rect.MidY;
        canvas.DrawLine(rect.Left, midY, rect.Right, midY, centerPaint);

        using var downloadPath = new SKPath();
        using var uploadPath = new SKPath();
        using var downloadArea = new SKPath();
        using var uploadArea = new SKPath();

        const int points = 34;
        for (var i = 0; i < points; i++)
        {
            var t = i / (float)(points - 1);
            var x = rect.Left + rect.Width * t;
            var downMagnitude = Math.Clamp(0.48f + MathF.Sin(t * 6.1f + seed * 3.7f) * 0.22f + MathF.Cos(t * 9.4f) * 0.08f, 0.08f, 0.9f);
            var upMagnitude = Math.Clamp(0.22f + MathF.Sin(t * 4.3f + seed * 2.4f) * 0.12f + MathF.Cos(t * 8.8f + 0.5f) * 0.06f, 0.05f, 0.56f);
            var downY = midY - downMagnitude * (rect.Height * 0.42f);
            var upY = midY + upMagnitude * (rect.Height * 0.42f);

            if (i == 0)
            {
                downloadPath.MoveTo(x, downY);
                uploadPath.MoveTo(x, upY);
                downloadArea.MoveTo(x, midY);
                downloadArea.LineTo(x, downY);
                uploadArea.MoveTo(x, midY);
                uploadArea.LineTo(x, upY);
            }
            else
            {
                downloadPath.LineTo(x, downY);
                uploadPath.LineTo(x, upY);
                downloadArea.LineTo(x, downY);
                uploadArea.LineTo(x, upY);
            }
        }

        downloadArea.LineTo(rect.Right, midY);
        downloadArea.Close();
        uploadArea.LineTo(rect.Right, midY);
        uploadArea.Close();

        canvas.DrawPath(downloadArea, downloadFill);
        canvas.DrawPath(uploadArea, uploadFill);
        canvas.DrawPath(downloadPath, downloadPaint);
        canvas.DrawPath(uploadPath, uploadPaint);

        using var labelPaint = CreateTextPaint(11, TextSecondary, false);
        canvas.DrawText("DL", rect.Right - 26, rect.Top + 12, labelPaint);
        canvas.DrawText("UP", rect.Right - 26, rect.Bottom - 4, labelPaint);
    }

    private void DrawTimelineFrame(SKCanvas canvas, SKRect rect)
    {
        using var borderPaint = new SKPaint
        {
            Color = new SKColor(120, 220, 240, 40),
            StrokeWidth = 1,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var gridPaint = new SKPaint
        {
            Color = new SKColor(100, 190, 215, 20),
            StrokeWidth = 1,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        canvas.DrawRect(rect, borderPaint);

        for (var y = 1; y < 4; y++)
        {
            var lineY = rect.Top + rect.Height * y / 4f;
            canvas.DrawLine(rect.Left, lineY, rect.Right, lineY, gridPaint);
        }

        for (var x = 1; x < 8; x++)
        {
            var lineX = rect.Left + rect.Width * x / 8f;
            canvas.DrawLine(lineX, rect.Top, lineX, rect.Bottom, gridPaint);
        }
    }

    private static SKPaint CreateTextPaint(float size, SKColor color, bool bold)
    {
        return new SKPaint
        {
            Color = color,
            TextSize = size,
            IsAntialias = true,
            Typeface = bold ? SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) : SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
        };
    }

    private sealed record WidgetData(WidgetKind Kind, SKRect Bounds, string Title, string Value, SKColor Color, float Ratio);

    private enum WidgetKind
    {
        Title,
        Label,
        Info,
        Metric,
        Gauge,
        BarGauge,
        Timeline,
        NetworkTimeline,
    }
}
