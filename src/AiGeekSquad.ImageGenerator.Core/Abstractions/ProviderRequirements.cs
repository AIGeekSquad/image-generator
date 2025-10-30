namespace AiGeekSquad.ImageGenerator.Core.Abstractions;

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
