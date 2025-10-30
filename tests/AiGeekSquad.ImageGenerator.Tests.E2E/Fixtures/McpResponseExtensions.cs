namespace AiGeekSquad.ImageGenerator.Tests.E2E.Fixtures;

/// <summary>
/// Extension methods for MCP responses
/// </summary>
public static class McpResponseExtensions
{
    /// <summary>
    /// Checks if the response contains an error
    /// </summary>
    public static bool ContainError(this McpResponse response)
    {
        return !response.IsSuccess || response.Content.Contains("\"error\"");
    }
    
    /// <summary>
    /// Checks if the response is successful
    /// </summary>
    public static bool BeSuccessful(this McpResponse response)
    {
        return response.IsSuccess && !response.Content.Contains("\"error\"");
    }
}
