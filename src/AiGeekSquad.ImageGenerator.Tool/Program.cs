using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Extensibility;
using AiGeekSquad.ImageGenerator.Core.Providers;
using AiGeekSquad.ImageGenerator.Core.Services;
using AiGeekSquad.ImageGenerator.Tool.Tools;
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
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Register provider loader
builder.Services.AddSingleton<IProviderLoader, AssemblyProviderLoader>();

// Register HttpClient for providers
builder.Services.AddHttpClient();

// Check if at least one provider is configured
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var googleProjectId = builder.Configuration["Google:ProjectId"] ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

if (string.IsNullOrEmpty(openAiApiKey) && string.IsNullOrEmpty(googleProjectId))
{
    throw new InvalidOperationException(
        "No image generation providers are configured. " +
        "Please configure at least one provider by setting either:\n" +
        "- OPENAI_API_KEY environment variable or OpenAI:ApiKey in appsettings.json\n" +
        "- GOOGLE_PROJECT_ID environment variable or Google:ProjectId in appsettings.json");
}

// Register built-in providers
if (!string.IsNullOrEmpty(openAiApiKey))
{
    builder.Services.AddSingleton<IImageGenerationProvider>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("OpenAI");
        var endpoint = config["OpenAI:Endpoint"];
        var defaultModel = config["OpenAI:DefaultModel"];
        
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Registering OpenAI provider (API key configured)");
        
        return new OpenAIImageProvider(openAiApiKey, endpoint, defaultModel, httpClient);
    });
}

if (!string.IsNullOrEmpty(googleProjectId))
{
    builder.Services.AddSingleton<IImageGenerationProvider>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("Google");
        var location = config["Google:Location"] ?? "us-central1";
        var defaultModel = config["Google:DefaultModel"];
        
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Registering Google provider (project ID configured), location: {Location}", 
            location);
        
        return new GoogleImageProvider(googleProjectId, location, defaultModel, httpClient);
    });
}

// Load external providers from assemblies if specified via command line or config
var providerAssemblies = builder.Configuration.GetSection("ExternalProviders:Assemblies").Get<string[]>()
    ?? args.Where(a => a.StartsWith("--provider-assembly="))
           .Select(a => a.Substring("--provider-assembly=".Length))
           .ToArray();

if (providerAssemblies?.Length > 0)
{
    builder.Services.AddSingleton<IEnumerable<IImageGenerationProvider>>(sp =>
    {
        var loader = sp.GetRequiredService<IProviderLoader>();
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var externalProviders = new List<IImageGenerationProvider>();

        foreach (var assemblyPath in providerAssemblies)
        {
            logger.LogInformation("Loading providers from: {AssemblyPath}", assemblyPath);
            var providers = loader.LoadProvidersFromAssembly(assemblyPath);
            externalProviders.AddRange(providers);
        }

        logger.LogInformation("Loaded {Count} external provider(s)", externalProviders.Count);
        return externalProviders;
    });
}

// Register image generation service
builder.Services.AddSingleton<IImageGenerationService>(sp =>
{
    var builtInProviders = sp.GetServices<IImageGenerationProvider>().Where(p => p != null);
    var externalProviders = sp.GetService<IEnumerable<IImageGenerationProvider>>() ?? Enumerable.Empty<IImageGenerationProvider>();
    
    var allProviders = builtInProviders.Concat(externalProviders).ToList();
    
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Registered {Count} provider(s): {Providers}", 
        allProviders.Count, 
        string.Join(", ", allProviders.Select(p => p.ProviderName)));
    
    return new ImageGenerationService(allProviders);
});

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<ImageGenerationTools>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting AiGeekSquad Image Generator MCP Server...");

await app.RunAsync();
