using AiGeekSquad.ImageGenerator.Core.Adapters;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Providers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.AI;
using Moq;

namespace AiGeekSquad.ImageGenerator.Tests.Providers;

/// <summary>
/// Unit tests for OpenAIImageProvider using mocked adapters
/// </summary>
public class OpenAIImageProviderTests
{
    private readonly Mock<IOpenAIAdapter> _mockAdapter;
    private readonly OpenAIImageProvider _provider;

    public OpenAIImageProviderTests()
    {
        _mockAdapter = new Mock<IOpenAIAdapter>();
        _provider = new OpenAIImageProvider(_mockAdapter.Object, null);
    }

    [Fact]
    public void ProviderName_ShouldReturn_OpenAI()
    {
        // Act
        var name = _provider.ProviderName;

        // Assert
        name.Should().Be("OpenAI");
    }

    [Fact]
    public void GetCapabilities_ShouldReturnCorrectCapabilities()
    {
        // Act
        var capabilities = _provider.GetCapabilities();

        // Assert
        using var scope = new AssertionScope();
        capabilities.Should().NotBeNull();
        capabilities.ExampleModels.Should().Contain(new[]
        {
            ImageModels.OpenAI.DallE3,
            ImageModels.OpenAI.DallE2,
            ImageModels.OpenAI.GptImage1
        });
        capabilities.SupportedOperations.Should().Contain(new[]
        {
            ImageOperation.Generate,
            ImageOperation.Edit,
            ImageOperation.Variation
        });
        capabilities.SupportsMultiModalInput.Should().BeFalse();
        capabilities.AcceptsCustomModels.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateImageAsync_WithValidRequest_ShouldCallAdapter()
    {
        // Arrange
        var request = new ImageGenerationRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "A sunset over mountains")
            },
            Model = ImageModels.OpenAI.DallE3,
            Size = ImageModels.Sizes.Square1024,
            Quality = ImageModels.Quality.HD
        };

        _mockAdapter
            .Setup(x => x.GenerateImageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(("url", null));

        // Act
        var result = await _provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Images.Should().ContainSingle();
        result.Model.Should().Be(ImageModels.OpenAI.DallE3);
        result.ProviderName.Should().Be("OpenAI");

        _mockAdapter.Verify(x => x.GenerateImageAsync(
            "A sunset over mountains",
            ImageModels.OpenAI.DallE3,
            ImageModels.Sizes.Square1024,
            ImageModels.Quality.HD,
            It.IsAny<string>(),
            1,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateImageAsync_WithBase64Response_ShouldReturnBase64Data()
    {
        // Arrange
        var request = new ImageGenerationRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "A cat")
            },
            Model = ImageModels.OpenAI.DallE2
        };

        _mockAdapter
            .Setup(x => x.GenerateImageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, "base64encodeddata"));

        // Act
        var result = await _provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Images.Should().ContainSingle();
        result.Images[0].DataBase64.Should().Be("base64encodeddata");
    }

    [Fact]
    public async Task EditImageAsync_WithValidRequest_ShouldCallAdapter()
    {
        // Arrange
        var request = new ImageEditRequest
        {
            OriginalImage = "image.png",
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "Add a hat")
            },
            Model = ImageModels.OpenAI.DallE2
        };

        _mockAdapter
            .Setup(x => x.EditImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(("edited-url", null));

        // Act
        var result = await _provider.EditImageAsync(request, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Images.Should().ContainSingle();
        result.Images[0].Url.Should().Be("edited-url");
        result.ProviderName.Should().Be("OpenAI");

        _mockAdapter.Verify(x => x.EditImageAsync(
            It.IsAny<Stream>(),
            "Add a hat",
            It.IsAny<string>(),
            It.IsAny<string>(),
            1,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVariationAsync_WithValidRequest_ShouldCallAdapter()
    {
        // Arrange
        var request = new ImageVariationRequest
        {
            OriginalImage = "image.png",
            Model = ImageModels.OpenAI.DallE2,
            Count = 2
        };

        _mockAdapter
            .Setup(x => x.CreateVariationAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(("variation-url", null));

        // Act
        var result = await _provider.CreateVariationAsync(request, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Images.Should().ContainSingle();
        result.Images[0].Url.Should().Be("variation-url");

        _mockAdapter.Verify(x => x.CreateVariationAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            2,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateImageFromConversationAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new() { Role = "user", Text = "Generate an image" }
            },
            Model = ImageModels.OpenAI.DallE3
        };

        // Act
        Func<Task> act = async () => await _provider.GenerateImageFromConversationAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>();
    }
}
