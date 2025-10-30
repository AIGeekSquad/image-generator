using System.Text.Json;
using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Core.Services;

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
        ArgumentNullException.ThrowIfNull(args);

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

        ValidatePromptRequirement(args, result);
        ValidateNumberOfImages(args, result);
        ValidateSizeFormat(args, result);
        ValidateQualityValues(args, result);
        ValidateStyleValues(args, result);
        ValidateConversationFormat(args, result);
        ValidateImageFormat(args, result);

        return result;
    }

    private static void ValidatePromptRequirement(ParsedArguments args, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(args.Prompt) &&
            (args.Conversation == null || args.Conversation.Count == 0))
        {
            result.Errors.Add("Either 'prompt' or valid 'conversationJson' is required");
        }
    }

    private static void ValidateNumberOfImages(ParsedArguments args, ValidationResult result)
    {
        if (args.NumberOfImages < 1 || args.NumberOfImages > 10)
        {
            result.Errors.Add("NumberOfImages must be between 1 and 10");
        }
    }

    private static void ValidateSizeFormat(ParsedArguments args, ValidationResult result)
    {
        if (!string.IsNullOrEmpty(args.Size) && args.ParsedSize == null)
        {
            result.Errors.Add($"Invalid size format: '{args.Size}'. Expected format: 'WIDTHxHEIGHT' (e.g., '1024x1024')");
        }
    }

    private static void ValidateQualityValues(ParsedArguments args, ValidationResult result)
    {
        if (!string.IsNullOrEmpty(args.Quality) &&
            !IsValidQuality(args.Quality))
        {
            result.Errors.Add("Quality must be either 'standard' or 'hd'");
        }
    }

    private static void ValidateStyleValues(ParsedArguments args, ValidationResult result)
    {
        if (!string.IsNullOrEmpty(args.Style) &&
            !IsValidStyle(args.Style))
        {
            result.Errors.Add("Style must be either 'vivid' or 'natural'");
        }
    }

    private static void ValidateConversationFormat(ParsedArguments args, ValidationResult result)
    {
        if (args.Conversation != null && args.Conversation.Count == 0)
        {
            result.Errors.Add("Conversation must contain at least one message");
        }
    }

    private static void ValidateImageFormat(ParsedArguments args, ValidationResult result)
    {
        if (!string.IsNullOrEmpty(args.Image) && !IsValidImageFormat(args.Image))
        {
            result.Errors.Add("Image must be a valid base64 encoded image, data URL, or HTTP URL");
        }
    }

    private static bool IsValidQuality(string quality) =>
        quality == "standard" || quality == "hd";

    private static bool IsValidStyle(string style) =>
        style == "vivid" || style == "natural";

    private static readonly char[] SizeSeparators = { 'x', 'X' };

    /// <summary>
    /// Parses a size string like "1024x1024" into structured format
    /// </summary>
    /// <param name="sizeString">Size string in format "WIDTHxHEIGHT"</param>
    /// <returns>Parsed size object, or null if invalid</returns>
    public static ImageSize? ParseSize(string? sizeString)
    {
        if (string.IsNullOrWhiteSpace(sizeString))
            return null;

        var parts = sizeString.Split(SizeSeparators, StringSplitOptions.None);
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