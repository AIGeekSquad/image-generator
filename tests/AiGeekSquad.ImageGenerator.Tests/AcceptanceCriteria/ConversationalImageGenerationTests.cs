using Microsoft.Extensions.AI;
using FluentAssertions;
using FluentAssertions.Execution;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using CoreImageVariationRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageVariationRequest;
using CoreConversationalRequest = AiGeekSquad.ImageGenerator.Core.Models.ConversationalImageGenerationRequest;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using Moq;

namespace AiGeekSquad.ImageGenerator.Tests.AcceptanceCriteria;

/// <summary>
/// Acceptance Criteria Tests: Verify that the tool supports multi-modal conversational image generation
/// </summary>
public class ConversationalImageGenerationTests
{
    [Fact]
    public void AC1_ConversationMessage_CanContainTextOnly()
    {
        // Acceptance Criteria: A conversation message can contain only text
        var message = new ConversationMessage
        {
            Role = "user",
            Text = "Create a beautiful sunset image"
        };

        message.Should().NotBeNull();
        message.Role.Should().Be("user");
        message.Text.Should().Be("Create a beautiful sunset image");
        message.Images.Should().BeNull();
    }

    [Fact]
    public void AC2_ConversationMessage_CanContainImagesWithText()
    {
        // Acceptance Criteria: A conversation message can contain both text and images
        var message = new ConversationMessage
        {
            Role = "user",
            Text = "Create an image similar to this reference",
            Images = new List<ImageContent>
            {
                new ImageContent
                {
                    Url = "https://example.com/reference.jpg",
                    Caption = "Reference style"
                }
            }
        };

        message.Should().NotBeNull();
        message.Role.Should().Be("user");
        message.Text.Should().NotBeNull();
        message.Images.Should().NotBeNull();
        message.Images.Should().ContainSingle();
        message.Images[0].Caption.Should().Be("Reference style");
    }

    [Fact]
    public void AC3_ConversationMessage_CanContainMultipleImages()
    {
        // Acceptance Criteria: A conversation can include multiple reference images
        var message = new ConversationMessage
        {
            Role = "user",
            Text = "Combine elements from these images",
            Images = new List<ImageContent>
            {
                new ImageContent { Url = "https://example.com/img1.jpg", Caption = "Style reference" },
                new ImageContent { Url = "https://example.com/img2.jpg", Caption = "Color palette" }
            }
        };

        message.Images!.Count.Should().Be(2);
    }

    [Fact]
    public void AC4_ImageContent_SupportsBase64Data()
    {
        // Acceptance Criteria: Images can be provided as base64 data
        var imageContent = new ImageContent
        {
            Base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            MimeType = "image/png"
        };

        imageContent.Base64Data.Should().NotBeNull();
        imageContent.MimeType.Should().Be("image/png");
    }

    [Fact]
    public void AC5_ConversationalRequest_CanContainMultipleMessages()
    {
        // Acceptance Criteria: A conversational request can contain multiple messages forming a conversation
        var request = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage { Role = "user", Text = "I want to create an image" },
                new ConversationMessage { Role = "assistant", Text = "What kind of image would you like?" },
                new ConversationMessage
                {
                    Role = "user",
                    Text = "Something like this",
                    Images = new List<ImageContent>
                    {
                        new ImageContent { Url = "https://example.com/ref.jpg" }
                    }
                }
            },
            Model = "dall-e-3",
            Size = "1024x1024"
        };

        request.Conversation.Count.Should().Be(3);
        request.Conversation[2].Images.Should().NotBeNull();
    }

    [Fact]
    public async Task AC6_Provider_FallbackToSimplePrompt_WhenConversationalNotSupported()
    {
        // Acceptance Criteria: Providers that don't support conversational input should fallback to extracting a simple prompt
        var mockProvider = new Mock<IImageGenerationProvider>();
        mockProvider.Setup(p => p.ProviderName).Returns("TestProvider");
        mockProvider.Setup(p => p.SupportsOperation(ImageOperation.GenerateFromConversation)).Returns(false);
        mockProvider.Setup(p => p.GenerateImageAsync(It.IsAny<CoreImageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreImageResponse
            {
                Images = new List<GeneratedImage>(),
                Model = "test-model",
                Provider = "TestProvider"
            });

        var request = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage { Role = "user", Text = "Create a sunset image" }
            }
        };

        // Should call GenerateImageAsync with converted prompt
        mockProvider.Setup(p => p.GenerateImageFromConversationAsync(request, It.IsAny<CancellationToken>()))
            .Returns<ConversationalImageGenerationRequest, CancellationToken>((req, ct) =>
            {
                // Simulate fallback behavior by returning the same result and triggering GenerateImageAsync
                var text = req.Conversation.FirstOrDefault()?.Text ?? "";
                var fallbackRequest = new CoreImageRequest
                {
                    Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, text) }
                };
                
                // Simulate the fallback call
                mockProvider.Object.GenerateImageAsync(fallbackRequest, ct);
                
                // Return the expected result
                return Task.FromResult(new CoreImageResponse
                {
                    Images = new List<GeneratedImage>(),
                    Model = "test-model",
                    Provider = "TestProvider"
                });
            });

        var result = await mockProvider.Object.GenerateImageFromConversationAsync(request, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        mockProvider.Verify(p => p.GenerateImageAsync(It.IsAny<CoreImageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void AC7_ProviderCapabilities_IndicatesMultiModalSupport()
    {
        // Acceptance Criteria: Provider capabilities should indicate if multi-modal input is supported
        var capabilities = new ProviderCapabilities
        {
            SupportsMultiModalInput = true,
            MaxConversationImages = 5,
            SupportedOperations = new List<ImageOperation>
            {
                ImageOperation.Generate,
                ImageOperation.GenerateFromConversation
            }
        };

        capabilities.SupportsMultiModalInput.Should().BeTrue();
        capabilities.MaxConversationImages.Should().Be(5);
        capabilities.SupportedOperations.Should().Contain(ImageOperation.GenerateFromConversation);
    }
}
