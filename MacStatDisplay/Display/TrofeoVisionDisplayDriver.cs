namespace MacStatDisplay.Display;

using HidSharp;

using LcdDriver.TrofeoVision;

using SkiaSharp;

internal sealed class TrofeoVisionDisplayDriver : IDisplayDriver
{
    private ScreenDevice? screen;

    public int Width => 1280;

    public int Height => 480;

    public int RefreshIntervalSeconds => 1;

    public void Dispose()
    {
        screen?.Dispose();
    }

    public bool Initialize()
    {
        var hidDevice = DeviceList.Local
            .GetHidDevices(ScreenDevice.VendorId, ScreenDevice.ProductId)
            .FirstOrDefault();
        if (hidDevice is null)
        {
            screen = null;
            return false;
        }

        screen = new ScreenDevice(hidDevice);
        return true;
    }

    public void Draw(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        screen!.DrawJpeg(data.ToArray());
    }
}
