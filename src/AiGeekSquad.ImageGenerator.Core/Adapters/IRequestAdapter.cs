using AiGeekSquad.ImageGenerator.Core.Models;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Interface for adapting different request formats to the unified request model
/// </summary>
public interface IRequestAdapter<in TSource>
{
    /// <summary>
    /// Adapts a source request format to the unified request model
    /// </summary>
    /// <param name="source">Source request object</param>
    /// <returns>Unified request model</returns>
    UnifiedImageRequest Adapt(TSource source);
}