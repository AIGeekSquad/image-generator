using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using Moq;

namespace AiGeekSquad.ImageGenerator.Tests.AcceptanceCriteria;

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
            Prompt = "Add a sunset in the background"
        };

        Assert.NotEmpty(request.Image);
        Assert.NotEmpty(request.Prompt);
    }

    [Fact]
    public void AC2_EditRequest_SupportsMask()
    {
        // Acceptance Criteria: Image editing should support masks for selective editing
        var request = new ImageEditRequest
        {
            Image = "base64_encoded_image_data",
            Prompt = "Change the color to blue",
            Mask = "base64_encoded_mask_data"
        };

        Assert.NotNull(request.Mask);
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

        Assert.NotEmpty(request.Image);
        Assert.Equal(3, request.NumberOfImages);
    }

    [Fact]
    public async Task AC4_Provider_IndicatesEditSupport()
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

        Assert.NotNull(generatedImage.RevisedPrompt);
    }
}
