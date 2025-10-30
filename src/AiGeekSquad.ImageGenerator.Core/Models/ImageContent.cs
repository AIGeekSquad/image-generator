namespace AiGeekSquad.ImageGenerator.Core.Models;

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
