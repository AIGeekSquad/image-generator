using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Abstractions;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

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
        ArgumentNullException.ThrowIfNull(source);

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
