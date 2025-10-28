using AiGeekSquad.ImageGenerator.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Context for provider selection decisions
/// </summary>
public class ProviderSelectionContext
{
    /// <summary>
    /// Explicitly requested provider name (highest priority)
    /// </summary>
    public string? PreferredProvider { get; set; }
    
    /// <summary>
    /// Required model for the operation
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// Required operation capability
    /// </summary>
    public ImageOperation Operation { get; set; } = ImageOperation.Generate;
    
    /// <summary>
    /// Additional capabilities required
    /// </summary>
    public List<string> RequiredCapabilities { get; set; } = new();
    
    /// <summary>
    /// Previous providers that have failed (for fallback logic)
    /// </summary>
    public HashSet<string> FailedProviders { get; set; } = new();
}

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

/// <summary>
/// Smart provider selection strategy with fallback support
/// </summary>
public class SmartProviderSelector : IProviderSelectionStrategy
{
    private readonly IProviderRegistry _registry;
    private readonly ILogger<SmartProviderSelector> _logger;

    /// <summary>
    /// Initializes the provider selector
    /// </summary>
    public SmartProviderSelector(
        IProviderRegistry registry, 
        ILogger<SmartProviderSelector> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Selects the best available provider based on the given context
    /// </summary>
    public async Task<IImageGenerationProvider> SelectProviderAsync(
        ProviderSelectionContext context, 
        IServiceProvider services)
    {
        var options = await GetProviderOptionsAsync(context, services);
        
        if (!options.Any())
        {
            ThrowNoSuitableProvidersException(context, services);
        }

        var selected = options[0];
        _logger.LogInformation("Selected provider '{Provider}' for operation '{Operation}'", 
            selected.ProviderName, context.Operation);
        
        return selected;
    }
    
    private void ThrowNoSuitableProvidersException(
        ProviderSelectionContext context,
        IServiceProvider services)
    {
        var availableProviders = _registry.GetAvailableFactories(services)
            .Select(f => f.Name)
            .ToList();
        
        var modelInfo = context.Model != null ? $" with model '{context.Model}'" : "";
        var message = $"No suitable providers found for operation '{context.Operation}'{modelInfo}" +
                     $". Available providers: {string.Join(", ", availableProviders)}";
        
        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Gets a prioritized list of provider options for the given context
    /// </summary>
    public async Task<List<IImageGenerationProvider>> GetProviderOptionsAsync(
        ProviderSelectionContext context, 
        IServiceProvider services)
    {
        var availableFactories = _registry.GetAvailableFactories(services).ToList();
        var candidates = await ScoreFactoriesAsync(availableFactories, context);
        var sortedFactories = SortFactoriesByScore(candidates);
        
        return CreateProviderInstances(sortedFactories, services);
    }
    
    private async Task<List<(IProviderFactory Factory, int Score)>> ScoreFactoriesAsync(
        List<IProviderFactory> factories,
        ProviderSelectionContext context)
    {
        var candidates = new List<(IProviderFactory Factory, int Score)>();
        
        foreach (var factory in factories)
        {
            if (ShouldSkipFactory(factory, context))
            {
                continue;
            }
            
            var score = await ScoreProviderAsync(factory, context);
            if (score > 0)
            {
                candidates.Add((factory, score));
            }
        }
        
        return candidates;
    }
    
    private bool ShouldSkipFactory(IProviderFactory factory, ProviderSelectionContext context)
    {
        if (context.FailedProviders.Contains(factory.Name))
        {
            _logger.LogDebug("Skipping previously failed provider: {Provider}", factory.Name);
            return true;
        }
        
        return false;
    }
    
    private static List<IProviderFactory> SortFactoriesByScore(
        List<(IProviderFactory Factory, int Score)> candidates)
    {
        return candidates
            .OrderByDescending(c => c.Score)
            .ThenByDescending(c => c.Factory.GetMetadata().Priority)
            .Select(c => c.Factory)
            .ToList();
    }
    
    private List<IImageGenerationProvider> CreateProviderInstances(
        List<IProviderFactory> factories,
        IServiceProvider services)
    {
        var providers = new List<IImageGenerationProvider>();
        
        foreach (var factory in factories)
        {
            var provider = TryCreateProvider(factory, services);
            if (provider != null)
            {
                providers.Add(provider);
            }
        }
        
        return providers;
    }
    
    private IImageGenerationProvider? TryCreateProvider(
        IProviderFactory factory,
        IServiceProvider services)
    {
        try
        {
            var provider = factory.Create(services);
            _logger.LogDebug("Added provider option: {Provider}", provider.ProviderName);
            return provider;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create provider from factory: {Factory}", factory.Name);
            return null;
        }
    }

    /// <summary>
    /// Scores a provider factory based on how well it matches the selection context
    /// </summary>
    private Task<int> ScoreProviderAsync(IProviderFactory factory, ProviderSelectionContext context)
    {
        try
        {
            var metadata = factory.GetMetadata();
            var capabilities = metadata.Capabilities;
            
            if (!SupportsOperation(capabilities, context.Operation))
            {
                return Task.FromResult(0);
            }
            
            var modelScore = CalculateModelScore(capabilities, context.Model);
            if (modelScore < 0)
            {
                return Task.FromResult(0);
            }
            
            var score = CalculateTotalScore(factory, context, metadata, modelScore);
            
            _logger.LogDebug("Provider {Provider} scored {Score} for context", factory.Name, score);
            return Task.FromResult(score);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error scoring provider factory: {Factory}", factory.Name);
            return Task.FromResult(0);
        }
    }
    
    private static bool SupportsOperation(ProviderCapabilities capabilities, ImageOperation operation)
    {
        return capabilities.SupportedOperations.Contains(operation);
    }
    
    private static int CalculateModelScore(ProviderCapabilities capabilities, string? model)
    {
        if (string.IsNullOrEmpty(model))
        {
            return 0;
        }
        
        var hasExplicitModel = capabilities.ExampleModels.Any(m =>
            m.Equals(model, StringComparison.OrdinalIgnoreCase));
        
        if (hasExplicitModel)
        {
            return 50; // Explicit model support
        }
        
        if (capabilities.AcceptsCustomModels)
        {
            return 25; // Can try custom model
        }
        
        return -1; // Cannot handle the model
    }
    
    private static int CalculateTotalScore(
        IProviderFactory factory, 
        ProviderSelectionContext context, 
        ProviderMetadata metadata,
        int modelScore)
    {
        var score = 100; // Base score for operation support
        
        // Explicit provider match (highest priority)
        if (IsPreferredProvider(factory.Name, context.PreferredProvider))
        {
            score += 1000;
        }
        
        score += modelScore;
        score += metadata.Priority;
        
        return score;
    }
    
    private static bool IsPreferredProvider(string factoryName, string? preferredProvider)
    {
        return !string.IsNullOrEmpty(preferredProvider) &&
               factoryName.Equals(preferredProvider, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Fallback provider selector that tries multiple providers in sequence
/// </summary>
public class FallbackProviderSelector : IProviderSelectionStrategy
{
    private readonly SmartProviderSelector _primarySelector;
    private readonly ILogger<FallbackProviderSelector> _logger;

    /// <summary>
    /// Initializes the fallback selector
    /// </summary>
    public FallbackProviderSelector(
        IProviderRegistry registry,
        ILogger<FallbackProviderSelector> logger)
    {
        _primarySelector = new SmartProviderSelector(
            registry, 
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SmartProviderSelector>.Instance);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Selects a provider with automatic fallback on failure
    /// </summary>
    public async Task<IImageGenerationProvider> SelectProviderAsync(
        ProviderSelectionContext context, 
        IServiceProvider services)
    {
        var originalFailedProviders = new HashSet<string>(context.FailedProviders);
        const int maxAttempts = 3;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var provider = await TrySelectProviderAsync(context, services, attempt, maxAttempts);
            if (provider != null)
            {
                return provider;
            }
        }

        // Reset to original state and throw final exception
        context.FailedProviders = originalFailedProviders;
        return await _primarySelector.SelectProviderAsync(context, services);
    }
    
    private async Task<IImageGenerationProvider?> TrySelectProviderAsync(
        ProviderSelectionContext context, 
        IServiceProvider services,
        int attempt,
        int maxAttempts)
    {
        try
        {
            var provider = await _primarySelector.SelectProviderAsync(context, services);
            _logger.LogDebug("Selected provider '{Provider}' on attempt {Attempt}", 
                provider.ProviderName, attempt + 1);
            return provider;
        }
        catch (InvalidOperationException ex) when (attempt < maxAttempts - 1)
        {
            await HandleSelectionFailureAsync(context, services, attempt, ex);
            return null;
        }
    }
    
    private async Task HandleSelectionFailureAsync(
        ProviderSelectionContext context,
        IServiceProvider services,
        int attempt,
        InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Provider selection failed on attempt {Attempt}", attempt + 1);
        
        var availableProviders = await _primarySelector.GetProviderOptionsAsync(context, services);
        var topProvider = availableProviders.FirstOrDefault();
        
        if (topProvider != null)
        {
            context.FailedProviders.Add(topProvider.ProviderName);
        }
    }

    /// <summary>
    /// Gets provider options using the primary selector
    /// </summary>
    public async Task<List<IImageGenerationProvider>> GetProviderOptionsAsync(
        ProviderSelectionContext context, 
        IServiceProvider services)
    {
        return await _primarySelector.GetProviderOptionsAsync(context, services);
    }
}