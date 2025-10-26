using ImageGenerator.Core.Models;

namespace ImageGenerator.Core.Abstractions;

/// <summary>
/// Service for managing and routing requests to different image generation providers
/// </summary>
public interface IImageGenerationService
{
    /// <summary>
    /// Get all registered providers
    /// </summary>
    IReadOnlyList<IImageGenerationProvider> GetProviders();

    /// <summary>
    /// Get a specific provider by name
    /// </summary>
    IImageGenerationProvider? GetProvider(string providerName);

    /// <summary>
    /// Generate image using the specified provider
    /// </summary>
    Task<ImageGenerationResponse> GenerateImageAsync(
        string providerName,
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Edit image using the specified provider
    /// </summary>
    Task<ImageGenerationResponse> EditImageAsync(
        string providerName,
        ImageEditRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create variation using the specified provider
    /// </summary>
    Task<ImageGenerationResponse> CreateVariationAsync(
        string providerName,
        ImageVariationRequest request,
        CancellationToken cancellationToken = default);
}
