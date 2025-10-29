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
    /// Configuration key constants to avoid magic strings
    /// </summary>
    private static class ConfigurationKeys
    {
        // OpenAI configuration keys
        public const string OpenAIApiKey = "OpenAI:ApiKey";
        public const string OpenAIEndpoint = "OpenAI:Endpoint";
        public const string OpenAIDefaultModel = "OpenAI:DefaultModel";

        // Google configuration keys
        public const string GoogleProjectId = "Google:ProjectId";
        public const string GoogleLocation = "Google:Location";
        public const string GoogleDefaultModel = "Google:DefaultModel";

        // External providers configuration
        public const string ExternalProvidersAssemblies = "ExternalProviders:Assemblies";

        // Environment variable names
        public const string OpenAIApiKeyEnv = "OPENAI_API_KEY";
        public const string GoogleProjectIdEnv = "GOOGLE_PROJECT_ID";

        // Command line argument prefixes
        public const string ProviderAssemblyArgPrefix = "--provider-assembly=";
    }

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
        var openAiApiKey = builder.Configuration[ConfigurationKeys.OpenAIApiKey] ?? Environment.GetEnvironmentVariable(ConfigurationKeys.OpenAIApiKeyEnv);
        var googleProjectId = builder.Configuration[ConfigurationKeys.GoogleProjectId] ?? Environment.GetEnvironmentVariable(ConfigurationKeys.GoogleProjectIdEnv);

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
                $"- {ConfigurationKeys.OpenAIApiKeyEnv} environment variable or {ConfigurationKeys.OpenAIApiKey} in appsettings.json\n" +
                $"- {ConfigurationKeys.GoogleProjectIdEnv} environment variable or {ConfigurationKeys.GoogleProjectId} in appsettings.json");
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
            var endpoint = config[ConfigurationKeys.OpenAIEndpoint];
            var defaultModel = config[ConfigurationKeys.OpenAIDefaultModel];

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
            var location = config[ConfigurationKeys.GoogleLocation] ?? "us-central1";
            var defaultModel = config[ConfigurationKeys.GoogleDefaultModel];

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
        var providerAssemblies = builder.Configuration.GetSection(ConfigurationKeys.ExternalProvidersAssemblies).Get<string[]>()
            ?? args.Where(a => a.StartsWith(ConfigurationKeys.ProviderAssemblyArgPrefix))
                   .Select(a => a.Substring(ConfigurationKeys.ProviderAssemblyArgPrefix.Length))
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
            var builtInProviders = sp.GetServices<IImageGenerationProvider>();
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
