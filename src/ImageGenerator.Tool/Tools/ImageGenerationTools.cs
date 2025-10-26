using System.ComponentModel;
using System.Text.Json;
using ImageGenerator.Core.Abstractions;
using ImageGenerator.Core.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace ImageGenerator.Tool.Tools;

/// <summary>
/// MCP tools for image generation.
/// These tools can be invoked by MCP clients to perform image generation operations.
/// </summary>
internal class ImageGenerationTools(
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
            var request = new ImageGenerationRequest
            {
                Prompt = prompt,
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
            var request = new ImageEditRequest
            {
                Image = image,
                Prompt = prompt,
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
            var request = new ImageVariationRequest
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
}
