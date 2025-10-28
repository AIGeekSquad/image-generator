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

/// <summary>
/// Configuration requirements for a provider
/// </summary>
public class ProviderRequirements
{
    /// <summary>
    /// Required environment variables or configuration keys
    /// </summary>
    public List<string> RequiredEnvironmentVariables { get; init; } = new();
    
    /// <summary>
    /// Required configuration sections
    /// </summary>
    public List<string> RequiredConfigurationSections { get; init; } = new();
    
    /// <summary>
    /// Minimum dependencies required (assembly names)
    /// </summary>
    public List<string> RequiredDependencies { get; init; } = new();
    
    /// <summary>
    /// Optional dependencies that enhance functionality
    /// </summary>
    public List<string> OptionalDependencies { get; init; } = new();
}