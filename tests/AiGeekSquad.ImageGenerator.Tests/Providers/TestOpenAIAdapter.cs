using AiGeekSquad.ImageGenerator.Core.Adapters;
using OpenAI.Images;
using System.Reflection;

namespace AiGeekSquad.ImageGenerator.Tests.Providers;

/// <summary>
/// Test implementation of IOpenAIAdapter for testing without requiring API keys
/// </summary>
internal class TestOpenAIAdapter : IOpenAIAdapter
{
    private readonly Uri? _imageUri;
    private readonly string? _revisedPrompt;

    public TestOpenAIAdapter(Uri? imageUri = null, string? revisedPrompt = null)
    {
        _imageUri = imageUri ?? new Uri("https://example.com/test-image.png");
        _revisedPrompt = revisedPrompt;
    }

    public Task<GeneratedImage> GenerateImageAsync(
        string model,
        string prompt,
        ImageGenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        var image = CreateGeneratedImage(_imageUri, _revisedPrompt);
        return Task.FromResult(image);
    }

    public Task<GeneratedImage> GenerateImageEditAsync(
        string model,
        Stream image,
        string imageName,
        string prompt,
        ImageEditOptions options,
        CancellationToken cancellationToken = default)
    {
        var generatedImage = CreateGeneratedImage(_imageUri, _revisedPrompt);
        return Task.FromResult(generatedImage);
    }

    public Task<GeneratedImage> GenerateImageVariationAsync(
        string model,
        Stream image,
        string imageName,
        ImageVariationOptions options,
        CancellationToken cancellationToken = default)
    {
        var generatedImage = CreateGeneratedImage(_imageUri, null);
        return Task.FromResult(generatedImage);
    }

    private static GeneratedImage CreateGeneratedImage(Uri? imageUri, string? revisedPrompt)
    {
        // Use the internal constructor that accepts imageUri and revisedPrompt
        var type = typeof(GeneratedImage);
        var constructor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(BinaryData), typeof(Uri), typeof(string), typeof(IDictionary<string, BinaryData>)],
            null);
        
        if (constructor != null)
        {
            var instance = constructor.Invoke([null!, imageUri!, revisedPrompt!, null!]);
            return (GeneratedImage)instance;
        }
        
        // Fallback: use parameterless constructor if the above fails
        var instance2 = Activator.CreateInstance(type, nonPublic: true)!;
        return (GeneratedImage)instance2;
    }
}
