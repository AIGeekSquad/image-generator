using System.Reflection;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace AiGeekSquad.ImageGenerator.Core.Extensibility;

/// <summary>
/// Default implementation for loading external providers from assemblies
/// </summary>
public class AssemblyProviderLoader : IProviderLoader
{
    private readonly ILogger<AssemblyProviderLoader> _logger;

    /// <summary>
    /// Initializes a new instance of the AssemblyProviderLoader
    /// </summary>
    /// <param name="logger">Logger for diagnostic messages</param>
    public AssemblyProviderLoader(ILogger<AssemblyProviderLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads image generation providers from an external assembly
    /// </summary>
    /// <param name="assemblyPath">Path to the assembly file containing provider implementations</param>
    /// <returns>Collection of loaded providers</returns>
    public IEnumerable<IImageGenerationProvider> LoadProvidersFromAssembly(string assemblyPath)
    {
        var providers = new List<IImageGenerationProvider>();

        try
        {
            if (!File.Exists(assemblyPath))
            {
                _logger.LogError("Assembly file not found: {AssemblyPath}", assemblyPath);
                return providers;
            }

            _logger.LogInformation("Loading providers from assembly: {AssemblyPath}", assemblyPath);

            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyPath);

            // Find all types that implement IImageGenerationProvider
            var providerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IImageGenerationProvider).IsAssignableFrom(t))
                .ToList();

            _logger.LogInformation("Found {Count} provider type(s) in assembly", providerTypes.Count);

            foreach (var providerType in providerTypes)
            {
                try
                {
                    // Try to create an instance with parameterless constructor
                    var provider = Activator.CreateInstance(providerType) as IImageGenerationProvider;
                    if (provider != null)
                    {
                        providers.Add(provider);
                        _logger.LogInformation("Loaded provider: {ProviderName} from type {TypeName}", 
                            provider.ProviderName, providerType.FullName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to instantiate provider type {TypeName}. " +
                        "Make sure the provider has a parameterless constructor or register it manually.", 
                        providerType.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading providers from assembly: {AssemblyPath}", assemblyPath);
        }

        return providers;
    }
}
