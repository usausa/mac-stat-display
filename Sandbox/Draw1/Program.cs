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
        new("CPU", new SKRect(24, 28, 398, 228),
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
        0.42f),
        new("MEMORY", new SKRect(24, 252, 398, 452),
        [
            new MetricLine("Usage", "68%", Warning),
            new MetricLine("Used", "21.8 GB", TextPrimary),
            new MetricLine("App", "14.2 GB", AccentSoft),
            new MetricLine("Wired", "4.7 GB", TextSecondary),
            new MetricLine("Swap", "512 MB", Danger)
        ],
        0.68f),
        new("DISK", new SKRect(442, 28, 836, 228),
        [
            new MetricLine("Usage", "74%  /  468 GB free of 1.8 TB", Warning),
            new MetricLine("Read", "824 KB/s", AccentSoft),
            new MetricLine("Write", "391 KB/s", Accent)
        ],
        0.74f),
        new("NETWORK", new SKRect(442, 252, 836, 452),
        [
            new MetricLine("Download", "18.4 MB/s", Accent),
            new MetricLine("Upload", "2.6 MB/s", AccentSoft)
        ],
        0.51f),
        new("PROCESS", new SKRect(858, 28, 1256, 178),
        [
            new MetricLine("Processes", "412", TextPrimary),
            new MetricLine("Threads", "6489", TextPrimary)
        ],
        0.36f),
        new("HW MONITOR", new SKRect(858, 198, 1256, 372),
        [
            new MetricLine("CPU Temp", "63 C", Warning),
            new MetricLine("GPU Temp", "57 C", AccentSoft),
            new MetricLine("System Power", "214 W", TextPrimary),
            new MetricLine("CPU Power", "88 W", Accent),
            new MetricLine("GPU Power", "126 W", Danger)
        ],
        0.59f),
        new("SYSTEM", new SKRect(858, 392, 1256, 452),
        [
            new MetricLine("Uptime", "03d 14h 22m", Good)
        ],
        0.82f)
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
        DrawHudLines(canvas);

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

        DrawGauge(canvas, new SKRect(rect.Left + 20, rect.Top + 58, rect.Left + 104, rect.Top + 142), panel.Ratio);

        var x = rect.Left + 122;
        var y = rect.Top + 72;
        var maxValueWidth = panel.Items.Max(static item => item.Value.Length) * 12f;

        foreach (var item in panel.Items)
        {
            using var namePaint = CreateTextPaint(13, TextSecondary, false);
            using var valuePaint = CreateTextPaint(15, item.Color, true);

            canvas.DrawText(item.Name, x, y, namePaint);
            canvas.DrawText(item.Value, rect.Right - maxValueWidth - 22, y, valuePaint);
            y += 22;
        }

        DrawSparkline(canvas, new SKRect(rect.Left + 18, rect.Bottom - 42, rect.Right - 18, rect.Bottom - 16), panel.Ratio);
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

        var arcRect = rect;
        canvas.DrawArc(arcRect, 135, 270, false, trackPaint);
        canvas.DrawArc(arcRect, 135, 270 * Math.Clamp(ratio, 0f, 1f), false, valuePaint);
        canvas.DrawText($"{ratio * 100:0}%", rect.Left + 20, rect.MidY + 6, textPaint);
        canvas.DrawText("LOAD", rect.Left + 26, rect.MidY + 24, percentPaint);
    }

    private void DrawSparkline(SKCanvas canvas, SKRect rect, float seed)
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

        const int points = 28;
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

    private void DrawHudLines(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(0, 245, 255, 45),
            StrokeWidth = 1,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        canvas.DrawLine(420, 18, 420, 462, paint);
        canvas.DrawLine(844, 18, 844, 462, paint);
        canvas.DrawLine(20, 240, 1258, 240, paint);
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

    private sealed record PanelData(string Title, SKRect Bounds, IReadOnlyList<MetricLine> Items, float Ratio);

    private sealed record MetricLine(string Name, string Value, SKColor Color);
}
