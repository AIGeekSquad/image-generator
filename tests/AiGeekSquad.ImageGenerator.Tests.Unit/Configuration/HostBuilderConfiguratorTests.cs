using AiGeekSquad.ImageGenerator.Tool.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AiGeekSquad.ImageGenerator.Tests.Unit.Configuration;

/// <summary>
/// Unit tests for HostBuilderConfigurator
/// </summary>
[Trait("Category", "Unit")]
public class HostBuilderConfiguratorTests
{
    [Fact]
    public void ConfigureServices_WithoutProviders_ThrowsInvalidOperationException()
    {
        // Arrange
        var args = Array.Empty<string>();
        var builder = Host.CreateApplicationBuilder();
        // Clear environment that might have API keys
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>());

        // Act
        var act = () => HostBuilderConfigurator.ConfigureServices(builder, args);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No image generation providers are configured*");
    }
}
