using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiGeekSquad.ImageGenerator.Core.Factories;

/// <summary>
/// Factory for creating Google image generation providers with dependency injection
/// </summary>
public class GoogleProviderFactory : IProviderFactory
{
    /// <summary>
    /// The unique name of the provider this factory creates
    /// </summary>
    public string Name => "Google";

    /// <summary>
    /// Gets metadata about the Google provider's capabilities and requirements
    /// </summary>
    public ProviderMetadata GetMetadata()
    {
        return new ProviderMetadata
        {
            Name = Name,
            Description = "Google image generation provider supporting Imagen 3 and Imagen 2 models",
            Capabilities = new ProviderCapabilities
            {
                ExampleModels = new List<string>
                {
                    ImageModels.Google.Imagen3,
                    ImageModels.Google.Imagen2,
                    ImageModels.Google.ImagenFast
                },
                SupportedOperations = new List<ImageOperation>
                {
                    ImageOperation.Generate
                },
                DefaultModel = ImageModels.Google.Default,
                AcceptsCustomModels = true,
                SupportsMultiModalInput = false,
                Features = new Dictionary<string, object>
                {
                    ["supportsAspectRatio"] = true,
                    ["maxImages"] = 4,
                    ["supportedLanguages"] = new[] { "en", "es", "fr", "de", "it", "pt", "hi", "ja", "ko", "zh" },
                    ["supportedSafety"] = true
                }
            },
            Requirements = new ProviderRequirements
            {
                RequiredEnvironmentVariables = new List<string> { "GOOGLE_PROJECT_ID" },
                RequiredConfigurationSections = new List<string>(),
                RequiredDependencies = new List<string> { "Google.Cloud.AIPlatform.V1" },
                OptionalDependencies = new List<string>()
            },
            Priority = 90 // Slightly lower than OpenAI by default
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
            
            // Check for project ID in configuration or environment
            var projectId = configuration?["Google:ProjectId"] ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");
            
            return !string.IsNullOrWhiteSpace(projectId);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates an instance of the Google image generation provider
    /// </summary>
    /// <param name="services">Service provider for dependency injection</param>
    /// <returns>A configured Google image generation provider instance</returns>
    public IImageGenerationProvider Create(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        
        // Get configuration values
        var projectId = configuration["Google:ProjectId"] ?? Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new InvalidOperationException(
                "Google Project ID not found. Set GOOGLE_PROJECT_ID environment variable or Google:ProjectId in configuration.");
        }
        
        var location = configuration["Google:Location"] ?? "us-central1";
        var defaultModel = configuration["Google:DefaultModel"];
        
        // Create named HttpClient for Google
        var httpClient = httpClientFactory.CreateClient("Google");
        
        return new GoogleImageProvider(projectId, location, defaultModel, httpClient);
    }
}