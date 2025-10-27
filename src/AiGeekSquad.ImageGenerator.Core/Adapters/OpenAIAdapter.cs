using OpenAI;
using OpenAI.Images;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Production implementation of IOpenAIAdapter using actual OpenAI SDK
/// </summary>
internal class OpenAIAdapter : IOpenAIAdapter
{
    private readonly OpenAIClient _client;

    public OpenAIAdapter(OpenAIClient client)
    {
        _client = client;
    }

    public async Task<GeneratedImage> GenerateImageAsync(
        string model,
        string prompt,
        ImageGenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        var imageClient = _client.GetImageClient(model);
        var result = await imageClient.GenerateImageAsync(prompt, options, cancellationToken);
        return result.Value;
    }

    public async Task<GeneratedImage> GenerateImageEditAsync(
        string model,
        Stream image,
        string imageName,
        string prompt,
        ImageEditOptions options,
        CancellationToken cancellationToken = default)
    {
        var imageClient = _client.GetImageClient(model);
        var result = await imageClient.GenerateImageEditAsync(image, imageName, prompt, options, cancellationToken);
        return result.Value;
    }

    public async Task<GeneratedImage> GenerateImageVariationAsync(
        string model,
        Stream image,
        string imageName,
        ImageVariationOptions options,
        CancellationToken cancellationToken = default)
    {
        var imageClient = _client.GetImageClient(model);
        var result = await imageClient.GenerateImageVariationAsync(image, imageName, options, cancellationToken);
        return result.Value;
    }
}
