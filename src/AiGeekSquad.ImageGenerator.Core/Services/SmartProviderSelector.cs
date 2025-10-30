using AiGeekSquad.ImageGenerator.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace AiGeekSquad.ImageGenerator.Core.Services;

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
        
        if (options.Count == 0)
        {
            var availableProviders = _registry.GetAvailableFactories(services)
                .Select(f => f.Name)
                .ToList();
                
            throw new InvalidOperationException(
                $"No suitable providers found for operation '{context.Operation}'" +
                (context.Model != null ? $" with model '{context.Model}'" : "") +
                $". Available providers: {string.Join(", ", availableProviders)}");
        }

        var selected = options[0];
        _logger.LogInformation("Selected provider '{Provider}' for operation '{Operation}'", 
            selected.ProviderName, context.Operation);
        
        return selected;
    }

    /// <summary>
    /// Gets a prioritized list of provider options for the given context
    /// </summary>
    public async Task<List<IImageGenerationProvider>> GetProviderOptionsAsync(
        ProviderSelectionContext context, 
        IServiceProvider services)
    {
        var candidates = new List<(IProviderFactory Factory, int Score)>();

        // Get all available factories
        var availableFactories = _registry.GetAvailableFactories(services).ToList();
        
        foreach (var factory in availableFactories)
        {
            // Skip failed providers
            if (context.FailedProviders.Contains(factory.Name))
            {
                _logger.LogDebug("Skipping previously failed provider: {Provider}", factory.Name);
                continue;
            }

            var score = await ScoreProviderAsync(factory, context);
            if (score > 0)
            {
                candidates.Add((factory, score));
            }
        }

        // Sort by score (highest first), then by priority
        var sortedFactories = candidates
            .OrderByDescending(c => c.Score)
            .ThenByDescending(c => c.Factory.GetMetadata().Priority)
            .Select(c => c.Factory)
            .ToList();

        // Create provider instances
        var providers = new List<IImageGenerationProvider>();
        foreach (var factory in sortedFactories)
        {
            try
            {
                var provider = factory.Create(services);
                providers.Add(provider);
                _logger.LogDebug("Added provider option: {Provider}", provider.ProviderName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create provider from factory: {Factory}", factory.Name);
            }
        }

        return providers;
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
            int score = 0;

            // Explicit provider match (highest priority)
            if (!string.IsNullOrEmpty(context.PreferredProvider) &&
                factory.Name.Equals(context.PreferredProvider, StringComparison.OrdinalIgnoreCase))
            {
                score += 1000;
            }

            // Operation support (required)
            if (!capabilities.SupportedOperations.Contains(context.Operation))
            {
                return Task.FromResult(0); // Cannot handle the operation
            }
            score += 100;

            // Model support
            if (!string.IsNullOrEmpty(context.Model))
            {
                var hasExplicitModel = capabilities.ExampleModels.Any(m =>
                    m.Equals(context.Model, StringComparison.OrdinalIgnoreCase));
                
                if (hasExplicitModel)
                {
                    score += 50; // Explicit model support
                }
                else if (capabilities.AcceptsCustomModels)
                {
                    score += 25; // Can try custom model
                }
                else
                {
                    return Task.FromResult(0); // Cannot handle the model
                }
            }

            // Base priority from metadata
            score += metadata.Priority;

            _logger.LogDebug("Provider {Provider} scored {Score} for context", factory.Name, score);
            return Task.FromResult(score);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error scoring provider factory: {Factory}", factory.Name);
            return Task.FromResult(0);
        }
    }
}
