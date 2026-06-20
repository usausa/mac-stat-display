namespace MacStatDisplay;

internal static partial class Log
{
    // Startup

    [LoggerMessage(Level = LogLevel.Information, Message = "Service start.")]
    public static partial void InfoServiceStart(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Runtime: os=[{osDescription}], framework=[{frameworkDescription}], rid=[{runtimeIdentifier}]")]
    public static partial void InfoServiceSettingsRuntime(this ILogger logger, string osDescription, string frameworkDescription, string runtimeIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "Environment: version=[{version}], directory=[{directory}]")]
    public static partial void InfoServiceSettingsEnvironment(this ILogger logger, Version? version, string directory);

    // Setting

    [LoggerMessage(Level = LogLevel.Warning, Message = "Widget placement is out of grid range. type=[{type}], column=[{column}], row=[{row}], columnSpan=[{columnSpan}], rowSpan=[{rowSpan}]")]
    public static partial void WarnWidgetPlacementOutOfRange(this ILogger logger, string type, int column, int row, int columnSpan, int rowSpan);

    // Error

    [LoggerMessage(Level = LogLevel.Error, Message = "Unknown exception.")]
    public static partial void ErrorUnknownException(this ILogger logger, Exception ex);
}
