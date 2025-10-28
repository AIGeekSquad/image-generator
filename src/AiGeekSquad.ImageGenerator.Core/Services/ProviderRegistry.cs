using AiGeekSquad.ImageGenerator.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Default implementation of provider registry for managing provider factories
/// </summary>
public class ProviderRegistry : IProviderRegistry
{
    private readonly IEnumerable<IProviderFactory> _factories;
    private readonly ILogger<ProviderRegistry> _logger;

    /// <summary>
    /// Initializes the provider registry with available factories
    /// </summary>
    /// <param name="factories">Collection of provider factories</param>
    /// <param name="logger">Logger for diagnostic messages</param>
    public ProviderRegistry(
        IEnumerable<IProviderFactory> factories,
        ILogger<ProviderRegistry> logger)
    {
        _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var factoryList = _factories.ToList();
        _logger.LogInformation("Registered {Count} provider factories: {Factories}",
            factoryList.Count, 
            string.Join(", ", factoryList.Select(f => f.Name)));
    }

    /// <summary>
    /// Get all registered provider factories
    /// </summary>
    /// <returns>Collection of all registered factories</returns>
    public IEnumerable<IProviderFactory> GetFactories()
    {
        return _factories;
    }

    /// <summary>
    /// Get a specific provider factory by name
    /// </summary>
    /// <param name="name">Provider name (case-insensitive)</param>
    /// <returns>Factory if found, null otherwise</returns>
    public IProviderFactory? GetFactory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return _factories.FirstOrDefault(f => 
            f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all factories that can currently create providers
    /// </summary>
    /// <param name="services">Service provider for checking availability</param>
    /// <returns>Collection of available factories</returns>
    public IEnumerable<IProviderFactory> GetAvailableFactories(IServiceProvider services)
    {
        var available = new List<IProviderFactory>();
        
        foreach (var factory in _factories)
        {
            try
            {
                if (factory.CanCreate(services))
                {
                    available.Add(factory);
                }
                else
                {
                    _logger.LogDebug("Factory '{FactoryName}' is not available (missing requirements)", 
                        factory.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking availability for factory '{FactoryName}'", 
                    factory.Name);
            }
        }

        _logger.LogDebug("Found {Count} available factories out of {Total}", 
            available.Count, _factories.Count());
        
        return available;
    }

    /// <summary>
    /// Get factories that support a specific operation
    /// </summary>
    /// <param name="operation">Required operation</param>
    /// <param name="services">Service provider for checking availability</param>
    /// <returns>Collection of compatible factories</returns>
    public IEnumerable<IProviderFactory> GetFactoriesForOperation(
        ImageOperation operation, 
        IServiceProvider services)
    {
        var compatible = new List<IProviderFactory>();
        
        foreach (var factory in GetAvailableFactories(services))
        {
            try
            {
                var metadata = factory.GetMetadata();
                if (metadata.Capabilities.SupportedOperations.Contains(operation))
                {
                    compatible.Add(factory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking operation support for factory '{FactoryName}'", 
                    factory.Name);
            }
        }

        _logger.LogDebug("Found {Count} factories supporting operation '{Operation}'", 
            compatible.Count, operation);
        
        return compatible;
    }

    /// <summary>
    /// Get factories that support a specific model
    /// </summary>
    /// <param name="model">Model name or identifier</param>
    /// <param name="services">Service provider for checking availability</param>
    /// <returns>Collection of compatible factories</returns>
    public IEnumerable<IProviderFactory> GetFactoriesForModel(
        string model, 
        IServiceProvider services)
    {
        if (string.IsNullOrWhiteSpace(model))
            return Enumerable.Empty<IProviderFactory>();

        var compatible = new List<IProviderFactory>();
        
        foreach (var factory in GetAvailableFactories(services))
        {
            try
            {
                var metadata = factory.GetMetadata();
                var capabilities = metadata.Capabilities;
                
                // Check if model is explicitly listed
                var hasExplicitModel = capabilities.ExampleModels.Any(m => 
                    m.Equals(model, StringComparison.OrdinalIgnoreCase));
                
                // Check if provider accepts custom models
                var acceptsCustomModels = capabilities.AcceptsCustomModels;
                
                if (hasExplicitModel || acceptsCustomModels)
                {
                    compatible.Add(factory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking model support for factory '{FactoryName}'", 
                    factory.Name);
            }
        }

        _logger.LogDebug("Found {Count} factories supporting model '{Model}'", 
            compatible.Count, model);
        
        return compatible;
    }
}