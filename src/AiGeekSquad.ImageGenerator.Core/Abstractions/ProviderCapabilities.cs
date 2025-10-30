namespace AiGeekSquad.ImageGenerator.Core.Abstractions;

/// <summary>
/// Provider capabilities and metadata
/// </summary>
public record ProviderCapabilities
{
    /// <summary>
    /// List of example/commonly used models (not exhaustive)
    /// </summary>
    public List<string> ExampleModels { get; init; } = new();

    /// <summary>
    /// Supported operations
    /// </summary>
    public List<ImageOperation> SupportedOperations { get; init; } = new();

    /// <summary>
    /// Default model to use if none is specified
    /// </summary>
    public string? DefaultModel { get; init; }

    /// <summary>
    /// Provider-specific features and capabilities
    /// </summary>
    public Dictionary<string, object> Features { get; init; } = new();

    /// <summary>
    /// Accepts any model string (for forward compatibility)
    /// </summary>
    public bool AcceptsCustomModels { get; init; } = true;

    /// <summary>
    /// Supports multi-modal input (images in conversation)
    /// </summary>
    public bool SupportsMultiModalInput { get; init; } = false;

    /// <summary>
    /// Maximum number of images that can be included in conversation context
    /// </summary>
    public int? MaxConversationImages { get; init; }
}
