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

// ---------------------------------------------------------------------------
// Color Palette
// ---------------------------------------------------------------------------

static class Palette
{
    public static readonly SKColor Background = new(5, 11, 22);
    public static readonly SKColor Panel = new(12, 24, 44, 210);
    public static readonly SKColor PanelAlt = new(10, 20, 36, 240);
    public static readonly SKColor Border = new(50, 100, 150, 70);
    public static readonly SKColor HeaderBg = new(8, 18, 38, 230);
    public static readonly SKColor Primary = new(225, 242, 255);
    public static readonly SKColor Secondary = new(120, 168, 204);
    public static readonly SKColor Accent = new(0, 200, 255);
    public static readonly SKColor Accent2 = new(90, 255, 196);
    public static readonly SKColor Warning = new(255, 196, 64);
    public static readonly SKColor Danger = new(255, 88, 100);
    public static readonly SKColor GaugeTrack = new(50, 80, 110, 70);
    public static readonly SKColor GridLine = new(40, 80, 120, 30);
}

// ---------------------------------------------------------------------------
// Dashboard Renderer
// ---------------------------------------------------------------------------

static class DashboardRenderer
{
    private const float Pad = 8f;
    private const float ColGap = 5f;
    private const float RowGap = 3f;
    private const float LabelH = 24f;
    private const float MetricH = 28f;
    private const float GaugeH = 130f;
    private const float BarGaugeH = 48f;
    private const float HeaderH = 44f;
    private const float CornerR = 6f;

    public static void Render(SKCanvas canvas, int width, int height, SystemSnapshot snap)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(snap);

        canvas.Clear(Palette.Background);
        DrawBackground(canvas, width, height);

        var header = new SKRect(Pad, Pad, width - Pad, Pad + HeaderH);
        DrawHeader(canvas, header, snap);

        var bodyTop = header.Bottom + ColGap;
        var bodyBottom = height - Pad;
        var bodyWidth = width - 2f * Pad;
        var totalGaps = 4f * ColGap;
        float[] colW = [256f, 224f, 256f, 240f, bodyWidth - totalGaps - 256f - 224f - 256f - 240f];
        var colX = new float[5];
        colX[0] = Pad;
        for (var i = 1; i < 5; i++)
        {
            colX[i] = colX[i - 1] + colW[i - 1] + ColGap;
        }

        DrawCpuColumn(canvas, colX[0], bodyTop, colW[0], snap);
        DrawCpuDetailColumn(canvas, colX[1], bodyTop, colW[1], bodyBottom, snap);
        DrawMemoryColumn(canvas, colX[2], bodyTop, colW[2], snap);
        DrawDiskNetworkColumn(canvas, colX[3], bodyTop, colW[3], snap);
        DrawHwPowerSystemColumn(canvas, colX[4], bodyTop, colW[4], snap);
    }

    private static void DrawBackground(SKCanvas canvas, int width, int height)
    {
        using var gradient = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [new SKColor(4, 10, 24), new SKColor(8, 22, 40), new SKColor(3, 8, 18)],
                [0f, 0.5f, 1f],
                SKShaderTileMode.Clamp),
            IsAntialias = true
        };
        canvas.DrawRect(0, 0, width, height, gradient);

        using var gridPaint = new SKPaint
        {
            Color = Palette.GridLine,
            StrokeWidth = 0.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };
        for (var x = 0; x <= width; x += 40)
        {
            canvas.DrawLine(x, 0, x, height, gridPaint);
        }

        for (var y = 0; y <= height; y += 40)
        {
            canvas.DrawLine(0, y, width, y, gridPaint);
        }

        using var glow = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(width * 0.2f, height * 0.15f),
                300f,
                [Palette.Accent.WithAlpha(30), SKColors.Transparent],
                [0f, 1f],
                SKShaderTileMode.Clamp),
            IsAntialias = true
        };
        canvas.DrawRect(0, 0, width, height, glow);
    }

    private static void DrawHeader(SKCanvas canvas, SKRect rect, SystemSnapshot snap)
    {
        DrawPanel(canvas, rect, Palette.HeaderBg);

        using var titlePaint = MakeText(20f, Palette.Primary, true);
        using var labelPaint = MakeText(10f, Palette.Secondary);
        using var valuePaint = MakeText(13f, Palette.Accent, true);

        canvas.DrawText("SYSTEM MONITOR", rect.Left + 12f, rect.MidY + 7f, titlePaint);

        var rightX = rect.Right - 10f;

        var timeText = snap.Timestamp;
        var timeWidth = valuePaint.MeasureText(timeText);
        canvas.DrawText(timeText, rightX - timeWidth, rect.MidY + 5f, valuePaint);
        var timeLabelW = labelPaint.MeasureText("TIME ");
        canvas.DrawText("TIME", rightX - timeWidth - timeLabelW - 4f, rect.MidY + 5f, labelPaint);

        var uptimeX = rightX - timeWidth - timeLabelW - 24f;
        var uptimeValW = valuePaint.MeasureText(snap.Uptime);
        canvas.DrawText(snap.Uptime, uptimeX - uptimeValW, rect.MidY + 5f, valuePaint);
        var uptimeLblW = labelPaint.MeasureText("UPTIME ");
        canvas.DrawText("UPTIME", uptimeX - uptimeValW - uptimeLblW - 4f, rect.MidY + 5f, labelPaint);
    }

    private static void DrawCpuColumn(SKCanvas canvas, float x, float y, float w, SystemSnapshot snap)
    {
        y = DrawSectionLabel(canvas, x, y, w, "CPU");
        y = DrawCircularGauge(canvas, x, y, w, GaugeH, snap.CpuUsage, "CPU", Palette.Accent);
        y += RowGap;
        foreach (var m in snap.CpuBasicMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }
    }

    private static void DrawCpuDetailColumn(SKCanvas canvas, float x, float y, float w, float bottom, SystemSnapshot snap)
    {
        y = DrawSectionLabel(canvas, x, y, w, "LOAD / FREQ");
        foreach (var m in snap.CpuDetailMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }

        y += RowGap;
        DrawTimeline(canvas, new SKRect(x, y, x + w, bottom), snap.CpuTimeline, "CPU TIMELINE", Palette.Accent);
    }

    private static void DrawMemoryColumn(SKCanvas canvas, float x, float y, float w, SystemSnapshot snap)
    {
        y = DrawSectionLabel(canvas, x, y, w, "MEMORY");
        y = DrawCircularGauge(canvas, x, y, w, GaugeH, snap.MemoryUsage, "MEM", Palette.Accent2);
        y += RowGap;
        foreach (var m in snap.MemoryMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }

        y += RowGap;
        y = DrawSectionLabel(canvas, x, y, w, "PROCESS");
        foreach (var m in snap.ProcessMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }
    }

    private static void DrawDiskNetworkColumn(SKCanvas canvas, float x, float y, float w, SystemSnapshot snap)
    {
        y = DrawSectionLabel(canvas, x, y, w, "DISK");
        y = DrawBarGauge(canvas, x, y, w, snap.DiskUsage, snap.DiskFreeTotal, Palette.Warning);
        y += RowGap;
        foreach (var m in snap.DiskMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }

        y += RowGap;
        y = DrawSectionLabel(canvas, x, y, w, "NETWORK");
        foreach (var m in snap.NetworkMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }
    }

    private static void DrawHwPowerSystemColumn(SKCanvas canvas, float x, float y, float w, SystemSnapshot snap)
    {
        y = DrawSectionLabel(canvas, x, y, w, "HW MONITOR");
        foreach (var m in snap.HwMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }

        y += RowGap;
        y = DrawSectionLabel(canvas, x, y, w, "POWER");
        foreach (var m in snap.PowerMetrics)
        {
            y = DrawMetricWidget(canvas, x, y, w, m);
            y += RowGap;
        }

        y += RowGap;
        y = DrawSectionLabel(canvas, x, y, w, "SYSTEM");
        DrawMetricWidget(canvas, x, y, w, new MetricWidget("Uptime", snap.Uptime, Palette.Accent2));
    }

    private static float DrawSectionLabel(SKCanvas canvas, float x, float y, float w, string title)
    {
        var rect = new SKRect(x, y, x + w, y + LabelH);
        DrawPanel(canvas, rect, Palette.PanelAlt);

        using var linePaint = new SKPaint
        {
            Color = Palette.Accent.WithAlpha(120),
            StrokeWidth = 2f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };
        canvas.DrawLine(rect.Left + 8f, rect.MidY, rect.Left + 22f, rect.MidY, linePaint);

        using var textPaint = MakeText(13f, Palette.Primary, true);
        canvas.DrawText(title, rect.Left + 28f, rect.MidY + 4.5f, textPaint);

        return rect.Bottom + RowGap;
    }

    private static float DrawMetricWidget(SKCanvas canvas, float x, float y, float w, MetricWidget metric)
    {
        var rect = new SKRect(x, y, x + w, y + MetricH);
        DrawPanel(canvas, rect, Palette.Panel);

        using var labelPaint = MakeText(10f, Palette.Secondary);
        using var valuePaint = MakeText(13f, metric.Tone, true);

        DrawWrappedLabel(canvas, rect, metric.Label, labelPaint);

        var valBounds = new SKRect();
        valuePaint.MeasureText(metric.Value, ref valBounds);
        canvas.DrawText(metric.Value, rect.Right - 8f - valBounds.Width, rect.MidY + 4.5f, valuePaint);

        return rect.Bottom;
    }

    private static float DrawCircularGauge(SKCanvas canvas, float x, float y, float w, float h, float percent, string title, SKColor color)
    {
        var rect = new SKRect(x, y, x + w, y + h);
        DrawPanel(canvas, rect, Palette.Panel);

        var center = new SKPoint(rect.MidX, rect.MidY + 4f);
        var radius = MathF.Min(rect.Width, rect.Height) * 0.30f;
        var arcRect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

        using var trackPaint = new SKPaint
        {
            Color = Palette.GaugeTrack,
            StrokeWidth = 9f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round
        };
        using var arcPaint = new SKPaint
        {
            Color = color,
            StrokeWidth = 9f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round
        };

        canvas.DrawArc(arcRect, 135f, 270f, false, trackPaint);
        canvas.DrawArc(arcRect, 135f, 270f * Math.Clamp(percent / 100f, 0f, 1f), false, arcPaint);

        using var titlePaint = MakeText(11f, Palette.Secondary, true);
        using var numberPaint = MakeText(20f, Palette.Primary, true);
        using var suffixPaint = MakeText(10f, color);

        DrawCentered(canvas, title, center.X, rect.Top + 16f, titlePaint);
        DrawCentered(canvas, $"{percent:0}%", center.X, center.Y + 7f, numberPaint);
        DrawCentered(canvas, "USAGE", center.X, center.Y + 22f, suffixPaint);

        return rect.Bottom;
    }

    private static float DrawBarGauge(SKCanvas canvas, float x, float y, float w, float percent, string summary, SKColor color)
    {
        var rect = new SKRect(x, y, x + w, y + BarGaugeH);
        DrawPanel(canvas, rect, Palette.Panel);

        using var summaryPaint = MakeText(10f, Palette.Secondary);
        using var pctPaint = MakeText(13f, color, true);

        var pctText = $"{percent:0}%";
        var pctBounds = new SKRect();
        pctPaint.MeasureText(pctText, ref pctBounds);
        canvas.DrawText(summary, rect.Left + 8f, rect.Top + 15f, summaryPaint);
        canvas.DrawText(pctText, rect.Right - 8f - pctBounds.Width, rect.Top + 15f, pctPaint);

        var barRect = new SKRect(rect.Left + 8f, rect.Top + 24f, rect.Right - 8f, rect.Bottom - 8f);
        using var trackFill = new SKPaint { IsAntialias = true, Color = Palette.GaugeTrack, Style = SKPaintStyle.Fill };
        using var barFill = new SKPaint { IsAntialias = true, Color = color.WithAlpha(180), Style = SKPaintStyle.Fill };
        using var barBorder = new SKPaint { IsAntialias = true, Color = color.WithAlpha(80), Style = SKPaintStyle.Stroke, StrokeWidth = 1f };

        canvas.DrawRoundRect(barRect, 4f, 4f, trackFill);
        var fillW = barRect.Width * Math.Clamp(percent / 100f, 0f, 1f);
        canvas.DrawRoundRect(new SKRect(barRect.Left, barRect.Top, barRect.Left + fillW, barRect.Bottom), 4f, 4f, barFill);
        canvas.DrawRoundRect(barRect, 4f, 4f, barBorder);

        return rect.Bottom;
    }

    private static void DrawTimeline(SKCanvas canvas, SKRect rect, float[] points, string title, SKColor color)
    {
        DrawPanel(canvas, rect, Palette.PanelAlt);

        using var titlePaint = MakeText(11f, Palette.Secondary, true);
        canvas.DrawText(title, rect.Left + 8f, rect.Top + 16f, titlePaint);

        var chartRect = new SKRect(rect.Left + 8f, rect.Top + 24f, rect.Right - 8f, rect.Bottom - 16f);

        using var gridPaint = new SKPaint { IsAntialias = true, Color = Palette.GridLine, StrokeWidth = 0.5f };
        for (var i = 0; i < 4; i++)
        {
            var gy = chartRect.Top + chartRect.Height / 3f * i;
            canvas.DrawLine(chartRect.Left, gy, chartRect.Right, gy, gridPaint);
        }

        using var linePaint = new SKPaint
        {
            IsAntialias = true,
            Color = color,
            StrokeWidth = 2f,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        using var fillPaint = new SKPaint { IsAntialias = true, Color = color.WithAlpha(24), Style = SKPaintStyle.Fill };

        using var path = new SKPath();
        using var fillPath = new SKPath();
        for (var i = 0; i < points.Length; i++)
        {
            var px = chartRect.Left + chartRect.Width * i / (points.Length - 1f);
            var py = chartRect.Bottom - chartRect.Height * (points[i] / 100f);
            if (i == 0)
            {
                path.MoveTo(px, py);
                fillPath.MoveTo(px, chartRect.Bottom);
                fillPath.LineTo(px, py);
            }
            else
            {
                path.LineTo(px, py);
                fillPath.LineTo(px, py);
            }
        }

        fillPath.LineTo(chartRect.Right, chartRect.Bottom);
        fillPath.Close();

        canvas.DrawPath(fillPath, fillPaint);
        canvas.DrawPath(path, linePaint);

        using var axisLabelPaint = MakeText(9f, Palette.Secondary);
        canvas.DrawText("0s", chartRect.Left, rect.Bottom - 4f, axisLabelPaint);
        var nowBounds = new SKRect();
        axisLabelPaint.MeasureText("now", ref nowBounds);
        canvas.DrawText("now", chartRect.Right - nowBounds.Width, rect.Bottom - 4f, axisLabelPaint);
    }

    private static void DrawPanel(SKCanvas canvas, SKRect rect, SKColor color)
    {
        using var fill = new SKPaint { IsAntialias = true, Color = color, Style = SKPaintStyle.Fill };
        using var border = new SKPaint { IsAntialias = true, Color = Palette.Border, StrokeWidth = 1f, Style = SKPaintStyle.Stroke };
        canvas.DrawRoundRect(rect, CornerR, CornerR, fill);
        canvas.DrawRoundRect(rect, CornerR, CornerR, border);
    }

    private static void DrawWrappedLabel(SKCanvas canvas, SKRect rect, string label, SKPaint paint)
    {
        var maxW = rect.Width * 0.52f;
        if (paint.MeasureText(label) <= maxW)
        {
            canvas.DrawText(label, rect.Left + 8f, rect.MidY + 3.5f, paint);
            return;
        }

        var words = label.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var line1 = words[0];
        var line2 = string.Empty;
        for (var i = 1; i < words.Length; i++)
        {
            var candidate = $"{line1} {words[i]}";
            if (paint.MeasureText(candidate) <= maxW)
            {
                line1 = candidate;
            }
            else
            {
                line2 = string.Join(' ', words[i..]);
                break;
            }
        }

        if (string.IsNullOrEmpty(line2))
        {
            canvas.DrawText(label, rect.Left + 8f, rect.MidY + 3.5f, paint);
        }
        else
        {
            canvas.DrawText(line1, rect.Left + 8f, rect.MidY - 2f, paint);
            canvas.DrawText(line2, rect.Left + 8f, rect.MidY + 9f, paint);
        }
    }

    private static void DrawCentered(SKCanvas canvas, string text, float cx, float baselineY, SKPaint paint)
    {
        var bounds = new SKRect();
        paint.MeasureText(text, ref bounds);
        canvas.DrawText(text, cx - bounds.Width / 2f, baselineY, paint);
    }

    private static SKPaint MakeText(float size, SKColor color, bool bold = false)
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
}

// ---------------------------------------------------------------------------
// Data Model
// ---------------------------------------------------------------------------

internal sealed record MetricWidget(string Label, string Value, SKColor Tone);

internal sealed record SystemSnapshot(
    float CpuUsage,
    float MemoryUsage,
    float DiskUsage,
    string DiskFreeTotal,
    string Uptime,
    string Timestamp,
    MetricWidget[] CpuBasicMetrics,
    MetricWidget[] CpuDetailMetrics,
    MetricWidget[] MemoryMetrics,
    MetricWidget[] DiskMetrics,
    MetricWidget[] NetworkMetrics,
    MetricWidget[] ProcessMetrics,
    MetricWidget[] HwMetrics,
    MetricWidget[] PowerMetrics,
    float[] CpuTimeline)
{
    public static SystemSnapshot CreateSample()
    {
        return new SystemSnapshot(
            CpuUsage: 62f,
            MemoryUsage: 74f,
            DiskUsage: 68f,
            DiskFreeTotal: "412 GB / 1.28 TB",
            Uptime: "3d 07h 42m",
            Timestamp: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            CpuBasicMetrics:
            [
                new("CPU System", "18 %", Palette.Warning),
                new("CPU User", "44 %", Palette.Accent),
                new("CPU Idle", "38 %", Palette.Secondary),
                new("E-Core Usage", "49 %", Palette.Accent),
                new("P-Core Usage", "71 %", Palette.Danger)
            ],
            CpuDetailMetrics:
            [
                new("Load Avg 1m", "4.38", Palette.Primary),
                new("Load Avg 5m", "3.92", Palette.Primary),
                new("Load Avg 15m", "3.27", Palette.Primary),
                new("CPU Freq", "3860 MHz", Palette.Primary),
                new("E-Core Freq", "2980 MHz", Palette.Accent),
                new("P-Core Freq", "4210 MHz", Palette.Danger)
            ],
            MemoryMetrics:
            [
                new("Used Memory", "23.7 GB", Palette.Primary),
                new("App Memory", "9.4 GB", Palette.Primary),
                new("Wired Memory", "6.8 GB", Palette.Primary),
                new("Swap Used", "512 MB", Palette.Warning)
            ],
            DiskMetrics:
            [
                new("Disk Read", "982 KB/s", Palette.Accent2),
                new("Disk Write", "1,248 KB/s", Palette.Accent)
            ],
            NetworkMetrics:
            [
                new("Download", "12.4 MB/s", Palette.Accent),
                new("Upload", "2.8 MB/s", Palette.Accent2)
            ],
            ProcessMetrics:
            [
                new("Processes", "418", Palette.Primary),
                new("Threads", "5,932", Palette.Primary)
            ],
            HwMetrics:
            [
                new("CPU Temp", "71 ℃", Palette.Warning),
                new("GPU Temp", "64 ℃", Palette.Accent2)
            ],
            PowerMetrics:
            [
                new("System Power", "82 W", Palette.Primary),
                new("CPU Power", "31 W", Palette.Warning),
                new("GPU Power", "24 W", Palette.Accent)
            ],
            CpuTimeline: [28f, 34f, 51f, 43f, 39f, 63f, 58f, 72f, 61f, 56f, 67f, 49f, 44f, 57f, 62f]);
    }
};
