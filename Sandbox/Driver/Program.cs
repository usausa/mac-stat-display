using HidSharp;

var device = DeviceList.Local
    .GetHidDevices(LcdDriver.TrofeoVision.ScreenDevice.VendorId, LcdDriver.TrofeoVision.ScreenDevice.ProductId)
    .FirstOrDefault();
if (device is null)
{
    Console.WriteLine("Device not found.");
    return;
}

using var screen = new LcdDriver.TrofeoVision.ScreenDevice(device);

var jpegBytes = await File.ReadAllBytesAsync("image-1280x480.jpg");

var interval = TimeSpan.FromSeconds(1);
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    // ReSharper disable once AccessToDisposedClosure
    cts.Cancel();
};

while (!cts.Token.IsCancellationRequested)
{
    screen.DrawJpeg(jpegBytes);

    try
    {
        await Task.Delay(interval, cts.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
}
