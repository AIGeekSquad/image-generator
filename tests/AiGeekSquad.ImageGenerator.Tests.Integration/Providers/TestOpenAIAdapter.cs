using System.Reflection;
using System.ClientModel;
using AiGeekSquad.ImageGenerator.Core.Adapters;
using OpenAI.Images;

namespace AiGeekSquad.ImageGenerator.Tests.Integration.Providers;

/// <summary>
/// Test implementation of IOpenAIAdapter for testing without requiring API keys
/// </summary>
internal class TestOpenAIAdapter : IOpenAIAdapter
{
    private readonly BinaryData? _imageData;
    private readonly string? _revisedPrompt;

    public TestOpenAIAdapter(BinaryData? imageData = null, string? revisedPrompt = null)
    {
        _imageData = imageData ?? CreateSampleImageData();
        _revisedPrompt = revisedPrompt;
    }

    private static BinaryData CreateSampleImageData()
    {
        // Create a simple 1x1 pixel PNG as Base64 for testing
        var base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
        var bytes = Convert.FromBase64String(base64Data);
        return new BinaryData(bytes);
    }

    public Task<GeneratedImage> GenerateImageAsync(
        string model,
        string prompt,
        ImageGenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        var image = CreateGeneratedImage(_imageData, _revisedPrompt);
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
        var generatedImage = CreateGeneratedImage(_imageData, _revisedPrompt);
        return Task.FromResult(generatedImage);
    }

    public Task<GeneratedImage> GenerateImageVariationAsync(
        string model,
        Stream image,
        string imageName,
        ImageVariationOptions options,
        CancellationToken cancellationToken = default)
    {
        var generatedImage = CreateGeneratedImage(_imageData, null);
        return Task.FromResult(generatedImage);
    }

    private static GeneratedImage CreateGeneratedImage(BinaryData? imageData, string? revisedPrompt)
    {
        // Use the internal constructor that accepts BinaryData, Uri, and revisedPrompt
        var type = typeof(GeneratedImage);
        var constructor = type.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(BinaryData), typeof(Uri), typeof(string), typeof(IDictionary<string, BinaryData>)],
            null);
        
        if (constructor != null)
        {
            var testUri = new Uri("https://example.com/test-image.png");
            var instance = constructor.Invoke([imageData!, testUri, revisedPrompt!, null!]);
            return (GeneratedImage)instance;
        }
        
        // Fallback: use parameterless constructor if the above fails
        var instance2 = Activator.CreateInstance(type, nonPublic: true)!;
        return (GeneratedImage)instance2;
    }
}
