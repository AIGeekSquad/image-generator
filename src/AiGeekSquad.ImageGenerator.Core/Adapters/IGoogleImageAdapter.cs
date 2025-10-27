using Google.Cloud.AIPlatform.V1;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Adapter interface for Google Cloud AI Platform operations - enables unit testing without API keys
/// </summary>
public interface IGoogleImageAdapter
{
    Task<PredictResponse> PredictAsync(
        PredictRequest request,
        CancellationToken cancellationToken = default);
}
