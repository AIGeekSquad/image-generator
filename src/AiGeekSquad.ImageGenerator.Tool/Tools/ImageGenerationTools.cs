using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using CoreImageVariationRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageVariationRequest;
using CoreConversationalRequest = AiGeekSquad.ImageGenerator.Core.Models.ConversationalImageGenerationRequest;
using CoreConversationMessage = AiGeekSquad.ImageGenerator.Core.Models.ConversationMessage;

namespace AiGeekSquad.ImageGenerator.Tool.Tools;

/// <summary>
/// MCP tools for image generation.
/// These tools can be invoked by MCP clients to perform image generation operations.
/// </summary>
public class ImageGenerationTools(
    IImageGenerationService imageService,
    ILogger<ImageGenerationTools> logger)
{
    [McpServerTool]
    [Description("Generate an image from a text prompt using various AI providers (OpenAI, Google, etc.)")]
    public async Task<string> GenerateImage(
        [Description("The text prompt describing the image to generate")] string prompt,
        [Description("The provider to use (e.g., 'OpenAI', 'Google')")] string? provider = null,
        [Description("The model to use (e.g., 'dall-e-3', 'gpt-image-1', 'imagen-3.0-generate-001')")] string? model = null,
        [Description("The size of the image (e.g., '1024x1024', '1792x1024')")] string? size = null,
        [Description("The quality of the image ('standard' or 'hd')")] string? quality = null,
        [Description("The style of the image ('vivid' or 'natural')")] string? style = null,
        [Description("Number of images to generate")] int numberOfImages = 1)
    {
        try
        {
            // Validate arguments first
            var validationErrors = ValidateGenerateImageArgs(prompt, quality, style, size, numberOfImages);
            if (validationErrors.Any())
            {
                var errorResponse = new { error = string.Join("; ", validationErrors) };
                return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });
            }

            // Create a ChatMessage with the text prompt
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, prompt)
            };

            var request = new CoreImageRequest
            {
                Messages = messages,
                Model = model,
                Size = size,
                Quality = quality,
                Style = style,
                NumberOfImages = numberOfImages
            };

            var result = await imageService.GenerateImageAsync(
                provider ?? "OpenAI",
                request);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating image");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("Generate an image from a conversational context with multi-modal support (text and image references)")]
    public async Task<string> GenerateImageFromConversation(
        [Description("JSON array of conversation messages with role, text, and optional images. Example: [{\"role\":\"user\",\"text\":\"Create an image\",\"images\":[{\"url\":\"...\",\"caption\":\"reference\"}]}]")] 
        string conversationJson,
        [Description("The provider to use (e.g., 'OpenAI', 'Google')")] string? provider = null,
        [Description("The model to use")] string? model = null,
        [Description("The size of the image")] string? size = null,
        [Description("The quality of the image")] string? quality = null,
        [Description("The style of the image")] string? style = null,
        [Description("Number of images to generate")] int numberOfImages = 1)
    {
        try
        {
            var conversation = JsonSerializer.Deserialize<List<CoreConversationMessage>>(conversationJson);
            if (conversation == null || conversation.Count == 0)
            {
                return JsonSerializer.Serialize(new { error = "Conversation is required and must contain at least one message" });
            }

            var request = new CoreConversationalRequest
            {
                Conversation = conversation,
                Model = model,
                Size = size,
                Quality = quality,
                Style = style,
                NumberOfImages = numberOfImages
            };

            var result = await imageService.GenerateImageFromConversationAsync(
                provider ?? "OpenAI",
                request);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error parsing conversation JSON");
            return JsonSerializer.Serialize(new { error = $"Invalid conversation JSON: {ex.Message}" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating image from conversation");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("Edit an existing image based on a text prompt (OpenAI DALL-E 2 supported)")]
    public async Task<string> EditImage(
        [Description("The image to edit (base64 encoded or URL)")] string image,
        [Description("The text prompt describing the desired changes")] string prompt,
        [Description("Optional mask image (base64 encoded or URL)")] string? mask = null,
        [Description("The provider to use (e.g., 'OpenAI')")] string? provider = null,
        [Description("The model to use (e.g., 'dall-e-2')")] string? model = null,
        [Description("The size of the output image")] string? size = null,
        [Description("Number of images to generate")] int numberOfImages = 1)
    {
        try
        {
            // Create a ChatMessage with the edit prompt
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, prompt)
            };

            var request = new CoreImageEditRequest
            {
                Image = image,
                Messages = messages,
                Mask = mask,
                Model = model,
                Size = size,
                NumberOfImages = numberOfImages
            };

            var result = await imageService.EditImageAsync(
                provider ?? "OpenAI",
                request);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing image");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("Create variations of an existing image (OpenAI DALL-E 2 supported)")]
    public async Task<string> CreateVariation(
        [Description("The image to create variations from (base64 encoded or URL)")] string image,
        [Description("The provider to use (e.g., 'OpenAI')")] string? provider = null,
        [Description("The model to use (e.g., 'dall-e-2')")] string? model = null,
        [Description("The size of the output images")] string? size = null,
        [Description("Number of variations to generate")] int numberOfImages = 1)
    {
        try
        {
            var request = new CoreImageVariationRequest
            {
                Image = image,
                Model = model,
                Size = size,
                NumberOfImages = numberOfImages
            };

            var result = await imageService.CreateVariationAsync(
                provider ?? "OpenAI",
                request);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating image variation");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool]
    [Description("List all available image generation providers and their capabilities")]
    public string ListProviders()
    {
        try
        {
            var providers = imageService.GetProviders();
            var providerInfo = providers.Select(p => new
            {
                Name = p.ProviderName,
                Capabilities = p.GetCapabilities()
            }).ToList();

            return JsonSerializer.Serialize(providerInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing providers");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private static List<string> ValidateGenerateImageArgs(string prompt, string? quality, string? style, string? size, int numberOfImages)
    {
        var errors = new List<string>();

        // Validate prompt
        if (string.IsNullOrWhiteSpace(prompt))
            errors.Add("Prompt is required");

        // Validate numberOfImages
        if (numberOfImages < 1 || numberOfImages > 10)
            errors.Add("NumberOfImages must be between 1 and 10");

        // Validate quality
        if (!string.IsNullOrEmpty(quality) && !IsValidQuality(quality))
            errors.Add($"Quality must be 'standard' or 'hd', got '{quality}'");

        // Validate style
        if (!string.IsNullOrEmpty(style) && !IsValidStyle(style))
            errors.Add($"Style must be 'vivid' or 'natural', got '{style}'");

        // Validate size
        if (!string.IsNullOrEmpty(size) && !IsValidSize(size))
            errors.Add($"Size must be in format 'WIDTHxHEIGHT' (e.g., '1024x1024'), got '{size}'");

        return errors;
    }

    private static bool IsValidQuality(string quality) =>
        quality.Equals("standard", StringComparison.OrdinalIgnoreCase) ||
        quality.Equals("hd", StringComparison.OrdinalIgnoreCase);

    private static bool IsValidStyle(string style) =>
        style.Equals("vivid", StringComparison.OrdinalIgnoreCase) ||
        style.Equals("natural", StringComparison.OrdinalIgnoreCase);

    private static bool IsValidSize(string size)
    {
        if (string.IsNullOrWhiteSpace(size))
            return false;

        var parts = size.Split('x', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 &&
               int.TryParse(parts[0], out var width) && width > 0 &&
               int.TryParse(parts[1], out var height) && height > 0;
    }
}
