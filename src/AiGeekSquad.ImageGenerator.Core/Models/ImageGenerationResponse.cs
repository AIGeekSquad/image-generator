namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Response containing generated image(s)
/// </summary>
public record ImageGenerationResponse
{
    /// <summary>
    /// Generated images
    /// </summary>
    public required List<GeneratedImage> Images { get; init; }

    /// <summary>
    /// The model used for generation
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// The provider that generated the images
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Timestamp of generation
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
