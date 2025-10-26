namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Request for editing an existing image
/// </summary>
public record ImageEditRequest
{
    /// <summary>
    /// The image to edit (base64 encoded or URL)
    /// </summary>
    public required string Image { get; init; }

    /// <summary>
    /// The prompt describing the desired changes
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Optional mask image (base64 encoded or URL)
    /// </summary>
    public string? Mask { get; init; }

    /// <summary>
    /// The model to use for editing
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// The size of the output image
    /// </summary>
    public string? Size { get; init; }

    /// <summary>
    /// Number of images to generate
    /// </summary>
    public int NumberOfImages { get; init; } = 1;

    /// <summary>
    /// Additional provider-specific parameters
    /// </summary>
    public Dictionary<string, object>? AdditionalParameters { get; init; }
}
