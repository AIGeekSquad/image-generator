using AiGeekSquad.ImageGenerator.Core.Abstractions;

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
