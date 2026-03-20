namespace MacStatDisplay.Settings;

internal sealed record DisplaySettings
{
    public bool UseMock { get; init; }

    public string Driver { get; init; } = default!;

    public int UpdatePeriod { get; init; } = 3;

    public int DeviceRetrySeconds { get; init; } = 5;

    public GridSettings Grid { get; init; } = new();

    public List<WidgetEntry> Widgets { get; init; } = [];
}

internal sealed record GridSettings
{
    public int Columns { get; init; } = 4;

    public int Rows { get; init; } = 4;
}

internal sealed record WidgetEntry
{
    public string Type { get; init; } = default!;

    public int Column { get; init; }

    public int Row { get; init; }

    public int ColumnSpan { get; init; } = 1;

    public int RowSpan { get; init; } = 1;

    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> Parameters { get; init; } = [];
}
