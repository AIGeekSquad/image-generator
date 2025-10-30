namespace AiGeekSquad.ImageGenerator.Core.Models;

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
