using AiGeekSquad.ImageGenerator.Core.Adapters;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Providers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.AI;
using Moq;

namespace AiGeekSquad.ImageGenerator.Tests.Providers;

/// <summary>
/// Unit tests for GoogleImageProvider using mocked adapters
/// </summary>
public class GoogleImageProviderTests
{
    private readonly Mock<IGoogleImageAdapter> _mockAdapter;
    private readonly GoogleImageProvider _provider;

    public GoogleImageProviderTests()
    {
        _mockAdapter = new Mock<IGoogleImageAdapter>();
        _provider = new GoogleImageProvider(_mockAdapter.Object, null);
    }

    [Fact]
    public void ProviderName_ShouldReturn_Google()
    {
        // Act
        var name = _provider.ProviderName;

        // Assert
        name.Should().Be("Google");
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
            ImageModels.Google.Imagen3,
            ImageModels.Google.Imagen2,
            ImageModels.Google.ImagenFast
        });
        capabilities.SupportedOperations.Should().Contain(ImageOperation.Generate);
        capabilities.SupportedOperations.Should().NotContain(ImageOperation.Edit);
        capabilities.SupportedOperations.Should().NotContain(ImageOperation.Variation);
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
                new(ChatRole.User, "A beautiful landscape")
            },
            Model = ImageModels.Google.Imagen3,
            Count = 1
        };

        var mockResponseBytes = new byte[] { 1, 2, 3, 4 };
        _mockAdapter
            .Setup(x => x.PredictAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<byte[]> { mockResponseBytes });

        // Act
        var result = await _provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Images.Should().ContainSingle();
        result.Images[0].DataBase64.Should().NotBeNullOrEmpty();
        result.Model.Should().Be(ImageModels.Google.Imagen3);
        result.ProviderName.Should().Be("Google");

        _mockAdapter.Verify(x => x.PredictAsync(
            ImageModels.Google.Imagen3,
            "A beautiful landscape",
            1,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateImageAsync_WithMultipleImages_ShouldReturnAll()
    {
        // Arrange
        var request = new ImageGenerationRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "A cat")
            },
            Model = ImageModels.Google.Imagen2,
            Count = 3
        };

        var mockResponseBytes1 = new byte[] { 1, 2, 3 };
        var mockResponseBytes2 = new byte[] { 4, 5, 6 };
        var mockResponseBytes3 = new byte[] { 7, 8, 9 };
        
        _mockAdapter
            .Setup(x => x.PredictAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<byte[]> { mockResponseBytes1, mockResponseBytes2, mockResponseBytes3 });

        // Act
        var result = await _provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Images.Should().HaveCount(3);
        result.Images.All(img => !string.IsNullOrEmpty(img.DataBase64)).Should().BeTrue();
    }

    [Fact]
    public async Task EditImageAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new ImageEditRequest
        {
            OriginalImage = "image.png",
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "Edit this")
            }
        };

        // Act
        Func<Task> act = async () => await _provider.EditImageAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task CreateVariationAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new ImageVariationRequest
        {
            OriginalImage = "image.png"
        };

        // Act
        Func<Task> act = async () => await _provider.CreateVariationAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>();
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
            }
        };

        // Act
        Func<Task> act = async () => await _provider.GenerateImageFromConversationAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>();
    }
}
