namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;

using SkiaSharp;

internal interface IWidget
{
    void Initialize(IReadOnlyDictionary<string, string> parameters);

    void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor);
}
