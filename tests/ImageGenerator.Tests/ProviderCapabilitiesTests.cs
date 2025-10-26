using ImageGenerator.Core.Abstractions;

namespace ImageGenerator.Tests;

public class ProviderCapabilitiesTests
{
    [Fact]
    public void ProviderCapabilities_CanBeCreated()
    {
        // Arrange & Act
        var capabilities = new ProviderCapabilities
        {
            ExampleModels = new List<string> { "model1", "model2" },
            SupportedOperations = new List<ImageOperation> { ImageOperation.Generate },
            DefaultModel = "model1",
            AcceptsCustomModels = true,
            Features = new Dictionary<string, object>
            {
                ["feature1"] = "value1"
            }
        };

        // Assert
        Assert.Equal(2, capabilities.ExampleModels.Count);
        Assert.Single(capabilities.SupportedOperations);
        Assert.Equal("model1", capabilities.DefaultModel);
        Assert.True(capabilities.AcceptsCustomModels);
        Assert.Single(capabilities.Features);
    }

    [Fact]
    public void ProviderCapabilities_DefaultValues()
    {
        // Arrange & Act
        var capabilities = new ProviderCapabilities();

        // Assert
        Assert.Empty(capabilities.ExampleModels);
        Assert.Empty(capabilities.SupportedOperations);
        Assert.Null(capabilities.DefaultModel);
        Assert.True(capabilities.AcceptsCustomModels); // Default is true
        Assert.Empty(capabilities.Features);
    }
}
