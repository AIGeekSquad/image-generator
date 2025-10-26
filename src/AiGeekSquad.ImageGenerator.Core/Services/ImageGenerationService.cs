using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Service for managing multiple image generation providers
/// </summary>
public class ImageGenerationService : IImageGenerationService
{
    private readonly Dictionary<string, IImageGenerationProvider> _providers;

    public ImageGenerationService(IEnumerable<IImageGenerationProvider> providers)
    {
        _providers = providers.ToDictionary(
            p => p.ProviderName,
            p => p,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IImageGenerationProvider> GetProviders()
    {
        return _providers.Values.ToList();
    }

    public IImageGenerationProvider? GetProvider(string providerName)
    {
        _providers.TryGetValue(providerName, out var provider);
        return provider;
    }

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

    public async Task<ImageGenerationResponse> GenerateImageFromConversationAsync(
        string providerName,
        ConversationalImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider(providerName)
            ?? throw new InvalidOperationException($"Provider '{providerName}' not found");

        return await provider.GenerateImageFromConversationAsync(request, cancellationToken);
    }

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
