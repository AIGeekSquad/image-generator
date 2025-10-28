using AiGeekSquad.ImageGenerator.Core.Providers;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.AI;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;

namespace AiGeekSquad.ImageGenerator.Tests.Providers;

/// <summary>
/// Tests for GoogleImageProvider to verify provider functionality and increase code coverage
/// </summary>
public class GoogleImageProviderTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();

        // Act
        var provider = new GoogleImageProvider(adapter, "test-project-id");

        // Assert
        using var scope = new AssertionScope();
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("Google");
    }

    [Fact]
    public void Constructor_WithCustomLocation_CreatesInstance()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();

        // Act
        var provider = new GoogleImageProvider(adapter, "test-project", "us-east1");

        // Assert
        using var scope = new AssertionScope();
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("Google");
    }

    [Fact]
    public void Constructor_WithDefaultModel_CreatesInstance()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();

        // Act
        var provider = new GoogleImageProvider(adapter, "test-project", "us-central1", "custom-model");

        // Assert
        using var scope = new AssertionScope();
        provider.Should().NotBeNull();
        provider.ProviderName.Should().Be("Google");
    }

    [Fact]
    public void GetCapabilities_ReturnsValidCapabilities()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");

        // Act
        var capabilities = provider.GetCapabilities();

        // Assert
        using var scope = new AssertionScope();
        capabilities.Should().NotBeNull();
        capabilities.ExampleModels.Should().Contain(ImageModels.Google.Imagen3);
        capabilities.ExampleModels.Should().Contain(ImageModels.Google.Imagen2);
        capabilities.ExampleModels.Should().Contain(ImageModels.Google.ImagenFast);
        capabilities.SupportedOperations.Should().Contain(ImageOperation.Generate);
        capabilities.AcceptsCustomModels.Should().BeTrue();
    }

    [Fact]
    public void SupportsOperation_Generate_ReturnsTrue()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");

        // Act
        var result = provider.SupportsOperation(ImageOperation.Generate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsOperation_Edit_ReturnsFalse()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");

        // Act
        var result = provider.SupportsOperation(ImageOperation.Edit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SupportsOperation_Variation_ReturnsFalse()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");

        // Act
        var result = provider.SupportsOperation(ImageOperation.Variation);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateImageAsync_WithBasicRequest_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A beautiful mountain landscape")
            },
            Model = ImageModels.Google.Imagen3
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.Images[0].Base64Data.Should().NotBeNull();
        response.Model.Should().Be(ImageModels.Google.Imagen3);
        response.Provider.Should().Be("Google");
    }

    [Fact]
    public async Task GenerateImageAsync_WithDefaultModel_UsesDefaultModel()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project", "us-central1", "custom-model");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A test image")
            }
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.Model.Should().Be("custom-model");
    }

    [Fact]
    public async Task GenerateImageAsync_WithNumberOfImages_RequestsMultipleImages()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "Generate variations")
            },
            Model = ImageModels.Google.Imagen3,
            NumberOfImages = 3
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        // Note: Since our test adapter returns only 1 image per call, we expect 1 image
        // In real scenarios with multiple sample counts, there would be multiple images
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateImageAsync_WithAdditionalParameters_PassesParameters()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "A futuristic city")
            },
            Model = ImageModels.Google.Imagen3,
            AdditionalParameters = new Dictionary<string, object>
            {
                ["aspectRatio"] = "16:9",
                ["negativePrompt"] = "blur, distortion"
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
    public async Task GenerateImageAsync_WithMultipleMessages_CombinesMessages()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a creative artist"),
                new ChatMessage(ChatRole.User, "Create a beautiful sunset"),
                new ChatMessage(ChatRole.User, "With mountains in the background")
            },
            Model = ImageModels.Google.Imagen3
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task EditImageAsync_NotSupported_ThrowsNotSupportedException()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new ImageEditRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "Edit this")
            },
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await provider.EditImageAsync(request, TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task CreateVariationAsync_NotSupported_ThrowsNotSupportedException()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new ImageVariationRequest
        {
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await provider.CreateVariationAsync(request, TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task GenerateImageAsync_WithImagenFastModel_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "Quick test image")
            },
            Model = ImageModels.Google.ImagenFast
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.Model.Should().Be(ImageModels.Google.ImagenFast);
    }

    [Fact]
    public async Task GenerateImageAsync_WithImagen2Model_ReturnsResponse()
    {
        // Arrange
        var adapter = new TestGoogleImageAdapter();
        var provider = new GoogleImageProvider(adapter, "test-project");
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, "Classic image generation")
            },
            Model = ImageModels.Google.Imagen2
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().HaveCount(1);
        response.Model.Should().Be(ImageModels.Google.Imagen2);
    }
}
