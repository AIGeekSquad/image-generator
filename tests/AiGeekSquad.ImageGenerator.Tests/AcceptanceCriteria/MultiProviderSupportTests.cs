using Microsoft.Extensions.AI;
using FluentAssertions;
using FluentAssertions.Execution;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
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
        using var scope = new AssertionScope();
        ImageModels.OpenAI.DallE2.Should().NotBeEmpty();
        ImageModels.OpenAI.DallE3.Should().NotBeEmpty();
        ImageModels.OpenAI.GPTImage1.Should().NotBeEmpty();
        ImageModels.OpenAI.GPTImage1Mini.Should().NotBeEmpty();
    }

    [Fact]
    public void AC2_Google_ModelsAreDefined()
    {
        // Acceptance Criteria: Tool should support Google Imagen models
        using var scope = new AssertionScope();
        ImageModels.Google.Imagen2.Should().NotBeEmpty();
        ImageModels.Google.Imagen3.Should().NotBeEmpty();
        ImageModels.Google.ImagenFast.Should().NotBeEmpty();
    }

    [Fact]
    public void AC3_ImageRequest_SupportsProviderSpecificParameters()
    {
        // Acceptance Criteria: Requests should support provider-specific parameters
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") },
            Model = "dall-e-3",
            AdditionalParameters = new Dictionary<string, object>
            {
                ["customParam1"] = "value1",
                ["customParam2"] = 42
            }
        };

        using var scope = new AssertionScope();
        request.AdditionalParameters.Should().NotBeNull();
        request.AdditionalParameters.Count.Should().Be(2);
        request.AdditionalParameters["customParam1"].Should().Be("value1");
        request.AdditionalParameters["customParam2"].Should().Be(42);
    }

    [Fact]
    public void AC4_ImageRequest_SupportsQualityAndStyle()
    {
        // Acceptance Criteria: Tool should support quality and style parameters
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") },
            Quality = ImageModels.Quality.HD,
            Style = ImageModels.Style.Vivid
        };

        using var scope = new AssertionScope();
        request.Quality.Should().Be(ImageModels.Quality.HD);
        request.Style.Should().Be(ImageModels.Style.Vivid);
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
            var request = new CoreImageRequest
            {
                Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") },
                Size = size
            };

            request.Size.Should().Be(size);
        }
    }

    [Fact]
    public void AC6_ImageRequest_SupportsMultipleImages()
    {
        // Acceptance Criteria: Tool should support generating multiple images in a single request
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") },
            NumberOfImages = 4
        };

        request.NumberOfImages.Should().Be(4);
    }

    [Fact]
    public void AC7_ImageResponse_ContainsProviderAndModelInfo()
    {
        // Acceptance Criteria: Responses should contain information about the provider and model used
        var response = new CoreImageResponse
        {
            Images = new List<GeneratedImage>(),
            Model = "dall-e-3",
            Provider = "OpenAI",
            CreatedAt = DateTime.UtcNow
        };

        using var scope = new AssertionScope();
        response.Model.Should().Be("dall-e-3");
        response.Provider.Should().Be("OpenAI");
        response.CreatedAt.Should().NotBe(default);
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

        using var scope = new AssertionScope();
        imageWithUrl.Url.Should().NotBeNull();
        imageWithUrl.Base64Data.Should().BeNull();
        imageWithBase64.Base64Data.Should().NotBeNull();
        imageWithBase64.Url.Should().BeNull();
    }
}
