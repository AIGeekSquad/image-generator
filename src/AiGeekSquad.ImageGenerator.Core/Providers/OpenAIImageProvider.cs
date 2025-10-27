using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Images;
using Microsoft.Extensions.AI;
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
    private readonly string? _defaultDeployment;

    public override string ProviderName => "OpenAI";

    protected override ProviderCapabilities Capabilities { get; }

    /// <summary>
    /// Creates an OpenAI provider with the specified configuration
    /// </summary>
    public OpenAIImageProvider(string apiKey, string? endpoint = null, string? defaultDeployment = null, HttpClient? httpClient = null)
        : this(CreateAdapter(apiKey, endpoint), defaultDeployment, httpClient)
    {
    }

    /// <summary>
    /// Creates an OpenAI provider with a custom adapter (useful for testing)
    /// </summary>
    public OpenAIImageProvider(IOpenAIAdapter adapter, string? defaultDeployment = null, HttpClient? httpClient = null)
        : base(httpClient)
    {
        _adapter = adapter;
        _defaultDeployment = defaultDeployment;

        Capabilities = new ProviderCapabilities
        {
            ExampleModels = new List<string>
            {
                ImageModels.OpenAI.DallE3,
                ImageModels.OpenAI.DallE2,
                ImageModels.OpenAI.GPTImage1,
                ImageModels.OpenAI.GPT5Image
            },
            SupportedOperations = new List<ImageOperation>
            {
                ImageOperation.Generate,
                ImageOperation.Edit,
                ImageOperation.Variation
            },
            DefaultModel = _defaultDeployment ?? ImageModels.OpenAI.DallE3,
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

    public override async Task<CoreImageResponse> GenerateImageAsync(
        CoreImageRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = GetModelOrDefault(request.Model);
        var prompt = ExtractTextFromMessages(request.Messages);

        var options = new OpenAI.Images.ImageGenerationOptions
        {
            Size = ParseSize(request.Size),
            Quality = ParseQuality(request.Quality),
            Style = ParseStyle(request.Style),
            ResponseFormat = GeneratedImageFormat.Uri
        };

        var result = await _adapter.GenerateImageAsync(model, prompt, options, cancellationToken);

        return BuildSingleImageResponse(
            result.ImageUri?.ToString(),
            model,
            result.RevisedPrompt);
    }

    public override async Task<CoreImageResponse> EditImageAsync(
        CoreImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? ImageModels.OpenAI.DallE2;
        var prompt = ExtractTextFromMessages(request.Messages);
        var imageStream = await ConvertToStreamAsync(request.Image, cancellationToken);

        var result = await _adapter.GenerateImageEditAsync(
            model,
            imageStream,
            "image.png",
            prompt,
            new ImageEditOptions
            {
                Size = ParseSize(request.Size),
                ResponseFormat = GeneratedImageFormat.Uri
            },
            cancellationToken);

        return BuildSingleImageResponse(
            result.ImageUri?.ToString(),
            model,
            result.RevisedPrompt);
    }

    public override async Task<CoreImageResponse> CreateVariationAsync(
        CoreImageVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? ImageModels.OpenAI.DallE2;
        var imageStream = await ConvertToStreamAsync(request.Image, cancellationToken);

        var result = await _adapter.GenerateImageVariationAsync(
            model,
            imageStream,
            "image.png",
            new ImageVariationOptions
            {
                Size = ParseSize(request.Size),
                ResponseFormat = GeneratedImageFormat.Uri
            },
            cancellationToken);

        return BuildSingleImageResponse(result.ImageUri?.ToString(), model);
    }

    private static GeneratedImageSize? ParseSize(string? size)
    {
        return size switch
        {
            ImageModels.Sizes.Square256 => GeneratedImageSize.W256xH256,
            ImageModels.Sizes.Square512 => GeneratedImageSize.W512xH512,
            ImageModels.Sizes.Square1024 => GeneratedImageSize.W1024xH1024,
            ImageModels.Sizes.Wide1792x1024 => GeneratedImageSize.W1792xH1024,
            ImageModels.Sizes.Tall1024x1792 => GeneratedImageSize.W1024xH1792,
            _ => null
        };
    }

    private static GeneratedImageQuality? ParseQuality(string? quality)
    {
        return quality switch
        {
            ImageModels.Quality.Standard => GeneratedImageQuality.Standard,
            ImageModels.Quality.HD => GeneratedImageQuality.High,
            _ => null
        };
    }

    private static GeneratedImageStyle? ParseStyle(string? style)
    {
        return style switch
        {
            ImageModels.Style.Vivid => GeneratedImageStyle.Vivid,
            ImageModels.Style.Natural => GeneratedImageStyle.Natural,
            _ => null
        };
    }
}
