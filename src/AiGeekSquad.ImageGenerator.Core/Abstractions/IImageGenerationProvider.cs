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
