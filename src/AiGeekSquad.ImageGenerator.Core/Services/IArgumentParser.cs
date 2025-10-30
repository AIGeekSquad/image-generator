namespace AiGeekSquad.ImageGenerator.Core.Services;

/// <summary>
/// Parser for MCP tool arguments with type-safe conversion and validation
/// </summary>
public interface IArgumentParser
{
    /// <summary>
    /// Parses MCP tool arguments into a strongly-typed object
    /// </summary>
    /// <param name="args">Dictionary of argument names and values from MCP</param>
    /// <returns>Parsed arguments object</returns>
    ParsedArguments Parse(Dictionary<string, object?> args);
    
    /// <summary>
    /// Validates parsed arguments according to business rules
    /// </summary>
    /// <param name="args">Parsed arguments to validate</param>
    /// <returns>Validation result with any errors</returns>
    ValidationResult Validate(ParsedArguments args);
}
