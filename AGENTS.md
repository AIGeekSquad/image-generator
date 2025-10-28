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

## 💎 CRITICAL: Respect User Time - Test Before Presenting

**The user's time is their most valuable resource.** When you present work as "ready" or "done", you must have:

1. **Tested it yourself thoroughly** - Don't make the user your QA
2. **Fixed obvious issues** - Syntax errors, import problems, broken logic
3. **Verified it actually works** - Run tests, check structure, validate logic
4. **Only then present it** - "This is ready for your review" means YOU'VE already validated it

**User's role:** Strategic decisions, design approval, business context, stakeholder judgment
**Your role:** Implementation, testing, debugging, fixing issues before engaging user

**Anti-pattern**: "I've implemented X, can you test it and let me know if it works?"
**Correct pattern**: "I've implemented and tested X. Tests pass, structure verified, logic validated. Ready for your review. Here is how you can verify."

**Remember**: Every time you ask the user to debug something you could have caught, you're wasting their time on non-stakeholder work. Be thorough BEFORE engaging them.

## Configuration Requirements

**Provider Configuration:**
- Requires at least one provider: `OPENAI_API_KEY` OR `GOOGLE_PROJECT_ID`
- Google provider defaults to `us-central1` location if not specified
- OpenAI supports custom endpoints via `OpenAI:Endpoint` config (not just api.openai.com)

## Testing Framework Versions

**Testing Framework Versions:**
- `xunit.v3` version `3.1.0`
- `FluentAssertions` version `7.2.0`

**FluentAssertions AssertionScope Pattern:**
- **Use AssertionScope ONLY for multiple assertions (3+ recommended)** - groups related assertions and reports all failures together
- **DO NOT use AssertionScope for single assertions** - adds unnecessary overhead and reduces readability
- **Correct usage example:**
```csharp
// Assert - Multiple related assertions
using var scope = new AssertionScope();
response.Should().NotBeNull();
response.Images.Should().NotBeEmpty();
response.Images[0].Url.Should().NotBeNullOrEmpty();
response.Model.Should().Be("expected-model");
response.Provider.Should().Be("ExpectedProvider");
```
- **Incorrect usage example:**
```csharp
// Assert - Single assertion (AssertionScope not needed)
using var scope = new AssertionScope();
provider.Should().NotBeNull(); // Just use: provider.Should().NotBeNull();
```

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

## Solution Structure and Test Organization

**Solution File Management:**
- Solution file: `AiGeekSquad.ImageGenerator.slnx` (Visual Studio slnx format)
- Use `dotnet sln add <project-path>` to add new projects to solution
- Always verify `dotnet build` passes after adding new projects

**Test Project Structure:**
- `tests/AiGeekSquad.ImageGenerator.Tests.Integration/` - Integration tests (component interactions)
- `tests/AiGeekSquad.ImageGenerator.Tests.Unit/` - Pure unit tests (mockable, no external dependencies)
- `tests/AiGeekSquad.ImageGenerator.Tests.E2E/` - End-to-end tests (MCP server testing)

**Package Reference Consistency:**
- Use `ModelContextProtocol` (NOT `ModelContextProtocol.Server`) - the latter doesn't exist
- All test projects use same versions: xunit.v3 3.1.0, FluentAssertions 7.2.0
- E2E tests require additional packages: `ModelContextProtocol`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection`

**Test Organization Rules:**
- Unit tests: Fast, isolated, mockable - no real HTTP calls, no external dependencies
- Integration tests: Test component interactions, may use TestHttpClient or test adapters
- E2E tests: Full MCP server lifecycle, real tool execution, may require API keys (skipped if not available)