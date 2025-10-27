using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Providers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.AI;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;

namespace AiGeekSquad.ImageGenerator.Tests.Providers;

/// <summary>
/// Unit tests for ImageProviderBase helper methods
/// </summary>
public class ImageProviderBaseTests
{
    private class TestProvider : ImageProviderBase
    {
        public override string ProviderName => "Test";

        protected override ProviderCapabilities Capabilities { get; } = new()
        {
            ExampleModels = new List<string> { "test-model" },
            SupportedOperations = new List<ImageOperation> { ImageOperation.Generate },
            SupportsMultiModalInput = false,
            AcceptsCustomModels = true
        };

        public override Task<CoreImageResponse> GenerateImageAsync(
            CoreImageRequest request, 
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // Expose protected methods for testing
        public CoreImageResponse TestBuildResponse(
            string model, 
            List<GeneratedImage> images)
        {
            return BuildResponse(model, images);
        }

        public CoreImageResponse TestBuildSingleImageResponse(
            string model, 
            string imageUrl)
        {
            return BuildSingleImageResponse(model, imageUrl);
        }

        public CoreImageResponse TestBuildSingleImageResponseFromBase64(
            string model, 
            string base64Data)
        {
            return BuildSingleImageResponseFromBase64(model, base64Data);
        }

        public string TestExtractTextFromMessages(IList<ChatMessage> messages)
        {
            return ExtractTextFromMessages(messages);
        }
    }

    [Fact]
    public void SupportsOperation_WithSupportedOperation_ReturnsTrue()
    {
        // Arrange
        var provider = new TestProvider();

        // Act
        var result = provider.SupportsOperation(ImageOperation.Generate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsOperation_WithUnsupportedOperation_ReturnsFalse()
    {
        // Arrange
        var provider = new TestProvider();

        // Act
        var result = provider.SupportsOperation(ImageOperation.Edit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetCapabilities_ReturnsCorrectCapabilities()
    {
        // Arrange
        var provider = new TestProvider();

        // Act
        var capabilities = provider.GetCapabilities();

        // Assert
        using var scope = new AssertionScope();
        capabilities.Should().NotBeNull();
        capabilities.ExampleModels.Should().Contain("test-model");
        capabilities.SupportedOperations.Should().Contain(ImageOperation.Generate);
        capabilities.SupportsMultiModalInput.Should().BeFalse();
        capabilities.AcceptsCustomModels.Should().BeTrue();
    }

    [Fact]
    public void BuildResponse_WithValidInputs_ReturnsCorrectResponse()
    {
        // Arrange
        var provider = new TestProvider();
        var images = new List<GeneratedImage>
        {
            new() { Url = "http://example.com/image1.png" },
            new() { Url = "http://example.com/image2.png" }
        };

        // Act
        var response = provider.TestBuildResponse("test-model", images);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Model.Should().Be("test-model");
        response.ProviderName.Should().Be("Test");
        response.Images.Should().HaveCount(2);
        response.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void BuildSingleImageResponse_WithUrl_ReturnsCorrectResponse()
    {
        // Arrange
        var provider = new TestProvider();

        // Act
        var response = provider.TestBuildSingleImageResponse("test-model", "http://example.com/image.png");

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Model.Should().Be("test-model");
        response.Images.Should().ContainSingle();
        response.Images[0].Url.Should().Be("http://example.com/image.png");
    }

    [Fact]
    public void BuildSingleImageResponseFromBase64_WithData_ReturnsCorrectResponse()
    {
        // Arrange
        var provider = new TestProvider();

        // Act
        var response = provider.TestBuildSingleImageResponseFromBase64("test-model", "base64data");

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Model.Should().Be("test-model");
        response.Images.Should().ContainSingle();
        response.Images[0].DataBase64.Should().Be("base64data");
    }

    [Fact]
    public void ExtractTextFromMessages_WithSingleMessage_ReturnsText()
    {
        // Arrange
        var provider = new TestProvider();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello world")
        };

        // Act
        var result = provider.TestExtractTextFromMessages(messages);

        // Assert
        result.Should().Be("Hello world");
    }

    [Fact]
    public void ExtractTextFromMessages_WithMultipleMessages_ReturnsConcatenated()
    {
        // Arrange
        var provider = new TestProvider();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "First message"),
            new(ChatRole.Assistant, "Assistant response"),
            new(ChatRole.User, "Second message")
        };

        // Act
        var result = provider.TestExtractTextFromMessages(messages);

        // Assert
        result.Should().Contain("First message");
        result.Should().Contain("Assistant response");
        result.Should().Contain("Second message");
    }

    [Fact]
    public void ExtractTextFromMessages_WithEmptyList_ReturnsEmptyString()
    {
        // Arrange
        var provider = new TestProvider();
        var messages = new List<ChatMessage>();

        // Act
        var result = provider.TestExtractTextFromMessages(messages);

        // Assert
        result.Should().BeEmpty();
    }
}
