using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Core.Providers;

/// <summary>
/// Base class for image generation providers with common functionality
/// </summary>
public abstract class ImageProviderBase : IImageGenerationProvider
{
    public abstract string ProviderName { get; }

    protected abstract ProviderCapabilities Capabilities { get; }

    public virtual ProviderCapabilities GetCapabilities() => Capabilities;

    public abstract Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    public virtual Task<ImageGenerationResponse> GenerateImageFromConversationAsync(
        ConversationalImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SupportsOperation(ImageOperation.GenerateFromConversation))
        {
            // Fallback: try to convert conversation to simple prompt
            var prompt = ConvertConversationToPrompt(request.Conversation);
            return GenerateImageAsync(new ImageGenerationRequest
            {
                Prompt = prompt,
                Model = request.Model,
                Size = request.Size,
                Quality = request.Quality,
                Style = request.Style,
                NumberOfImages = request.NumberOfImages,
                AdditionalParameters = request.AdditionalParameters
            }, cancellationToken);
        }

        throw new NotImplementedException($"Conversational image generation not implemented for {ProviderName}");
    }

    public virtual Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SupportsOperation(ImageOperation.Edit))
        {
            throw new NotSupportedException($"{ProviderName} does not support image editing");
        }

        throw new NotImplementedException($"Image editing not implemented for {ProviderName}");
    }

    public virtual Task<ImageGenerationResponse> CreateVariationAsync(
        ImageVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SupportsOperation(ImageOperation.Variation))
        {
            throw new NotSupportedException($"{ProviderName} does not support image variations");
        }

        throw new NotImplementedException($"Image variations not implemented for {ProviderName}");
    }

    public virtual bool SupportsOperation(ImageOperation operation)
    {
        return Capabilities.SupportedOperations.Contains(operation);
    }

    /// <summary>
    /// Get the model to use, defaulting to provider's default if not specified
    /// </summary>
    protected string GetModelOrDefault(string? requestedModel)
    {
        return requestedModel ?? Capabilities.DefaultModel ?? 
               throw new InvalidOperationException($"{ProviderName} has no default model configured");
    }

    /// <summary>
    /// Helper to convert image data to stream
    /// </summary>
    protected static Stream ConvertToStream(string imageData)
    {
        if (imageData.StartsWith("data:image"))
        {
            // Base64 data URL
            var base64 = imageData.Split(',')[1];
            var bytes = Convert.FromBase64String(base64);
            return new MemoryStream(bytes);
        }
        else if (imageData.StartsWith("http"))
        {
            // URL - download the image
            using var httpClient = new HttpClient();
            var bytes = httpClient.GetByteArrayAsync(imageData).Result;
            return new MemoryStream(bytes);
        }
        else
        {
            // Assume it's base64 encoded
            var bytes = Convert.FromBase64String(imageData);
            return new MemoryStream(bytes);
        }
    }

    /// <summary>
    /// Helper to convert stream to base64
    /// </summary>
    protected static async Task<string> ConvertStreamToBase64Async(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    /// <summary>
    /// Convert a conversation to a simple text prompt (fallback for providers that don't support conversational input)
    /// </summary>
    protected virtual string ConvertConversationToPrompt(List<ConversationMessage> conversation)
    {
        var promptParts = new List<string>();

        foreach (var message in conversation)
        {
            if (!string.IsNullOrEmpty(message.Text))
            {
                promptParts.Add(message.Text);
            }

            if (message.Images != null && message.Images.Count > 0)
            {
                foreach (var image in message.Images)
                {
                    if (!string.IsNullOrEmpty(image.Caption))
                    {
                        promptParts.Add($"[Reference image: {image.Caption}]");
                    }
                }
            }
        }

        return string.Join("\n", promptParts);
    }
}
