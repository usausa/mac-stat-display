namespace MacStatDisplay.Settings;

internal sealed record DisplaySettings
{
    public int UpdatePeriod { get; init; } = 3;

    public bool UseMock { get; init; }
}
