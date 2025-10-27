using Google.Cloud.AIPlatform.V1;

namespace AiGeekSquad.ImageGenerator.Core.Adapters;

/// <summary>
/// Production implementation of IGoogleImageAdapter using actual Google Cloud SDK
/// </summary>
internal class GoogleImageAdapter : IGoogleImageAdapter
{
    private readonly PredictionServiceClient _client;

    public GoogleImageAdapter(PredictionServiceClient client)
    {
        _client = client;
    }

    public async Task<PredictResponse> PredictAsync(
        PredictRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _client.PredictAsync(request, cancellationToken);
    }
}
