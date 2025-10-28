using Microsoft.Extensions.AI;
using AiGeekSquad.ImageGenerator.Core.Abstractions;

namespace AiGeekSquad.ImageGenerator.Core.Models;

/// <summary>
/// Unified image request model that handles both simple prompts and conversational contexts
/// </summary>
public class UnifiedImageRequest
{
    /// <summary>
    /// The primary text prompt for image generation
    /// </summary>
    public string Prompt { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional context messages for conversational requests
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new();
    
    /// <summary>
    /// Images referenced in the request (for editing, variations, or context)
    /// </summary>
    public List<ImageReference> Images { get; set; } = new();
    
    /// <summary>
    /// Image generation parameters
    /// </summary>
    public ImageParameters Parameters { get; set; } = new();
    
    /// <summary>
    /// The specific provider to use (optional - will use selection strategy if not specified)
    /// </summary>
    public string? PreferredProvider { get; set; }
    
    /// <summary>
    /// The operation type being requested
    /// </summary>
    public ImageOperation Operation { get; set; } = ImageOperation.Generate;
    
    /// <summary>
    /// Additional provider-specific parameters
    /// </summary>
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();
}

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

/// <summary>
/// Role of an image in a request
/// </summary>
public enum ImageRole
{
    /// <summary>Source image for editing or variations</summary>
    Source,
    
    /// <summary>Mask image for selective editing</summary>
    Mask,
    
    /// <summary>Reference image for style or content guidance</summary>
    Reference,
    
    /// <summary>Example image in conversation context</summary>
    Example
}

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