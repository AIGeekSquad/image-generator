using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Tool.Tools;
using AiGeekSquad.ImageGenerator.Core.Services;
using AiGeekSquad.ImageGenerator.Core.Factories;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AiGeekSquad.ImageGenerator.Tests.E2E.Fixtures;

/// <summary>
/// Test fixture for creating MCP server instances for E2E testing
/// </summary>
public class McpServerFixture : IAsyncLifetime
{
    private IHost? _host;
    private ImageGenerationTools? _tools;
    private IServiceProvider? _services;

    /// <summary>
    /// Configuration for testing
    /// </summary>
    public IConfiguration Configuration { get; private set; } = null!;
    
    /// <summary>
    /// Whether the fixture has API keys configured for testing
    /// </summary>
    public bool HasApiKeys { get; private set; }
    
    /// <summary>
    /// Skip reason if no API keys are available
    /// </summary>
    public string SkipReason { get; private set; } = string.Empty;

    /// <summary>
    /// Initialize the MCP server for testing
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Create test configuration
        var configData = new Dictionary<string, string?>
        {
            ["OpenAI:ApiKey"] = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            ["Google:ProjectId"] = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"),
            ["Google:Location"] = "us-central1",
            ["Logging:LogLevel:Default"] = "Warning" // Reduce noise in tests
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Check API key availability
        var openAiKey = Configuration["OpenAI:ApiKey"];
        var googleProjectId = Configuration["Google:ProjectId"];
        
        HasApiKeys = !string.IsNullOrEmpty(openAiKey) || !string.IsNullOrEmpty(googleProjectId);
        SkipReason = HasApiKeys ? string.Empty : 
            "No API keys configured. Set OPENAI_API_KEY or GOOGLE_PROJECT_ID environment variables.";

        // Build test host
        var builder = Host.CreateApplicationBuilder();
        
        // Configure logging to capture output
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
        
        // Override configuration
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddConfiguration(Configuration);

        // Register services
        builder.Services.AddHttpClient();
        
        // Register provider factories
        builder.Services.AddSingleton<IProviderFactory, OpenAIProviderFactory>();
        builder.Services.AddSingleton<IProviderFactory, GoogleProviderFactory>();
        
        // Register provider registry
        builder.Services.AddSingleton<IProviderRegistry, ProviderRegistry>();
        
        // Register argument parser
        builder.Services.AddSingleton<IArgumentParser, McpArgumentParser>();
        
        // Register provider selection strategy
        builder.Services.AddSingleton<IProviderSelectionStrategy, SmartProviderSelector>();
        
        // Register image generation service (updated to use factories)
        builder.Services.AddSingleton<IImageGenerationService>(sp =>
        {
            var registry = sp.GetRequiredService<IProviderRegistry>();
            var availableFactories = registry.GetAvailableFactories(sp);
            var providers = availableFactories.Select(f => f.Create(sp)).ToList();
            
            return new ImageGenerationService(providers);
        });

        // Register MCP tools
        builder.Services.AddSingleton<ImageGenerationTools>();

        _host = builder.Build();
        _services = _host.Services;
        _tools = _services.GetRequiredService<ImageGenerationTools>();
        
        await _host.StartAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Send a request to the MCP tools (simulates MCP client call)
    /// </summary>
    public async Task<McpResponse> SendMcpRequest(
        string toolName, 
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken = default)
    {
        if (_tools == null)
            throw new InvalidOperationException("Fixture not initialized");

        try
        {
            string response = toolName switch
            {
                "generate_image" => await _tools.GenerateImage(
                    arguments.GetValueOrDefault("prompt")?.ToString() ?? string.Empty,
                    arguments.GetValueOrDefault("provider")?.ToString(),
                    arguments.GetValueOrDefault("model")?.ToString(),
                    arguments.GetValueOrDefault("size")?.ToString(),
                    arguments.GetValueOrDefault("quality")?.ToString(),
                    arguments.GetValueOrDefault("style")?.ToString(),
                    GetInt(arguments, "numberOfImages", 1)),

                "generate_image_from_conversation" => await _tools.GenerateImageFromConversation(
                    arguments.GetValueOrDefault("conversationJson")?.ToString() ?? "[]",
                    arguments.GetValueOrDefault("provider")?.ToString(),
                    arguments.GetValueOrDefault("model")?.ToString(),
                    arguments.GetValueOrDefault("size")?.ToString(),
                    arguments.GetValueOrDefault("quality")?.ToString(),
                    arguments.GetValueOrDefault("style")?.ToString(),
                    GetInt(arguments, "numberOfImages", 1)),

                "edit_image" => await _tools.EditImage(
                    arguments.GetValueOrDefault("image")?.ToString() ?? string.Empty,
                    arguments.GetValueOrDefault("prompt")?.ToString() ?? string.Empty,
                    arguments.GetValueOrDefault("mask")?.ToString(),
                    arguments.GetValueOrDefault("provider")?.ToString(),
                    arguments.GetValueOrDefault("model")?.ToString(),
                    arguments.GetValueOrDefault("size")?.ToString(),
                    GetInt(arguments, "numberOfImages", 1)),

                "create_variation" => await _tools.CreateVariation(
                    arguments.GetValueOrDefault("image")?.ToString() ?? string.Empty,
                    arguments.GetValueOrDefault("provider")?.ToString(),
                    arguments.GetValueOrDefault("model")?.ToString(),
                    arguments.GetValueOrDefault("size")?.ToString(),
                    GetInt(arguments, "numberOfImages", 1)),

                "list_providers" => _tools.ListProviders(),

                _ => throw new ArgumentException($"Unknown tool: {toolName}")
            };

            return new McpResponse
            {
                IsSuccess = !response.Contains("\"error\""),
                Content = response,
                ToolName = toolName
            };
        }
        catch (Exception ex)
        {
            return new McpResponse
            {
                IsSuccess = false,
                Content = JsonSerializer.Serialize(new { error = ex.Message }),
                ToolName = toolName,
                Exception = ex
            };
        }
    }

    /// <summary>
    /// Get available providers from the service
    /// </summary>
    public List<string> GetAvailableProviders()
    {
        if (_services == null)
            return new List<string>();

        var service = _services.GetService<IImageGenerationService>();
        return service?.GetProviders().Select(p => p.ProviderName).ToList() ?? new List<string>();
    }

    /// <summary>
    /// Configure a provider to simulate failure
    /// </summary>
    public void ConfigureProviderToFail(string providerName)
    {
        // This would be implemented with a test double or mock
        // For now, we'll document the need for this capability
    }

    private static int GetInt(Dictionary<string, object?> args, string key, int defaultValue)
    {
        if (!args.TryGetValue(key, out var value) || value == null)
            return defaultValue;

        return value switch
        {
            int intValue => intValue,
            string strValue when int.TryParse(strValue, out var parsed) => parsed,
            double doubleValue => (int)doubleValue,
            float floatValue => (int)floatValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Clean up resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync(TestContext.Current.CancellationToken);
            _host.Dispose();
        }
    }
}

/// <summary>
/// Response from an MCP tool call
/// </summary>
public class McpResponse
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// JSON response content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the tool that was called
    /// </summary>
    public string ToolName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
}

/// <summary>
/// Extension methods for MCP responses
/// </summary>
public static class McpResponseExtensions
{
    /// <summary>
    /// Checks if the response contains an error
    /// </summary>
    public static bool ContainError(this McpResponse response)
    {
        return !response.IsSuccess || response.Content.Contains("\"error\"");
    }
    
    /// <summary>
    /// Checks if the response is successful
    /// </summary>
    public static bool BeSuccessful(this McpResponse response)
    {
        return response.IsSuccess && !response.Content.Contains("\"error\"");
    }
}