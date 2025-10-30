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
