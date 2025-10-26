using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Core.Abstractions;

/// <summary>
/// Interface for image generation providers
/// </summary>
public interface IImageGenerationProvider
{
    /// <summary>
    /// The name of the provider (e.g., "OpenAI", "Google")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Generate image(s) from a text prompt
    /// </summary>
    Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate image(s) from a conversational context with multi-modal support
    /// </summary>
    Task<ImageGenerationResponse> GenerateImageFromConversationAsync(
        ConversationalImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Edit an existing image based on a prompt
    /// </summary>
    Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create variations of an existing image
    /// </summary>
    Task<ImageGenerationResponse> CreateVariationAsync(
        ImageVariationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the provider supports a specific operation
    /// </summary>
    bool SupportsOperation(ImageOperation operation);

    /// <summary>
    /// Get provider-specific capabilities and metadata
    /// </summary>
    ProviderCapabilities GetCapabilities();
}

/// <summary>
/// Supported image operations
/// </summary>
public enum ImageOperation
{
    Generate,
    GenerateFromConversation,
    Edit,
    Variation
}

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
