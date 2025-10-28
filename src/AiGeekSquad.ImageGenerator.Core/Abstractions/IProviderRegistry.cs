namespace AiGeekSquad.ImageGenerator.Core.Abstractions;

/// <summary>
/// Registry for managing and discovering image generation provider factories
/// </summary>
public interface IProviderRegistry
{
    /// <summary>
    /// Get all registered provider factories
    /// </summary>
    /// <returns>Collection of all registered factories</returns>
    IEnumerable<IProviderFactory> GetFactories();
    
    /// <summary>
    /// Get a specific provider factory by name
    /// </summary>
    /// <param name="name">Provider name (case-insensitive)</param>
    /// <returns>Factory if found, null otherwise</returns>
    IProviderFactory? GetFactory(string name);
    
    /// <summary>
    /// Get all factories that can currently create providers
    /// </summary>
    /// <param name="services">Service provider for checking availability</param>
    /// <returns>Collection of available factories</returns>
    IEnumerable<IProviderFactory> GetAvailableFactories(IServiceProvider services);
    
    /// <summary>
    /// Get factories that support a specific operation
    /// </summary>
    /// <param name="operation">Required operation</param>
    /// <param name="services">Service provider for checking availability</param>
    /// <returns>Collection of compatible factories</returns>
    IEnumerable<IProviderFactory> GetFactoriesForOperation(
        ImageOperation operation, 
        IServiceProvider services);
    
    /// <summary>
    /// Get factories that support a specific model
    /// </summary>
    /// <param name="model">Model name or identifier</param>
    /// <param name="services">Service provider for checking availability</param>
    /// <returns>Collection of compatible factories</returns>
    IEnumerable<IProviderFactory> GetFactoriesForModel(
        string model, 
        IServiceProvider services);
}