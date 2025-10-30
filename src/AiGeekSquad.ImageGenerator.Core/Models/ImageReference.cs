namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Reference to an image in a request
/// </summary>
public class ImageReference
{
    /// <summary>
    /// URL to the image
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Base64 encoded image data
    /// </summary>
    public string? Base64Data { get; set; }
    
    /// <summary>
    /// MIME type of the image
    /// </summary>
    public string? MimeType { get; set; }
    
    /// <summary>
    /// Caption or description of the image
    /// </summary>
    public string? Caption { get; set; }
    
    /// <summary>
    /// Role of this image in the request (source, mask, reference, etc.)
    /// </summary>
    public ImageRole Role { get; set; } = ImageRole.Source;
}
