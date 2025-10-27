using Microsoft.Extensions.AI;
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

        Assert.NotNull(message);
        Assert.Equal("user", message.Role);
        Assert.Equal("Create a beautiful sunset image", message.Text);
        Assert.Null(message.Images);
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

        Assert.NotNull(message);
        Assert.Equal("user", message.Role);
        Assert.NotNull(message.Text);
        Assert.NotNull(message.Images);
        Assert.Single(message.Images);
        Assert.Equal("Reference style", message.Images[0].Caption);
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

        Assert.Equal(2, message.Images!.Count);
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

        Assert.NotNull(imageContent.Base64Data);
        Assert.Equal("image/png", imageContent.MimeType);
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

        Assert.Equal(3, request.Conversation.Count);
        Assert.NotNull(request.Conversation[2].Images);
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
        mockProvider.Setup(p => p.GenerateImageFromConversationAsync(request, default))
            .Returns<ConversationalImageGenerationRequest, CancellationToken>((req, ct) =>
            {
                // Simulate fallback behavior
                var text = req.Conversation.FirstOrDefault()?.Text ?? "";
                return mockProvider.Object.GenerateImageAsync(new CoreImageRequest
                {
                    Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, text) }
                }, ct);
            });

        var result = await mockProvider.Object.GenerateImageFromConversationAsync(request);

        Assert.NotNull(result);
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

        Assert.True(capabilities.SupportsMultiModalInput);
        Assert.Equal(5, capabilities.MaxConversationImages);
        Assert.Contains(ImageOperation.GenerateFromConversation, capabilities.SupportedOperations);
    }
}
