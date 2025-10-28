using Microsoft.Extensions.AI;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
using CoreImageEditRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageEditRequest;
using FluentAssertions;
using FluentAssertions.Execution;
using AiGeekSquad.ImageGenerator.Core.Providers;
using AiGeekSquad.ImageGenerator.Core.Models;
using SixLabors.ImageSharp;

namespace AiGeekSquad.ImageGenerator.Tests.AcceptanceCriteria;

/// <summary>
/// End-to-End Integration Tests with actual API calls
/// Tests use xUnit v3 SkipUnless to conditionally skip when API keys are not available
/// Note: These tests require actual API keys - set environment variables OPENAI_API_KEY or GOOGLE_PROJECT_ID
/// </summary>
public class EndToEndIntegrationTests
{
    public static bool HasOpenAIApiKey => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
    public static bool HasGoogleProjectId => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"));
    public static bool HasAnyProvider => HasOpenAIApiKey || HasGoogleProjectId;
    public static bool HasOpenAIOrganizationVerified => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_ORG_VERIFIED"));
    
    private static string OpenAIApiKey => Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
    private static string GoogleProjectId => Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID")!;
    private static string GoogleLocation => Environment.GetEnvironmentVariable("GOOGLE_LOCATION") ?? "us-central1";

    [Fact(Skip = "Requires OPENAI_API_KEY environment variable", SkipUnless = nameof(HasOpenAIApiKey))]
    public async Task E2E_OpenAI_GenerateImage_WithDallE3()
    {
        // Arrange

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
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().NotBeEmpty();
        response.Images[0].Url.Should().NotBeNullOrEmpty();
        response.Model.Should().Be(ImageModels.OpenAI.DallE3);
        response.Provider.Should().Be("OpenAI");
    }

    [Fact(Skip = "Requires OPENAI_API_KEY environment variable", SkipUnless = nameof(HasOpenAIApiKey))]
    public async Task E2E_OpenAI_GenerateImage_WithGPTImage1Mini()
    {
        // Arrange
        // Note: GPT Image models require organization verification at https://platform.openai.com/settings/organization/general
        var provider = new OpenAIImageProvider(OpenAIApiKey);
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "A cute cat playing with a ball of yarn")
            },
            Model = ImageModels.OpenAI.GPTImage1Mini,
            Size = "1024x1024"
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().NotBeEmpty();
        response.Images[0].Base64Data.Should().NotBeNullOrEmpty(); // GPT Image models return base64 data, not URLs
        response.Model.Should().Be(ImageModels.OpenAI.GPTImage1Mini);
        response.Provider.Should().Be("OpenAI");
    }

    [Fact(Skip = "Requires GOOGLE_PROJECT_ID environment variable", SkipUnless = nameof(HasGoogleProjectId))]
    public async Task E2E_Google_GenerateImage_WithImagen3()
    {
        // Arrange

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
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().NotBeEmpty();
        response.Images[0].Base64Data.Should().NotBeNullOrEmpty();
        
        // Verify base64 can be decoded
        var imageBytes = Convert.FromBase64String(response.Images[0].Base64Data!);
        imageBytes.Should().NotBeEmpty();
    }

    [Fact(Skip = "Requires OPENAI_API_KEY environment variable", SkipUnless = nameof(HasOpenAIApiKey))]
    public async Task E2E_OpenAI_EditImage_WithGPTImage1Mini()
    {
        // Arrange
        // Note: GPT Image models require organization verification at https://platform.openai.com/settings/organization/general
        var provider = new OpenAIImageProvider(OpenAIApiKey);
        
        // Use the awesome_man.png asset for editing
        byte[] testImageBytes = CreateSimpleTestImage();
        string testImageBase64 = Convert.ToBase64String(testImageBytes);
        
        var editRequest = new CoreImageEditRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, "Add a red circle in the center")
            },
            Image = testImageBase64,
            Model = ImageModels.OpenAI.GPTImage1Mini,
            Size = "1024x1024"
        };

        // Act
        var response = await provider.EditImageAsync(editRequest, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().NotBeEmpty();
        response.Images[0].Base64Data.Should().NotBeNullOrEmpty(); // GPT Image models return base64 data
        response.Model.Should().Be(ImageModels.OpenAI.GPTImage1Mini);
        response.Provider.Should().Be("OpenAI");
    }

    [Fact(Skip = "Requires OPENAI_API_KEY environment variable", SkipUnless = nameof(HasOpenAIApiKey))]
    public async Task E2E_GenerateImage_FromConversation_WithMultipleMessages()
    {
        // Arrange

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
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().NotBeEmpty();
    }

    [Fact(Skip = "Requires OPENAI_API_KEY environment variable", SkipUnless = nameof(HasOpenAIApiKey))]
    public async Task E2E_GenerateImage_WithMultiModalInput()
    {
        // Arrange

        var provider = new OpenAIImageProvider(OpenAIApiKey);
        
        // Create a simple reference image
        byte[] referenceImage = CreateSimpleTestImage();
        
        var request = new CoreImageRequest
        {
            Messages = new List<ChatMessage>
            {
                new(ChatRole.User, new AIContent[]
                {
                    new TextContent("Create a simple landscape image"),
                    new DataContent(referenceImage, "image/png")
                })
            },
            Model = ImageModels.OpenAI.DallE3,
            Size = "1024x1024"
        };

        // Act
        var response = await provider.GenerateImageAsync(request, TestContext.Current.CancellationToken);

        // Assert - Provider extracts text even if it doesn't support multi-modal
        using var scope = new AssertionScope();
        response.Should().NotBeNull();
        response.Images.Should().NotBeEmpty();
    }

    [Fact(Skip = "Requires OPENAI_API_KEY or GOOGLE_PROJECT_ID environment variable", SkipUnless = nameof(HasAnyProvider))]
    public async Task E2E_MultipleProviders_SameRequest()
    {
        // Arrange

        var prompt = "A beautiful sunset over the ocean";
        var results = new List<CoreImageResponse>();

        // Act - Test with available providers
        if (HasOpenAIApiKey)
        {
            var openAiProvider = new OpenAIImageProvider(OpenAIApiKey);
            var openAiRequest = new CoreImageRequest
            {
                Messages = new List<ChatMessage> { new(ChatRole.User, prompt) },
                Model = ImageModels.OpenAI.DallE3
            };
            results.Add(await openAiProvider.GenerateImageAsync(openAiRequest, TestContext.Current.CancellationToken));
        }

        if (HasGoogleProjectId)
        {
            var googleProvider = new GoogleImageProvider(GoogleProjectId, GoogleLocation);
            var googleRequest = new CoreImageRequest
            {
                Messages = new List<ChatMessage> { new(ChatRole.User, prompt) },
                Model = ImageModels.Google.Imagen3
            };
            results.Add(await googleProvider.GenerateImageAsync(googleRequest, TestContext.Current.CancellationToken));
        }

        // Assert - All providers should successfully generate images
        using var scope = new AssertionScope();
        results.Should().NotBeEmpty();
        results.Should().OnlyContain(r => r.Images.Count > 0);
    }

    /// <summary>
    /// Creates a test image using the existing PNG asset - guaranteed to work with OpenAI
    /// </summary>
    private static byte[] CreateSimpleTestImage()
    {
        // Use the existing awesome_man.png asset which is a valid PNG that works with OpenAI
        var testProjectDir = Path.GetDirectoryName(typeof(EndToEndIntegrationTests).Assembly.Location)!;
        var assetPath = Path.Combine(testProjectDir, "..", "..", "..", "Assets", "awesome_man.png");
        assetPath = Path.GetFullPath(assetPath);
        
        if (!File.Exists(assetPath))
        {
            throw new FileNotFoundException($"Test asset not found: {assetPath}");
        }
        
        // Load and re-encode using ImageSharp to ensure OpenAI compatibility
        using var image = Image.Load(assetPath);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
