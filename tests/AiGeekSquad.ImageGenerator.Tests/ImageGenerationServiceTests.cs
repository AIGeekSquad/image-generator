using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Services;
using Microsoft.Extensions.AI;
using Moq;
using FluentAssertions;
using FluentAssertions.Execution;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;

namespace AiGeekSquad.ImageGenerator.Tests;

public class ImageGenerationServiceTests
{
    [Fact]
    public void GetProviders_ReturnsAllRegisteredProviders()
    {
        // Arrange
        var provider1 = new Mock<IImageGenerationProvider>();
        provider1.Setup(p => p.ProviderName).Returns("Provider1");
        
        var provider2 = new Mock<IImageGenerationProvider>();
        provider2.Setup(p => p.ProviderName).Returns("Provider2");
        
        var providers = new List<IImageGenerationProvider> { provider1.Object, provider2.Object };
        var service = new ImageGenerationService(providers);

        // Act
        var result = service.GetProviders();

        // Assert
        using var scope = new AssertionScope();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.ProviderName == "Provider1");
        result.Should().Contain(p => p.ProviderName == "Provider2");
    }

    [Fact]
    public void GetProvider_WithValidName_ReturnsProvider()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);

        // Act
        var result = service.GetProvider("TestProvider");

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result!.ProviderName.Should().Be("TestProvider");
    }

    [Fact]
    public void GetProvider_WithInvalidName_ReturnsNull()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);

        // Act
        var result = service.GetProvider("NonExistentProvider");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GenerateImageAsync_WithValidProvider_CallsProviderGenerateAsync()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.Generate)).Returns(true);
        provider.Setup(p => p.GenerateImageAsync(It.IsAny<CoreImageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreImageResponse
            {
                Images = new List<GeneratedImage>(),
                Model = "test-model",
                Provider = "TestProvider"
            });
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new CoreImageRequest 
        { 
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") }
        };

        // Act
        var result = await service.GenerateImageAsync("TestProvider", request);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Provider.Should().Be("TestProvider");
        provider.Verify(p => p.GenerateImageAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateImageAsync_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var service = new ImageGenerationService(new List<IImageGenerationProvider>());
        var request = new CoreImageRequest 
        { 
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") }
        };

        // Act & Assert
        var act = async () => await service.GenerateImageAsync("NonExistentProvider", request);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateImageAsync_WithUnsupportedOperation_ThrowsException()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.Generate)).Returns(false);
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new CoreImageRequest 
        { 
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") }
        };

        // Act & Assert
        var act = async () => await service.GenerateImageAsync("TestProvider", request);
        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task EditImageAsync_WithValidProvider_CallsProviderEditAsync()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.Edit)).Returns(true);
        provider.Setup(p => p.EditImageAsync(It.IsAny<ImageEditRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreImageResponse
            {
                Images = new List<GeneratedImage> { new() { Url = "https://example.com/edited.png" } },
                Model = "dall-e-2",
                Provider = "TestProvider"
            });
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new ImageEditRequest
        { 
            Messages = new List<ChatMessage> { new(ChatRole.User, "Edit this image") },
            Image = "https://example.com/image.png"
        };

        // Act
        var result = await service.EditImageAsync("TestProvider", request);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Provider.Should().Be("TestProvider");
        result.Images.Should().HaveCount(1);
        provider.Verify(p => p.EditImageAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditImageAsync_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var service = new ImageGenerationService(new List<IImageGenerationProvider>());
        var request = new ImageEditRequest
        { 
            Messages = new List<ChatMessage> { new(ChatRole.User, "Edit") },
            Image = "https://example.com/image.png"
        };

        // Act & Assert
        var act = async () => await service.EditImageAsync("NonExistentProvider", request);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EditImageAsync_WithUnsupportedOperation_ThrowsException()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.Edit)).Returns(false);
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new ImageEditRequest
        { 
            Messages = new List<ChatMessage> { new(ChatRole.User, "Edit") },
            Image = "https://example.com/image.png"
        };

        // Act & Assert
        var act = async () => await service.EditImageAsync("TestProvider", request);
        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task CreateVariationAsync_WithValidProvider_CallsProviderCreateVariationAsync()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.Variation)).Returns(true);
        provider.Setup(p => p.CreateVariationAsync(It.IsAny<ImageVariationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreImageResponse
            {
                Images = new List<GeneratedImage> { new() { Url = "https://example.com/variation.png" } },
                Model = "dall-e-2",
                Provider = "TestProvider"
            });
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new ImageVariationRequest
        { 
            Image = "https://example.com/image.png"
        };

        // Act
        var result = await service.CreateVariationAsync("TestProvider", request);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Provider.Should().Be("TestProvider");
        result.Images.Should().HaveCount(1);
        provider.Verify(p => p.CreateVariationAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVariationAsync_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var service = new ImageGenerationService(new List<IImageGenerationProvider>());
        var request = new ImageVariationRequest
        { 
            Image = "https://example.com/image.png"
        };

        // Act & Assert
        var act = async () => await service.CreateVariationAsync("NonExistentProvider", request);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateVariationAsync_WithUnsupportedOperation_ThrowsException()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.Variation)).Returns(false);
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new ImageVariationRequest
        { 
            Image = "https://example.com/image.png"
        };

        // Act & Assert
        var act = async () => await service.CreateVariationAsync("TestProvider", request);
        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task GenerateImageFromConversationAsync_WithValidProvider_CallsProvider()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.GenerateFromConversation)).Returns(true);
        provider.Setup(p => p.GenerateImageFromConversationAsync(It.IsAny<ConversationalImageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreImageResponse
            {
                Images = new List<GeneratedImage> { new() { Url = "https://example.com/image.png" } },
                Model = "test-model",
                Provider = "TestProvider"
            });
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new ConversationalImageGenerationRequest
        { 
            Conversation = new List<ConversationMessage>
            {
                new() { Role = "user", Text = "Create an image" }
            }
        };

        // Act
        var result = await service.GenerateImageFromConversationAsync("TestProvider", request);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Provider.Should().Be("TestProvider");
        provider.Verify(p => p.GenerateImageFromConversationAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}
