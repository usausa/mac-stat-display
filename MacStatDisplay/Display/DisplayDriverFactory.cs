namespace MacStatDisplay.Display;

internal static class DisplayDriverFactory
{
    internal static IDisplayDriver Create(string driver) =>
        driver switch
        {
            "TrofeoVision" => new TrofeoVisionDisplayDriver(),
            "File" => new FileDisplayDriver(),
            _ => throw new InvalidOperationException($"Unsupported display driver: {driver}")
        };
}
