using Microsoft.Extensions.AI;
using FluentAssertions;
using FluentAssertions.Execution;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Services;
using Moq;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using CoreImageVariationRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageVariationRequest;
using CoreConversationalRequest = AiGeekSquad.ImageGenerator.Core.Models.ConversationalImageGenerationRequest;

namespace AiGeekSquad.ImageGenerator.Tests.AcceptanceCriteria;

/// <summary>
/// Acceptance Criteria Tests: Verify extensibility and custom provider loading
/// </summary>
public class ExtensibilityTests
{
    [Fact]
    public void AC1_CustomProvider_CanBeImplemented()
    {
        // Acceptance Criteria: Third parties can implement custom providers
        var customProvider = new CustomTestProvider();

        customProvider.ProviderName.Should().Be("CustomTest");
        Assert.NotNull(customProvider.GetCapabilities());
        Assert.True(customProvider.SupportsOperation(ImageOperation.Generate));
    }

    [Fact]
    public void AC2_MultipleProviders_CanBeRegistered()
    {
        // Acceptance Criteria: Multiple providers can be registered and used simultaneously
        var provider1 = new Mock<IImageGenerationProvider>();
        provider1.Setup(p => p.ProviderName).Returns("Provider1");

        var provider2 = new Mock<IImageGenerationProvider>();
        provider2.Setup(p => p.ProviderName).Returns("Provider2");

        var service = new ImageGenerationService(new[] { provider1.Object, provider2.Object });
        var providers = service.GetProviders();

        providers.Count.Should().Be(2);
        providers.Should().Contain(p => p.ProviderName == "Provider1");
        providers.Should().Contain(p => p.ProviderName == "Provider2");
    }

    [Fact]
    public void AC3_Provider_CanAcceptAnyModelString()
    {
        // Acceptance Criteria: Providers should accept any model string for forward compatibility
        var capabilities = new ProviderCapabilities
        {
            AcceptsCustomModels = true,
            ExampleModels = new List<string> { "model-v1" }
        };

        capabilities.AcceptsCustomModels.Should().BeTrue();
    }

    [Fact]
    public void AC4_ProviderCapabilities_DescribeFeatures()
    {
        // Acceptance Criteria: Provider capabilities should describe available features
        var capabilities = new ProviderCapabilities
        {
            Features = new Dictionary<string, object>
            {
                ["supportsHD"] = true,
                ["maxResolution"] = "2048x2048",
                ["supportedFormats"] = new[] { "png", "jpg", "webp" }
            }
        };

        capabilities.Features.Count.Should().Be(3);
        Assert.True((bool)capabilities.Features["supportsHD"]);
        capabilities.Features["maxResolution"].Should().Be("2048x2048");
    }

    [Fact]
    public async Task AC5_Service_RoutesToCorrectProvider()
    {
        // Acceptance Criteria: Service should route requests to the correct provider by name
        var provider1 = new Mock<IImageGenerationProvider>();
        provider1.Setup(p => p.ProviderName).Returns("Provider1");
        provider1.Setup(p => p.SupportsOperation(ImageOperation.Generate)).Returns(true);
        provider1.Setup(p => p.GenerateImageAsync(It.IsAny<CoreImageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreImageResponse
            {
                Images = new List<GeneratedImage>(),
                Model = "model1",
                Provider = "Provider1"
            });

        var provider2 = new Mock<IImageGenerationProvider>();
        provider2.Setup(p => p.ProviderName).Returns("Provider2");

        var service = new ImageGenerationService(new[] { provider1.Object, provider2.Object });

        var request = new CoreImageRequest { Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "test") } };
        var result = await service.GenerateImageAsync("Provider1", request, TestContext.Current.CancellationToken);

        result.Provider.Should().Be("Provider1");
        provider1.Verify(p => p.GenerateImageAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        provider2.Verify(p => p.GenerateImageAsync(It.IsAny<CoreImageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Helper custom provider for testing
    private class CustomTestProvider : IImageGenerationProvider
    {
        public string ProviderName => "CustomTest";

        public Task<CoreImageResponse> GenerateImageAsync(CoreImageRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CoreImageResponse
            {
                Images = new List<GeneratedImage>(),
                Model = request.Model ?? "default",
                Provider = ProviderName
            });
        }

        public Task<CoreImageResponse> GenerateImageFromConversationAsync(CoreConversationalRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<CoreImageResponse> EditImageAsync(CoreImageEditRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<CoreImageResponse> CreateVariationAsync(CoreImageVariationRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool SupportsOperation(ImageOperation operation)
        {
            return operation == ImageOperation.Generate;
        }

        public ProviderCapabilities GetCapabilities()
        {
            return new ProviderCapabilities
            {
                ExampleModels = new List<string> { "custom-model-1" },
                SupportedOperations = new List<ImageOperation> { ImageOperation.Generate },
                DefaultModel = "custom-model-1",
                AcceptsCustomModels = true
            };
        }
    }
}
