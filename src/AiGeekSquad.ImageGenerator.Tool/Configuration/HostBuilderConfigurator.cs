using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Extensibility;
using AiGeekSquad.ImageGenerator.Core.Providers;
using AiGeekSquad.ImageGenerator.Core.Services;
using AiGeekSquad.ImageGenerator.Tool.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiGeekSquad.ImageGenerator.Tool.Configuration;

/// <summary>
/// Configures the host builder for the MCP server with image generation services and providers
/// </summary>
public static class HostBuilderConfigurator
{
    /// <summary>
    /// Configures all services for the image generation MCP server
    /// </summary>
    /// <param name="builder">Host application builder</param>
    /// <param name="args">Command line arguments</param>
    public static void ConfigureServices(HostApplicationBuilder builder, string[] args)
    {
        ConfigureLogging(builder);
        ConfigureConfiguration(builder, args);
        RegisterCoreServices(builder);
        RegisterProviders(builder, args);
        RegisterMcpServices(builder);
    }

    /// <summary>
    /// Configures logging to use stderr for all output (stdout is reserved for MCP protocol)
    /// </summary>
    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
    }

    /// <summary>
    /// Configures configuration sources
    /// </summary>
    private static void ConfigureConfiguration(HostApplicationBuilder builder, string[] args)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    }

    /// <summary>
    /// Registers core services
    /// </summary>
    private static void RegisterCoreServices(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IProviderLoader, AssemblyProviderLoader>();
        builder.Services.AddHttpClient();
    }

    /// <summary>
    /// Registers image generation providers (built-in and external)
    /// </summary>
    private static void RegisterProviders(HostApplicationBuilder builder, string[] args)
    {
        // Get API keys from configuration or environment
        var openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var googleProjectId = builder.Configuration["Google:ProjectId"] ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

        // Validate at least one provider is configured
        ValidateProviderConfiguration(openAiApiKey, googleProjectId);

        // Register built-in providers
        RegisterOpenAIProvider(builder, openAiApiKey);
        RegisterGoogleProvider(builder, googleProjectId);

        // Register external providers
        RegisterExternalProviders(builder, args);

        // Register image generation service
        RegisterImageGenerationService(builder);
    }

    /// <summary>
    /// Validates that at least one provider is configured
    /// </summary>
    private static void ValidateProviderConfiguration(string? openAiApiKey, string? googleProjectId)
    {
        if (string.IsNullOrEmpty(openAiApiKey) && string.IsNullOrEmpty(googleProjectId))
        {
            throw new InvalidOperationException(
                "No image generation providers are configured. " +
                "Please configure at least one provider by setting either:\n" +
                "- OPENAI_API_KEY environment variable or OpenAI:ApiKey in appsettings.json\n" +
                "- GOOGLE_PROJECT_ID environment variable or Google:ProjectId in appsettings.json");
        }
    }

    /// <summary>
    /// Registers OpenAI provider if API key is configured
    /// </summary>
    private static void RegisterOpenAIProvider(HostApplicationBuilder builder, string? openAiApiKey)
    {
        if (string.IsNullOrEmpty(openAiApiKey))
            return;

        builder.Services.AddSingleton<IImageGenerationProvider>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("OpenAI");
            var endpoint = config["OpenAI:Endpoint"];
            var defaultModel = config["OpenAI:DefaultModel"];

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HostBuilderConfigurator");
            logger.LogInformation("Registering OpenAI provider (API key configured)");

            return new OpenAIImageProvider(openAiApiKey, endpoint, defaultModel, httpClient);
        });
    }

    /// <summary>
    /// Registers Google provider if project ID is configured
    /// </summary>
    private static void RegisterGoogleProvider(HostApplicationBuilder builder, string? googleProjectId)
    {
        if (string.IsNullOrEmpty(googleProjectId))
            return;

        builder.Services.AddSingleton<IImageGenerationProvider>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("Google");
            var location = config["Google:Location"] ?? "us-central1";
            var defaultModel = config["Google:DefaultModel"];

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HostBuilderConfigurator");
            logger.LogInformation("Registering Google provider (project ID configured), location: {Location}",
                location);

            return new GoogleImageProvider(googleProjectId, location, defaultModel, httpClient);
        });
    }

    /// <summary>
    /// Registers external providers loaded from assemblies
    /// </summary>
    private static void RegisterExternalProviders(HostApplicationBuilder builder, string[] args)
    {
        var providerAssemblies = builder.Configuration.GetSection("ExternalProviders:Assemblies").Get<string[]>()
            ?? args.Where(a => a.StartsWith("--provider-assembly="))
                   .Select(a => a.Substring("--provider-assembly=".Length))
                   .ToArray();

        if (providerAssemblies?.Length > 0)
        {
            builder.Services.AddSingleton<IEnumerable<IImageGenerationProvider>>(sp =>
            {
                var loader = sp.GetRequiredService<IProviderLoader>();
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HostBuilderConfigurator");
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
    }

    /// <summary>
    /// Registers the image generation service that aggregates all providers
    /// </summary>
    private static void RegisterImageGenerationService(HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IImageGenerationService>(sp =>
        {
            var builtInProviders = sp.GetServices<IImageGenerationProvider>().Where(p => p != null);
            var externalProviders = sp.GetService<IEnumerable<IImageGenerationProvider>>() ?? Enumerable.Empty<IImageGenerationProvider>();

            var allProviders = builtInProviders.Concat(externalProviders).ToList();

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("HostBuilderConfigurator");
            logger.LogInformation("Registered {Count} provider(s): {Providers}",
                allProviders.Count,
                string.Join(", ", allProviders.Select(p => p.ProviderName)));

            return new ImageGenerationService(allProviders);
        });
    }

    /// <summary>
    /// Registers MCP server services
    /// </summary>
    private static void RegisterMcpServices(HostApplicationBuilder builder)
    {
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<ImageGenerationTools>();
    }
}
