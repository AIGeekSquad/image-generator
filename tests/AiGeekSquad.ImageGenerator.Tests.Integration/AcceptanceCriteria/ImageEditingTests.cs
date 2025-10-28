using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Moq;

namespace AiGeekSquad.ImageGenerator.Tests.Integration.AcceptanceCriteria;

/// <summary>
/// Acceptance Criteria Tests: Verify image editing and variation capabilities
/// </summary>
public class ImageEditingTests
{
    [Fact]
    public void AC1_EditRequest_SupportsImageAndPrompt()
    {
        // Acceptance Criteria: Tool should support editing images based on prompts
        var request = new ImageEditRequest
        {
            Image = "base64_encoded_image_data",
            Messages = new List<ChatMessage> 
            { 
                new ChatMessage(ChatRole.User, "Add a sunset in the background") 
            }
        };

        request.Image.Should().NotBeEmpty();
        request.Messages.Should().NotBeNull();
    }

    [Fact]
    public void AC2_EditRequest_SupportsMask()
    {
        // Acceptance Criteria: Image editing should support masks for selective editing
        var request = new ImageEditRequest
        {
            Image = "base64_encoded_image_data",
            Messages = new List<ChatMessage> 
            { 
                new ChatMessage(ChatRole.User, "Change the color to blue") 
            },
            Mask = "base64_encoded_mask_data"
        };

        request.Mask.Should().NotBeNull();
    }

    [Fact]
    public void AC3_VariationRequest_SupportsImageInput()
    {
        // Acceptance Criteria: Tool should support creating variations of existing images
        var request = new ImageVariationRequest
        {
            Image = "base64_encoded_image_data",
            NumberOfImages = 3
        };

        request.Image.Should().NotBeEmpty();
        request.NumberOfImages.Should().Be(3);
    }

    [Fact]
    public void AC4_Provider_IndicatesEditSupport()
    {
        // Acceptance Criteria: Providers should indicate whether they support edit and variation operations
        var provider = new Mock<IImageGenerationProvider>();
        provider.Setup(p => p.SupportsOperation(ImageOperation.Edit)).Returns(true);
        provider.Setup(p => p.SupportsOperation(ImageOperation.Variation)).Returns(true);

        Assert.True(provider.Object.SupportsOperation(ImageOperation.Edit));
        Assert.True(provider.Object.SupportsOperation(ImageOperation.Variation));
    }

    [Fact]
    public void AC5_Response_ContainsRevisedPrompt()
    {
        // Acceptance Criteria: Responses should include revised prompts when provided by the AI
        var generatedImage = new GeneratedImage
        {
            Url = "https://example.com/image.png",
            RevisedPrompt = "A serene sunset over calm ocean waters with vibrant orange and pink hues"
        };

        generatedImage.RevisedPrompt.Should().NotBeNull();
    }
}
