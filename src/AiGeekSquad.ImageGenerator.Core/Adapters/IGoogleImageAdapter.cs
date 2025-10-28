using Google.Cloud.AIPlatform.V1;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Adapter interface for Google Cloud AI Platform operations - enables unit testing without API keys
/// </summary>
public interface IGoogleImageAdapter
{
    /// <summary>
    /// Executes a prediction request for image generation using Google Imagen models
    /// </summary>
    /// <param name="request">The prediction request with prompt and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prediction response containing generated images in base64 format</returns>
    Task<PredictResponse> PredictAsync(
        PredictRequest request,
        CancellationToken cancellationToken = default);
}
