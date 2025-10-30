namespace AiGeekSquad.ImageGenerator.Tests.E2E.Fixtures;

/// <summary>
/// Response from an MCP tool call
/// </summary>
public class McpResponse
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// JSON response content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the tool that was called
    /// </summary>
    public string ToolName { get; set; } = string.Empty;
    
    /// <summary>
    /// Exception if one occurred
    /// </summary>
    public Exception? Exception { get; set; }
}
