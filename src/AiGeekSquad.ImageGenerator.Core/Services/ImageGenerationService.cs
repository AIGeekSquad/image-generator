using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Service for managing multiple image generation providers
/// </summary>
public class ImageGenerationService : IImageGenerationService
{
    private readonly Dictionary<string, IImageGenerationProvider> _providers;

    /// <summary>
    /// Initializes the image generation service with the available providers
    /// </summary>
    /// <param name="providers">Collection of image generation providers to register</param>
    public ImageGenerationService(IEnumerable<IImageGenerationProvider> providers)
    {
        _providers = providers.ToDictionary(
            p => p.ProviderName,
            p => p,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all registered image generation providers
    /// </summary>
    /// <returns>Read-only list of all providers</returns>
    public IReadOnlyList<IImageGenerationProvider> GetProviders()
    {
        return _providers.Values.ToList();
    }

    /// <summary>
    /// Gets a specific provider by name
    /// </summary>
    /// <param name="providerName">Name of the provider (case-insensitive)</param>
    /// <returns>The provider if found, otherwise null</returns>
    public IImageGenerationProvider? GetProvider(string providerName)
    {
        _providers.TryGetValue(providerName, out var provider);
        return provider;
    }

    /// <summary>
    /// Generates an image using the specified provider
    /// </summary>
    /// <param name="providerName">Name of the provider to use</param>
    /// <param name="request">Image generation request with prompt and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing generated image(s)</returns>
    /// <exception cref="InvalidOperationException">Thrown when provider is not found</exception>
    /// <exception cref="NotSupportedException">Thrown when provider doesn't support generation</exception>
    public async Task<ImageGenerationResponse> GenerateImageAsync(
        string providerName,
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(providerName)
            ?? throw new InvalidOperationException($"Provider '{providerName}' not found");

        if (!provider.SupportsOperation(ImageOperation.Generate))
        {
            throw new NotSupportedException($"Provider '{providerName}' does not support image generation");
        }

        return await provider.GenerateImageAsync(request, cancellationToken);
    }

    /// <summary>
    /// Generates an image from a conversational context with multiple messages
    /// </summary>
    /// <param name="providerName">Name of the provider to use</param>
    /// <param name="request">Conversational image generation request with message history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing generated image(s)</returns>
    /// <exception cref="InvalidOperationException">Thrown when provider is not found</exception>
    public async Task<ImageGenerationResponse> GenerateImageFromConversationAsync(
        string providerName,
        ConversationalImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(providerName)
            ?? throw new InvalidOperationException($"Provider '{providerName}' not found");

        return await provider.GenerateImageFromConversationAsync(request, cancellationToken);
    }

    /// <summary>
    /// Edits an existing image based on a text prompt
    /// </summary>
    /// <param name="providerName">Name of the provider to use</param>
    /// <param name="request">Image edit request with source image and edit instructions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing edited image(s)</returns>
    /// <exception cref="InvalidOperationException">Thrown when provider is not found</exception>
    /// <exception cref="NotSupportedException">Thrown when provider doesn't support editing</exception>
    public async Task<ImageGenerationResponse> EditImageAsync(
        string providerName,
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(providerName)
            ?? throw new InvalidOperationException($"Provider '{providerName}' not found");

        if (!provider.SupportsOperation(ImageOperation.Edit))
        {
            throw new NotSupportedException($"Provider '{providerName}' does not support image editing");
        }

        return await provider.EditImageAsync(request, cancellationToken);
    }

    /// <summary>
    /// Creates variations of an existing image
    /// </summary>
    /// <param name="providerName">Name of the provider to use</param>
    /// <param name="request">Image variation request with source image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing variation image(s)</returns>
    /// <exception cref="InvalidOperationException">Thrown when provider is not found</exception>
    /// <exception cref="NotSupportedException">Thrown when provider doesn't support variations</exception>
    public async Task<ImageGenerationResponse> CreateVariationAsync(
        string providerName,
        ImageVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(providerName)
            ?? throw new InvalidOperationException($"Provider '{providerName}' not found");

        if (!provider.SupportsOperation(ImageOperation.Variation))
        {
            throw new NotSupportedException($"Provider '{providerName}' does not support image variations");
        }

        return await provider.CreateVariationAsync(request, cancellationToken);
    }
}
