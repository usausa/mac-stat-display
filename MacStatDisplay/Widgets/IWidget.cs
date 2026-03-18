namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

internal interface IWidget
{
    void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper);
}
