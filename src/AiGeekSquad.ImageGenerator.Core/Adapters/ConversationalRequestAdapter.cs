using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Abstractions;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

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
        ArgumentNullException.ThrowIfNull(source);

        var images = new List<ImageReference>();
        var messages = new List<Microsoft.Extensions.AI.ChatMessage>();
        var prompt = string.Empty;

        // Convert conversation messages
        if (source.Conversation != null)
        {
            foreach (var convMsg in source.Conversation)
            {
                var role = MapConversationRole(convMsg.Role);
                var chatMessage = new Microsoft.Extensions.AI.ChatMessage(role, convMsg.Text ?? string.Empty);

                ProcessConversationImages(convMsg.Images, images, chatMessage);

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

    private static Microsoft.Extensions.AI.ChatRole MapConversationRole(string? role)
    {
        return role?.ToLowerInvariant() switch
        {
            "user" => Microsoft.Extensions.AI.ChatRole.User,
            "assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
            "system" => Microsoft.Extensions.AI.ChatRole.System,
            _ => Microsoft.Extensions.AI.ChatRole.User
        };
    }

    private static void ProcessConversationImages(
        List<ImageContent>? conversationImages,
        List<ImageReference> images,
        Microsoft.Extensions.AI.ChatMessage chatMessage)
    {
        if (conversationImages == null || conversationImages.Count == 0)
            return;

        foreach (var img in conversationImages)
        {
            AddImageReference(img, images);
            AddImageToMessageContent(img, chatMessage);
        }
    }

    private static void AddImageReference(ImageContent img, List<ImageReference> images)
    {
        images.Add(new ImageReference
        {
            Url = img.Url,
            Base64Data = img.Base64Data,
            MimeType = img.MimeType,
            Caption = img.Caption,
            Role = ImageRole.Reference
        });
    }

    private static void AddImageToMessageContent(ImageContent img, Microsoft.Extensions.AI.ChatMessage chatMessage)
    {
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
