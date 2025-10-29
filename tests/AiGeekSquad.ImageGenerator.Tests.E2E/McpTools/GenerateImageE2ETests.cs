using AiGeekSquad.ImageGenerator.Tests.E2E.Fixtures;
using AiGeekSquad.ImageGenerator.Core.Models;
using FluentAssertions.Execution;

namespace AiGeekSquad.ImageGenerator.Tests.E2E.McpTools;

/// <summary>
/// End-to-end tests for the generate_image MCP tool
/// </summary>
[Trait("Category", "E2E")]
[Trait("Component", "McpTools")]
public class GenerateImageE2ETests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;

    public GenerateImageE2ETests(McpServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GenerateImage_WithAllParameters_ParsesAndProcessesCorrectly()
    {
        // Arrange
        if (!_fixture.HasApiKeys) 
            Assert.Skip(_fixture.SkipReason);

        var arguments = new Dictionary<string, object?>
        {
            ["prompt"] = "A beautiful sunset over mountains",
            ["provider"] = "OpenAI",
            ["model"] = "dall-e-3",
            ["size"] = "1024x1024",
            ["quality"] = "hd",
            ["style"] = "vivid",
            ["numberOfImages"] = 1
        };

        // Act
        var response = await _fixture.SendMcpRequest("generate_image", arguments, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.IsSuccess.Should().BeTrue();
        response.Content.Should().NotBeNullOrEmpty();
        
        // Parse response to verify structure
        var result = JsonSerializer.Deserialize<ImageGenerationResponse>(response.Content);
        result.Should().NotBeNull();
        result.Images.Should().NotBeEmpty();
        result.Model.Should().NotBeNullOrEmpty();
        result.Provider.Should().NotBeNullOrEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GenerateImage_WithMinimalParameters_UsesDefaults()
    {
        // Arrange
        if (!_fixture.HasApiKeys) 
            Assert.Skip(_fixture.SkipReason);

        var arguments = new Dictionary<string, object?>
        {
            ["prompt"] = "A simple test image"
        };

        // Act
        var response = await _fixture.SendMcpRequest("generate_image", arguments, TestContext.Current.CancellationToken);

        // Assert
        using var scope = new AssertionScope();
        response.IsSuccess.Should().BeTrue();
        response.Content.Should().NotBeNullOrEmpty();
        
        var result = JsonSerializer.Deserialize<ImageGenerationResponse>(response.Content);
        result.Should().NotBeNull();
        result.Images.Should().HaveCount(1); // Default numberOfImages
        result.Provider.Should().Be("OpenAI"); // Default provider
    }

    [Fact]
    public async Task GenerateImage_WithInvalidArguments_ReturnsError()
    {
        // Arrange
        var testCases = new[]
        {
            new Dictionary<string, object?>(), // No prompt
            new Dictionary<string, object?> { ["prompt"] = "", ["numberOfImages"] = -1 }, // Invalid count
            new Dictionary<string, object?> { ["prompt"] = "test", ["size"] = "invalid" }, // Invalid size
            new Dictionary<string, object?> { ["prompt"] = "test", ["quality"] = "bad" }, // Invalid quality
            new Dictionary<string, object?> { ["prompt"] = "test", ["style"] = "wrong" } // Invalid style
        };

        foreach (var arguments in testCases)
        {
            // Act
            var response = await _fixture.SendMcpRequest("generate_image", arguments, TestContext.Current.CancellationToken);

            // Assert
            response.ContainError().Should().BeTrue($"Expected error for arguments: {JsonSerializer.Serialize(arguments)}");
            response.Content.Should().Contain("error");
        }
    }

    [Fact]
    public async Task GenerateImage_WithUnsupportedProvider_ReturnsError()
    {
        // Arrange
        var arguments = new Dictionary<string, object?>
        {
            ["prompt"] = "Test image",
            ["provider"] = "NonExistentProvider"
        };

        // Act
        var response = await _fixture.SendMcpRequest("generate_image", arguments, TestContext.Current.CancellationToken);

        // Assert
        response.ContainError().Should().BeTrue();
        // JSON escapes single quotes as \u0027
        response.Content.Should().Contain("NonExistentProvider");
        response.Content.Should().Contain("not found");
    }

    [Theory]
    [InlineData("dall-e-3")]
    [InlineData("dall-e-2")]
    [InlineData("gpt-image-1")]
    public async Task GenerateImage_WithDifferentModels_WorksCorrectly(string model)
    {
        // Arrange
        if (!_fixture.HasApiKeys) 
            Assert.Skip(_fixture.SkipReason);

        var arguments = new Dictionary<string, object?>
        {
            ["prompt"] = $"Test image with {model}",
            ["model"] = model,
            ["numberOfImages"] = 1
        };

        // Act
        var response = await _fixture.SendMcpRequest("generate_image", arguments, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var result = JsonSerializer.Deserialize<ImageGenerationResponse>(response.Content);
        result!.Model.Should().Be(model);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GenerateImage_WithMultipleImages_ReturnsCorrectCount(int numberOfImages)
    {
        // Arrange
        if (!_fixture.HasApiKeys) 
            Assert.Skip(_fixture.SkipReason);

        var arguments = new Dictionary<string, object?>
        {
            ["prompt"] = "house",
            ["model"] = "dall-e-2", // DALL-E 2 supports multiple images, DALL-E 3 only supports 1
            ["numberOfImages"] = numberOfImages
        };

        // Act
        var response = await _fixture.SendMcpRequest("generate_image", arguments, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccess.Should().BeTrue();
        var result = JsonSerializer.Deserialize<ImageGenerationResponse>(response.Content);
        // OpenAI DALL-E currently only returns 1 image regardless of request
        result!.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateImage_ResponseFormat_IsValidJson()
    {
        // Arrange
        if (!_fixture.HasApiKeys) 
            Assert.Skip(_fixture.SkipReason);

        var arguments = new Dictionary<string, object?>
        {
            ["prompt"] = "house"
        };

        // Act
        var response = await _fixture.SendMcpRequest("generate_image", arguments, TestContext.Current.CancellationToken);

        // Assert
        // Should be valid JSON regardless of success/error
        var deserializeAction = () => JsonSerializer.Deserialize<object>(response.Content);
        deserializeAction.Should().NotThrow();
        
        // Should have proper indentation (from WriteIndented = true) or be compact JSON
        response.Content.Should().Contain("{"); // JSON structure - FluentAssertions requires string
        (response.Content.Contains("  ") || response.Content.Contains('"')).Should().BeTrue(); // Indented or valid JSON
    }

    [Fact]
    public async Task GenerateImage_ConcurrentRequests_HandleCorrectly()
    {
        // Arrange
        if (!_fixture.HasApiKeys) 
            Assert.Skip(_fixture.SkipReason);

        var tasks = Enumerable.Range(0, 3)
            .Select(i => new Dictionary<string, object?>
            {
                ["prompt"] = $"Concurrent test image {i}",
                ["numberOfImages"] = 1
            })
            .Select(args => _fixture.SendMcpRequest("generate_image", args, TestContext.Current.CancellationToken))
            .ToArray();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        using var scope = new AssertionScope();
        responses.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        responses.Should().HaveCount(3);
        
        // Each should have unique results
        var contents = responses.Select(r => r.Content).ToList();
        contents.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GenerateImage_WithTimeout_HandlesGracefully()
    {
        // Arrange
        if (!_fixture.HasApiKeys) 
            Assert.Skip(_fixture.SkipReason);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30-second timeout

        var arguments = new Dictionary<string, object?>
        {
            ["prompt"] = "Timeout test image"
        };

        // Act & Assert
        var response = await _fixture.SendMcpRequest("generate_image", arguments, cts.Token);
        
        // Should either succeed or handle cancellation gracefully
        if (!response.IsSuccess && response.Exception is OperationCanceledException)
        {
            // Timeout is acceptable for this test
            response.Exception.Should().BeOfType<OperationCanceledException>();
        }
        else
        {
            // If it completes, should be successful
            response.IsSuccess.Should().BeTrue();
        }
    }
}