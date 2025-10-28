using AiGeekSquad.ImageGenerator.Core.Providers;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.AI;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using CoreImageVariationRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageVariationRequest;

namespace AiGeekSquad.ImageGenerator.Tests.Providers;

/// <summary>
/// Tests for OpenAIImageProvider to verify provider functionality and increase code coverage
/// </summary>
public class OpenAIImageProviderTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var provider = new OpenAIImageProvider("test-api-key");

        // Assert
        using var scope = new AssertionScope();
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("OpenAI");
    }

    [Fact]
    public void Constructor_WithCustomAdapter_CreatesInstance()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();

        // Act
        var provider = new OpenAIImageProvider(adapter);

        // Assert
        using var scope = new AssertionScope();
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("OpenAI");
    }

    [Fact]
    public void Constructor_WithAzureEndpoint_CreatesInstance()
    {
        // Arrange & Act
        var provider = new OpenAIImageProvider(
            "test-api-key",
            "https://test.openai.azure.com",
            "test-deployment");

        // Assert
        using var scope = new AssertionScope();
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("OpenAI");
    }

    [Fact]
    public void GetCapabilities_ReturnsValidCapabilities()
    {
        // Arrange
        var provider = new OpenAIImageProvider("test-api-key");

        // Act
        var capabilities = provider.GetCapabilities();

        // Assert
        using var scope = new AssertionScope();
        capabilities.Should().NotBeNull();
        capabilities.ExampleModels.Should().Contain(ImageModels.OpenAI.DallE3);
        capabilities.ExampleModels.Should().Contain(ImageModels.OpenAI.DallE2);
        capabilities.ExampleModels.Should().Contain(ImageModels.OpenAI.GPTImage1);
        capabilities.SupportedOperations.Should().Contain(ImageOperation.Generate);
        capabilities.SupportedOperations.Should().Contain(ImageOperation.Edit);
        capabilities.SupportedOperations.Should().Contain(ImageOperation.Variation);
        capabilities.DefaultModel.Should().Be(ImageModels.OpenAI.DallE3);
        capabilities.AcceptsCustomModels.Should().BeTrue();
    }

    [Fact]
    public void SupportsOperation_Generate_ReturnsTrue()
    {
        // Arrange
        var provider = new OpenAIImageProvider("test-api-key");

        // Act
        var result = provider.SupportsOperation(ImageOperation.Generate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsOperation_Edit_ReturnsTrue()
    {
        // Arrange
        var provider = new OpenAIImageProvider("test-api-key");

        // Act
        var result = provider.SupportsOperation(ImageOperation.Edit);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsOperation_Variation_ReturnsTrue()
    {
        // Arrange
        var provider = new OpenAIImageProvider("test-api-key");

        // Act
        var result = provider.SupportsOperation(ImageOperation.Variation);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateImageAsync_WithBasicRequest_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A beautiful sunset")
            },
            Model = ImageModels.OpenAI.DallE3
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.Images[0].Url.Should().NotBeNull();
        response.Model.Should().Be(ImageModels.OpenAI.DallE3);
        response.Provider.Should().Be("OpenAI");
    }

    [Fact]
    public async Task GenerateImageAsync_WithSize_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A cat")
            },
            Size = ImageModels.Sizes.Square1024
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateImageAsync_WithQuality_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A dog")
            },
            Quality = ImageModels.Quality.HD
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateImageAsync_WithStyle_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A landscape")
            },
            Style = ImageModels.Style.Vivid
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task EditImageAsync_WithValidRequest_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageEditRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "Add a hat")
            },
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
        };

        // Act
        var response = await provider.EditImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.Images[0].Url.Should().NotBeNull();
        response.Provider.Should().Be("OpenAI");
    }

    [Fact]
    public async Task CreateVariationAsync_WithValidRequest_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageVariationRequest
        {
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
        };

        // Act
        var response = await provider.CreateVariationAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.Images[0].Url.Should().NotBeNull();
        response.Provider.Should().Be("OpenAI");
    }

    [Fact]
    public async Task GenerateImageAsync_WithDefaultDeployment_UsesDefaultModel()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter, "custom-deployment");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A test")
            }
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateImageAsync_WithMultipleContentParts_ExtractsText()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var message = new ChatMessage(ChatRole.User, "Part 1");
        message.Contents.Add(new TextContent("Part 2"));
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage> { message }
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task EditImageAsync_WithSize_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageEditRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "Edit this")
            },
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=",
            Size = ImageModels.Sizes.Square512
        };

        // Act
        var response = await provider.EditImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateVariationAsync_WithSize_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestOpenAIAdapter();
        var provider = new OpenAIImageProvider(adapter);
        var request = new CoreImageVariationRequest
        {
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=",
            Size = ImageModels.Sizes.Wide1792x1024
        };

        // Act
        var response = await provider.CreateVariationAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }
}
