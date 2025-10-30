namespace AiGeekSquad.ImageGenerator.Core.Abstractions;

/// <summary>
/// Supported image operations
/// </summary>
public enum ImageOperation
{
    /// <summary>
    /// Basic image generation from text prompt
    /// </summary>
    Generate,
    
    /// <summary>
    /// Image generation from conversational context with multiple messages
    /// </summary>
    GenerateFromConversation,
    
    /// <summary>
    /// Edit an existing image based on a text prompt
    /// </summary>
    Edit,
    
    /// <summary>
    /// Create variations of an existing image
    /// </summary>
    Variation
}
