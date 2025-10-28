# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## MCP Server Architecture

This is an MCP (Model Context Protocol) server, not a typical CLI tool:
- Uses `ModelContextProtocol` package v0.4.0-preview.3 (preview version)
- Tool command name is `aigeeksquad-imagegen` when installed as dotnet tool
- **Critical**: All logging goes to stderr, stdout is reserved for MCP protocol messages
- Use `AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace)` pattern

## Provider Loading Gotchas

**External Provider Loading:**
- External providers loaded via `--provider-assembly=path` CLI args or `ExternalProviders:Assemblies` config
- `AssemblyProviderLoader` requires parameterless constructors - providers with DI dependencies will fail silently
- Built-in providers are conditionally registered only if API keys/project IDs are present

**Provider Registration Pattern:**
```csharp
// Built-ins registered as singletons with factory delegates
builder.Services.AddSingleton<IImageGenerationProvider>(sp => /* factory */);
// External providers registered as IEnumerable<IImageGenerationProvider>
```

## Request/Response Format Duality

**Two Message Formats Coexist:**
- `Microsoft.Extensions.AI.ChatMessage` (for standard requests)
- Custom `ConversationMessage` (for conversational requests with image support)
- `ImageProviderBase.ConvertConversationToChatMessages()` provides automatic conversion
- Base64 data URLs must have `data:image` prefix for proper parsing

## Configuration Requirements

**Provider Configuration:**
- Requires at least one provider: `OPENAI_API_KEY` OR `GOOGLE_PROJECT_ID`
- Google provider defaults to `us-central1` location if not specified
- OpenAI supports custom endpoints via `OpenAI:Endpoint` config (not just api.openai.com)

## Testing Framework Versions

**Uses Preview/Alpha Packages:**
- `xunit.v3` version `1.0.0` (v3, not stable v2)
- `FluentAssertions` version `7.0.0-alpha.4` (alpha version)
- Use `AssertionScope` pattern for multiple assertions in single test

**xUnit v3 Specific Patterns:**
- **CancellationToken Usage (xUnit1051)**: Tests calling methods that accept `CancellationToken` MUST use `TestContext.Current.CancellationToken` instead of `CancellationToken.None` for responsive test cancellation. See: https://xunit.net/xunit.analyzers/rules/xUnit1051
- **Async Test Methods (CS1998)**: xUnit v3 allows async test methods without await operators, but consider if the test actually needs to be async or can be synchronous

## Build Commands

**Core Library (NuGet package):**
```bash
dotnet pack src/AiGeekSquad.ImageGenerator.Core/
```

**Tool (dotnet tool):**
```bash
dotnet pack src/AiGeekSquad.ImageGenerator.Tool/
dotnet tool install -g --add-source ./artifacts AiGeekSquad.ImageGenerator
```

## Extensibility Pattern

**Custom Provider Development:**
- Inherit from `ImageProviderBase` for common functionality
- Implement parameterless constructor for assembly loading
- Use `SupportsOperation()` to declare capabilities
- `ImageProviderBase` provides automatic conversational → standard request fallback

## HttpClient Usage

**Shared vs Injected Pattern:**
- `ImageProviderBase` uses static `SharedHttpClient` if none provided
- Providers should accept `HttpClient?` in constructor for testability
- Named HttpClient registration: `"OpenAI"`, `"Google"` for provider-specific configuration