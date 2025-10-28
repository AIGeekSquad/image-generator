# AiGeekSquad.ImageGenerator.Core

Core library for creating extensible image generation providers for the AiGeekSquad Image Generator MCP tool.

## Overview

This package provides the abstractions and base implementations needed to create custom image generation providers that integrate with the AiGeekSquad Image Generator tool.

## Installing

```bash
dotnet add package AiGeekSquad.ImageGenerator.Core
```

## Creating a Custom Provider

### Step 1: Reference the Core Package

Add the package to your project:

```xml
<PackageReference Include="AiGeekSquad.ImageGenerator.Core" Version="1.0.0" />
```

### Step 2: Implement Your Provider

Create a class that inherits from `ImageProviderBase`:

```csharp
using AiGeekSquad.ImageGenerator.Core.Abstractions;
using AiGeekSquad.ImageGenerator.Core.Models;
using AiGeekSquad.ImageGenerator.Core.Providers;

namespace MyCompany.ImageProviders;

public class MyCustomImageProvider : ImageProviderBase
{
    private readonly string _apiKey;

    public MyCustomImageProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    public override string ProviderName => "MyCustomProvider";

    protected override ProviderCapabilities Capabilities { get; } = new()
    {
        ExampleModels = new List<string> 
        { 
            "my-model-v1", 
            "my-model-v2" 
        },
        SupportedOperations = new List<ImageOperation> 
        { 
            ImageOperation.Generate 
        },
        DefaultModel = "my-model-v1",
        AcceptsCustomModels = true,
        Features = new Dictionary<string, object>
        {
            ["maxResolution"] = "2048x2048",
            ["supportsBatch"] = true
        }
    };

    public override async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = GetModelOrDefault(request.Model);
        
        // Your implementation here
        // Call your AI service API, process the request, etc.
        
        return new ImageGenerationResponse
        {
            Images = new List<GeneratedImage>
            {
                new GeneratedImage
                {
                    Url = "https://example.com/generated-image.png",
                    // or Base64Data = "..."
                }
            },
            Model = model,
            Provider = ProviderName,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Step 3: Package Your Provider

Create a class library project and package it:

```bash
dotnet pack --configuration Release
```

### Step 4: Use Your Provider

Users can load your provider using the `--provider-assembly` option:

```bash
aigeeksquad-imagegen --provider-assembly path/to/MyCustomProvider.dll
```

## Key Abstractions

### IImageGenerationProvider

The main interface for image generation providers. Provides methods for:
- `GenerateImageAsync` - Generate images from text prompts
- `EditImageAsync` - Edit existing images
- `CreateVariationAsync` - Create variations of images
- `SupportsOperation` - Check if an operation is supported
- `GetCapabilities` - Get provider capabilities

### ImageProviderBase

Abstract base class that implements `IImageGenerationProvider` with common functionality:
- Automatic operation support checking
- Model resolution with defaults
- Helper methods for image conversion

### ProviderCapabilities

Metadata about provider capabilities:
- `ExampleModels` - List of commonly used models
- `SupportedOperations` - Operations this provider supports
- `DefaultModel` - Default model to use
- `AcceptsCustomModels` - Whether custom model strings are accepted
- `Features` - Dictionary of provider-specific features

### Models

Request and response models for image operations:
- `ImageGenerationRequest` - Request to generate images
- `ImageEditRequest` - Request to edit images
- `ImageVariationRequest` - Request to create variations
- `ImageGenerationResponse` - Response with generated images
- `GeneratedImage` - Individual generated image data

## Advanced: Parameterless Constructor

For automatic loading via assembly scanning, your provider should have a parameterless constructor. You can use configuration to initialize:

```csharp
public class MyCustomImageProvider : ImageProviderBase
{
    private readonly string _apiKey;

    // Parameterless constructor for assembly loading
    public MyCustomImageProvider()
    {
        // Load from environment or config
        _apiKey = Environment.GetEnvironmentVariable("MY_PROVIDER_API_KEY") 
            ?? throw new InvalidOperationException("MY_PROVIDER_API_KEY not set");
    }

    // Constructor for explicit instantiation
    public MyCustomImageProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    // ... rest of implementation
}
```

## Built-in Providers

The core package includes two built-in providers as reference implementations:

### OpenAIImageProvider
- Supports DALL-E 2, DALL-E 3, GPT Image 1
- Implements all operations (Generate, Edit, Variation)
- Supports quality and style parameters

### GoogleImageProvider  
- Supports Imagen 2 and Imagen 3
- Supports image generation
- Integrates with Google Cloud Vertex AI

## Example: Complete Custom Provider

See the [sample providers](https://github.com/AIGeekSquad/image-generator/tree/main/samples) directory for complete examples.

## Documentation

For full documentation, visit: https://github.com/AIGeekSquad/image-generator

## License

MIT License
