using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Adapters;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiGeekSquad.ImageGenerator.Core.Factories;

/// <summary>
/// Factory for creating OpenAI image generation providers with dependency injection
/// </summary>
public class OpenAIProviderFactory : IProviderFactory
{
    /// <summary>
    /// The unique name of the provider this factory creates
    /// </summary>
    public string Name => "OpenAI";

    /// <summary>
    /// Gets metadata about the OpenAI provider's capabilities and requirements
    /// </summary>
    public ProviderMetadata GetMetadata()
    {
        return new ProviderMetadata
        {
            Name = Name,
            Description = "OpenAI image generation provider supporting DALL-E 3, DALL-E 2, and GPT Image models",
            Capabilities = new ProviderCapabilities
            {
                ExampleModels = new List<string>
                {
                    ImageModels.OpenAI.DallE3,
                    ImageModels.OpenAI.DallE2,
                    ImageModels.OpenAI.GPTImage1,
                    ImageModels.OpenAI.GPTImage1Mini
                },
                SupportedOperations = new List<ImageOperation>
                {
                    ImageOperation.Generate,
                    ImageOperation.Edit,
                    ImageOperation.Variation
                },
                DefaultModel = ImageModels.OpenAI.Default,
                AcceptsCustomModels = true,
                SupportsMultiModalInput = false,
                Features = new Dictionary<string, object>
                {
                    ["supportsQuality"] = true,
                    ["supportsStyle"] = true,
                    ["maxImages"] = 10,
                    ["supportedSizes"] = new[]
                    {
                        ImageModels.Sizes.Square1024,
                        ImageModels.Sizes.Wide1792x1024,
                        ImageModels.Sizes.Tall1024x1792,
                        ImageModels.Sizes.Square512,
                        ImageModels.Sizes.Square256
                    }
                }
            },
            Requirements = new ProviderRequirements
            {
                RequiredEnvironmentVariables = new List<string> { "OPENAI_API_KEY" },
                RequiredConfigurationSections = new List<string>(),
                RequiredDependencies = new List<string> { "Azure.AI.OpenAI" },
                OptionalDependencies = new List<string>()
            },
            Priority = 100 // Standard priority
        };
    }

    /// <summary>
    /// Checks if this factory can create a provider with the given service provider
    /// </summary>
    /// <param name="services">Service provider for dependency resolution</param>
    /// <returns>True if the provider can be created, false otherwise</returns>
    public bool CanCreate(IServiceProvider services)
    {
        try
        {
            var configuration = services.GetService<IConfiguration>();
            
            // Check for API key in configuration or environment
            var apiKey = configuration?["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            
            return !string.IsNullOrWhiteSpace(apiKey);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates an instance of the OpenAI image generation provider
    /// </summary>
    /// <param name="services">Service provider for dependency injection</param>
    /// <returns>A configured OpenAI image generation provider instance</returns>
    public IImageGenerationProvider Create(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        
        // Get configuration values
        var apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key not found. Set OPENAI_API_KEY environment variable or OpenAI:ApiKey in configuration.");
        }
        
        var endpoint = configuration["OpenAI:Endpoint"];
        var defaultModel = configuration["OpenAI:DefaultModel"];
        
        // Create named HttpClient for OpenAI
        var httpClient = httpClientFactory.CreateClient("OpenAI");
        
        // Create OpenAI provider with the correct constructor signature
        return new OpenAIImageProvider(apiKey, endpoint, defaultModel, httpClient);
    }
}