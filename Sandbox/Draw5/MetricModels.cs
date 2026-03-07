namespace Draw5;

using SkiaSharp;

/// <summary>ウィジェット表示タイプ</summary>
internal enum WidgetType
{
    Value,
    CircularGauge,
    BarGauge,
}

/// <summary>1 つの計測項目</summary>
internal sealed record MetricItem(
    string Label,
    string Value,
    string Unit,
    WidgetType Type = WidgetType.Value,
    string? Detail = null);

/// <summary>セクション単位のグループ</summary>
internal sealed record MetricSection(string Name, SKColor AccentColor, IReadOnlyList<MetricItem> Items);

/// <summary>デモ用サンプルデータ</summary>
internal static class SampleMetrics
{
    public static IReadOnlyList<MetricSection> Create() =>
    [
        new("CPU", new SKColor(0, 200, 255),
        [
            new("CPU使用率", "35", "%", WidgetType.CircularGauge, "P 42% / E 28%"),
            new("システム", "5.2", "%"),
            new("ユーザー", "29.8", "%"),
            new("アイドル", "65.0", "%"),
            new("Eコア使用率", "28", "%"),
            new("Pコア使用率", "42", "%"),
            new("Load 1m", "1.52", ""),
            new("Load 5m", "1.28", ""),
            new("Load 15m", "0.95", ""),
            new("CPU周波数", "3200", "MHz"),
            new("Eコア周波数", "2400", "MHz"),
            new("Pコア周波数", "3800", "MHz"),
        ]),
        new("メモリ", new SKColor(255, 100, 150),
        [
            new("使用率", "72", "%", WidgetType.CircularGauge, "使用済み 11.5 GB"),
            new("使用済み", "11.5", "GB"),
            new("アプリケーション", "8.2", "GB"),
            new("ワイヤード", "2.8", "GB"),
            new("スワップ", "512", "MB"),
        ]),
        new("ディスク", new SKColor(255, 210, 60),
        [
            new("使用率", "58", "%", WidgetType.BarGauge, "198 / 475 GB"),
            new("空き / 容量", "198 / 475", "GB"),
            new("読み込み", "152.3", "KB/s"),
            new("書き込み", "84.7", "KB/s"),
        ]),
        new("ネットワーク", new SKColor(100, 210, 120),
        [
            new("DL", "5.23", "MB/s"),
            new("UP", "1.12", "MB/s"),
        ]),
        new("プロセス", new SKColor(170, 130, 255),
        [
            new("プロセス数", "352", ""),
            new("スレッド数", "1,847", ""),
        ]),
        new("HWモニター", new SKColor(255, 150, 70),
        [
            new("CPU温度", "62", "℃"),
            new("GPU温度", "55", "℃"),
            new("電力合計", "48.5", "W"),
            new("電力:CPU", "28.2", "W"),
            new("電力:GPU", "15.8", "W"),
        ]),
    ];
}
