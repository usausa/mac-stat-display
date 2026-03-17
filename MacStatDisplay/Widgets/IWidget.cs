namespace MacStatDisplay.Widgets;

using SkiaSharp;

internal interface IWidget
{
    void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor, DrawHelper helper);
}
