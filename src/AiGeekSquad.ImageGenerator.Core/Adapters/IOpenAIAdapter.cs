using OpenAI.Images;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Adapter interface for OpenAI SDK operations - enables unit testing without API keys
/// </summary>
public interface IOpenAIAdapter
{
    /// <summary>
    /// Generates an image from a text prompt using the specified model
    /// </summary>
    /// <param name="model">The model to use (e.g., "dall-e-3")</param>
    /// <param name="prompt">Text description of the desired image</param>
    /// <param name="options">Generation options including size, quality, and style</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated image with URL and optional revised prompt</returns>
    Task<GeneratedImage> GenerateImageAsync(
        string model,
        string prompt,
        ImageGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Edits an existing image based on a text prompt
    /// </summary>
    /// <param name="model">The model to use (typically "dall-e-2" for editing)</param>
    /// <param name="image">Stream containing the image to edit</param>
    /// <param name="imageName">Name of the image file</param>
    /// <param name="prompt">Text description of the desired edits</param>
    /// <param name="options">Edit options including size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Edited image with URL and optional revised prompt</returns>
    Task<GeneratedImage> GenerateImageEditAsync(
        string model,
        Stream image,
        string imageName,
        string prompt,
        ImageEditOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a variation of an existing image
    /// </summary>
    /// <param name="model">The model to use (typically "dall-e-2" for variations)</param>
    /// <param name="image">Stream containing the source image</param>
    /// <param name="imageName">Name of the image file</param>
    /// <param name="options">Variation options including size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated variation image with URL</returns>
    Task<GeneratedImage> GenerateImageVariationAsync(
        string model,
        Stream image,
        string imageName,
        ImageVariationOptions options,
        CancellationToken cancellationToken = default);
}
