using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Tests;

public class ImageModelsTests
{
    [Fact]
    public void OpenAI_Models_AreDefined()
    {
        // Verify all OpenAI models are properly defined
        Assert.NotNull(ImageModels.OpenAI.DallE3);
        Assert.NotNull(ImageModels.OpenAI.DallE2);
        Assert.NotNull(ImageModels.OpenAI.GPTImage1);
        Assert.NotNull(ImageModels.OpenAI.GPT5Image);
        Assert.NotNull(ImageModels.OpenAI.Default);
    }

    [Fact]
    public void Google_Models_AreDefined()
    {
        // Verify all Google models are properly defined
        Assert.NotNull(ImageModels.Google.Imagen3);
        Assert.NotNull(ImageModels.Google.Imagen2);
        Assert.NotNull(ImageModels.Google.ImagenFast);
        Assert.NotNull(ImageModels.Google.Default);
    }

    [Fact]
    public void Sizes_AreDefined()
    {
        // Verify all standard sizes are defined
        Assert.NotNull(ImageModels.Sizes.Square1024);
        Assert.NotNull(ImageModels.Sizes.Square512);
        Assert.NotNull(ImageModels.Sizes.Square256);
        Assert.NotNull(ImageModels.Sizes.Wide1792x1024);
        Assert.NotNull(ImageModels.Sizes.Tall1024x1792);
    }

    [Fact]
    public void Quality_Options_AreDefined()
    {
        // Verify quality options are defined
        Assert.NotNull(ImageModels.Quality.Standard);
        Assert.NotNull(ImageModels.Quality.HD);
    }

    [Fact]
    public void Style_Options_AreDefined()
    {
        // Verify style options are defined
        Assert.NotNull(ImageModels.Style.Vivid);
        Assert.NotNull(ImageModels.Style.Natural);
    }
}
