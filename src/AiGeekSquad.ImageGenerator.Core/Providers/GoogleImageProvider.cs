using Google.Cloud.AIPlatform.V1;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using System.Text.Json;

namespace AiGeekSquad.ImageGenerator.Core.Providers;

/// <summary>
/// Google Gemini/Imagen provider for image generation - supports any current or future Imagen models
/// </summary>
public class GoogleImageProvider : ImageProviderBase
{
    private readonly PredictionServiceClient _client;
    private readonly string _projectId;
    private readonly string _location;

    public override string ProviderName => "Google";

    protected override ProviderCapabilities Capabilities { get; }

    public GoogleImageProvider(string projectId, string location = "us-central1", string? defaultModel = null)
    {
        _projectId = projectId;
        _location = location;
        _client = PredictionServiceClient.Create();

        Capabilities = new ProviderCapabilities
        {
            ExampleModels = new List<string>
            {
                ImageModels.Google.Imagen3,
                ImageModels.Google.Imagen2,
                ImageModels.Google.ImagenFast
            },
            SupportedOperations = new List<ImageOperation>
            {
                ImageOperation.Generate
                // Edit and Variation can be added in future when supported
            },
            DefaultModel = defaultModel ?? ImageModels.Google.Imagen3,
            AcceptsCustomModels = true,
            Features = new Dictionary<string, object>
            {
                ["supportsMultipleSamples"] = true,
                ["location"] = location,
                ["projectId"] = projectId
            }
        };
    }

    public override async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = GetModelOrDefault(request.Model);
        
        var endpoint = $"projects/{_projectId}/locations/{_location}/publishers/google/models/{model}";

        var parameters = new Dictionary<string, object>
        {
            ["sampleCount"] = request.NumberOfImages
        };

        // Merge in any additional parameters
        if (request.AdditionalParameters != null)
        {
            foreach (var param in request.AdditionalParameters)
            {
                parameters[param.Key] = param.Value;
            }
        }

        // Add quality/style if provided
        if (!string.IsNullOrEmpty(request.Quality))
        {
            parameters["quality"] = request.Quality;
        }

        if (!string.IsNullOrEmpty(request.Style))
        {
            parameters["style"] = request.Style;
        }

        var instances = new[]
        {
            new { prompt = request.Prompt }
        };

        var instancesValue = Google.Protobuf.WellKnownTypes.Value.Parser.ParseJson(
            JsonSerializer.Serialize(instances));
        
        var parametersValue = Google.Protobuf.WellKnownTypes.Value.Parser.ParseJson(
            JsonSerializer.Serialize(parameters));

        var predictRequest = new PredictRequest
        {
            Endpoint = endpoint,
            Instances = { instancesValue },
            Parameters = parametersValue
        };

        var response = await _client.PredictAsync(predictRequest, cancellationToken);

        var images = new List<GeneratedImage>();
        
        foreach (var prediction in response.Predictions)
        {
            var predictionDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                prediction.ToString());

            if (predictionDict != null && predictionDict.TryGetValue("bytesBase64Encoded", out var imageData))
            {
                images.Add(new GeneratedImage
                {
                    Base64Data = imageData.GetString()
                });
            }
        }

        return new ImageGenerationResponse
        {
            Images = images,
            Model = model,
            Provider = ProviderName,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["location"] = _location,
                ["projectId"] = _projectId
            }
        };
    }

    // Edit and Variation operations will use the base class implementation
    // which throws NotSupportedException since they're not in SupportedOperations
}
