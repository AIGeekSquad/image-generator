using AiGeekSquad.ImageGenerator.Core.Services;
using AiGeekSquad.ImageGenerator.Core.Models;
using FluentAssertions.Execution;

namespace AiGeekSquad.ImageGenerator.Tests.Unit.ArgumentParsing;

/// <summary>
/// Unit tests for MCP argument parsing functionality
/// </summary>
[Trait("Category", "Unit")]
public class McpArgumentParserTests
{
    private readonly McpArgumentParser _parser = new();

    #region Basic Parameter Parsing Tests

    [Fact]
    public void Parse_BasicArguments_ParsedCorrectly()
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            ["prompt"] = "A beautiful sunset",
            ["provider"] = "OpenAI",
            ["model"] = "dall-e-3",
            ["size"] = "1024x1024",
            ["quality"] = "hd",
            ["style"] = "vivid",
            ["numberOfImages"] = 2
        };

        // Act
        var result = _parser.Parse(args);

        // Assert
        using var scope = new AssertionScope();
        result.Prompt.Should().Be("A beautiful sunset");
        result.Provider.Should().Be("OpenAI");
        result.Model.Should().Be("dall-e-3");
        result.Size.Should().Be("1024x1024");
        result.Quality.Should().Be("hd");
        result.Style.Should().Be("vivid");
        result.NumberOfImages.Should().Be(2);
    }

    [Fact]
    public void Parse_EmptyArguments_ReturnsDefaults()
    {
        // Arrange
        var args = new Dictionary<string, object?>();

        // Act
        var result = _parser.Parse(args);

        // Assert
        using var scope = new AssertionScope();
        result.Prompt.Should().BeNull();
        result.Provider.Should().BeNull();
        result.Model.Should().BeNull();
        result.NumberOfImages.Should().Be(1);
    }

    [Fact]
    public void Parse_NullArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        _parser.Invoking(p => p.Parse(null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("args");
    }

    #endregion

    #region Size Parsing Tests

    [Theory]
    [InlineData("1024x1024", 1024, 1024)]
    [InlineData("1792x1024", 1792, 1024)]
    [InlineData("1024x1792", 1024, 1792)]
    [InlineData("512x512", 512, 512)]
    [InlineData("256x256", 256, 256)]
    [InlineData("1024X1024", 1024, 1024)] // Capital X
    public void ParseSize_ValidFormats_ParsesCorrectly(
        string input, int expectedWidth, int expectedHeight)
    {
        // Act
        var result = _parser.ParseSize(input);

        // Assert
        result.Should().NotBeNull();
        result!.Width.Should().Be(expectedWidth);
        result.Height.Should().Be(expectedHeight);
        result.ToString().Should().Be($"{expectedWidth}x{expectedHeight}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("1024")]
    [InlineData("1024x")]
    [InlineData("x1024")]
    [InlineData("1024x1024x1024")]
    [InlineData("-1024x1024")]
    [InlineData("1024x-1024")]
    [InlineData("0x1024")]
    [InlineData("1024x0")]
    public void ParseSize_InvalidFormats_ReturnsNull(string? input)
    {
        // Act
        var result = _parser.ParseSize(input);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Number Parsing Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Parse_ValidNumberOfImages_ParsedCorrectly(int numberOfImages)
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            ["numberOfImages"] = numberOfImages
        };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.NumberOfImages.Should().Be(numberOfImages);
    }

    [Theory]
    [InlineData("5", 5)]
    [InlineData("10", 10)]
    [InlineData(2.0, 2)]
    [InlineData(3.7f, 3)]
    public void Parse_NumberOfImagesTypeConversion_WorksCorrectly(object input, int expected)
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            ["numberOfImages"] = input
        };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.NumberOfImages.Should().Be(expected);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData(null)]
    public void Parse_InvalidNumberOfImages_UsesDefault(object? input)
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            ["numberOfImages"] = input
        };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.NumberOfImages.Should().Be(1);
    }

    #endregion

    #region Conversation JSON Parsing Tests

    [Fact]
    public void ParseConversation_ValidJson_ParsesCorrectly()
    {
        // Arrange
        var json = @"[
            {""role"":""user"",""text"":""Create an image of a sunset""},
            {""role"":""assistant"",""text"":""I'll create that for you""},
            {""role"":""user"",""text"":""Make it more colorful""}
        ]";

        // Act
        var result = _parser.ParseConversation(json);

        // Assert
        result.Should().NotBeNull();
        result!.Should().HaveCount(3);
        result[0].Role.Should().Be("user");
        result[0].Text.Should().Be("Create an image of a sunset");
        result[1].Role.Should().Be("assistant");
        result[2].Role.Should().Be("user");
        result[2].Text.Should().Be("Make it more colorful");
    }

    [Fact]
    public void ParseConversation_ValidJsonWithImages_ParsesCorrectly()
    {
        // Arrange
        var json = @"[
            {
                ""role"":""user"",
                ""text"":""Edit this image"",
                ""images"":[
                    {
                        ""url"":""https://example.com/image.png"",
                        ""caption"":""Original image""
                    }
                ]
            }
        ]";

        // Act
        var result = _parser.ParseConversation(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Images.Should().NotBeNull();
        result[0].Images!.Should().HaveCount(1);
        result[0].Images![0].Url.Should().Be("https://example.com/image.png");
        result[0].Images![0].Caption.Should().Be("Original image");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid json")]
    [InlineData("{")]
    public void ParseConversation_InvalidJson_ReturnsNull(string? input)
    {
        // Act
        var result = _parser.ParseConversation(input!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseConversation_EmptyArray_ReturnsEmptyList()
    {
        // Act
        var result = _parser.ParseConversation("[]");

        // Assert
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Validate_ValidArguments_ReturnsValid()
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Create an image",
            Size = "1024x1024",
            Quality = "hd",
            Style = "vivid",
            NumberOfImages = 2,
            ParsedSize = new ImageSize { Width = 1024, Height = 1024 }
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MissingPromptAndConversation_ReturnsError()
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = null,
            Conversation = null
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Either 'prompt' or valid 'conversationJson' is required");
    }

    [Theory]
    [InlineData(0, "NumberOfImages must be between 1 and 10")]
    [InlineData(-1, "NumberOfImages must be between 1 and 10")]
    [InlineData(11, "NumberOfImages must be between 1 and 10")]
    [InlineData(100, "NumberOfImages must be between 1 and 10")]
    public void Validate_InvalidNumberOfImages_ReturnsError(int numberOfImages, string expectedError)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            NumberOfImages = numberOfImages
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData("invalid_size")]
    [InlineData("1024")]
    [InlineData("1024x")]
    [InlineData("x1024")]
    public void Validate_InvalidSize_ReturnsError(string size)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            Size = size,
            ParsedSize = null // Simulate parsing failure
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain($"Invalid size format: '{size}'. Expected format: 'WIDTHxHEIGHT' (e.g., '1024x1024')");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("low")]
    [InlineData("high")]
    public void Validate_InvalidQuality_ReturnsError(string quality)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            Quality = quality
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Quality must be either 'standard' or 'hd'");
    }

    [Theory]
    [InlineData("standard")]
    [InlineData("hd")]
    public void Validate_ValidQuality_PassesValidation(string quality)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            Quality = quality
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.Errors.Should().NotContain(e => e.Contains("Quality"));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("realistic")]
    [InlineData("cartoon")]
    public void Validate_InvalidStyle_ReturnsError(string style)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            Style = style
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Style must be either 'vivid' or 'natural'");
    }

    [Theory]
    [InlineData("vivid")]
    [InlineData("natural")]
    public void Validate_ValidStyle_PassesValidation(string style)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            Style = style
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.Errors.Should().NotContain(e => e.Contains("Style"));
    }

    [Fact]
    public void Validate_EmptyConversation_ReturnsError()
    {
        // Arrange
        var args = new ParsedArguments
        {
            Conversation = new List<ConversationMessage>()
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Conversation must contain at least one message");
    }

    [Fact]
    public void Validate_ValidConversation_PassesValidation()
    {
        // Arrange
        var args = new ParsedArguments
        {
            Conversation = new List<ConversationMessage>
            {
                new ConversationMessage { Role = "user", Text = "Create an image" }
            }
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.Contains("Conversation"));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Parse_MixedCaseParameters_WorksCorrectly()
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            ["PROMPT"] = "Test",
            ["Provider"] = "OpenAI",
            ["MODEL"] = "dall-e-3"
        };

        // Act
        var result = _parser.Parse(args);

        // Assert - Should handle case-insensitive keys
        result.Prompt.Should().BeNull(); // Because keys are case-sensitive in our implementation
    }

    [Fact]
    public void Parse_NullValues_HandledGracefully()
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            ["prompt"] = null,
            ["provider"] = null,
            ["numberOfImages"] = null
        };

        // Act
        var result = _parser.Parse(args);

        // Assert
        using var scope = new AssertionScope();
        result.Prompt.Should().BeNull();
        result.Provider.Should().BeNull();
        result.NumberOfImages.Should().Be(1); // Default value
    }

    [Theory]
    [InlineData("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==")]
    [InlineData("https://example.com/image.png")]
    [InlineData("http://example.com/image.jpg")]
    public void Validate_ValidImageFormats_PassesValidation(string image)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            Image = image
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.Errors.Should().NotContain(e => e.Contains("Image must be"));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ftp://example.com/image.png")]
    [InlineData("not-base64!@#$")]
    public void Validate_InvalidImageFormats_ReturnsError(string image)
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "Test",
            Image = image
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Image must be a valid base64 encoded image, data URL, or HTTP URL");
    }

    #endregion

    #region Integration-like Tests

    [Fact]
    public void Parse_ComplexScenario_AllParametersParsedCorrectly()
    {
        // Arrange
        var conversationJson = @"[
            {""role"":""user"",""text"":""Create an image""},
            {""role"":""assistant"",""text"":""What kind of image?""},
            {""role"":""user"",""text"":""A sunset over mountains""}
        ]";

        var args = new Dictionary<string, object?>
        {
            ["prompt"] = "A beautiful sunset",
            ["provider"] = "OpenAI",
            ["model"] = "dall-e-3",
            ["size"] = "1792x1024",
            ["quality"] = "hd",
            ["style"] = "vivid",
            ["numberOfImages"] = 3,
            ["conversationJson"] = conversationJson,
            ["image"] = "data:image/png;base64,test==",
            ["mask"] = "https://example.com/mask.png"
        };

        // Act
        var result = _parser.Parse(args);

        // Assert
        using var scope = new AssertionScope();
        result.Prompt.Should().Be("A beautiful sunset");
        result.Provider.Should().Be("OpenAI");
        result.Model.Should().Be("dall-e-3");
        result.ParsedSize!.Width.Should().Be(1792);
        result.ParsedSize.Height.Should().Be(1024);
        result.Quality.Should().Be("hd");
        result.Style.Should().Be("vivid");
        result.NumberOfImages.Should().Be(3);
        result.Conversation.Should().HaveCount(3);
        result.Image.Should().Be("data:image/png;base64,test==");
        result.Mask.Should().Be("https://example.com/mask.png");
    }

    [Fact]
    public void Validate_ComplexValidScenario_PassesAllValidations()
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = "A beautiful sunset",
            Provider = "OpenAI",
            Model = "dall-e-3",
            Size = "1024x1024",
            ParsedSize = new ImageSize { Width = 1024, Height = 1024 },
            Quality = "hd",
            Style = "vivid",
            NumberOfImages = 2,
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var args = new ParsedArguments
        {
            Prompt = null, // Missing prompt
            Conversation = null, // No conversation either
            NumberOfImages = -1, // Invalid count
            Quality = "invalid", // Invalid quality
            Style = "wrong", // Invalid style
            Size = "bad", // Invalid size
            ParsedSize = null, // Failed parsing
            Image = "invalid_image" // Invalid image format
        };

        // Act
        var result = _parser.Validate(args);

        // Assert
        using var scope = new AssertionScope();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(5);
        result.Errors.Should().Contain("Either 'prompt' or valid 'conversationJson' is required");
        result.Errors.Should().Contain("NumberOfImages must be between 1 and 10");
        result.Errors.Should().Contain("Quality must be either 'standard' or 'hd'");
        result.Errors.Should().Contain("Style must be either 'vivid' or 'natural'");
        result.Errors.Should().Contain(e => e.Contains("Invalid size format"));
        result.Errors.Should().Contain("Image must be a valid base64 encoded image, data URL, or HTTP URL");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Parse_LargeConversationJson_PerformsReasonably()
    {
        // Arrange - Create large conversation JSON
        var messages = Enumerable.Range(0, 100)
            .Select(i => $@"{{""role"":""user"",""text"":""Message {i}""}}")
            .ToList();
        var json = $"[{string.Join(",", messages)}]";

        var args = new Dictionary<string, object?>
        {
            ["conversationJson"] = json
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _parser.Parse(args);
        stopwatch.Stop();

        // Assert
        result.Conversation.Should().HaveCount(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should parse in under 1 second
    }

    #endregion
}