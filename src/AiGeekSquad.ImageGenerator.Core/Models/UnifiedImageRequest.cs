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