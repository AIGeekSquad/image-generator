namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Represents a message in a conversation for image generation
/// </summary>
public record ConversationMessage
{
    /// <summary>
    /// The role of the message sender
    /// </summary>
    public required string Role { get; init; } // "user", "assistant", "system"

    /// <summary>
    /// Text content of the message
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Image content references
    /// </summary>
    public List<ImageContent>? Images { get; init; }
}

/// <summary>
/// Represents an image in a conversation
/// </summary>
public record ImageContent
{
    /// <summary>
    /// Image URL
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Base64 encoded image data
    /// </summary>
    public string? Base64Data { get; init; }

    /// <summary>
    /// MIME type (e.g., "image/png", "image/jpeg")
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Optional description or caption for the image
    /// </summary>
    public string? Caption { get; init; }
}

/// <summary>
/// Request for generating an image with conversation context
/// </summary>
public record ConversationalImageGenerationRequest
{
    /// <summary>
    /// Conversation history providing context for image generation
    /// </summary>
    public required List<ConversationMessage> Conversation { get; init; }

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
