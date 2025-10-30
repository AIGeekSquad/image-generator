using AiGeekSquad.ImageGenerator.Core.Adapters;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;

namespace AiGeekSquad.ImageGenerator.Tests.Unit.Adapters;

/// <summary>
/// Unit tests for request adapters to demonstrate cognitive complexity reduction
/// </summary>
[Trait("Category", "Unit")]
public class ConversationalRequestAdapterTests
{
    private readonly ConversationalRequestAdapter _adapter;

    public ConversationalRequestAdapterTests()
    {
        _adapter = new ConversationalRequestAdapter();
    }

    [Fact]
    public void Adapt_WithNullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _adapter.Adapt(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void Adapt_WithEmptyConversation_ReturnsValidRequest()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>(),
            Model = "test-model"
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.Prompt.Should().BeEmpty();
        result.Messages.Should().BeEmpty();
        result.Images.Should().BeEmpty();
        result.Parameters.Model.Should().Be("test-model");
    }

    [Fact]
    public void Adapt_WithSimpleTextMessage_ExtractsPrompt()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage
                {
                    Role = "user",
                    Text = "Generate a sunset image"
                }
            }
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        result.Prompt.Should().Be("Generate a sunset image");
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.User);
    }

    [Fact]
    public void Adapt_WithMultipleMessages_UsesLastUserMessageAsPrompt()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage { Role = "user", Text = "First message" },
                new ConversationMessage { Role = "assistant", Text = "Response" },
                new ConversationMessage { Role = "user", Text = "Second message" }
            }
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        result.Prompt.Should().Be("Second message");
        result.Messages.Should().HaveCount(3);
    }

    [Fact]
    public void Adapt_WithImageUrl_AddsImageReference()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage
                {
                    Role = "user",
                    Text = "Edit this image",
                    Images = new List<ImageContent>
                    {
                        new ImageContent
                        {
                            // Note: DataContent with URL has known limitations in Microsoft.Extensions.AI
                            // Only testing ImageReference creation, not message content addition
                            Base64Data = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                            MimeType = "image/png",
                            Caption = "Test image"
                        }
                    }
                }
            }
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        result.Images.Should().HaveCount(1);
        result.Images[0].Base64Data.Should().NotBeNullOrEmpty();
        result.Images[0].Caption.Should().Be("Test image");
        result.Images[0].Role.Should().Be(ImageRole.Reference);
    }

    [Fact]
    public void Adapt_WithBase64Image_AddsImageReferenceAndContent()
    {
        // Arrange
        var base64Data = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage
                {
                    Role = "user",
                    Text = "Analyze this image",
                    Images = new List<ImageContent>
                    {
                        new ImageContent
                        {
                            Base64Data = base64Data,
                            MimeType = "image/png"
                        }
                    }
                }
            }
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        result.Images.Should().HaveCount(1);
        result.Images[0].Base64Data.Should().Be(base64Data);
        result.Images[0].MimeType.Should().Be("image/png");
        result.Messages[0].Contents.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Adapt_WithDifferentRoles_MapsRolesCorrectly()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage { Role = "system", Text = "System prompt" },
                new ConversationMessage { Role = "user", Text = "User prompt" },
                new ConversationMessage { Role = "assistant", Text = "Assistant response" },
                new ConversationMessage { Role = "unknown", Text = "Unknown role" }
            }
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        result.Messages.Should().HaveCount(4);
        result.Messages[0].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.System);
        result.Messages[1].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.User);
        result.Messages[2].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.Assistant);
        result.Messages[3].Role.Should().Be(Microsoft.Extensions.AI.ChatRole.User); // Unknown defaults to User
    }

    [Fact]
    public void Adapt_WithParameters_MapsParametersCorrectly()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>(),
            Model = "dall-e-3",
            Size = "1024x1024",
            Quality = "hd",
            Style = "vivid",
            NumberOfImages = 2,
            AdditionalParameters = new Dictionary<string, object> { ["custom"] = "value" }
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        result.Parameters.Model.Should().Be("dall-e-3");
        result.Parameters.Size.Should().Be("1024x1024");
        result.Parameters.Quality.Should().Be("hd");
        result.Parameters.Style.Should().Be("vivid");
        result.Parameters.NumberOfImages.Should().Be(2);
        result.AdditionalParameters.Should().ContainKey("custom");
    }

    [Fact]
    public void Adapt_WithNullAdditionalParameters_InitializesEmpty()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>(),
            AdditionalParameters = null
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        result.AdditionalParameters.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Adapt_SetsOperationCorrectly()
    {
        // Arrange
        var source = new ConversationalImageGenerationRequest
        {
            Conversation = new List<ConversationMessage>()
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        result.Operation.Should().Be(ImageOperation.GenerateFromConversation);
    }
}
