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
}
