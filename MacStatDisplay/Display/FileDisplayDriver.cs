namespace MacStatDisplay.Display;

using SkiaSharp;

internal sealed class FileDisplayDriver : IDisplayDriver
{
    public int Width => 1280;

    public int Height => 480;

    public int RefreshIntervalSeconds => 0;

    public void Dispose()
    {
    }

    public bool Initialize() => true;

    public void Draw(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

        var filePath = Path.Combine(AppContext.BaseDirectory, "display.jpg");
        using var stream = File.Create(filePath);
        data.SaveTo(stream);
    }
}
