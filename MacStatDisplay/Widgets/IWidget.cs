namespace MacStatDisplay.Widgets;

using MacStatDisplay.Monitor;
using SkiaSharp;

internal interface IWidget
{
    /// <summary>Called once after construction with widget-specific parameters from configuration.</summary>
    void Initialize(IReadOnlyDictionary<string, string> parameters) { }

    void Draw(SKCanvas canvas, SKRect rect, ISystemMonitor monitor);
}
