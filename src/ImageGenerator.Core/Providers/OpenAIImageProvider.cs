using Azure.AI.OpenAI;
using OpenAI;
using ImageGenerator.Core.Abstractions;
using ImageGenerator.Core.Models;
using OpenAI.Images;
using GeneratedImageModel = ImageGenerator.Core.Models.GeneratedImage;

namespace ImageGenerator.Core.Providers;

/// <summary>
/// OpenAI image generation provider supporting DALL-E and all current/future models
/// </summary>
public class OpenAIImageProvider : ImageProviderBase
{
    private readonly OpenAIClient _client;
    private readonly string? _defaultDeployment;

    public override string ProviderName => "OpenAI";

    protected override ProviderCapabilities Capabilities { get; }

    public OpenAIImageProvider(string apiKey, string? endpoint = null, string? defaultDeployment = null)
    {
        _defaultDeployment = defaultDeployment;
        
        if (!string.IsNullOrEmpty(endpoint))
        {
            // Azure OpenAI
            _client = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(apiKey));
        }
        else
        {
            // Standard OpenAI
            _client = new OpenAIClient(apiKey);
        }

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

    public override async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = GetModelOrDefault(request.Model);
        var imageClient = _client.GetImageClient(model);

        var options = new ImageGenerationOptions
        {
            Size = ParseSize(request.Size),
            Quality = ParseQuality(request.Quality),
            Style = ParseStyle(request.Style),
            ResponseFormat = GeneratedImageFormat.Uri
        };

        var result = await imageClient.GenerateImageAsync(
            request.Prompt,
            options,
            cancellationToken);

        var images = new List<GeneratedImageModel>();
        
        if (result.Value != null)
        {
            images.Add(new GeneratedImageModel
            {
                Url = result.Value.ImageUri?.ToString(),
                RevisedPrompt = result.Value.RevisedPrompt
            });
        }

        return new ImageGenerationResponse
        {
            Images = images,
            Model = model,
            Provider = ProviderName,
            CreatedAt = DateTime.UtcNow
        };
    }

    public override async Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        // Use the requested model or fall back to DALL-E 2 for editing
        var model = request.Model ?? ImageModels.OpenAI.DallE2;
        var imageClient = _client.GetImageClient(model);

        var imageStream = ConvertToStream(request.Image);
        var imageName = "image.png";

        var result = await imageClient.GenerateImageEditAsync(
            imageStream,
            imageName,
            request.Prompt,
            new ImageEditOptions
            {
                Size = ParseSize(request.Size),
                ResponseFormat = GeneratedImageFormat.Uri
            },
            cancellationToken);

        var images = new List<GeneratedImageModel>();
        
        if (result.Value != null)
        {
            images.Add(new GeneratedImageModel
            {
                Url = result.Value.ImageUri?.ToString(),
                RevisedPrompt = result.Value.RevisedPrompt
            });
        }

        return new ImageGenerationResponse
        {
            Images = images,
            Model = model,
            Provider = ProviderName,
            CreatedAt = DateTime.UtcNow
        };
    }

    public override async Task<ImageGenerationResponse> CreateVariationAsync(
        ImageVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Use the requested model or fall back to DALL-E 2 for variations
        var model = request.Model ?? ImageModels.OpenAI.DallE2;
        var imageClient = _client.GetImageClient(model);

        var imageStream = ConvertToStream(request.Image);
        var imageName = "image.png";

        var result = await imageClient.GenerateImageVariationAsync(
            imageStream,
            imageName,
            new ImageVariationOptions
            {
                Size = ParseSize(request.Size),
                ResponseFormat = GeneratedImageFormat.Uri
            },
            cancellationToken);

        var images = new List<GeneratedImageModel>();
        
        if (result.Value != null)
        {
            images.Add(new GeneratedImageModel
            {
                Url = result.Value.ImageUri?.ToString()
            });
        }

        return new ImageGenerationResponse
        {
            Images = images,
            Model = model,
            Provider = ProviderName,
            CreatedAt = DateTime.UtcNow
        };
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
