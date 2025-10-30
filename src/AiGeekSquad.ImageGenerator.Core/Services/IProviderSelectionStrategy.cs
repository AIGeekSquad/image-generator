using AiGeekSquad.ImageGenerator.Core.Abstractions;

namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Interface for provider selection strategies
/// </summary>
public interface IProviderSelectionStrategy
{
    /// <summary>
    /// Selects the best available provider based on the given context
    /// </summary>
    /// <param name="context">Selection context with preferences and requirements</param>
    /// <param name="services">Service provider for creating instances</param>
    /// <returns>Selected provider instance</returns>
    Task<IImageGenerationProvider> SelectProviderAsync(
        ProviderSelectionContext context, 
        IServiceProvider services);
    
    /// <summary>
    /// Gets a prioritized list of provider options for the given context
    /// </summary>
    /// <param name="context">Selection context</param>
    /// <param name="services">Service provider for checking availability</param>
    /// <returns>Ordered list of suitable providers</returns>
    Task<List<IImageGenerationProvider>> GetProviderOptionsAsync(
        ProviderSelectionContext context, 
        IServiceProvider services);
}
