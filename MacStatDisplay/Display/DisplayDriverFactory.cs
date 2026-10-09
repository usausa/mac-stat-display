namespace MacStatDisplay.Display;

internal static class DisplayDriverFactory
{
    internal static IDisplayDriver Create(string driver, int quality) =>
        driver switch
        {
            "TrofeoVision" => new TrofeoVisionDisplayDriver(quality),
            "File" => new FileDisplayDriver(quality),
            _ => throw new InvalidOperationException($"Unsupported display driver: {driver}")
        };
}
