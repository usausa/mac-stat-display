namespace MacStatDisplay.Display;

using SkiaSharp;

internal interface IDisplayDriver : IDisposable
{
    int Width { get; }

    int Height { get; }

    int RefreshIntervalSeconds { get; }

    bool Initialize();

    void Draw(SKSurface surface);
}
