using Microsoft.Extensions.AI;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using CoreImageVariationRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageVariationRequest;
using FluentAssertions;
using FluentAssertions.Execution;
using AiGeekSquad.ImageGenerator.Core.Providers;
using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Tests.AcceptanceCriteria;

/// <summary>
/// End-to-End Integration Tests with actual API calls
/// Tests are dynamically skipped when API keys are not available using xUnit v2.9
/// Note: These tests require actual API keys - set environment variables OPENAI_API_KEY or GOOGLE_PROJECT_ID
/// </summary>
public class EndToEndIntegrationTests
{
    private static string? OpenAIApiKey => Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    private static string? GoogleProjectId => Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");
    private static string GoogleLocation => Environment.GetEnvironmentVariable("GOOGLE_LOCATION") ?? "us-central1";

    [Fact]
    public async Task E2E_OpenAI_GenerateImage_WithDallE3()
    {
        // Arrange
        if (string.IsNullOrEmpty(OpenAIApiKey))
        {
            // Write informative message and return early instead of throwing
            // This is the xUnit v2 pattern for conditional tests
            return; // Test passes but does nothing
        }

        var provider = new OpenAIImageProvider(OpenAIApiKey);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "A serene mountain landscape at sunset with purple and orange skies")
            },
            Model = ImageModels.OpenAI.DallE3,
            Size = "1024x1024"
        };

        // Act
        var response = await provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images[0].Url.Should().NotBeNullOrEmpty();
            response.Model.Should().Be(ImageModels.OpenAI.DallE3);
            response.Provider.Should().Be("OpenAI");
        }
    }

    [Fact]
    public async Task E2E_OpenAI_GenerateImage_WithDallE2()
    {
        // Arrange
        if (string.IsNullOrEmpty(OpenAIApiKey))
        {
            return; // Skip test
        }

        var provider = new OpenAIImageProvider(OpenAIApiKey);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "A cute cat playing with a ball of yarn")
            },
            Model = ImageModels.OpenAI.DallE2,
            Size = "512x512"
        };

        // Act
        var response = await provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images[0].Url.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task E2E_Google_GenerateImage_WithImagen3()
    {
        // Arrange
        if (string.IsNullOrEmpty(GoogleProjectId))
        {
            return; // Skip test
        }

        var provider = new GoogleImageProvider(GoogleProjectId, GoogleLocation);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "A futuristic city with flying cars and neon lights")
            },
            Model = ImageModels.Google.Imagen3,
            Size = "1024x1024"
        };

        // Act
        var response = await provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images[0].Base64Data.Should().NotBeNullOrEmpty();
            
            // Verify base64 can be decoded
            var imageBytes = Convert.FromBase64String(response.Images[0].Base64Data!);
            imageBytes.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task E2E_OpenAI_EditImage_WithDallE2()
    {
        // Arrange
        if (string.IsNullOrEmpty(OpenAIApiKey))
        {
            return; // Skip test
        }

        var provider = new OpenAIImageProvider(OpenAIApiKey);
        
        // Create a simple test image (1x1 PNG with transparency) and convert to base64
        byte[] testImageBytes = CreateSimpleTestImage();
        string testImageBase64 = Convert.ToBase64String(testImageBytes);
        
        var editRequest = new CoreImageEditRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "Add a red circle in the center")
            },
            Image = testImageBase64,
            Model = ImageModels.OpenAI.DallE2,
            Size = "512x512"
        };

        // Act
        var response = await provider.EditImageAsync(editRequest, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
            response.Images[0].Url.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task E2E_OpenAI_CreateVariation_WithDallE2()
    {
        // Arrange
        if (string.IsNullOrEmpty(OpenAIApiKey))
        {
            return; // Skip test
        }

        var provider = new OpenAIImageProvider(OpenAIApiKey);
        
        // Create a simple test image and convert to base64
        byte[] testImageBytes = CreateSimpleTestImage();
        string testImageBase64 = Convert.ToBase64String(testImageBytes);
        
        var variationRequest = new CoreImageVariationRequest
        {
            Image = testImageBase64,
            Model = ImageModels.OpenAI.DallE2,
            NumberOfImages = 2,
            Size = "512x512"
        };

        // Act
        var response = await provider.CreateVariationAsync(variationRequest, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Images.Should().HaveCount(2);
            response.Images.Should().OnlyContain(img => !string.IsNullOrEmpty(img.Url));
        }
    }

    [Fact]
    public async Task E2E_GenerateImage_FromConversation_WithMultipleMessages()
    {
        // Arrange
        if (string.IsNullOrEmpty(OpenAIApiKey))
        {
            return; // Skip test
        }

        var provider = new OpenAIImageProvider(OpenAIApiKey);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "Create an image of a robot"),
                new(ChatRole.Assistant, "What style would you like?"),
                new(ChatRole.User, "Make it in a cyberpunk style with neon colors")
            },
            Model = ImageModels.OpenAI.DallE3,
            Size = "1024x1024"
        };

        // Act
        var response = await provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task E2E_GenerateImage_WithMultiModalInput()
    {
        // Arrange
        if (string.IsNullOrEmpty(OpenAIApiKey))
        {
            return; // Skip test
        }

        var provider = new OpenAIImageProvider(OpenAIApiKey);
        
        // Create a simple reference image
        byte[] referenceImage = CreateSimpleTestImage();
        
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, new AIContent[]
                {
                    new TextContent("Create an image in a similar style to this reference"),
                    new DataContent(referenceImage, "image/png")
                })
            },
            Model = ImageModels.OpenAI.DallE3,
            Size = "1024x1024"
        };

        // Act
        var response = await provider.GenerateImageAsync(request, CancellationToken.None);

        // Assert - Provider extracts text even if it doesn't support multi-modal
        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.Images.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task E2E_MultipleProviders_SameRequest()
    {
        // Arrange
        var hasOpenAI = !string.IsNullOrEmpty(OpenAIApiKey);
        var hasGoogle = !string.IsNullOrEmpty(GoogleProjectId);
        
        if (!hasOpenAI && !hasGoogle)
        {
            return; // Skip test
        }

        var prompt = "A beautiful sunset over the ocean";
        var results = new List<CoreImageResponse>();

        // Act - Test with available providers
        if (hasOpenAI)
        {
            var openAiProvider = new OpenAIImageProvider(OpenAIApiKey!);
            var openAiRequest = new CoreImageRequest
            {
                Messages = new List<ChatMessage> { new(ChatRole.User, prompt) },
                Model = ImageModels.OpenAI.DallE3
            };
            results.Add(await openAiProvider.GenerateImageAsync(openAiRequest, CancellationToken.None));
        }

        if (hasGoogle)
        {
            var googleProvider = new GoogleImageProvider(GoogleProjectId!, GoogleLocation);
            var googleRequest = new CoreImageRequest
            {
                Messages = new List<ChatMessage> { new(ChatRole.User, prompt) },
                Model = ImageModels.Google.Imagen3
            };
            results.Add(await googleProvider.GenerateImageAsync(googleRequest, CancellationToken.None));
        }

        // Assert - All providers should successfully generate images
        using (new AssertionScope())
        {
            results.Should().NotBeEmpty();
            results.Should().OnlyContain(r => r.Images.Count > 0);
        }
    }

    /// <summary>
    /// Creates a minimal valid PNG image (1x1 pixel, transparent)
    /// </summary>
    private static byte[] CreateSimpleTestImage()
    {
        // Minimal 1x1 transparent PNG
        return new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
            0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, // IDAT chunk
            0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
            0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
            0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, // IEND chunk
            0x42, 0x60, 0x82
        };
    }
}
