using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;

namespace AiGeekSquad.ImageGenerator.Tests;

public class ImageModelsTests
{
    [Fact]
    public void OpenAI_Models_AreDefined()
    {
        // Verify all OpenAI models are properly defined
        using (new AssertionScope())
        {
            ImageModels.OpenAI.DallE3.Should().NotBeNullOrEmpty();
            ImageModels.OpenAI.DallE2.Should().NotBeNullOrEmpty();
            ImageModels.OpenAI.GPTImage1.Should().NotBeNullOrEmpty();
            ImageModels.OpenAI.GPT5Image.Should().NotBeNullOrEmpty();
            ImageModels.OpenAI.Default.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Google_Models_AreDefined()
    {
        // Verify all Google models are properly defined
        using (new AssertionScope())
        {
            ImageModels.Google.Imagen3.Should().NotBeNullOrEmpty();
            ImageModels.Google.Imagen2.Should().NotBeNullOrEmpty();
            ImageModels.Google.ImagenFast.Should().NotBeNullOrEmpty();
            ImageModels.Google.Default.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Sizes_AreDefined()
    {
        // Verify all standard sizes are defined
        using (new AssertionScope())
        {
            ImageModels.Sizes.Square1024.Should().NotBeNullOrEmpty();
            ImageModels.Sizes.Square512.Should().NotBeNullOrEmpty();
            ImageModels.Sizes.Square256.Should().NotBeNullOrEmpty();
            ImageModels.Sizes.Wide1792x1024.Should().NotBeNullOrEmpty();
            ImageModels.Sizes.Tall1024x1792.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Quality_Options_AreDefined()
    {
        // Verify quality options are defined
        using (new AssertionScope())
        {
            ImageModels.Quality.Standard.Should().NotBeNullOrEmpty();
            ImageModels.Quality.HD.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Style_Options_AreDefined()
    {
        // Verify style options are defined
        using (new AssertionScope())
        {
            ImageModels.Style.Vivid.Should().NotBeNullOrEmpty();
            ImageModels.Style.Natural.Should().NotBeNullOrEmpty();
        }
    }
}
