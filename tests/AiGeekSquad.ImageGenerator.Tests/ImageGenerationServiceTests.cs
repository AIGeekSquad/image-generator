using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Services;
using Moq;

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
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.ProviderName == "Provider1");
        Assert.Contains(result, p => p.ProviderName == "Provider2");
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
        Assert.NotNull(result);
        Assert.Equal("TestProvider", result.ProviderName);
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
        Assert.Null(result);
    }

    [Fact]
    public async Task GenerateImageAsync_WithValidProvider_CallsProviderGenerateAsync()
    {
        // Arrange
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SupportsOperation(ImageOperation.Generate)).Returns(true);
        provider.Setup(p => p.GenerateImageAsync(It.IsAny<ImageGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageGenerationResponse
            {
                Images = new List<GeneratedImage>(),
                Model = "test-model",
                Provider = "TestProvider"
            });
        
        var providers = new List<IImageGenerationProvider> { provider.Object };
        var service = new ImageGenerationService(providers);
        var request = new ImageGenerationRequest { Prompt = "test" };

        // Act
        var result = await service.GenerateImageAsync("TestProvider", request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestProvider", result.Provider);
        provider.Verify(p => p.GenerateImageAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateImageAsync_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        var service = new ImageGenerationService(new List<IImageGenerationProvider>());
        var request = new ImageGenerationRequest { Prompt = "test" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateImageAsync("NonExistentProvider", request));
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
        var request = new ImageGenerationRequest { Prompt = "test" };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => service.GenerateImageAsync("TestProvider", request));
    }
}
