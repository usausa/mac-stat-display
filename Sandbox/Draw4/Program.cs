using System.Globalization;
using SkiaSharp;

const int canvasWidth = 1280;
const int canvasHeight = 480;
const int headerHeight = 76;
const int outerPadding = 18;
const int contentGap = 12;
const int columns = 4;
const int rows = 4;

var metrics = DashboardMetrics.CreateSample();
using var surface = SKSurface.Create(new SKImageInfo(canvasWidth, canvasHeight));
var canvas = surface.Canvas;

canvas.Clear(new SKColor(11, 15, 20));

DrawBackground(canvas, canvasWidth, canvasHeight);
DrawHeader(canvas, metrics, canvasWidth);
DrawWidgets(canvas, metrics, canvasWidth, canvasHeight);

using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using var stream = File.Open("output.png", FileMode.Create, FileAccess.Write, FileShare.None);
data.SaveTo(stream);

static void DrawBackground(SKCanvas canvas, int width, int height)
{
    using var paint = new SKPaint
    {
        Shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(width, height),
            new[]
            {
                new SKColor(15, 20, 28),
                new SKColor(8, 10, 16)
            },
            null,
            SKShaderTileMode.Clamp),
        IsAntialias = true
    };

    canvas.DrawRect(new SKRect(0, 0, width, height), paint);
}

static void DrawHeader(SKCanvas canvas, DashboardMetrics metrics, int width)
{
    var headerRect = new SKRect(outerPadding, outerPadding, width - outerPadding, outerPadding + headerHeight);

    using var panelPaint = CreatePanelPaint();
    using var borderPaint = CreateBorderPaint();
    canvas.DrawRoundRect(headerRect, 18, 18, panelPaint);
    canvas.DrawRoundRect(headerRect, 18, 18, borderPaint);

    using var titlePaint = CreateTextPaint(26, SKColors.White, true);
    using var subtitlePaint = CreateTextPaint(13, new SKColor(151, 161, 176));
    using var accentPaint = CreateTextPaint(16, new SKColor(127, 225, 255), true);
    using var valuePaint = CreateTextPaint(18, SKColors.White, true);

    canvas.DrawText("MAC STAT DISPLAY", headerRect.Left + 20, headerRect.Top + 30, titlePaint);
    canvas.DrawText("Realtime System Monitor", headerRect.Left + 20, headerRect.Top + 52, subtitlePaint);

    DrawHeaderMetric(canvas, width - 420, headerRect.Top + 24, "UPTIME", metrics.Uptime, accentPaint, valuePaint);
    DrawHeaderMetric(canvas, width - 250, headerRect.Top + 24, "TIME", metrics.Timestamp, accentPaint, valuePaint);
}

static void DrawHeaderMetric(SKCanvas canvas, float x, float y, string label, string value, SKPaint labelPaint, SKPaint valuePaint)
{
    canvas.DrawText(label, x, y, labelPaint);
    canvas.DrawText(value, x, y + 24, valuePaint);
}

static void DrawWidgets(SKCanvas canvas, DashboardMetrics metrics, int width, int height)
{
    var gridTop = outerPadding + headerHeight + contentGap;
    var gridHeight = height - gridTop - outerPadding;
    var cellWidth = (width - (outerPadding * 2) - (contentGap * (columns - 1))) / (float)columns;
    var cellHeight = (gridHeight - (contentGap * (rows - 1))) / (float)rows;

    var widgets = new WidgetPlacement[]
    {
        new(0, 0, 1, 2, new("CPU", "Usage", $"{metrics.CpuUsage:0}%", "System 18%|User 54%|Idle 28%", metrics.CpuUsage, WidgetStyle.Ring, new SKColor(88, 166, 255))),
        new(1, 0, 1, 2, new("MEM", "Usage", $"{metrics.MemoryUsage:0}%", $"Used {metrics.MemoryUsedGb:0.0} GB|App {metrics.AppMemoryGb:0.0} GB|Swap {metrics.SwapUsedMb:0} MB", metrics.MemoryUsage, WidgetStyle.Ring, new SKColor(180, 120, 255))),
        new(2, 0, 1, 1, new("CPU", "Load Avg", metrics.LoadAverageShort, metrics.LoadAverageLong, null, WidgetStyle.TextOnly, new SKColor(127, 225, 255))),
        new(3, 0, 1, 1, new("CPU", "Clock", $"{metrics.CpuClockMHz:0} MHz", $"P {metrics.CpuPClockMHz:0} / E {metrics.CpuEClockMHz:0}", null, WidgetStyle.TextOnly, new SKColor(196, 181, 253))),
        new(2, 1, 1, 1, new("DISK", "Capacity", $"{metrics.DiskUsage:0}%", $"{metrics.DiskFreeGb:0} GB free / {metrics.DiskTotalGb:0} GB", metrics.DiskUsage, WidgetStyle.Bar, new SKColor(255, 99, 132))),
        new(3, 1, 1, 1, new("NET", "Download", $"{metrics.DownloadMb:0.00} MB/s", $"Upload {metrics.UploadMb:0.00} MB/s", null, WidgetStyle.TextOnly, new SKColor(94, 234, 212))),
        new(0, 2, 1, 1, new("MEM", "App Memory", $"{metrics.AppMemoryGb:0.0} GB", $"Wired {metrics.WiredMemoryGb:0.0} GB", null, WidgetStyle.TextOnly, new SKColor(255, 214, 102))),
        new(1, 2, 1, 1, new("MEM", "Swap", $"{metrics.SwapUsedMb:0} MB", "Compressed 512 MB", null, WidgetStyle.TextOnly, new SKColor(255, 214, 102))),
        new(2, 2, 1, 1, new("DISK", "Read", $"{metrics.DiskReadKb:0} KB/s", string.Empty, null, WidgetStyle.TextOnly, new SKColor(255, 154, 162))),
        new(3, 2, 1, 1, new("DISK", "Write", $"{metrics.DiskWriteKb:0} KB/s", string.Empty, null, WidgetStyle.TextOnly, new SKColor(255, 154, 162))),
        new(0, 3, 1, 1, new("PROC", "Processes", metrics.ProcessCount.ToString(CultureInfo.InvariantCulture), $"Threads {metrics.ThreadCount}", null, WidgetStyle.TextOnly, new SKColor(244, 114, 182))),
        new(1, 3, 1, 1, new(string.Empty, "CPU Temp", $"{metrics.CpuTempC:0} C", $"Power {metrics.CpuPowerW:0.0} W", null, WidgetStyle.TextOnly, new SKColor(251, 146, 60))),
        new(2, 3, 1, 1, new(string.Empty, "GPU Temp", $"{metrics.GpuTempC:0} C", $"Power {metrics.GpuPowerW:0.0} W", null, WidgetStyle.TextOnly, new SKColor(96, 165, 250))),
        new(3, 3, 1, 1, new("POWER", "System", $"{metrics.SystemPowerW:0.0} W", string.Empty, null, WidgetStyle.TextOnly, new SKColor(250, 204, 21)))
    };

    foreach (var widget in widgets)
    {
        var x = outerPadding + (widget.Column * (cellWidth + contentGap));
        var y = gridTop + (widget.Row * (cellHeight + contentGap));
        var widgetWidth = (cellWidth * widget.ColumnSpan) + (contentGap * (widget.ColumnSpan - 1));
        var widgetHeight = (cellHeight * widget.RowSpan) + (contentGap * (widget.RowSpan - 1));
        var rect = new SKRect(x, y, x + widgetWidth, y + widgetHeight);
        DrawWidget(canvas, rect, widget.Model);
    }
}

static void DrawWidget(SKCanvas canvas, SKRect rect, WidgetModel widget)
{
    using var panelPaint = CreatePanelPaint();
    using var borderPaint = CreateBorderPaint();
    using var categoryPaint = CreateTextPaint(16, SKColors.White, true);
    using var titlePaint = CreateTextPaint(16, SKColors.White, true);
    using var valuePaint = CreateTextPaint(32, widget.Accent, true);
    using var subPaint = CreateTextPaint(12, new SKColor(148, 163, 184));

    canvas.DrawRoundRect(rect, 16, 16, panelPaint);
    canvas.DrawRoundRect(rect, 16, 16, borderPaint);

    switch (widget.Style)
    {
        case WidgetStyle.Ring:
            DrawRingGauge(canvas, rect, widget, categoryPaint, titlePaint, valuePaint, subPaint);
            break;
        case WidgetStyle.Bar:
            DrawTitleBlock(canvas, rect, widget, categoryPaint, titlePaint);
            DrawBarGauge(canvas, rect, widget, valuePaint, subPaint);
            break;
        default:
            DrawTitleBlock(canvas, rect, widget, categoryPaint, titlePaint);
            DrawTextWidget(canvas, rect, widget, valuePaint, subPaint);
            break;
    }
}

static void DrawTitleBlock(SKCanvas canvas, SKRect rect, WidgetModel widget, SKPaint categoryPaint, SKPaint titlePaint)
{
    var left = rect.Left + 14;
    var top = rect.Top + 24;
    var label = string.IsNullOrWhiteSpace(widget.Category) ? widget.Title : $"{widget.Category} {widget.Title}";
    canvas.DrawText(label, left, top, titlePaint);
}

static void DrawRingGauge(SKCanvas canvas, SKRect rect, WidgetModel widget, SKPaint categoryPaint, SKPaint titlePaint, SKPaint valuePaint, SKPaint subPaint)
{
    var center = new SKPoint(rect.MidX, rect.MidY + 12);
    const float radius = 50;
    var detailX = rect.Right - 18;
    var detailTop = rect.Top + 54;

    using var trackPaint = new SKPaint
    {
        Color = new SKColor(42, 48, 61),
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 10,
        StrokeCap = SKStrokeCap.Round
    };

    using var valueArcPaint = new SKPaint
    {
        Color = widget.Accent,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 10,
        StrokeCap = SKStrokeCap.Round
    };

    var ringRect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
    DrawTitleBlock(canvas, rect, widget, categoryPaint, titlePaint);
    canvas.DrawArc(ringRect, 135, 270, false, trackPaint);
    canvas.DrawArc(ringRect, 135, (float)(270 * ((widget.Progress ?? 0) / 100f)), false, valueArcPaint);

    using var centerValuePaint = CreateTextPaint(38, widget.Accent, true);
    DrawCenteredText(canvas, widget.Value, center.X, center.Y + 12, centerValuePaint);

    var details = widget.Details.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    for (var index = 0; index < details.Length; index++)
    {
        DrawRightAlignedText(canvas, details[index], detailX, detailTop + (index * 18), subPaint);
    }
}

static void DrawBarGauge(SKCanvas canvas, SKRect rect, WidgetModel widget, SKPaint valuePaint, SKPaint subPaint)
{
    DrawRightAlignedText(canvas, widget.Value, rect.Right - 16, rect.MidY + 8, valuePaint);
    DrawSecondaryDetails(canvas, rect, widget.Details, subPaint);

    var barRect = new SKRect(rect.Left + 16, rect.Bottom - 26, rect.Right - 16, rect.Bottom - 16);
    using var trackPaint = new SKPaint { Color = new SKColor(42, 48, 61), IsAntialias = true };
    using var fillPaint = new SKPaint { Color = widget.Accent, IsAntialias = true };
    canvas.DrawRoundRect(barRect, 5, 5, trackPaint);

    var fillWidth = (barRect.Width * (widget.Progress ?? 0)) / 100f;
    canvas.DrawRoundRect(new SKRect(barRect.Left, barRect.Top, barRect.Left + (float)fillWidth, barRect.Bottom), 5, 5, fillPaint);
}

static void DrawTextWidget(SKCanvas canvas, SKRect rect, WidgetModel widget, SKPaint valuePaint, SKPaint subPaint)
{
    DrawRightAlignedText(canvas, widget.Value, rect.Right - 16, rect.MidY + 8, valuePaint);
    DrawSecondaryDetails(canvas, rect, widget.Details, subPaint);
}

static void DrawSecondaryDetails(SKCanvas canvas, SKRect rect, string details, SKPaint subPaint)
{
    if (string.IsNullOrWhiteSpace(details))
    {
        return;
    }

    DrawWrappedSubtext(canvas, details, rect.Left + 16, rect.MidY + 8, rect.Width - 32, subPaint, SKTextAlign.Left);
}

static void DrawRightAlignedText(SKCanvas canvas, string text, float right, float y, SKPaint paint)
{
    var measuredWidth = paint.MeasureText(text);
    canvas.DrawText(text, right - measuredWidth, y, paint);
}

static void DrawCenteredText(SKCanvas canvas, string text, float centerX, float y, SKPaint paint)
{
    var measuredWidth = paint.MeasureText(text);
    canvas.DrawText(text, centerX - (measuredWidth / 2f), y, paint);
}

static void DrawWrappedSubtext(SKCanvas canvas, string text, float x, float y, float maxWidth, SKPaint paint, SKTextAlign align)
{
    var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var line = string.Empty;
    var lineIndex = 0;
    var drawX = align == SKTextAlign.Right ? x + maxWidth : x;

    using var textPaint = CreateTextPaint(paint.TextSize, paint.Color);
    textPaint.TextAlign = align;

    foreach (var word in words)
    {
        var candidate = string.IsNullOrEmpty(line) ? word : $"{line} {word}";
        if (textPaint.MeasureText(candidate) <= maxWidth)
        {
            line = candidate;
            continue;
        }

        canvas.DrawText(line, drawX, y + (lineIndex * 14), textPaint);
        line = word;
        lineIndex++;
        if (lineIndex == 1)
        {
            continue;
        }

        break;
    }

    if (!string.IsNullOrEmpty(line) && lineIndex < 2)
    {
        canvas.DrawText(line, drawX, y + (lineIndex * 14), textPaint);
    }
}

static SKPaint CreatePanelPaint() => new()
{
    Color = new SKColor(18, 24, 34, 235),
    IsAntialias = true,
    Style = SKPaintStyle.Fill
};

static SKPaint CreateBorderPaint() => new()
{
    Color = new SKColor(255, 255, 255, 20),
    IsAntialias = true,
    Style = SKPaintStyle.Stroke,
    StrokeWidth = 1
};

static SKPaint CreateTextPaint(float size, SKColor color, bool bold = false) => new()
{
    Color = color,
    TextSize = size,
    IsAntialias = true,
    Typeface = bold ? SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) : SKTypeface.FromFamilyName("Arial")
};

enum WidgetStyle
{
    Ring,
    Bar,
    TextOnly
}

sealed record WidgetModel(
    string Category,
    string Title,
    string Value,
    string Details,
    double? Progress,
    WidgetStyle Style,
    SKColor Accent);

sealed record WidgetPlacement(
    int Column,
    int Row,
    int ColumnSpan,
    int RowSpan,
    WidgetModel Model);

sealed record DashboardMetrics(
    double CpuUsage,
    double CpuPCore,
    double CpuECore,
    string LoadAverageShort,
    string LoadAverageLong,
    double CpuClockMHz,
    double CpuPClockMHz,
    double CpuEClockMHz,
    double MemoryUsage,
    double MemoryUsedGb,
    double MemoryTotalGb,
    double AppMemoryGb,
    double WiredMemoryGb,
    double SwapUsedMb,
    double DiskUsage,
    double DiskFreeGb,
    double DiskTotalGb,
    double DiskReadKb,
    double DiskWriteKb,
    double DownloadMb,
    double UploadMb,
    int ProcessCount,
    int ThreadCount,
    double CpuTempC,
    double GpuTempC,
    double SystemPowerW,
    double CpuPowerW,
    double GpuPowerW,
    string Uptime,
    string Timestamp)
{
    public static DashboardMetrics CreateSample()
    {
        return new DashboardMetrics(
            CpuUsage: 72,
            CpuPCore: 84,
            CpuECore: 36,
            LoadAverageShort: "1m 7.22",
            LoadAverageLong: "5m 6.18  15m 5.04",
            CpuClockMHz: 3480,
            CpuPClockMHz: 3725,
            CpuEClockMHz: 2680,
            MemoryUsage: 64,
            MemoryUsedGb: 20.4,
            MemoryTotalGb: 32.0,
            AppMemoryGb: 9.6,
            WiredMemoryGb: 4.1,
            SwapUsedMb: 824,
            DiskUsage: 58,
            DiskFreeGb: 412,
            DiskTotalGb: 980,
            DiskReadKb: 812,
            DiskWriteKb: 296,
            DownloadMb: 12.8,
            UploadMb: 2.4,
            ProcessCount: 428,
            ThreadCount: 5480,
            CpuTempC: 67,
            GpuTempC: 58,
            SystemPowerW: 42.7,
            CpuPowerW: 18.6,
            GpuPowerW: 11.4,
            Uptime: "3d 08h 14m",
            Timestamp: DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }
}
