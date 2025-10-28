using AiGeekSquad.ImageGenerator.Core.Abstractions;

namespace AiGeekSquad.ImageGenerator.Core.Extensibility;

/// <summary>
/// Interface for loading external image generation providers
/// </summary>
public interface IProviderLoader
{
    /// <summary>
    /// Load providers from an external assembly
    /// </summary>
    /// <param name="assemblyPath">Path to the assembly containing provider implementations</param>
    /// <returns>List of loaded providers</returns>
    IEnumerable<IImageGenerationProvider> LoadProvidersFromAssembly(string assemblyPath);
}
