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

    // Note: Comprehensive testing of HostBuilderConfigurator is challenging in unit tests due to:
    // 1. The complex interaction between configuration sources (in-memory, environment, command line)
    // 2. The dependency on environment variables which are process-global
    // 3. The timing of when configuration sources are added vs. when they're read
    // 
    // The full configuration flow is already tested through:
    // - E2E tests (McpServerFixture tests the full host builder setup)
    // - Integration tests (test actual provider registration and service resolution)
    //
    // For detailed test coverage of the configuration logic, see:
    // - tests/AiGeekSquad.ImageGenerator.Tests.E2E/Fixtures/McpServerFixture.cs
    // - tests/AiGeekSquad.ImageGenerator.Tests.Integration/AcceptanceCriteria/EndToEndIntegrationTests.cs
}
