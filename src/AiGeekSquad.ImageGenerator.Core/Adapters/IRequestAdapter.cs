using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Abstractions;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Interface for adapting different request formats to the unified request model
/// </summary>
public interface IRequestAdapter<TSource>
{
    /// <summary>
    /// Adapts a source request format to the unified request model
    /// </summary>
    /// <param name="source">Source request object</param>
    /// <returns>Unified request model</returns>
    UnifiedImageRequest Adapt(TSource source);
}

/// <summary>
/// Adapter for converting legacy ImageGenerationRequest to unified format
/// </summary>
public class ImageGenerationRequestAdapter : IRequestAdapter<ImageGenerationRequest>
{
    /// <summary>
    /// Converts ImageGenerationRequest to UnifiedImageRequest
    /// </summary>
    /// <param name="source">Legacy request format</param>
    /// <returns>Unified request format</returns>
    public UnifiedImageRequest Adapt(ImageGenerationRequest source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return new UnifiedImageRequest
        {
            Prompt = ExtractPromptFromMessages(source.Messages),
            Messages = source.Messages?.ToList() ?? new List<Microsoft.Extensions.AI.ChatMessage>(),
            Images = new List<ImageReference>(),
            Parameters = new ImageParameters
            {
                Model = source.Model,
                Size = source.Size,
                Quality = source.Quality,
                Style = source.Style,
                NumberOfImages = source.NumberOfImages
            },
            Operation = ImageOperation.Generate,
            AdditionalParameters = source.AdditionalParameters ?? new Dictionary<string, object>()
        };
    }

    private static string ExtractPromptFromMessages(IList<Microsoft.Extensions.AI.ChatMessage>? messages)
    {
        if (messages == null || messages.Count == 0)
            return string.Empty;

        // Extract text content from all messages
        var textParts = new List<string>();
        foreach (var message in messages)
        {
            if (!string.IsNullOrEmpty(message.Text))
            {
                textParts.Add(message.Text);
            }
            
            // Also check Contents collection for TextContent
            if (message.Contents != null)
            {
                foreach (var content in message.Contents)
                {
                    if (content is Microsoft.Extensions.AI.TextContent textContent && 
                        !string.IsNullOrEmpty(textContent.Text))
                    {
                        textParts.Add(textContent.Text);
                    }
                }
            }
        }
        
        return string.Join("\n", textParts);
    }
}

/// <summary>
/// Adapter for converting conversational requests to unified format
/// </summary>
public class ConversationalRequestAdapter : IRequestAdapter<ConversationalImageGenerationRequest>
{
    /// <summary>
    /// Converts ConversationalImageGenerationRequest to UnifiedImageRequest
    /// </summary>
    /// <param name="source">Conversational request format</param>
    /// <returns>Unified request format</returns>
    public UnifiedImageRequest Adapt(ConversationalImageGenerationRequest source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var images = new List<ImageReference>();
        var messages = new List<Microsoft.Extensions.AI.ChatMessage>();
        var prompt = string.Empty;

        // Convert conversation messages
        if (source.Conversation != null)
        {
            foreach (var convMsg in source.Conversation)
            {
                var role = convMsg.Role?.ToLowerInvariant() switch
                {
                    "user" => Microsoft.Extensions.AI.ChatRole.User,
                    "assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
                    "system" => Microsoft.Extensions.AI.ChatRole.System,
                    _ => Microsoft.Extensions.AI.ChatRole.User
                };

                var chatMessage = new Microsoft.Extensions.AI.ChatMessage(role, convMsg.Text ?? string.Empty);

                // Extract images from conversation messages
                if (convMsg.Images != null && convMsg.Images.Count > 0)
                {
                    foreach (var img in convMsg.Images)
                    {
                        images.Add(new ImageReference
                        {
                            Url = img.Url,
                            Base64Data = img.Base64Data,
                            MimeType = img.MimeType,
                            Caption = img.Caption,
                            Role = ImageRole.Reference
                        });

                        // Also add to chat message content
                        if (!string.IsNullOrEmpty(img.Url))
                        {
                            chatMessage.Contents.Add(new Microsoft.Extensions.AI.DataContent(new Uri(img.Url), "image/*"));
                        }
                        else if (!string.IsNullOrEmpty(img.Base64Data))
                        {
                            var bytes = Convert.FromBase64String(img.Base64Data);
                            var mimeType = img.MimeType ?? "image/png";
                            chatMessage.Contents.Add(new Microsoft.Extensions.AI.DataContent(bytes, mimeType));
                        }
                    }
                }

                messages.Add(chatMessage);
                
                // Use the last user message as the primary prompt
                if (role == Microsoft.Extensions.AI.ChatRole.User && !string.IsNullOrEmpty(convMsg.Text))
                {
                    prompt = convMsg.Text;
                }
            }
        }

        return new UnifiedImageRequest
        {
            Prompt = prompt,
            Messages = messages,
            Images = images,
            Parameters = new ImageParameters
            {
                Model = source.Model,
                Size = source.Size,
                Quality = source.Quality,
                Style = source.Style,
                NumberOfImages = source.NumberOfImages
            },
            Operation = ImageOperation.GenerateFromConversation,
            AdditionalParameters = source.AdditionalParameters ?? new Dictionary<string, object>()
        };
    }
}