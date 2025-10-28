using AiGeekSquad.ImageGenerator.Core.Adapters;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ProtobufValue = Google.Protobuf.WellKnownTypes.Value;

namespace AiGeekSquad.ImageGenerator.Tests.Providers;

/// <summary>
/// Test implementation of IGoogleImageAdapter for testing without requiring GCP credentials
/// </summary>
internal class TestGoogleImageAdapter : IGoogleImageAdapter
{
    private readonly string? _base64Image;

    public TestGoogleImageAdapter(string? base64Image = null)
    {
        _base64Image = base64Image ?? "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";
    }

    public Task<PredictResponse> PredictAsync(
        PredictRequest request,
        CancellationToken cancellationToken = default)
    {
        // Create a mock PredictResponse with a base64 encoded image
        var response = new PredictResponse();
        
        // Create a Value with the image data structure that Google returns
        var prediction = new ProtobufValue
        {
            StructValue = new Struct
            {
                Fields =
                {
                    ["bytesBase64Encoded"] = ProtobufValue.ForString(_base64Image)
                }
            }
        };
        
        response.Predictions.Add(prediction);
        
        return Task.FromResult(response);
    }
}
