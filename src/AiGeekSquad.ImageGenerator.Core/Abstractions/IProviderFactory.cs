namespace AiGeekSquad.ImageGenerator.Core.Abstractions;

/// <summary>
/// Factory interface for creating image generation providers with dependency injection support
/// </summary>
public interface IProviderFactory
{
    /// <summary>
    /// The unique name of the provider this factory creates
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets metadata about the provider's capabilities and requirements
    /// </summary>
    ProviderMetadata GetMetadata();
    
    /// <summary>
    /// Checks if this factory can create a provider with the given service provider
    /// </summary>
    /// <param name="services">Service provider for dependency resolution</param>
    /// <returns>True if the provider can be created, false otherwise</returns>
    bool CanCreate(IServiceProvider services);
    
    /// <summary>
    /// Creates an instance of the image generation provider
    /// </summary>
    /// <param name="services">Service provider for dependency injection</param>
    /// <returns>A configured image generation provider instance</returns>
    IImageGenerationProvider Create(IServiceProvider services);
}