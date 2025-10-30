using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Parsed and type-safe MCP tool arguments
/// </summary>
public class ParsedArguments
{
    // Basic generation parameters
    /// <summary>Gets or sets the text prompt for image generation</summary>
    public string? Prompt { get; set; }
    /// <summary>Gets or sets the preferred image generation provider</summary>
    public string? Provider { get; set; }
    /// <summary>Gets or sets the model to use for image generation</summary>
    public string? Model { get; set; }
    /// <summary>Gets or sets the size of the generated image</summary>
    public string? Size { get; set; }
    /// <summary>Gets or sets the quality of the generated image</summary>
    public string? Quality { get; set; }
    /// <summary>Gets or sets the style of the generated image</summary>
    public string? Style { get; set; }
    /// <summary>Gets or sets the number of images to generate</summary>
    public int NumberOfImages { get; set; } = 1;
    
    // Image-specific parameters
    /// <summary>Gets or sets the source image for editing or variation</summary>
    public string? Image { get; set; }
    /// <summary>Gets or sets the mask image for targeted editing</summary>
    public string? Mask { get; set; }
    
    // Conversational parameters
    /// <summary>Gets or sets the conversation messages for conversational image generation</summary>
    public List<ConversationMessage>? Conversation { get; set; }
    
    // Parsed size information
    /// <summary>Gets or sets the parsed size information</summary>
    public ImageSize? ParsedSize { get; set; }
}
