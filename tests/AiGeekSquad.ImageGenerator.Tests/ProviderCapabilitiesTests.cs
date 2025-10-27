using AiGeekSquad.ImageGenerator.Core.Abstractions;
using FluentAssertions;
using FluentAssertions.Execution;

namespace AiGeekSquad.ImageGenerator.Tests;

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
        using (new AssertionScope())
        {
            capabilities.ExampleModels.Should().HaveCount(2);
            capabilities.SupportedOperations.Should().ContainSingle();
            capabilities.DefaultModel.Should().Be("model1");
            capabilities.AcceptsCustomModels.Should().BeTrue();
            capabilities.Features.Should().ContainSingle();
        }
    }

    [Fact]
    public void ProviderCapabilities_DefaultValues()
    {
        // Arrange & Act
        var capabilities = new ProviderCapabilities();

        // Assert
        using (new AssertionScope())
        {
            capabilities.ExampleModels.Should().BeEmpty();
            capabilities.SupportedOperations.Should().BeEmpty();
            capabilities.DefaultModel.Should().BeNull();
            capabilities.AcceptsCustomModels.Should().BeTrue(); // Default is true
            capabilities.Features.Should().BeEmpty();
        }
    }
}
