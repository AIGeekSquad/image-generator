namespace ImageGenerator.Core.Models;

/// <summary>
/// Request for generating an image
/// </summary>
public record ImageGenerationRequest
{
    /// <summary>
    /// The prompt to generate the image from
    /// </summary>
    public required string Prompt { get; init; }

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
