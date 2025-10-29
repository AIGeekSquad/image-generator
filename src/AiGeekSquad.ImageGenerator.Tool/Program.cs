using AiGeekSquad.ImageGenerator.Tool.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure all services using the host builder configurator
HostBuilderConfigurator.ConfigureServices(builder, args);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting AiGeekSquad Image Generator MCP Server...");

await app.RunAsync();
