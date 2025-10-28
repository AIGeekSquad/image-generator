using System.Text.Json;
using AiGeekSquad.ImageGenerator.Core.Models;

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

/// <summary>
/// Parsed image size dimensions
/// </summary>
public class ImageSize
{
    /// <summary>Gets or sets the image width in pixels</summary>
    public int Width { get; set; }
    /// <summary>Gets or sets the image height in pixels</summary>
    public int Height { get; set; }
    
    /// <summary>Returns a string representation of the image size</summary>
    public override string ToString() => $"{Width}x{Height}";
}

/// <summary>
/// Validation result for parsed arguments
/// </summary>
public class ValidationResult
{
    /// <summary>Gets whether the validation passed (no errors)</summary>
    public bool IsValid => !Errors.Any();
    /// <summary>Gets or sets the list of validation errors</summary>
    public List<string> Errors { get; set; } = new();
    /// <summary>Gets or sets the list of validation warnings</summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Default implementation of the MCP argument parser
/// </summary>
public class McpArgumentParser : IArgumentParser
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Parses MCP tool arguments into a strongly-typed object
    /// </summary>
    /// <param name="args">Dictionary of argument names and values from MCP</param>
    /// <returns>Parsed arguments object</returns>
    public ParsedArguments Parse(Dictionary<string, object?> args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        var parsed = new ParsedArguments();

        // Parse basic string parameters
        parsed.Prompt = GetString(args, "prompt");
        parsed.Provider = GetString(args, "provider");
        parsed.Model = GetString(args, "model");
        parsed.Size = GetString(args, "size");
        parsed.Quality = GetString(args, "quality");
        parsed.Style = GetString(args, "style");
        parsed.Image = GetString(args, "image");
        parsed.Mask = GetString(args, "mask");

        // Parse integer parameters
        parsed.NumberOfImages = GetInt(args, "numberOfImages", 1);

        // Parse complex parameters
        if (args.TryGetValue("conversationJson", out var convJson) && convJson != null)
        {
            parsed.Conversation = ParseConversation(convJson.ToString()!);
        }

        // Parse size into structured format
        if (!string.IsNullOrEmpty(parsed.Size))
        {
            parsed.ParsedSize = ParseSize(parsed.Size);
        }

        return parsed;
    }

    /// <summary>
    /// Validates parsed arguments according to business rules
    /// </summary>
    /// <param name="args">Parsed arguments to validate</param>
    /// <returns>Validation result with any errors</returns>
    public ValidationResult Validate(ParsedArguments args)
    {
        var result = new ValidationResult();

        // Validate prompt requirement (for non-conversation requests)
        if (string.IsNullOrWhiteSpace(args.Prompt) && 
            (args.Conversation == null || args.Conversation.Count == 0))
        {
            result.Errors.Add("Either 'prompt' or valid 'conversationJson' is required");
        }

        // Validate number of images
        if (args.NumberOfImages < 1 || args.NumberOfImages > 10)
        {
            result.Errors.Add("NumberOfImages must be between 1 and 10");
        }

        // Validate size format
        if (!string.IsNullOrEmpty(args.Size) && args.ParsedSize == null)
        {
            result.Errors.Add($"Invalid size format: '{args.Size}'. Expected format: 'WIDTHxHEIGHT' (e.g., '1024x1024')");
        }

        // Validate quality values
        if (!string.IsNullOrEmpty(args.Quality) && 
            args.Quality != "standard" && args.Quality != "hd")
        {
            result.Errors.Add("Quality must be either 'standard' or 'hd'");
        }

        // Validate style values
        if (!string.IsNullOrEmpty(args.Style) && 
            args.Style != "vivid" && args.Style != "natural")
        {
            result.Errors.Add("Style must be either 'vivid' or 'natural'");
        }

        // Validate conversation format
        if (args.Conversation != null && args.Conversation.Count == 0)
        {
            result.Errors.Add("Conversation must contain at least one message");
        }

        // Validate image format (for edit/variation operations)
        if (!string.IsNullOrEmpty(args.Image))
        {
            if (!IsValidImageFormat(args.Image))
            {
                result.Errors.Add("Image must be a valid base64 encoded image, data URL, or HTTP URL");
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a size string like "1024x1024" into structured format
    /// </summary>
    /// <param name="sizeString">Size string in format "WIDTHxHEIGHT"</param>
    /// <returns>Parsed size object, or null if invalid</returns>
    public ImageSize? ParseSize(string? sizeString)
    {
        if (string.IsNullOrWhiteSpace(sizeString))
            return null;

        var parts = sizeString.Split('x', 'X');
        if (parts.Length != 2)
            return null;

        if (int.TryParse(parts[0], out var width) && 
            int.TryParse(parts[1], out var height) &&
            width > 0 && height > 0)
        {
            return new ImageSize { Width = width, Height = height };
        }

        return null;
    }

    /// <summary>
    /// Parses conversation JSON into message objects
    /// </summary>
    /// <param name="conversationJson">JSON string containing conversation messages</param>
    /// <returns>List of conversation messages</returns>
    public List<ConversationMessage>? ParseConversation(string conversationJson)
    {
        if (string.IsNullOrWhiteSpace(conversationJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<ConversationMessage>>(conversationJson, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? GetString(Dictionary<string, object?> args, string key)
    {
        return args.TryGetValue(key, out var value) ? value?.ToString() : null;
    }

    private static int GetInt(Dictionary<string, object?> args, string key, int defaultValue)
    {
        if (!args.TryGetValue(key, out var value) || value == null)
            return defaultValue;

        return value switch
        {
            int intValue => intValue,
            string strValue when int.TryParse(strValue, out var parsed) => parsed,
            double doubleValue => (int)doubleValue,
            float floatValue => (int)floatValue,
            _ => defaultValue
        };
    }

    private static bool IsValidImageFormat(string image)
    {
        if (string.IsNullOrWhiteSpace(image))
            return false;

        // Check for data URL format
        if (image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return true;

        // Check for HTTP/HTTPS URL
        if (Uri.TryCreate(image, UriKind.Absolute, out var uri) && 
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            return true;

        // Check for base64 format (simple validation)
        try
        {
            Convert.FromBase64String(image);
            return true;
        }
        catch
        {
            return false;
        }
    }
}