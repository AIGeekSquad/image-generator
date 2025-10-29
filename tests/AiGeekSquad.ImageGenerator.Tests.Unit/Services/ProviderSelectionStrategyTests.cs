using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiGeekSquad.ImageGenerator.Tests.Unit.Services;

/// <summary>
/// Unit tests for provider selection strategies
/// </summary>
[Trait("Category", "Unit")]
public class SmartProviderSelectorTests
{
    private readonly ILogger<SmartProviderSelector> _logger;
    private static readonly ImageOperation[] s_generateOperation = new[] { ImageOperation.Generate };
    private static readonly string[] s_modelAB = new[] { "model-a", "model-b" };
    private static readonly string[] s_modelC = new[] { "model-c" };
    private static readonly string[] s_modelA = new[] { "model-a" };
    private static readonly string[] s_modelB = new[] { "model-b" };

    public SmartProviderSelectorTests()
    {
        _logger = NullLogger<SmartProviderSelector>.Instance;
    }

    [Fact]
    public void Constructor_WithNullRegistry_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new SmartProviderSelector(null!, _logger);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("registry");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new TestProviderRegistry(new List<IProviderFactory>());

        // Act & Assert
        var act = () => new SmartProviderSelector(registry, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task SelectProviderAsync_WithNoAvailableProviders_ThrowsInvalidOperationException()
    {
        // Arrange
        var registry = new TestProviderRegistry(new List<IProviderFactory>());
        var selector = new SmartProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act & Assert
        var act = async () => await selector.SelectProviderAsync(context, services);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No suitable providers found*");
    }

    [Fact]
    public async Task SelectProviderAsync_WithPreferredProvider_SelectsPreferredProvider()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", priority: 100, operations: new[] { ImageOperation.Generate });
        var factory2 = new TestProviderFactory("Provider2", priority: 200, operations: new[] { ImageOperation.Generate });
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1, factory2 });
        var selector = new SmartProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            PreferredProvider = "Provider1",
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = await selector.SelectProviderAsync(context, services);

        // Assert
        result.ProviderName.Should().Be("Provider1");
    }

    [Fact]
    public async Task SelectProviderAsync_WithModelRequirement_SelectsProviderSupportingModel()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", 
            priority: 100, 
            operations: s_generateOperation,
            models: s_modelAB);
        var factory2 = new TestProviderFactory("Provider2", 
            priority: 200, 
            operations: s_generateOperation,
            models: s_modelC);
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1, factory2 });
        var selector = new SmartProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Model = "model-a",
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = await selector.SelectProviderAsync(context, services);

        // Assert
        result.ProviderName.Should().Be("Provider1");
    }

    [Fact]
    public async Task SelectProviderAsync_WithFailedProviders_SkipsFailedProviders()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", priority: 200, operations: new[] { ImageOperation.Generate });
        var factory2 = new TestProviderFactory("Provider2", priority: 100, operations: new[] { ImageOperation.Generate });
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1, factory2 });
        var selector = new SmartProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Operation = ImageOperation.Generate,
            FailedProviders = new HashSet<string> { "Provider1" }
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = await selector.SelectProviderAsync(context, services);

        // Assert
        result.ProviderName.Should().Be("Provider2");
    }

    [Fact]
    public async Task GetProviderOptionsAsync_ReturnsProvidersSortedByScore()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", priority: 100, operations: new[] { ImageOperation.Generate });
        var factory2 = new TestProviderFactory("Provider2", priority: 200, operations: new[] { ImageOperation.Generate });
        var factory3 = new TestProviderFactory("Provider3", priority: 150, operations: new[] { ImageOperation.Generate });
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1, factory2, factory3 });
        var selector = new SmartProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var results = await selector.GetProviderOptionsAsync(context, services);

        // Assert
        using var scope = new AssertionScope();
        results.Should().HaveCount(3);
        results[0].ProviderName.Should().Be("Provider2"); // Highest priority
        results[1].ProviderName.Should().Be("Provider3"); // Medium priority
        results[2].ProviderName.Should().Be("Provider1"); // Lowest priority
    }

    [Fact]
    public async Task GetProviderOptionsAsync_WithUnsupportedOperation_ReturnsEmpty()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", priority: 100, operations: new[] { ImageOperation.Generate });
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1 });
        var selector = new SmartProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Operation = ImageOperation.Edit
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var results = await selector.GetProviderOptionsAsync(context, services);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProviderOptionsAsync_WithCustomModels_IncludesProvidersAcceptingCustomModels()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", 
            priority: 100, 
            operations: s_generateOperation,
            models: s_modelA,
            acceptsCustomModels: true);
        var factory2 = new TestProviderFactory("Provider2", 
            priority: 200, 
            operations: s_generateOperation,
            models: s_modelB,
            acceptsCustomModels: false);
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1, factory2 });
        var selector = new SmartProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Model = "custom-model-xyz",
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var results = await selector.GetProviderOptionsAsync(context, services);

        // Assert
        results.Should().HaveCount(1);
        results[0].ProviderName.Should().Be("Provider1");
    }
}

[Trait("Category", "Unit")]
public class FallbackProviderSelectorTests
{
    private readonly ILogger<FallbackProviderSelector> _logger;

    public FallbackProviderSelectorTests()
    {
        _logger = NullLogger<FallbackProviderSelector>.Instance;
    }

    [Fact]
    public void Constructor_WithNullRegistry_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new FallbackProviderSelector(null!, _logger);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("registry");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new TestProviderRegistry(new List<IProviderFactory>());

        // Act & Assert
        var act = () => new FallbackProviderSelector(registry, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task SelectProviderAsync_WithNoAvailableProviders_ThrowsInvalidOperationException()
    {
        // Arrange
        var registry = new TestProviderRegistry(new List<IProviderFactory>());
        var selector = new FallbackProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act & Assert
        var act = async () => await selector.SelectProviderAsync(context, services);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SelectProviderAsync_WithAvailableProvider_ReturnsProvider()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", priority: 100, operations: new[] { ImageOperation.Generate });
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1 });
        var selector = new FallbackProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = await selector.SelectProviderAsync(context, services);

        // Assert
        result.ProviderName.Should().Be("Provider1");
    }

    [Fact]
    public async Task GetProviderOptionsAsync_DelegatesToPrimarySelector()
    {
        // Arrange
        var factory1 = new TestProviderFactory("Provider1", priority: 100, operations: new[] { ImageOperation.Generate });
        var factory2 = new TestProviderFactory("Provider2", priority: 200, operations: new[] { ImageOperation.Generate });
        var registry = new TestProviderRegistry(new List<IProviderFactory> { factory1, factory2 });
        var selector = new FallbackProviderSelector(registry, _logger);
        var context = new ProviderSelectionContext
        {
            Operation = ImageOperation.Generate
        };
        var services = new ServiceCollection().BuildServiceProvider();

        // Act
        var results = await selector.GetProviderOptionsAsync(context, services);

        // Assert
        results.Should().HaveCount(2);
    }
}

[Trait("Category", "Unit")]
public class ProviderSelectionContextTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Act
        var context = new ProviderSelectionContext();

        // Assert
        using var scope = new AssertionScope();
        context.PreferredProvider.Should().BeNull();
        context.Model.Should().BeNull();
        context.Operation.Should().Be(ImageOperation.Generate);
        context.RequiredCapabilities.Should().NotBeNull().And.BeEmpty();
        context.FailedProviders.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var context = new ProviderSelectionContext();

        // Act
        context.PreferredProvider = "TestProvider";
        context.Model = "test-model";
        context.Operation = ImageOperation.Edit;
        context.RequiredCapabilities.Add("capability1");
        context.FailedProviders.Add("FailedProvider1");

        // Assert
        using var scope = new AssertionScope();
        context.PreferredProvider.Should().Be("TestProvider");
        context.Model.Should().Be("test-model");
        context.Operation.Should().Be(ImageOperation.Edit);
        context.RequiredCapabilities.Should().Contain("capability1");
        context.FailedProviders.Should().Contain("FailedProvider1");
    }
}

#region Test Helpers

internal class TestProviderRegistry : IProviderRegistry
{
    private readonly List<IProviderFactory> _factories;

    public TestProviderRegistry(List<IProviderFactory> factories)
    {
        _factories = factories;
    }

    public IEnumerable<IProviderFactory> GetFactories() => _factories;

    public IProviderFactory? GetFactory(string name) =>
        _factories.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<IProviderFactory> GetAvailableFactories(IServiceProvider services) => _factories;

    public IEnumerable<IProviderFactory> GetFactoriesForOperation(ImageOperation operation, IServiceProvider services) =>
        _factories.Where(f => f.GetMetadata().Capabilities.SupportedOperations.Contains(operation));

    public IEnumerable<IProviderFactory> GetFactoriesForModel(string model, IServiceProvider services) =>
        _factories.Where(f =>
        {
            var caps = f.GetMetadata().Capabilities;
            return caps.ExampleModels.Any(m => m.Equals(model, StringComparison.OrdinalIgnoreCase)) ||
                   caps.AcceptsCustomModels;
        });
}

internal class TestProviderFactory : IProviderFactory
{
    private readonly string _name;
    private readonly ProviderMetadata _metadata;

    public TestProviderFactory(
        string name,
        int priority = 100,
        ImageOperation[]? operations = null,
        string[]? models = null,
        bool acceptsCustomModels = false)
    {
        _name = name;
        _metadata = new ProviderMetadata
        {
            Name = name,
            Priority = priority,
            Capabilities = new ProviderCapabilities
            {
                SupportedOperations = new List<ImageOperation>(operations ?? new[] { ImageOperation.Generate }),
                ExampleModels = new List<string>(models ?? Array.Empty<string>()),
                AcceptsCustomModels = acceptsCustomModels
            }
        };
    }

    public string Name => _name;

    public ProviderMetadata GetMetadata() => _metadata;

    public bool CanCreate(IServiceProvider services) => true;

    public IImageGenerationProvider Create(IServiceProvider services) =>
        new TestProvider(_name);
}

internal class TestProvider : IImageGenerationProvider
{
    public TestProvider(string name)
    {
        ProviderName = name;
    }

    public string ProviderName { get; }

    public Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ImageGenerationResponse> GenerateImageFromConversationAsync(
        ConversationalImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ImageGenerationResponse> CreateVariationAsync(
        ImageVariationRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool SupportsOperation(ImageOperation operation) => true;

    public ProviderCapabilities GetCapabilities() => new ProviderCapabilities
    {
        SupportedOperations = new List<ImageOperation> { ImageOperation.Generate }
    };
}

#endregion
