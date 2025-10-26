using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Tests.AcceptanceCriteria;

/// <summary>
/// Acceptance Criteria Tests: Verify support for multiple AI providers and models
/// </summary>
public class MultiProviderSupportTests
{
    [Fact]
    public void AC1_OpenAI_ModelsAreDefined()
    {
        // Acceptance Criteria: Tool should support OpenAI DALL-E 2, DALL-E 3, GPT Image 1, and future models
        Assert.NotEmpty(ImageModels.OpenAI.DallE2);
        Assert.NotEmpty(ImageModels.OpenAI.DallE3);
        Assert.NotEmpty(ImageModels.OpenAI.GPTImage1);
        Assert.NotEmpty(ImageModels.OpenAI.GPT5Image);
    }

    [Fact]
    public void AC2_Google_ModelsAreDefined()
    {
        // Acceptance Criteria: Tool should support Google Imagen models
        Assert.NotEmpty(ImageModels.Google.Imagen2);
        Assert.NotEmpty(ImageModels.Google.Imagen3);
        Assert.NotEmpty(ImageModels.Google.ImagenFast);
    }

    [Fact]
    public void AC3_ImageRequest_SupportsProviderSpecificParameters()
    {
        // Acceptance Criteria: Requests should support provider-specific parameters
        var request = new ImageGenerationRequest
        {
            Prompt = "test",
            Model = "dall-e-3",
            AdditionalParameters = new Dictionary<string, object>
            {
                ["customParam1"] = "value1",
                ["customParam2"] = 42
            }
        };

        Assert.NotNull(request.AdditionalParameters);
        Assert.Equal(2, request.AdditionalParameters.Count);
        Assert.Equal("value1", request.AdditionalParameters["customParam1"]);
        Assert.Equal(42, request.AdditionalParameters["customParam2"]);
    }

    [Fact]
    public void AC4_ImageRequest_SupportsQualityAndStyle()
    {
        // Acceptance Criteria: Tool should support quality and style parameters
        var request = new ImageGenerationRequest
        {
            Prompt = "test",
            Quality = ImageModels.Quality.HD,
            Style = ImageModels.Style.Vivid
        };

        Assert.Equal(ImageModels.Quality.HD, request.Quality);
        Assert.Equal(ImageModels.Style.Vivid, request.Style);
    }

    [Fact]
    public void AC5_ImageRequest_SupportsDifferentSizes()
    {
        // Acceptance Criteria: Tool should support different image sizes
        var sizes = new[]
        {
            ImageModels.Sizes.Square256,
            ImageModels.Sizes.Square512,
            ImageModels.Sizes.Square1024,
            ImageModels.Sizes.Wide1792x1024,
            ImageModels.Sizes.Tall1024x1792
        };

        foreach (var size in sizes)
        {
            var request = new ImageGenerationRequest
            {
                Prompt = "test",
                Size = size
            };

            Assert.Equal(size, request.Size);
        }
    }

    [Fact]
    public void AC6_ImageRequest_SupportsMultipleImages()
    {
        // Acceptance Criteria: Tool should support generating multiple images in a single request
        var request = new ImageGenerationRequest
        {
            Prompt = "test",
            NumberOfImages = 4
        };

        Assert.Equal(4, request.NumberOfImages);
    }

    [Fact]
    public void AC7_ImageResponse_ContainsProviderAndModelInfo()
    {
        // Acceptance Criteria: Responses should contain information about the provider and model used
        var response = new ImageGenerationResponse
        {
            Images = new List<GeneratedImage>(),
            Model = "dall-e-3",
            Provider = "OpenAI",
            CreatedAt = DateTime.UtcNow
        };

        Assert.Equal("dall-e-3", response.Model);
        Assert.Equal("OpenAI", response.Provider);
        Assert.NotEqual(default, response.CreatedAt);
    }

    [Fact]
    public void AC8_GeneratedImage_SupportsUrlAndBase64()
    {
        // Acceptance Criteria: Generated images can be returned as URLs or base64 data
        var imageWithUrl = new GeneratedImage
        {
            Url = "https://example.com/image.png"
        };

        var imageWithBase64 = new GeneratedImage
        {
            Base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQ="
        };

        Assert.NotNull(imageWithUrl.Url);
        Assert.Null(imageWithUrl.Base64Data);
        Assert.NotNull(imageWithBase64.Base64Data);
        Assert.Null(imageWithBase64.Url);
    }
}
