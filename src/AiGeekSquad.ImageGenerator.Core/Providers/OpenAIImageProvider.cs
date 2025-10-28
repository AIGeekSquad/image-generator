using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Images;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using CoreImageVariationRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageVariationRequest;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Adapters;

namespace AiGeekSquad.ImageGenerator.Core.Providers;

/// <summary>
/// OpenAI image generation provider supporting DALL-E and all current/future models
/// </summary>
public class OpenAIImageProvider : ImageProviderBase
{
    private readonly IOpenAIAdapter _adapter;

    /// <summary>
    /// Gets the provider name
    /// </summary>
    public override string ProviderName => "OpenAI";

    /// <summary>
    /// Gets the provider capabilities
    /// </summary>
    protected override ProviderCapabilities Capabilities { get; }

    /// <summary>
    /// Creates an OpenAI provider with the specified configuration
    /// </summary>
    /// <param name="apiKey">OpenAI API key</param>
    /// <param name="endpoint">Optional Azure OpenAI endpoint URL</param>
    /// <param name="defaultDeployment">Optional default model deployment name</param>
    /// <param name="httpClient">HTTP client for downloading images</param>
    public OpenAIImageProvider(string apiKey, string? endpoint = null, string? defaultDeployment = null, HttpClient? httpClient = null)
        : this(CreateAdapter(apiKey, endpoint), defaultDeployment, httpClient ?? new HttpClient())
    {
    }

    /// <summary>
    /// Creates an OpenAI provider with a custom adapter (useful for testing)
    /// </summary>
    /// <param name="adapter">Custom OpenAI adapter implementation</param>
    /// <param name="defaultDeployment">Optional default model deployment name</param>
    /// <param name="httpClient">HTTP client for downloading images</param>
    public OpenAIImageProvider(IOpenAIAdapter adapter, string? defaultDeployment = null, HttpClient? httpClient = null)
        : base(httpClient ?? new HttpClient())
    {
        _adapter = adapter;
        var defaultModel = defaultDeployment ?? ImageModels.OpenAI.DallE3;

        Capabilities = new ProviderCapabilities
        {
            ExampleModels = new List<string>
            {
                ImageModels.OpenAI.DallE3,
                ImageModels.OpenAI.DallE2,
                ImageModels.OpenAI.GPTImage1,
                ImageModels.OpenAI.GPTImage1Mini
            },
            SupportedOperations = new List<ImageOperation>
            {
                ImageOperation.Generate,
                ImageOperation.Edit,
                ImageOperation.Variation
            },
            DefaultModel = defaultModel,
            AcceptsCustomModels = true,
            Features = new Dictionary<string, object>
            {
                ["supportsHD"] = true,
                ["supportsStyles"] = true,
                ["maxSize"] = "1792x1024",
                ["formats"] = new[] { "png" }
            }
        };
    }

    private static IOpenAIAdapter CreateAdapter(string apiKey, string? endpoint)
    {
        OpenAIClient client;
        
        if (!string.IsNullOrEmpty(endpoint))
        {
            // Azure OpenAI
            client = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(apiKey));
        }
        else
        {
            // Standard OpenAI
            client = new OpenAIClient(apiKey);
        }

        return new OpenAIAdapter(client);
    }

    /// <summary>
    /// Generates an image using OpenAI's image generation models
    /// </summary>
    /// <param name="request">Image generation request with messages and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the generated image URL and optional revised prompt</returns>
    public override async Task<CoreImageResponse> GenerateImageAsync(
        CoreImageRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = GetModelOrDefault(request.Model);
        var prompt = ExtractTextFromMessages(request.Messages);

        var options = new OpenAI.Images.ImageGenerationOptions();
        
        // Only set ResponseFormat for DALL-E models - gpt-image-1 always returns base64 and doesn't support this parameter
        if (model.StartsWith("dall-e-", StringComparison.OrdinalIgnoreCase))
        {
            options.ResponseFormat = GeneratedImageFormat.Uri;
        }

        var size = ParseSize(request.Size);
        if (size.HasValue)
        {
            options.Size = size.Value;
        }

        var quality = ParseQuality(request.Quality);
        if (quality.HasValue)
        {
            options.Quality = quality.Value;
        }

        var style = ParseStyle(request.Style);
        if (style.HasValue)
        {
            options.Style = style.Value;
        }

        // Note: Multiple image generation with DALL-E 2 is currently limited by OpenAI SDK
        // The SDK's GenerateImageAsync method only returns a single image at this time

        var result = await _adapter.GenerateImageAsync(model, prompt, options, cancellationToken);

        // Handle response based on model type
        return await BuildImageResponseAsync(result, model, cancellationToken);
    }

    /// <summary>
    /// Edits an existing image based on a text prompt using DALL-E 2
    /// </summary>
    /// <param name="request">Image edit request with source image and instructions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the edited image URL and optional revised prompt</returns>
    public override async Task<CoreImageResponse> EditImageAsync(
        CoreImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? ImageModels.OpenAI.DallE2;
        var prompt = ExtractTextFromMessages(request.Messages);
        var imageStream = await ConvertToStreamAsync(request.Image, cancellationToken);

        var options = new ImageEditOptions();
        
        // Only set ResponseFormat for DALL-E models - gpt-image-1 always returns base64 and doesn't support this parameter
        if (model.StartsWith("dall-e-", StringComparison.OrdinalIgnoreCase))
        {
            options.ResponseFormat = GeneratedImageFormat.Uri;
        }

        var size = ParseSize(request.Size);
        if (size.HasValue)
        {
            options.Size = size.Value;
        }

        var result = await _adapter.GenerateImageEditAsync(
            model,
            imageStream,
            "image.png",
            prompt,
            options,
            cancellationToken);

        // Handle response based on model type
        return await BuildImageResponseAsync(result, model, cancellationToken);
    }

    /// <summary>
    /// Creates variations of an existing image using DALL-E 2
    /// </summary>
    /// <param name="request">Image variation request with source image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the variation image URL</returns>
    public override async Task<CoreImageResponse> CreateVariationAsync(
        CoreImageVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? ImageModels.OpenAI.DallE2;
        var imageStream = await ConvertToStreamAsync(request.Image, cancellationToken);

        var options = new ImageVariationOptions();
        
        // Only set ResponseFormat for DALL-E models - gpt-image-1 always returns base64 and doesn't support this parameter
        if (model.StartsWith("dall-e-", StringComparison.OrdinalIgnoreCase))
        {
            options.ResponseFormat = GeneratedImageFormat.Uri;
        }

        var size = ParseSize(request.Size);
        if (size.HasValue)
        {
            options.Size = size.Value;
        }

        var result = await _adapter.GenerateImageVariationAsync(
            model,
            imageStream,
            "image.png",
            options,
            cancellationToken);

        return await BuildSingleImageResponseFromUrlAsync(result.ImageUri?.ToString(), model, null, null, cancellationToken);
    }
    /// <summary>
    /// Builds a CoreImageResponse from an OpenAI result, handling both GPT image models (Base64) and DALL-E models (URLs)
    /// </summary>
    /// <param name="result">The OpenAI GeneratedImage result</param>
    /// <param name="model">The model name used for generation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A properly formatted CoreImageResponse</returns>
    private async Task<CoreImageResponse> BuildImageResponseAsync(
        OpenAI.Images.GeneratedImage result,
        string model,
        CancellationToken cancellationToken)
    {
        // Check model type first - GPT Image models return Base64, DALL-E models return URLs
        if (model.StartsWith("gpt-image", StringComparison.OrdinalIgnoreCase) && result.ImageBytes != null)
        {
            // GPT Image models - use Base64 data directly
            var imageBytes = result.ImageBytes.ToArray();
            var base64Data = Convert.ToBase64String((byte[])imageBytes);
            return new CoreImageResponse
            {
                Images = new List<Models.GeneratedImage>
                {
                    new()
                    {
                        Base64Data = base64Data,
                        Url = null,
                        RevisedPrompt = result.RevisedPrompt
                    }
                },
                Model = model,
                Provider = "OpenAI"
            };
        }
        else
        {
            // DALL-E models or fallback - download from URL
            return await BuildSingleImageResponseFromUrlAsync(
                result.ImageUri?.ToString(),
                model,
                result.RevisedPrompt,
                null,
                cancellationToken);
        }
    }


    private static GeneratedImageSize? ParseSize(string? size)
    {
        if (size == null) return null;
        
        if (size == ImageModels.Sizes.Square256) return GeneratedImageSize.W256xH256;
        if (size == ImageModels.Sizes.Square512) return GeneratedImageSize.W512xH512;
        if (size == ImageModels.Sizes.Square1024) return GeneratedImageSize.W1024xH1024;
        if (size == ImageModels.Sizes.Wide1792x1024) return GeneratedImageSize.W1792xH1024;
        if (size == ImageModels.Sizes.Tall1024x1792) return GeneratedImageSize.W1024xH1792;
        
        return null;
    }

    private static GeneratedImageQuality? ParseQuality(string? quality)
    {
        if (quality == null) return null;
        
        if (quality == ImageModels.Quality.Standard) return GeneratedImageQuality.Standard;
        if (quality == ImageModels.Quality.HD) return GeneratedImageQuality.High;
        
        return null;
    }

    private static GeneratedImageStyle? ParseStyle(string? style)
    {
        if (style == null) return null;
        
        if (style == ImageModels.Style.Vivid) return GeneratedImageStyle.Vivid;
        if (style == ImageModels.Style.Natural) return GeneratedImageStyle.Natural;
        
        return null;
    }
}
