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
    private static readonly SKColor PanelStroke = new(76, 233, 255, 180);
    private static readonly SKColor Accent = new(0, 245, 255);
    private static readonly SKColor AccentSoft = new(120, 255, 247);
    private static readonly SKColor TextPrimary = new(225, 250, 255);
    private static readonly SKColor TextSecondary = new(125, 187, 208);
    private static readonly SKColor Warning = new(255, 176, 72);
    private static readonly SKColor Danger = new(255, 94, 122);
    private static readonly SKColor Good = new(110, 255, 184);

    private readonly IReadOnlyList<PanelData> _panels =
    [
        new(
            "CPU",
            new SKRect(24, 54, 566, 286),
            [
                new MetricLine("Usage", "42%", Accent),
                new MetricLine("System", "11%", TextSecondary),
                new MetricLine("User", "24%", TextSecondary),
                new MetricLine("Idle", "58%", Good),
                new MetricLine("E-Core", "37%", AccentSoft),
                new MetricLine("P-Core", "61%", Warning),
                new MetricLine("Load 1/5/15", "3.12 / 2.84 / 2.41", TextPrimary),
                new MetricLine("Freq", "3560 MHz", TextPrimary),
                new MetricLine("E/P Freq", "2840 / 4012 MHz", TextPrimary)
            ],
            0.42f,
            PanelVisual.GaugeAndTimeline),
        new(
            "MEMORY",
            new SKRect(24, 302, 566, 456),
            [
                new MetricLine("Usage", "68%", Warning),
                new MetricLine("Used", "21.8 GB", TextPrimary),
                new MetricLine("App", "14.2 GB", AccentSoft),
                new MetricLine("Wired", "4.7 GB", TextSecondary),
                new MetricLine("Swap", "512 MB", Danger)
            ],
            0.68f,
            PanelVisual.GaugeAndTimeline),
        new(
            "NETWORK",
            new SKRect(590, 54, 944, 286),
            [
                new MetricLine("Download", "18.4 MB/s", Accent),
                new MetricLine("Upload", "2.6 MB/s", AccentSoft)
            ],
            0.51f,
            PanelVisual.NetworkTimeline),
        new(
            "DISK",
            new SKRect(590, 302, 944, 456),
            [
                new MetricLine("Usage", "74%", Warning),
                new MetricLine("Free / Total", "468 GB / 1.8 TB", TextPrimary),
                new MetricLine("Read", "824 KB/s", AccentSoft),
                new MetricLine("Write", "391 KB/s", Accent)
            ],
            0.74f,
            PanelVisual.GaugeOnly),
        new(
            "PROCESS",
            new SKRect(968, 54, 1256, 166),
            [
                new MetricLine("Processes", "412", TextPrimary),
                new MetricLine("Threads", "6489", TextPrimary)
            ],
            0.36f,
            PanelVisual.TextOnly),
        new(
            "HW MONITOR",
            new SKRect(968, 182, 1256, 356),
            [
                new MetricLine("CPU Temp", "63 C", Warning),
                new MetricLine("GPU Temp", "57 C", AccentSoft),
                new MetricLine("System Power", "214 W", TextPrimary),
                new MetricLine("CPU Power", "88 W", Accent),
                new MetricLine("GPU Power", "126 W", Danger)
            ],
            0.59f,
            PanelVisual.TextOnly),
        new(
            "SYSTEM",
            new SKRect(968, 372, 1256, 456),
            [
                new MetricLine("Uptime", "03d 14h 22m", Good)
            ],
            0.82f,
            PanelVisual.GaugeOnly)
    ];

    public void Render(string outputPath)
    {
        ArgumentNullException.ThrowIfNull(outputPath);

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(Background);
        DrawBackground(canvas);

        foreach (var panel in _panels)
        {
            DrawPanel(canvas, panel);
        }

        DrawHeader(canvas);
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

    private void DrawHeader(SKCanvas canvas)
    {
        using var titlePaint = CreateTextPaint(28, Accent, true);
        using var subPaint = CreateTextPaint(13, TextSecondary, false);
        using var linePaint = new SKPaint
        {
            Color = Accent,
            StrokeWidth = 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        canvas.DrawText("MAC SYSTEM STATUS", 34, 24, titlePaint);
        canvas.DrawText("SKIASHARP VISUAL OUTPUT / 1280x480 / SCI-FI DASHBOARD", 36, 44, subPaint);
        canvas.DrawLine(320, 16, 1244, 16, linePaint);
        canvas.DrawLine(960, 44, 1244, 44, linePaint);
    }

    private void DrawPanel(SKCanvas canvas, PanelData panel)
    {
        var rect = panel.Bounds;

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

        using var accentPaint = new SKPaint
        {
            Color = Accent,
            StrokeWidth = 3,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        var rounded = new SKRoundRect(rect, 16, 16);
        canvas.DrawRoundRect(rounded, shadowPaint);
        canvas.DrawRoundRect(rounded, fillPaint);
        canvas.DrawRoundRect(rounded, strokePaint);
        canvas.DrawLine(rect.Left + 18, rect.Top + 18, rect.Left + 118, rect.Top + 18, accentPaint);
        canvas.DrawLine(rect.Right - 98, rect.Bottom - 16, rect.Right - 18, rect.Bottom - 16, accentPaint);

        using var titlePaint = CreateTextPaint(18, TextPrimary, true);
        using var labelPaint = CreateTextPaint(12, TextSecondary, false);

        canvas.DrawText(panel.Title, rect.Left + 18, rect.Top + 40, titlePaint);
        canvas.DrawText("REALTIME TELEMETRY", rect.Right - 146, rect.Top + 40, labelPaint);

        DrawPanelContents(canvas, panel);
    }

    private void DrawPanelContents(SKCanvas canvas, PanelData panel)
    {
        var rect = panel.Bounds;
        switch (panel.Visual)
        {
            case PanelVisual.GaugeAndTimeline:
            {
                var gaugeRect = new SKRect(rect.Left + 18, rect.Top + 60, rect.Left + 108, rect.Top + 150);
                var statsRect = new SKRect(rect.Left + 126, rect.Top + 62, rect.Right - 18, rect.Top + 156);
                var timelineRect = new SKRect(rect.Left + 18, rect.Top + 172, rect.Right - 18, rect.Bottom - 20);
                DrawGauge(canvas, gaugeRect, panel.Ratio);
                DrawMetrics(canvas, panel.Items, statsRect, 4);
                DrawTimeline(canvas, timelineRect, panel.Ratio);
                break;
            }
            case PanelVisual.NetworkTimeline:
            {
                var statsRect = new SKRect(rect.Left + 20, rect.Top + 66, rect.Right - 20, rect.Top + 118);
                var timelineRect = new SKRect(rect.Left + 18, rect.Top + 130, rect.Right - 18, rect.Bottom - 18);
                DrawMetrics(canvas, panel.Items, statsRect, 12);
                DrawNetworkTimeline(canvas, timelineRect, panel.Ratio);
                break;
            }
            case PanelVisual.GaugeOnly:
            {
                var gaugeRect = new SKRect(rect.Left + 18, rect.Top + 64, rect.Left + 102, rect.Top + 148);
                var statsRect = new SKRect(rect.Left + 116, rect.Top + 70, rect.Right - 20, rect.Bottom - 24);
                DrawGauge(canvas, gaugeRect, panel.Ratio);
                DrawMetrics(canvas, panel.Items, statsRect, 8);
                break;
            }
            case PanelVisual.TextOnly:
            {
                var statsRect = new SKRect(rect.Left + 18, rect.Top + 66, rect.Right - 18, rect.Bottom - 18);
                DrawMetrics(canvas, panel.Items, statsRect, 8);
                break;
            }
        }
    }

    private void DrawMetrics(SKCanvas canvas, IReadOnlyList<MetricLine> items, SKRect rect, float rowGap)
    {
        using var namePaint = CreateTextPaint(13, TextSecondary, false);
        using var valuePaint = CreateTextPaint(15, TextPrimary, true);

        var rowHeight = (rect.Height - rowGap * Math.Max(0, items.Count - 1)) / Math.Max(1, items.Count);
        var valueX = rect.Right - 2;

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var top = rect.Top + index * (rowHeight + rowGap);
            var baseline = top + Math.Min(rowHeight * 0.72f, 16f);

            valuePaint.Color = item.Color;
            canvas.DrawText(item.Name, rect.Left, baseline, namePaint);

            var valueWidth = valuePaint.MeasureText(item.Value);
            canvas.DrawText(item.Value, valueX - valueWidth, baseline, valuePaint);

            using var separatorPaint = new SKPaint
            {
                Color = new SKColor(120, 200, 220, 24),
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            if (index < items.Count - 1)
            {
                var lineY = top + rowHeight + 2;
                canvas.DrawLine(rect.Left, lineY, rect.Right, lineY, separatorPaint);
            }
        }
    }

    private void DrawGauge(SKCanvas canvas, SKRect rect, float ratio)
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

        using var textPaint = CreateTextPaint(18, TextPrimary, true);
        using var percentPaint = CreateTextPaint(10, TextSecondary, false);

        canvas.DrawArc(rect, 135, 270, false, trackPaint);
        canvas.DrawArc(rect, 135, 270 * Math.Clamp(ratio, 0f, 1f), false, valuePaint);
        canvas.DrawText($"{ratio * 100:0}%", rect.Left + 16, rect.MidY + 6, textPaint);
        canvas.DrawText("LOAD", rect.Left + 22, rect.MidY + 24, percentPaint);
    }

    private void DrawTimeline(SKCanvas canvas, SKRect rect, float seed)
    {
        using var linePaint = new SKPaint
        {
            Color = Accent,
            StrokeWidth = 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var fillPaint = new SKPaint
        {
            Color = new SKColor(0, 245, 255, 28),
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

    private sealed record PanelData(string Title, SKRect Bounds, IReadOnlyList<MetricLine> Items, float Ratio, PanelVisual Visual);

    private sealed record MetricLine(string Name, string Value, SKColor Color);

    private enum PanelVisual
    {
        GaugeAndTimeline,
        NetworkTimeline,
        GaugeOnly,
        TextOnly
    }
}
