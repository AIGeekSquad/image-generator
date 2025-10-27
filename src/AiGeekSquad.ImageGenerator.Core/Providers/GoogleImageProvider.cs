using Google.Cloud.AIPlatform.V1;
using System.Text.Json;
using Microsoft.Extensions.AI;
using CoreImageRequest = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationRequest;
using CoreImageResponse = AiGeekSquad.ImageGenerator.Core.Models.ImageGenerationResponse;
using GeneratedImageModel = AiGeekSquad.ImageGenerator.Core.Models.GeneratedImage;
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Adapters;

namespace AiGeekSquad.ImageGenerator.Core.Providers;

/// <summary>
/// Google Gemini/Imagen provider for image generation - supports any current or future Imagen models
/// </summary>
public class GoogleImageProvider : ImageProviderBase
{
    private readonly IGoogleImageAdapter _adapter;
    private readonly string _projectId;
    private readonly string _location;

    public override string ProviderName => "Google";

    protected override ProviderCapabilities Capabilities { get; }

    /// <summary>
    /// Creates a Google provider with the specified configuration
    /// </summary>
    public GoogleImageProvider(string projectId, string location = "us-central1", string? defaultModel = null, HttpClient? httpClient = null)
        : this(new GoogleImageAdapter(PredictionServiceClient.Create()), projectId, location, defaultModel, httpClient)
    {
    }

    /// <summary>
    /// Creates a Google provider with a custom adapter (useful for testing)
    /// </summary>
    public GoogleImageProvider(IGoogleImageAdapter adapter, string projectId, string location = "us-central1", string? defaultModel = null, HttpClient? httpClient = null)
        : base(httpClient)
    {
        _adapter = adapter;
        _projectId = projectId;
        _location = location;

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

    public override async Task<CoreImageResponse> GenerateImageAsync(
        CoreImageRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = GetModelOrDefault(request.Model);
        var endpoint = $"projects/{_projectId}/locations/{_location}/publishers/google/models/{model}";
        var prompt = ExtractTextFromMessages(request.Messages);

        var predictRequest = BuildPredictRequest(endpoint, prompt, request);
        var response = await _adapter.PredictAsync(predictRequest, cancellationToken);

        var images = ExtractImagesFromPrediction(response);
        
        return BuildResponse(
            images,
            model,
            new Dictionary<string, object>
            {
                ["location"] = _location,
                ["projectId"] = _projectId
            });
    }

    private PredictRequest BuildPredictRequest(string endpoint, string prompt, CoreImageRequest request)
    {
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
            new { prompt = prompt }
        };

        var instancesValue = Google.Protobuf.WellKnownTypes.Value.Parser.ParseJson(
            JsonSerializer.Serialize(instances));
        
        var parametersValue = Google.Protobuf.WellKnownTypes.Value.Parser.ParseJson(
            JsonSerializer.Serialize(parameters));

        return new PredictRequest
        {
            Endpoint = endpoint,
            Instances = { instancesValue },
            Parameters = parametersValue
        };
    }

    private static List<GeneratedImageModel> ExtractImagesFromPrediction(PredictResponse response)
    {
        var images = new List<GeneratedImageModel>();
        
        foreach (var prediction in response.Predictions)
        {
            var predictionDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                prediction.ToString());

            if (predictionDict != null && predictionDict.TryGetValue("bytesBase64Encoded", out var imageData))
            {
                images.Add(new GeneratedImageModel
                {
                    Base64Data = imageData.GetString()
                });
            }
        }

        return images;
    }

    // Edit and Variation operations will use the base class implementation
    // which throws NotSupportedException since they're not in SupportedOperations
}
