using MacStatDisplay;

using Serilog;

// Builder
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options => options.ReadFrom.Configuration(builder.Configuration));

// Display settings
var displaySettings = builder.Configuration.GetSection("Display").Get<DisplaySettings>() ?? new DisplaySettings();
builder.Services.AddSingleton(displaySettings);

// System monitor
if (displaySettings.UseMock)
{
    builder.Services.AddSingleton<ISystemMonitor, MockSystemMonitor>();
}
else
{
    builder.Services.AddSingleton<ISystemMonitor, SystemMonitor>();
}

// Worker
builder.Services.AddHostedService<Worker>();

// Build
var host = builder.Build();

host.Run();
