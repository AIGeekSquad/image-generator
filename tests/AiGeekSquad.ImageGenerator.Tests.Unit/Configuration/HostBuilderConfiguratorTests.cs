using AiGeekSquad.ImageGenerator.Tool.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AiGeekSquad.ImageGenerator.Tests.Unit.Configuration;

/// <summary>
/// Unit tests for HostBuilderConfigurator
/// </summary>
/// <remarks>
/// Note: Comprehensive unit testing of HostBuilderConfigurator is limited due to:
/// - Complex interaction between configuration sources (in-memory, environment, command line)
/// - Process-global environment variables that affect all tests
/// - Timing issues with configuration source registration vs. reading
/// 
/// The HostBuilderConfigurator is comprehensively tested through:
/// - E2E tests: Full MCP server startup with real configuration (McpServerFixture)
/// - Integration tests: 78 tests covering provider registration and service resolution
/// - This ensures the configuration logic works correctly in realistic scenarios
/// </remarks>
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
