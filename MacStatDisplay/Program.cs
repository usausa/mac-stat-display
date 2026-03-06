using MacStatDisplay;

using Serilog;

// Builder
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options => options.ReadFrom.Configuration(builder.Configuration));

// Worker
builder.Services.AddHostedService<Worker>();

// Build
var host = builder.Build();

host.Run();
