using AiGeekSquad.ImageGenerator.Core.Adapters;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;

namespace AiGeekSquad.ImageGenerator.Tests.Unit.Adapters;

[Trait("Category", "Unit")]
public class ImageGenerationRequestAdapterTests
{
    private readonly ImageGenerationRequestAdapter _adapter;

    public ImageGenerationRequestAdapterTests()
    {
        _adapter = new ImageGenerationRequestAdapter();
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
    public void Adapt_WithSimpleRequest_MapsCorrectly()
    {
        // Arrange
        var source = new ImageGenerationRequest
        {
            Messages = new List<Microsoft.Extensions.AI.ChatMessage>
            {
                new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.User, "Test prompt")
            },
            Model = "test-model",
            Size = "512x512"
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        using var scope = new AssertionScope();
        // ChatMessage constructor adds text to Contents, so it appears twice
        result.Prompt.Should().Contain("Test prompt");
        result.Messages.Should().HaveCount(1);
        result.Parameters.Model.Should().Be("test-model");
        result.Parameters.Size.Should().Be("512x512");
        result.Operation.Should().Be(ImageOperation.Generate);
    }

    [Fact]
    public void Adapt_WithTextContent_ExtractsText()
    {
        // Arrange
        var message = new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.User, "");
        message.Contents.Add(new Microsoft.Extensions.AI.TextContent("Content from TextContent"));
        var source = new ImageGenerationRequest
        {
            Messages = new List<Microsoft.Extensions.AI.ChatMessage> { message }
        };

        // Act
        var result = _adapter.Adapt(source);

        // Assert
        result.Prompt.Should().Contain("Content from TextContent");
    }
}
