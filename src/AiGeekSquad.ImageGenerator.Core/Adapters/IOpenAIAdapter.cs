using OpenAI.Images;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Adapter interface for OpenAI SDK operations - enables unit testing without API keys
/// </summary>
public interface IOpenAIAdapter
{
    Task<GeneratedImage> GenerateImageAsync(
        string model,
        string prompt,
        ImageGenerationOptions options,
        CancellationToken cancellationToken = default);

    Task<GeneratedImage> GenerateImageEditAsync(
        string model,
        Stream image,
        string imageName,
        string prompt,
        ImageEditOptions options,
        CancellationToken cancellationToken = default);

    Task<GeneratedImage> GenerateImageVariationAsync(
        string model,
        Stream image,
        string imageName,
        ImageVariationOptions options,
        CancellationToken cancellationToken = default);
}
