namespace MacStatDisplay.Display;

using HidSharp;

using LcdDriver.TrofeoVision;

using SkiaSharp;

internal sealed class TrofeoVisionDisplayDriver(int quality) : IDisplayDriver
{
    private ScreenDevice? screen;

    private int quarterTurns;

    private SKData? frame;

    public int Width => 1280;

    public int Height => 480;

    public int RefreshIntervalSeconds => 1;

    public void Dispose()
    {
        screen?.Dispose();
        frame?.Dispose();
    }

    public bool Initialize()
    {
        screen?.Dispose();
        screen = null;

        var hidDevice = DeviceList.Local.GetHidDevices(UsbIds.VendorId, UsbIds.ProductId).FirstOrDefault();
        if (hidDevice is null)
        {
            return false;
        }

        screen = new ScreenDevice(hidDevice);
        if (screen.Handshake() is not { } info)
        {
            screen.Dispose();
            screen = null;
            return false;
        }

        quarterTurns = (int)info.GetRotateOption(ScreenOrientation.Landscape);
        return true;
    }

    public void Draw(SKSurface surface)
    {
        using var image = surface.Snapshot();
        var data = Encode(image);
        frame?.Dispose();

        frame = data;
        screen!.DrawJpeg(frame.AsSpan());
    }

    public void Refresh()
    {
        if (frame is not null)
        {
            screen!.DrawJpeg(frame.AsSpan());
        }
    }

    private SKData Encode(SKImage image)
    {
        if (quarterTurns == 0)
        {
            return image.Encode(SKEncodedImageFormat.Jpeg, quality);
        }

        var swap = (quarterTurns % 2) != 0;
        var width = swap ? image.Height : image.Width;
        var height = swap ? image.Width : image.Height;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Translate(width / 2f, height / 2f);
        canvas.RotateDegrees(quarterTurns * 90);
        canvas.Translate(-image.Width / 2f, -image.Height / 2f);
        canvas.DrawImage(image, 0, 0, SKSamplingOptions.Default);
        using var rotated = surface.Snapshot();
        return rotated.Encode(SKEncodedImageFormat.Jpeg, quality);
    }
}
