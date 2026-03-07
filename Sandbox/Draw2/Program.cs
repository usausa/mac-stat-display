using SkiaSharp;

const int canvasWidth = 1280;
const int canvasHeight = 480;
const string outputFileName = "output.png";

var snapshot = SystemSnapshot.CreateSample();

using var bitmap = new SKBitmap(canvasWidth, canvasHeight);
using var canvas = new SKCanvas(bitmap);

DashboardRenderer.Render(canvas, canvasWidth, canvasHeight, snapshot);

using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using var stream = File.Open(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None);
data.SaveTo(stream);

Console.WriteLine($"Rendered {outputFileName}");

static class DashboardRenderer
{
    private static readonly SKColor Background = SKColor.Parse("#07111f");
    private static readonly SKColor Panel = SKColor.Parse("#101d32");
    private static readonly SKColor PanelAlt = SKColor.Parse("#0c1729");
    private static readonly SKColor Hairline = SKColor.Parse("#1f3353");
    private static readonly SKColor PrimaryText = SKColors.White;
    private static readonly SKColor SecondaryText = SKColor.Parse("#8fa8cc");
    private static readonly SKColor Accent = SKColor.Parse("#4ec9f5");
    private static readonly SKColor Accent2 = SKColor.Parse("#6df5c1");
    private static readonly SKColor Warning = SKColor.Parse("#ffd166");
    private static readonly SKColor Danger = SKColor.Parse("#ff6b6b");

    public static void Render(SKCanvas canvas, int width, int height, SystemSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(snapshot);

        canvas.Clear(Background);

        using var paint = new SKPaint { IsAntialias = true };
        DrawBackgroundGlow(canvas, width, height, paint);

        const float outerPadding = 18f;
        const float headerHeight = 64f;
        const float gutter = 10f;

        var headerRect = new SKRect(outerPadding, outerPadding, width - outerPadding, outerPadding + headerHeight);
        DrawHeader(canvas, headerRect, snapshot);

        var bodyTop = headerRect.Bottom + gutter;
        var bodyRect = new SKRect(outerPadding, bodyTop, width - outerPadding, height - outerPadding);

        var columnGap = 10f;
        var leftWidth = 300f;
        var middleWidth = 390f;

        var leftRect = new SKRect(bodyRect.Left, bodyRect.Top, bodyRect.Left + leftWidth, bodyRect.Bottom);
        var middleRect = new SKRect(leftRect.Right + columnGap, bodyRect.Top, leftRect.Right + columnGap + middleWidth, bodyRect.Bottom);
        var rightRect = new SKRect(middleRect.Right + columnGap, bodyRect.Top, bodyRect.Right, bodyRect.Bottom);

        DrawMetricColumn(canvas, leftRect, snapshot);
        DrawGaugeColumn(canvas, middleRect, snapshot);
        DrawSystemColumn(canvas, rightRect, snapshot);
    }

    private static void DrawBackgroundGlow(SKCanvas canvas, int width, int height, SKPaint paint)
    {
        paint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(width * 0.18f, height * 0.12f),
            280f,
            [Accent.WithAlpha(44), SKColors.Transparent],
            [0f, 1f],
            SKShaderTileMode.Clamp);
        canvas.DrawRect(SKRect.Create(width, height), paint);

        paint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(width * 0.82f, height * 0.18f),
            320f,
            [Accent2.WithAlpha(30), SKColors.Transparent],
            [0f, 1f],
            SKShaderTileMode.Clamp);
        canvas.DrawRect(SKRect.Create(width, height), paint);
        paint.Shader = null;
    }

    private static void DrawHeader(SKCanvas canvas, SKRect rect, SystemSnapshot snapshot)
    {
        DrawPanel(canvas, rect, PanelAlt, 18f);

        using var titlePaint = CreateTextPaint(28f, PrimaryText, true);
        using var infoPaint = CreateTextPaint(14f, SecondaryText);
        using var valuePaint = CreateTextPaint(16f, Accent, true);

        canvas.DrawText("SYSTEM MONITOR", rect.Left + 18f, rect.Top + 28f, titlePaint);
        canvas.DrawText("High density widget dashboard", rect.Left + 18f, rect.Top + 48f, infoPaint);

        var rightX = rect.Right - 18f;
        DrawRightAlignedStat(canvas, rightX, rect.Top + 24f, "UPTIME", snapshot.Uptime, valuePaint, infoPaint);
        DrawRightAlignedStat(canvas, rightX, rect.Top + 45f, "TIME", snapshot.Timestamp, valuePaint, infoPaint);
    }

    private static void DrawMetricColumn(SKCanvas canvas, SKRect rect, SystemSnapshot snapshot)
    {
        const float gap = 8f;
        const float titleHeight = 34f;
        const float smallHeight = 41f;

        var y = rect.Top;
        y = DrawSectionTitle(canvas, new SKRect(rect.Left, y, rect.Right, y + titleHeight), "CPU");

        foreach (var metric in snapshot.CpuMetrics)
        {
            y += gap;
            DrawMetricWidget(canvas, new SKRect(rect.Left, y, rect.Right, y + smallHeight), metric.Label, metric.Value, metric.Tone);
            y += smallHeight;
        }

        y += gap;
        y = DrawSectionTitle(canvas, new SKRect(rect.Left, y, rect.Right, y + titleHeight), "MEMORY");
        foreach (var metric in snapshot.MemoryMetrics)
        {
            y += gap;
            DrawMetricWidget(canvas, new SKRect(rect.Left, y, rect.Right, y + smallHeight), metric.Label, metric.Value, metric.Tone);
            y += smallHeight;
        }
    }

    private static void DrawGaugeColumn(SKCanvas canvas, SKRect rect, SystemSnapshot snapshot)
    {
        const float titleHeight = 34f;
        const float gap = 10f;
        var y = rect.Top;

        y = DrawSectionTitle(canvas, new SKRect(rect.Left, y, rect.Right, y + titleHeight), "UTILIZATION");
        y += gap;

        var gaugeArea = new SKRect(rect.Left, y, rect.Right, y + 220f);
        var gaugeWidth = (gaugeArea.Width - gap) / 2f;
        DrawCircularGauge(canvas, new SKRect(gaugeArea.Left, gaugeArea.Top, gaugeArea.Left + gaugeWidth, gaugeArea.Bottom), "CPU", snapshot.CpuUsage, Accent, "busy");
        DrawCircularGauge(canvas, new SKRect(gaugeArea.Left + gaugeWidth + gap, gaugeArea.Top, gaugeArea.Right, gaugeArea.Bottom), "MEM", snapshot.MemoryUsage, Accent2, "used");

        y = gaugeArea.Bottom + gap;
        y = DrawSectionTitle(canvas, new SKRect(rect.Left, y, rect.Right, y + titleHeight), "DISK / NETWORK");
        y += gap;

        DrawBarGauge(canvas, new SKRect(rect.Left, y, rect.Right, y + 72f), "DISK USED", snapshot.DiskUsage, snapshot.DiskSummary, Warning);
        y += 82f;
        DrawMetricWidget(canvas, new SKRect(rect.Left, y, rect.Right, y + 44f), "Read throughput", snapshot.DiskRead, Accent);
        y += 52f;
        DrawMetricWidget(canvas, new SKRect(rect.Left, y, rect.Right, y + 44f), "Write throughput", snapshot.DiskWrite, Accent);
        y += 52f;
        DrawMetricWidget(canvas, new SKRect(rect.Left, y, rect.Right, y + 44f), "Network down", snapshot.NetworkDown, Accent2);
        y += 52f;
        DrawMetricWidget(canvas, new SKRect(rect.Left, y, rect.Right, y + 44f), "Network up", snapshot.NetworkUp, Accent2);
    }

    private static void DrawSystemColumn(SKCanvas canvas, SKRect rect, SystemSnapshot snapshot)
    {
        const float titleHeight = 34f;
        const float gap = 8f;
        var y = rect.Top;

        y = DrawSectionTitle(canvas, new SKRect(rect.Left, y, rect.Right, y + titleHeight), "PROCESS / HW / SYSTEM");

        foreach (var metric in snapshot.SystemMetrics)
        {
            y += gap;
            DrawMetricWidget(canvas, new SKRect(rect.Left, y, rect.Right, y + 43f), metric.Label, metric.Value, metric.Tone);
            y += 43f;
        }

        y += gap;
        DrawTimeline(canvas, new SKRect(rect.Left, y, rect.Right, rect.Bottom), snapshot);
    }

    private static float DrawSectionTitle(SKCanvas canvas, SKRect rect, string title)
    {
        DrawPanel(canvas, rect, PanelAlt, 14f);
        using var textPaint = CreateTextPaint(16f, PrimaryText, true);
        using var subPaint = CreateTextPaint(11f, SecondaryText);
        canvas.DrawText(title, rect.Left + 12f, rect.MidY + 5f, textPaint);
        canvas.DrawText("widgets", rect.Right - 52f, rect.MidY + 4f, subPaint);
        return rect.Bottom;
    }

    private static void DrawMetricWidget(SKCanvas canvas, SKRect rect, string label, string value, SKColor tone)
    {
        DrawPanel(canvas, rect, Panel, 14f);

        using var labelPaint = CreateTextPaint(12f, SecondaryText);
        using var valuePaint = CreateTextPaint(17f, tone, true);

        DrawWrappedLabel(canvas, rect, label, labelPaint);

        var bounds = new SKRect();
        valuePaint.MeasureText(value, ref bounds);
        canvas.DrawText(value, rect.Right - 14f - bounds.Width, rect.MidY + 7f, valuePaint);
    }

    private static void DrawCircularGauge(SKCanvas canvas, SKRect rect, string title, float percent, SKColor color, string suffix)
    {
        DrawPanel(canvas, rect, Panel, 20f);

        var center = new SKPoint(rect.MidX, rect.MidY + 2f);
        var radius = MathF.Min(rect.Width, rect.Height) * 0.31f;

        using var trackPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = Hairline,
            StrokeWidth = 14f,
            StrokeCap = SKStrokeCap.Round
        };

        using var valuePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = color,
            StrokeWidth = 14f,
            StrokeCap = SKStrokeCap.Round
        };

        var arcRect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
        canvas.DrawArc(arcRect, 135f, 270f, false, trackPaint);
        canvas.DrawArc(arcRect, 135f, 270f * (percent / 100f), false, valuePaint);

        using var titlePaint = CreateTextPaint(15f, SecondaryText, true);
        using var numberPaint = CreateTextPaint(28f, PrimaryText, true);
        using var footerPaint = CreateTextPaint(12f, color);

        DrawCenteredText(canvas, title, center.X, rect.Top + 28f, titlePaint);
        DrawCenteredText(canvas, $"{percent:0}%", center.X, center.Y + 10f, numberPaint);
        DrawCenteredText(canvas, suffix, center.X, center.Y + 32f, footerPaint);
    }

    private static void DrawBarGauge(SKCanvas canvas, SKRect rect, string title, float percent, string summary, SKColor color)
    {
        DrawPanel(canvas, rect, Panel, 16f);

        using var titlePaint = CreateTextPaint(13f, SecondaryText, true);
        using var summaryPaint = CreateTextPaint(15f, PrimaryText, true);
        using var barPaint = new SKPaint { IsAntialias = true, Color = color };
        using var trackPaint = new SKPaint { IsAntialias = true, Color = Hairline };

        canvas.DrawText(title, rect.Left + 14f, rect.Top + 20f, titlePaint);
        canvas.DrawText(summary, rect.Left + 14f, rect.Top + 41f, summaryPaint);

        var barRect = new SKRect(rect.Left + 14f, rect.Bottom - 18f, rect.Right - 14f, rect.Bottom - 8f);
        canvas.DrawRoundRect(barRect, 5f, 5f, trackPaint);

        var fillWidth = barRect.Width * (percent / 100f);
        var fillRect = new SKRect(barRect.Left, barRect.Top, barRect.Left + fillWidth, barRect.Bottom);
        canvas.DrawRoundRect(fillRect, 5f, 5f, barPaint);
    }

    private static void DrawTimeline(SKCanvas canvas, SKRect rect, SystemSnapshot snapshot)
    {
        DrawPanel(canvas, rect, PanelAlt, 18f);

        using var titlePaint = CreateTextPaint(14f, PrimaryText, true);
        using var labelPaint = CreateTextPaint(11f, SecondaryText);
        using var pathPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3f,
            Color = Accent,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = Accent.WithAlpha(18)
        };

        canvas.DrawText("CPU LOAD TIMELINE", rect.Left + 14f, rect.Top + 20f, titlePaint);

        var chartRect = new SKRect(rect.Left + 12f, rect.Top + 30f, rect.Right - 12f, rect.Bottom - 22f);
        using var gridPaint = new SKPaint { IsAntialias = true, Color = Hairline, StrokeWidth = 1f };
        for (var i = 0; i < 4; i++)
        {
            var y = chartRect.Top + (chartRect.Height / 3f) * i;
            canvas.DrawLine(chartRect.Left, y, chartRect.Right, y, gridPaint);
        }

        var points = snapshot.Timeline;
        using var path = new SKPath();
        using var fillPath = new SKPath();
        for (var i = 0; i < points.Length; i++)
        {
            var x = chartRect.Left + chartRect.Width * i / (points.Length - 1f);
            var y = chartRect.Bottom - chartRect.Height * (points[i] / 100f);
            if (i == 0)
            {
                path.MoveTo(x, y);
                fillPath.MoveTo(x, chartRect.Bottom);
                fillPath.LineTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
                fillPath.LineTo(x, y);
            }
        }

        fillPath.LineTo(chartRect.Right, chartRect.Bottom);
        fillPath.Close();

        canvas.DrawPath(fillPath, fillPaint);
        canvas.DrawPath(path, pathPaint);

        canvas.DrawText("-15m", chartRect.Left, rect.Bottom - 6f, labelPaint);
        DrawCenteredText(canvas, "-5m", chartRect.MidX, rect.Bottom - 6f, labelPaint);
        var latestBounds = new SKRect();
        labelPaint.MeasureText("now", ref latestBounds);
        canvas.DrawText("now", chartRect.Right - latestBounds.Width, rect.Bottom - 6f, labelPaint);
    }

    private static void DrawPanel(SKCanvas canvas, SKRect rect, SKColor color, float radius)
    {
        using var fill = new SKPaint { IsAntialias = true, Color = color };
        using var border = new SKPaint { IsAntialias = true, Color = Hairline, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
        canvas.DrawRoundRect(rect, radius, radius, fill);
        canvas.DrawRoundRect(rect, radius, radius, border);
    }

    private static SKPaint CreateTextPaint(float size, SKColor color, bool bold = false)
    {
        return new SKPaint
        {
            IsAntialias = true,
            Color = color,
            TextSize = size,
            Typeface = bold
                ? SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                : SKTypeface.FromFamilyName("Segoe UI")
        };
    }

    private static void DrawRightAlignedStat(SKCanvas canvas, float rightX, float baselineY, string label, string value, SKPaint valuePaint, SKPaint labelPaint)
    {
        var valueBounds = new SKRect();
        valuePaint.MeasureText(value, ref valueBounds);
        canvas.DrawText(value, rightX - valueBounds.Width, baselineY, valuePaint);
        canvas.DrawText(label, rightX - valueBounds.Width - 74f, baselineY, labelPaint);
    }

    private static void DrawWrappedLabel(SKCanvas canvas, SKRect rect, string label, SKPaint paint)
    {
        var maxWidth = rect.Width * 0.56f;
        var words = label.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var current = string.Empty;

        foreach (var word in words)
        {
            var next = string.IsNullOrEmpty(current) ? word : $"{current} {word}";
            if (paint.MeasureText(next) <= maxWidth || string.IsNullOrEmpty(current))
            {
                current = next;
                continue;
            }

            lines.Add(current);
            current = word;
            if (lines.Count == 1)
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(current) && lines.Count < 2)
        {
            lines.Add(current);
        }

        if (lines.Count == 0)
        {
            lines.Add(label);
        }

        var startY = lines.Count == 1 ? rect.MidY + 4f : rect.MidY - 2f;
        for (var i = 0; i < lines.Count && i < 2; i++)
        {
            canvas.DrawText(lines[i], rect.Left + 14f, startY + i * 13f, paint);
        }
    }

    private static void DrawCenteredText(SKCanvas canvas, string text, float centerX, float baselineY, SKPaint paint)
    {
        var bounds = new SKRect();
        paint.MeasureText(text, ref bounds);
        canvas.DrawText(text, centerX - bounds.Width / 2f, baselineY, paint);
    }
}

internal sealed record MetricWidget(string Label, string Value, SKColor Tone);

internal sealed record SystemSnapshot(
    float CpuUsage,
    float MemoryUsage,
    float DiskUsage,
    string DiskSummary,
    string DiskRead,
    string DiskWrite,
    string NetworkDown,
    string NetworkUp,
    string Uptime,
    string Timestamp,
    MetricWidget[] CpuMetrics,
    MetricWidget[] MemoryMetrics,
    MetricWidget[] SystemMetrics,
    float[] Timeline)
{
    public static SystemSnapshot CreateSample()
    {
        return new SystemSnapshot(
            CpuUsage: 62f,
            MemoryUsage: 74f,
            DiskUsage: 68f,
            DiskSummary: "412 GB free / 1.28 TB",
            DiskRead: "982 KB/s",
            DiskWrite: "1,248 KB/s",
            NetworkDown: "12.4 MB/s",
            NetworkUp: "2.8 MB/s",
            Uptime: "3d 07h 42m",
            Timestamp: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            CpuMetrics:
            [
                new("CPU usage", "62 %", DashboardPalette.Accent),
                new("CPU system", "18 %", DashboardPalette.Warning),
                new("CPU user", "44 %", DashboardPalette.Accent2),
                new("CPU idle", "38 %", DashboardPalette.Secondary),
                new("E-core usage", "49 %", DashboardPalette.Accent),
                new("P-core usage", "71 %", DashboardPalette.Danger),
                new("Load avg 1 min", "4.38", DashboardPalette.Primary),
                new("Load avg 5 min", "3.92", DashboardPalette.Primary),
                new("Load avg 15 min", "3.27", DashboardPalette.Primary),
                new("CPU freq", "3860 MHz", DashboardPalette.Primary),
                new("E-core freq", "2980 MHz", DashboardPalette.Accent),
                new("P-core freq", "4210 MHz", DashboardPalette.Danger)
            ],
            MemoryMetrics:
            [
                new("Memory usage", "74 %", DashboardPalette.Accent2),
                new("Used memory", "23.7 GB", DashboardPalette.Primary),
                new("App memory", "9.4 GB", DashboardPalette.Primary),
                new("Wired memory", "6.8 GB", DashboardPalette.Primary),
                new("Swap used", "512 MB", DashboardPalette.Warning)
            ],
            SystemMetrics:
            [
                new("Process count", "418", DashboardPalette.Primary),
                new("Thread count", "5,932", DashboardPalette.Primary),
                new("CPU temperature", "71 C", DashboardPalette.Warning),
                new("GPU temperature", "64 C", DashboardPalette.Accent2),
                new("System power", "82 W", DashboardPalette.Primary),
                new("CPU power", "31 W", DashboardPalette.Warning),
                new("GPU power", "24 W", DashboardPalette.Accent),
                new("System uptime", "3d 07h 42m", DashboardPalette.Primary)
            ],
            Timeline: [28f, 34f, 51f, 43f, 39f, 63f, 58f, 72f, 61f, 56f, 67f, 49f, 44f, 57f, 62f]);
    }
}

internal static class DashboardPalette
{
    public static readonly SKColor Primary = SKColors.White;
    public static readonly SKColor Secondary = SKColor.Parse("#8fa8cc");
    public static readonly SKColor Accent = SKColor.Parse("#4ec9f5");
    public static readonly SKColor Accent2 = SKColor.Parse("#6df5c1");
    public static readonly SKColor Warning = SKColor.Parse("#ffd166");
    public static readonly SKColor Danger = SKColor.Parse("#ff6b6b");
}
