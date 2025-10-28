namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Request for creating variations of an existing image
/// </summary>
public record ImageVariationRequest
{
    /// <summary>
    /// The image to create variations from (base64 encoded or URL)
    /// </summary>
    public required string Image { get; init; }

    /// <summary>
    /// The model to use for creating variations
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// The size of the output images
    /// </summary>
    public string? Size { get; init; }

    /// <summary>
    /// Number of variations to generate
    /// </summary>
    public int NumberOfImages { get; init; } = 1;

    /// <summary>
    /// Additional provider-specific parameters
    /// </summary>
    public Dictionary<string, object>? AdditionalParameters { get; init; }
}
