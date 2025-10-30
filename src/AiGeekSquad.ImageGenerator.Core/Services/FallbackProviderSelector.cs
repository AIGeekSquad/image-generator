using AiGeekSquad.ImageGenerator.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace AiGeekSquad.ImageGenerator.Core.Services;

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
        var attempts = 0;
        const int maxAttempts = 3;

        while (attempts < maxAttempts)
        {
            try
            {
                var provider = await _primarySelector.SelectProviderAsync(context, services);
                _logger.LogDebug("Selected provider '{Provider}' on attempt {Attempt}", 
                    provider.ProviderName, attempts + 1);
                return provider;
            }
            catch (InvalidOperationException ex) when (attempts < maxAttempts - 1)
            {
                _logger.LogWarning(ex, "Provider selection failed on attempt {Attempt}",
                    attempts + 1);
                
                // Add all currently known providers to failed list to force different selection
                var availableProviders = await _primarySelector.GetProviderOptionsAsync(context, services);
                foreach (var provider in availableProviders.Take(1)) // Just the top choice
                {
                    context.FailedProviders.Add(provider.ProviderName);
                }
                
                attempts++;
            }
        }

        // Reset to original state and throw final exception
        context.FailedProviders = originalFailedProviders;
        return await _primarySelector.SelectProviderAsync(context, services);
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
