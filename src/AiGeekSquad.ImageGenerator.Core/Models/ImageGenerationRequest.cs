using Microsoft.Extensions.AI;

namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Request for generating an image from a conversation/messages
/// </summary>
public record ImageGenerationRequest
{
    /// <summary>
    /// The messages containing the prompt and optional images/attachments
    /// Uses Microsoft.Extensions.AI.ChatMessage for multi-modal support
    /// </summary>
    public required IList<ChatMessage> Messages { get; init; }

    /// <summary>
    /// The model to use for generation
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// The size of the image (e.g., "1024x1024", "1792x1024")
    /// </summary>
    public string? Size { get; init; }

    /// <summary>
    /// The quality of the image (e.g., "standard", "hd")
    /// </summary>
    public string? Quality { get; init; }

    /// <summary>
    /// The style of the image (e.g., "vivid", "natural")
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Number of images to generate
    /// </summary>
    public int NumberOfImages { get; init; } = 1;

    /// <summary>
    /// Additional provider-specific parameters
    /// </summary>
    public Dictionary<string, object>? AdditionalParameters { get; init; }
}
