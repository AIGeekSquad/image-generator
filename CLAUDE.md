# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an AI-powered image generation tool built as a .NET MCP (Model Context Protocol) server. It provides a unified interface for multiple AI image generation providers (OpenAI DALL-E, Google Imagen) with extensible architecture for adding new providers.

**Critical MCP Server Constraints:**
- Uses `ModelContextProtocol` package v0.4.0-preview.3 (preview version)
- Tool command name: `aigeeksquad-imagegen` when installed as dotnet tool
- **All logging MUST go to stderr** - stdout is reserved for MCP protocol messages
- Use `AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace)` pattern

## Development Commands

### Building
```bash
# Build all projects (uses AiGeekSquad.ImageGenerator.slnx solution file)
dotnet build

# Build specific projects
dotnet build src/AiGeekSquad.ImageGenerator.Core/AiGeekSquad.ImageGenerator.Core.csproj
dotnet build src/AiGeekSquad.ImageGenerator.Tool/AiGeekSquad.ImageGenerator.Tool.csproj

# Package core library (NuGet package)
dotnet pack src/AiGeekSquad.ImageGenerator.Core/

# Package tool (dotnet tool)
dotnet pack src/AiGeekSquad.ImageGenerator.Tool/
dotnet tool install -g --add-source ./artifacts AiGeekSquad.ImageGenerator
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test projects
dotnet test tests/AiGeekSquad.ImageGenerator.Tests/AiGeekSquad.ImageGenerator.Tests.csproj  # Integration tests
dotnet test tests/AiGeekSquad.ImageGenerator.Tests.Unit/AiGeekSquad.ImageGenerator.Tests.Unit.csproj  # Unit tests
dotnet test tests/AiGeekSquad.ImageGenerator.Tests.E2E/AiGeekSquad.ImageGenerator.Tests.E2E.csproj  # E2E tests

# Run specific test method or class
dotnet test --filter "MethodName"
dotnet test --filter "ClassName"
```

### Running Locally
```bash
# Run the MCP tool locally
cd src/AiGeekSquad.ImageGenerator.Tool
dotnet run

# Install as global tool for testing
dotnet pack src/AiGeekSquad.ImageGenerator.Tool/AiGeekSquad.ImageGenerator.Tool.csproj --configuration Release
dotnet tool install --global --add-source ./nupkg AiGeekSquad.ImageGenerator
```

### Packaging
```bash
# Create NuGet packages
dotnet pack --configuration Release

# Package specific project
dotnet pack src/AiGeekSquad.ImageGenerator.Core/AiGeekSquad.ImageGenerator.Core.csproj --configuration Release
```

## Architecture

### Core Components

**AiGeekSquad.ImageGenerator.Core** - The foundational library containing:
- `IImageGenerationProvider` - Main interface that all providers must implement
- `ImageProviderBase` - Abstract base class with common functionality for providers
- `IImageGenerationService` - Service for managing multiple providers
- `IProviderFactory` - Factory interface for creating providers with dependency injection
- `IProviderRegistry` - Registry for managing and discovering provider factories
- `IProviderLoader` - Interface for loading external providers from assemblies
- Provider implementations: `OpenAIImageProvider`, `GoogleImageProvider`
- Model classes for requests/responses in the `Models` namespace
- Extensibility framework in the `Extensibility` namespace

**AiGeekSquad.ImageGenerator.Tool** - MCP server implementation:
- `Program.cs` - Entry point with dependency injection setup and provider registration
- `ImageGenerationTools` - MCP tool implementations for the four main operations
- Handles configuration from environment variables and appsettings.json

**Test Projects** - Multiple test projects with different scopes:
- `AiGeekSquad.ImageGenerator.Tests` - Integration tests (component interactions, may use TestHttpClient)
- `AiGeekSquad.ImageGenerator.Tests.Unit` - Unit tests (fast, isolated, mockable, no external dependencies)
- `AiGeekSquad.ImageGenerator.Tests.E2E` - End-to-end tests (full MCP server lifecycle, may require API keys)

### Provider Architecture

The system uses a sophisticated provider pattern with multiple layers for extensibility:

1. **Provider Interface**: `IImageGenerationProvider` defines the contract for all providers
2. **Base Class**: `ImageProviderBase` provides common functionality and utilities
3. **Provider Factory**: `IProviderFactory` creates providers with dependency injection support
4. **Provider Registry**: `IProviderRegistry` manages discovery and selection of available providers
5. **Provider Loader**: `IProviderLoader` enables loading external providers from assemblies
6. **Provider Metadata**: `ProviderMetadata` and `ProviderRequirements` describe capabilities and dependencies

Each provider supports different `ImageOperation` types:
- `Generate` - Create images from text prompts
- `GenerateFromConversation` - Multi-modal generation with conversation context
- `Edit` - Modify existing images (DALL-E 2 only)
- `Variation` - Create variations of existing images (DALL-E 2 only)

### Configuration

The tool supports configuration via:
- Environment variables: `OPENAI_API_KEY`, `GOOGLE_PROJECT_ID`, `OPENAI_ENDPOINT`
- `appsettings.json` with `OpenAI` and `Google` sections
- Command-line arguments including `--provider-assembly=path` for external providers
- External providers via `ExternalProviders:Assemblies` config section

**Configuration Requirements:**
- At least one provider must be configured: `OPENAI_API_KEY` OR `GOOGLE_PROJECT_ID`
- Google provider defaults to `us-central1` location if not specified
- OpenAI supports custom endpoints (not just api.openai.com) via `OpenAI:Endpoint`

### Key Patterns

- **Provider Factory Pattern**: Providers are created via factories with dependency injection and metadata
- **Registry Pattern**: Central registry manages provider discovery, selection, and filtering by capabilities
- **Extensibility Pattern**: External providers can be loaded dynamically from assemblies
- **Capabilities System**: Each provider declares supported operations, models, and requirements via `ProviderCapabilities` and `ProviderMetadata`
- **Request/Response Models**: Unified model classes in `Core.Models` namespace abstract provider differences
- **Dual Message Format**: Supports both `Microsoft.Extensions.AI.ChatMessage` and custom `ConversationMessage` with automatic conversion via `ImageProviderBase.ConvertConversationToChatMessages()`
- **HttpClient Integration**: Base provider class includes HTTP client for image downloading, supports named clients ("OpenAI", "Google")
- **MCP Protocol**: Tool communicates via stdin/stdout using Model Context Protocol

## Testing Strategy

Tests are organized across multiple projects with different scopes:

**Unit Tests** (`Tests.Unit` project):
- Fast, isolated tests for individual components
- Uses Moq for dependency mocking
- Focuses on business logic and individual classes

**Integration Tests** (`Tests` project):
- Tests component interactions and provider implementations
- Includes acceptance criteria tests in `AcceptanceCriteria` folder
- Uses real or mocked external services
- Includes `SixLabors.ImageSharp` for image processing validation

**End-to-End Tests** (`Tests.E2E` project):
- Full MCP protocol testing
- Tests complete request/response cycles
- Validates MCP tool implementations

All test projects use:
- **xUnit v3** (version 3.1.0) for test framework
- **FluentAssertions** (version 7.2.0) for readable assertions
- **Moq** for mocking dependencies

**Critical Testing Patterns:**
- **CancellationToken Usage**: Use `TestContext.Current.CancellationToken` instead of `CancellationToken.None` in tests
- **FluentAssertions AssertionScope**: Only use `AssertionScope` for multiple assertions (3+ recommended), not single assertions
- **Base64 Data URLs**: Must have `data:image` prefix for proper parsing in tests

Test files follow naming convention: `{ComponentName}Tests.cs`

## Adding New Providers

The provider system supports two approaches for adding new providers:

### Built-in Providers (Recommended for Core Providers)

1. Create a class inheriting from `ImageProviderBase`
2. Implement abstract members: `ProviderName`, `Capabilities`, and required async methods
3. Create a factory implementing `IProviderFactory` with metadata and dependency requirements
4. Register the factory in `Program.cs` dependency injection setup
5. Add configuration support for the provider's credentials/settings
6. Create comprehensive tests in appropriate test projects (unit, integration, e2e)

### External Providers (For Third-Party Extensions)

1. Create a separate assembly with provider implementation
2. Implement `IImageGenerationProvider` (or inherit from `ImageProviderBase`)
3. **Critical**: Use parameterless constructor - providers with DI dependencies will fail silently
4. Use `IProviderLoader.LoadProvidersFromAssembly()` to load at runtime
5. Load via `--provider-assembly=path` CLI args or `ExternalProviders:Assemblies` config
6. Provider metadata should declare dependencies and requirements via `ProviderMetadata`

### Key Considerations

- **Provider Registration Pattern**: Built-ins registered as singletons with factory delegates, externals as `IEnumerable<IImageGenerationProvider>`
- **HttpClient Pattern**: Providers should accept `HttpClient?` in constructor for testability, fall back to `SharedHttpClient`
- Use `ProviderMetadata` to declare capabilities, requirements, and priority
- Implement `ProviderRequirements` to specify configuration dependencies
- The registry system enables automatic provider discovery and selection
- Providers can declare `AcceptsCustomModels = true` for forward compatibility with new models
- External providers are loaded dynamically and must handle their own dependency resolution
- Use `ImageProviderBase.SupportsOperation()` to declare capabilities and get automatic conversational → standard request fallback

## Solution Structure

**Solution File Management:**
- Uses `AiGeekSquad.ImageGenerator.slnx` (Visual Studio slnx format)
- Use `dotnet sln add <project-path>` to add new projects to solution
- Always verify `dotnet build` passes after adding new projects

**Package Dependencies:**
- Use `ModelContextProtocol` (NOT `ModelContextProtocol.Server` - doesn't exist)
- E2E tests require: `ModelContextProtocol`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection`
- Consistent test versions: xunit.v3 3.1.0, FluentAssertions 7.2.0