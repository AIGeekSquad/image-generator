namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Image generation parameters
/// </summary>
public class ImageParameters
{
    /// <summary>
    /// The model to use for image generation
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// Image size (e.g., "1024x1024")
    /// </summary>
    public string? Size { get; set; }
    
    /// <summary>
    /// Image quality ("standard" or "hd")
    /// </summary>
    public string? Quality { get; set; }
    
    /// <summary>
    /// Image style ("vivid" or "natural")
    /// </summary>
    public string? Style { get; set; }
    
    /// <summary>
    /// Number of images to generate
    /// </summary>
    public int NumberOfImages { get; set; } = 1;
    
    /// <summary>
    /// Response format preference ("url" or "b64_json")
    /// </summary>
    public string? ResponseFormat { get; set; }
}
