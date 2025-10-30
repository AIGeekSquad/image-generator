namespace AiGeekSquad.ImageGenerator.Core.Abstractions;

/// <summary>
/// Metadata about a provider's capabilities, requirements, and configuration
/// </summary>
public class ProviderMetadata
{
    /// <summary>
    /// The provider's unique name
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Human-readable description of the provider
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Provider capabilities and supported operations
    /// </summary>
    public required ProviderCapabilities Capabilities { get; init; }
    
    /// <summary>
    /// Configuration requirements for this provider
    /// </summary>
    public ProviderRequirements Requirements { get; init; } = new();
    
    /// <summary>
    /// Provider priority for selection (higher = more preferred)
    /// </summary>
    public int Priority { get; init; } = 100;
}