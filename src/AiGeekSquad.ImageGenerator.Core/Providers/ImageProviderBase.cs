using AiGeekSquad.ImageGenerator.Core.Abstractions;
using Microsoft.Extensions.AI;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using CoreImageVariationRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageVariationRequest;
using CoreConversationMessage = AiGeekSquad.ImageGenerator.Core.Models.ConversationMessage;
using CoreConversationalRequest = AiGeekSquad.ImageGenerator.Core.Models.ConversationalImageGenerationRequest;
using GeneratedImageModel = AiGeekSquad.ImageGenerator.Core.Models.GeneratedImage;

namespace AiGeekSquad.ImageGenerator.Core.Providers;

/// <summary>
/// Base class for image generation providers with common functionality
/// </summary>
public abstract class ImageProviderBase : IImageGenerationProvider
{
    // Shared HttpClient for downloading images from URLs
    private static readonly HttpClient SharedHttpClient = new();
    
    /// <summary>
    /// HTTP client for downloading images from URLs
    /// </summary>
    protected readonly HttpClient HttpClient;

    /// <summary>
    /// Initializes a new instance of the provider base class
    /// </summary>
    /// <param name="httpClient">Optional HTTP client for downloading images (uses shared client if not provided)</param>
    protected ImageProviderBase(HttpClient? httpClient = null)
    {
        HttpClient = httpClient ?? SharedHttpClient;
    }

    /// <summary>
    /// Gets the unique name of this provider
    /// </summary>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Gets the capabilities of this provider
    /// </summary>
    protected abstract ProviderCapabilities Capabilities { get; }

    /// <summary>
    /// Gets the capabilities of this provider including supported operations and models
    /// </summary>
    /// <returns>Provider capabilities</returns>
    public virtual ProviderCapabilities GetCapabilities() => Capabilities;

    /// <summary>
    /// Generates an image from a text prompt
    /// </summary>
    /// <param name="request">Image generation request with messages and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing generated image(s)</returns>
    public abstract Task<CoreImageResponse> GenerateImageAsync(
        CoreImageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an image from a conversational context with fallback support
    /// </summary>
    /// <param name="request">Conversational request with message history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing generated image(s)</returns>
    public virtual Task<CoreImageResponse> GenerateImageFromConversationAsync(
        CoreConversationalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SupportsOperation(ImageOperation.GenerateFromConversation))
        {
            // Fallback: convert conversation messages to ChatMessage format
            var messages = ConvertConversationToChatMessages(request.Conversation);
            return GenerateImageAsync(new CoreImageRequest
            {
                Messages = messages,
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

    /// <summary>
    /// Edits an existing image based on a text prompt
    /// </summary>
    /// <param name="request">Image edit request with source image and instructions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing edited image(s)</returns>
    /// <exception cref="NotSupportedException">Thrown when provider doesn't support editing</exception>
    public virtual Task<CoreImageResponse> EditImageAsync(
        CoreImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SupportsOperation(ImageOperation.Edit))
        {
            throw new NotSupportedException($"{ProviderName} does not support image editing");
        }

        throw new NotImplementedException($"Image editing not implemented for {ProviderName}");
    }

    /// <summary>
    /// Creates variations of an existing image
    /// </summary>
    /// <param name="request">Image variation request with source image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing variation image(s)</returns>
    /// <exception cref="NotSupportedException">Thrown when provider doesn't support variations</exception>
    public virtual Task<CoreImageResponse> CreateVariationAsync(
        CoreImageVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SupportsOperation(ImageOperation.Variation))
        {
            throw new NotSupportedException($"{ProviderName} does not support image variations");
        }

        throw new NotImplementedException($"Image variations not implemented for {ProviderName}");
    }

    /// <summary>
    /// Checks if this provider supports a specific operation
    /// </summary>
    /// <param name="operation">The operation to check</param>
    /// <returns>True if the operation is supported, otherwise false</returns>
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
    /// Extract text content from ChatMessages
    /// </summary>
    protected static string ExtractTextFromMessages(IList<ChatMessage> messages)
    {
        var textParts = new List<string>();
        
        foreach (var message in messages)
        {
            // Get text from message.Text property
            if (!string.IsNullOrEmpty(message.Text))
            {
                textParts.Add(message.Text);
            }
            
            // Also check Contents collection for TextContent
            if (message.Contents != null)
            {
                foreach (var content in message.Contents)
                {
                    if (content is TextContent textContent && !string.IsNullOrEmpty(textContent.Text))
                    {
                        textParts.Add(textContent.Text);
                    }
                }
            }
        }
        
        return string.Join("\n", textParts);
    }

    /// <summary>
    /// Extract image contents from ChatMessages
    /// </summary>
    protected static List<Microsoft.Extensions.AI.DataContent> ExtractImagesFromMessages(IList<ChatMessage> messages)
    {
        var images = new List<Microsoft.Extensions.AI.DataContent>();
        
        foreach (var message in messages)
        {
            if (message.Contents != null)
            {
                foreach (var content in message.Contents)
                {
                    if (content is Microsoft.Extensions.AI.DataContent dataContent)
                    {
                        images.Add(dataContent);
                    }
                }
            }
        }
        
        return images;
    }

    /// <summary>
    /// Helper to convert image data to stream
    /// </summary>
    protected async Task<Stream> ConvertToStreamAsync(string imageData, CancellationToken cancellationToken = default)
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
            // URL - download the image using the HttpClient instance
            var bytes = await HttpClient.GetByteArrayAsync(imageData, cancellationToken);
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
    /// Convert custom ConversationMessage format to Microsoft.Extensions.AI ChatMessage
    /// </summary>
    protected virtual IList<ChatMessage> ConvertConversationToChatMessages(List<CoreConversationMessage> conversation)
    {
        var chatMessages = new List<ChatMessage>();

        foreach (var message in conversation)
        {
            var role = message.Role?.ToLowerInvariant() switch
            {
                "user" => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                "system" => ChatRole.System,
                _ => ChatRole.User
            };

            var chatMessage = new ChatMessage(role, message.Text ?? string.Empty);

            // Add image contents if present
            if (message.Images != null && message.Images.Count > 0)
            {
                foreach (var image in message.Images)
                {
                    if (!string.IsNullOrEmpty(image.Url))
                    {
                        chatMessage.Contents.Add(new Microsoft.Extensions.AI.DataContent(new Uri(image.Url), "image/*"));
                    }
                    else if (!string.IsNullOrEmpty(image.Base64Data))
                    {
                        var bytes = Convert.FromBase64String(image.Base64Data);
                        var mimeType = image.MimeType ?? "image/png";
                        chatMessage.Contents.Add(new Microsoft.Extensions.AI.DataContent(bytes, mimeType));
                    }
                }
            }

            chatMessages.Add(chatMessage);
        }

        return chatMessages;
    }

    /// <summary>
    /// Helper method to build a standard response with common metadata
    /// </summary>
    protected CoreImageResponse BuildResponse(
        IEnumerable<GeneratedImageModel> images,
        string model,
        Dictionary<string, object>? additionalMetadata = null)
    {
        return new CoreImageResponse
        {
            Images = images.ToList(),
            Model = model,
            Provider = ProviderName,
            CreatedAt = DateTime.UtcNow,
            Metadata = additionalMetadata
        };
    }

    /// <summary>
    /// Helper method to create a single-image response from URL
    /// </summary>
    protected CoreImageResponse BuildSingleImageResponse(
        string? url,
        string model,
        string? revisedPrompt = null,
        Dictionary<string, object>? additionalMetadata = null)
    {
        var image = new GeneratedImageModel
        {
            Url = url,
            RevisedPrompt = revisedPrompt
        };

        return BuildResponse(new[] { image }, model, additionalMetadata);
    }

    /// <summary>
    /// Helper method to create a single-image response from base64 data
    /// </summary>
    protected CoreImageResponse BuildSingleImageResponseFromBase64(
        string? base64Data,
        string model,
        Dictionary<string, object>? additionalMetadata = null)
    {
        var image = new GeneratedImageModel
        {
            Base64Data = base64Data
        };

        return BuildResponse(new[] { image }, model, additionalMetadata);
    }
}
