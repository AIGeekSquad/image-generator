using ImageGenerator.Core.Abstractions;
using ImageGenerator.Core.Providers;
using ImageGenerator.Core.Services;
using ImageGenerator.Tool.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Configure configuration sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

// Register image generation providers
builder.Services.AddSingleton<IImageGenerationProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    var endpoint = config["OpenAI:Endpoint"];
    var defaultModel = config["OpenAI:DefaultModel"];
    
    if (string.IsNullOrEmpty(apiKey))
    {
        sp.GetRequiredService<ILogger<Program>>().LogWarning(
            "OpenAI API key not configured. OpenAI provider will not be available. Set OPENAI_API_KEY environment variable or add to appsettings.json");
        return null!;
    }
    
    return new OpenAIImageProvider(apiKey, endpoint, defaultModel);
});

builder.Services.AddSingleton<IImageGenerationProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var projectId = config["Google:ProjectId"] ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");
    var location = config["Google:Location"] ?? "us-central1";
    var defaultModel = config["Google:DefaultModel"];
    
    if (string.IsNullOrEmpty(projectId))
    {
        sp.GetRequiredService<ILogger<Program>>().LogWarning(
            "Google Cloud project ID not configured. Google provider will not be available.");
        return null!;
    }
    
    return new GoogleImageProvider(projectId, location, defaultModel);
});

// Register image generation service
builder.Services.AddSingleton<IImageGenerationService>(sp =>
{
    var providers = sp.GetServices<IImageGenerationProvider>().Where(p => p != null).ToList();
    return new ImageGenerationService(providers);
});

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<ImageGenerationTools>();

await builder.Build().RunAsync();
