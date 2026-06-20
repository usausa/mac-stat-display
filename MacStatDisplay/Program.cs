using System.Runtime.InteropServices;

using MacStatDisplay;
using MacStatDisplay.Display;
using MacStatDisplay.Monitor;
using MacStatDisplay.Settings;

using Serilog;

// Builder
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.SetBasePath(AppContext.BaseDirectory);

// Service
builder.Services
    .AddWindowsService()
    .AddSystemd();

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options => options.ReadFrom.Configuration(builder.Configuration));

// Display settings
var displaySettings = builder.Configuration.GetSection("Display").Get<DisplaySettings>() ?? new DisplaySettings();
builder.Services.AddSingleton(displaySettings);

// System monitor
builder.Services.AddSingleton<ISystemMonitor>(_ => SystemMonitorFactory.Create(displaySettings.Monitor));

// Display driver
builder.Services.AddSingleton<IDisplayDriver>(_ => DisplayDriverFactory.Create(displaySettings.Driver));

// Worker
builder.Services.AddHostedService<Worker>();

// Build
var host = builder.Build();

// Startup
var log = host.Services.GetRequiredService<ILogger<Program>>();
log.InfoServiceStart();
log.InfoServiceSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
log.InfoServiceSettingsEnvironment(typeof(Program).Assembly.GetName().Version, Environment.CurrentDirectory);

host.Run();
