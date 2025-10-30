namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// A single generated image
/// </summary>
public record GeneratedImage
{
    /// <summary>
    /// The image URL
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// The image data as base64 encoded string
    /// </summary>
    public string? Base64Data { get; init; }

    /// <summary>
    /// The revised prompt (if any)
    /// </summary>
    public string? RevisedPrompt { get; init; }

    /// <summary>
    /// Additional metadata for this image
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
