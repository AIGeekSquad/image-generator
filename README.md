# AiGeekSquad.ImageGenerator

A .NET MCP (Model Context Protocol) tool for AI-powered image generation using various providers including OpenAI (DALL-E, GPT Image models) and Google (Imagen).

## Features

- 🎨 **Multiple Providers**: Support for OpenAI and Google AI platforms
- 🔧 **Extensible Architecture**: Easily add custom image generation providers
- 🎯 **Latest Models**: Support for DALL-E 3, GPT Image 1, GPT-5 Image (future), and Imagen 3
- 🔄 **Response API**: Iterative image manipulation with edits and variations
- 🛠️ **MCP Integration**: Works seamlessly with MCP-compatible AI assistants
- ⚡ **Modern C#**: Built with .NET 9.0 and latest C# features
- ✅ **Well Tested**: Comprehensive test coverage using xUnit
- 📦 **NuGet Package**: Easy installation as a global .NET tool

## Supported Operations

### Generate Images
Create images from text prompts using various AI models.

### Edit Images
Modify existing images based on text descriptions (OpenAI DALL-E 2).

### Create Variations
Generate variations of existing images (OpenAI DALL-E 2).

### List Providers
Get information about available providers and their capabilities.

## Supported Models

### OpenAI
- `dall-e-3` - Latest high-quality image generation
- `dall-e-2` - Previous generation (supports edits and variations)
- `gpt-image-1` - New GPT-based image generation
- `gpt-5-image` - Future GPT-5 image capabilities (placeholder)

### Google
- `imagen-3.0-generate-001` - Latest Imagen model
- `imagen-3.0-fast-generate-001` - Optimized for speed
- `imagegeneration@006` - Imagen 2

## Installation

### As a Global .NET Tool (Recommended)

```bash
dotnet tool install --global AiGeekSquad.ImageGenerator
```

### Update Existing Installation

```bash
dotnet tool update --global AiGeekSquad.ImageGenerator
```

### From Source

```bash
git clone https://github.com/AIGeekSquad/image-generator.git
cd image-generator
dotnet pack src/AiGeekSquad.ImageGenerator.Tool/AiGeekSquad.ImageGenerator.Tool.csproj --configuration Release
dotnet tool install --global --add-source ./nupkg AiGeekSquad.ImageGenerator
```

## Configuration

### Environment Variables

```bash
# OpenAI (required for OpenAI provider)
export OPENAI_API_KEY="your-openai-api-key"

# Optional: Azure OpenAI
export OPENAI_ENDPOINT="https://your-resource.openai.azure.com"

# Google (optional)
export GOOGLE_PROJECT_ID="your-google-cloud-project-id"
```

### appsettings.json

Alternatively, create an `appsettings.json` file in the tool's directory:

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Endpoint": "",
    "DefaultModel": "dall-e-3"
  },
  "Google": {
    "ProjectId": "your-google-cloud-project-id",
    "Location": "us-central1",
    "DefaultModel": "imagen-3.0-generate-001"
  }
}
```

## Usage

### As an MCP Server

Add to your MCP configuration (e.g., in GitHub Copilot or Claude Desktop):

```json
{
  "mcpServers": {
    "image-generator": {
      "command": "aigeeksquad-imagegen",
      "args": [],
      "env": {
        "OPENAI_API_KEY": "your-api-key"
      }
    }
  }
}
```

### Command Line

After installation, run the tool:

```bash
aigeeksquad-imagegen
```

The tool starts an MCP server that communicates via stdio.

### MCP Tools

#### generate_image
Generate an image from a text prompt.

**Parameters:**
- `prompt` (required): Text description of the image
- `provider`: Provider to use (default: "OpenAI")
- `model`: Model to use (e.g., "dall-e-3", "gpt-image-1")
- `size`: Image size (e.g., "1024x1024", "1792x1024")
- `quality`: Image quality ("standard" or "hd")
- `style`: Image style ("vivid" or "natural")
- `numberOfImages`: Number of images to generate (default: 1)

#### edit_image
Edit an existing image based on a prompt.

**Parameters:**
- `image` (required): Base64 encoded image or URL
- `prompt` (required): Description of desired changes
- `mask`: Optional mask image
- `provider`: Provider to use (default: "OpenAI")
- `model`: Model to use (default: "dall-e-2")
- `size`: Output image size
- `numberOfImages`: Number of images to generate (default: 1)

#### create_variation
Create variations of an existing image.

**Parameters:**
- `image` (required): Base64 encoded image or URL
- `provider`: Provider to use (default: "OpenAI")
- `model`: Model to use (default: "dall-e-2")
- `size`: Output image size
- `numberOfImages`: Number of variations (default: 1)

#### list_providers
List all available providers and their capabilities.

## Architecture

### Core Components

- **AiGeekSquad.ImageGenerator.Core**: Core library with abstractions and provider implementations
  - `IImageGenerationProvider`: Interface for image generation providers
  - `IImageGenerationService`: Service for managing multiple providers
  - `ImageProviderBase`: Base class for easy provider implementation
  - `ProviderCapabilities`: Metadata about provider capabilities

- **AiGeekSquad.ImageGenerator.Tool**: MCP server implementation
  - `ImageGenerationTools`: MCP tool implementations
  - `Program.cs`: Server configuration and startup

- **AiGeekSquad.ImageGenerator.Tests**: Comprehensive test suite using xUnit

### Extensibility

Adding a new provider is straightforward:

1. Create a class that inherits from `ImageProviderBase`
2. Implement the required abstract members
3. Register the provider in `Program.cs`

Example:

```csharp
public class CustomImageProvider : ImageProviderBase
{
    public override string ProviderName => "Custom";
    
    protected override ProviderCapabilities Capabilities { get; } = new()
    {
        ExampleModels = new List<string> { "custom-model-1" },
        SupportedOperations = new List<ImageOperation> { ImageOperation.Generate },
        DefaultModel = "custom-model-1",
        AcceptsCustomModels = true
    };

    public override async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

## Development

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Package

```bash
dotnet pack src/AiGeekSquad.ImageGenerator.Tool/AiGeekSquad.ImageGenerator.Tool.csproj --configuration Release --output ./nupkg
```

### Run Locally

```bash
cd src/AiGeekSquad.ImageGenerator.Tool
dotnet run
```

## Requirements

- .NET 9.0 SDK or later
- OpenAI API key (for OpenAI provider)
- Google Cloud project with Vertex AI enabled (for Google provider, optional)

## CI/CD and Code Quality

This project uses automated CI/CD with GitHub Actions for building, testing, and deploying to NuGet.org.

### Build and Deploy Workflow

The project automatically:
- ✅ Builds on every push and PR
- ✅ Runs all unit and integration tests
- ✅ Analyzes code with SonarQube Cloud
- ✅ Collects code coverage metrics
- ✅ Publishes to NuGet.org on main branch pushes

### SonarQube Integration

Code quality and security are continuously monitored using [SonarQube Cloud](https://sonarcloud.io/project/overview?id=AIGeekSquad_image-generator).

To set up SonarQube integration:
1. Create a project on [SonarCloud.io](https://sonarcloud.io)
2. Add `SONAR_TOKEN` secret to your GitHub repository
3. The workflow will automatically analyze code on each push/PR

### Required Secrets

Configure these secrets in your GitHub repository settings:
- `SONAR_TOKEN` - SonarQube Cloud authentication token
- `NUGET_API_KEY` - NuGet.org API key for publishing packages

### Manual Publishing to NuGet.org

```bash
# Pack the tool
dotnet pack src/AiGeekSquad.ImageGenerator.Tool/AiGeekSquad.ImageGenerator.Tool.csproj --configuration Release --output ./nupkg

# Publish to NuGet.org (requires API key)
dotnet nuget push ./nupkg/AiGeekSquad.ImageGenerator.1.0.0.nupkg --api-key YOUR_NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## Acknowledgments

- Built with [ModelContextProtocol SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- Uses [Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI)
- Integrates with [Azure OpenAI SDK](https://github.com/Azure/azure-sdk-for-net)
- Integrates with [Google Cloud AI Platform](https://cloud.google.com/vertex-ai)

