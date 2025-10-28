# Implementation TODO List

## Phase 1: Core Refactoring (Priority: High)

### 1.1 Provider Factory Pattern
- [ ] Create `IProviderFactory` interface in Core/Abstractions
- [ ] Create `ProviderMetadata` class with capabilities, requirements
- [ ] Implement `OpenAIProviderFactory` with DI support
- [ ] Implement `GoogleProviderFactory` with DI support
- [ ] Create `ExternalProviderFactory` for assembly-loaded providers
- [ ] Add factory registration in Program.cs

### 1.2 Provider Registry
- [ ] Create `IProviderRegistry` interface
- [ ] Implement `ProviderRegistry` with factory-based creation
- [ ] Add provider discovery mechanism
- [ ] Implement provider health check interface
- [ ] Add provider availability tracking

### 1.3 Remove Static Dependencies
- [ ] Remove static `SharedHttpClient` from `ImageProviderBase`
- [ ] Inject `IHttpClientFactory` into providers
- [ ] Create named HttpClient configurations
- [ ] Update all provider constructors

### 1.4 Unified Request/Response Models
- [ ] Create `UnifiedImageRequest` model
- [ ] Create `UnifiedImageResponse` model
- [ ] Create `IRequestAdapter<TFrom, TTo>` interface
- [ ] Implement `ChatMessageAdapter`
- [ ] Implement `ConversationMessageAdapter`
- [ ] Update `ImageProviderBase` to use unified models

## Phase 2: Provider Selection & MCP Improvements

### 2.1 Provider Selection Strategy
- [ ] Create `IProviderSelectionStrategy` interface
- [ ] Create `ProviderSelectionContext` class
- [ ] Implement `ExplicitProviderSelector` (by name)
- [ ] Implement `ModelBasedProviderSelector` (by model capability)
- [ ] Implement `CapabilityBasedProviderSelector` (by operation)
- [ ] Implement `SmartProviderSelector` (composite strategy)
- [ ] Add fallback chain support

### 2.2 Argument Parser
- [ ] Create `IArgumentParser` interface
- [ ] Create `ParsedArguments` class
- [ ] Create `ValidationResult` class
- [ ] Implement `McpArgumentParser` with type conversion
- [ ] Add size parsing (e.g., "1024x1024")
- [ ] Add JSON conversation parsing
- [ ] Add quality/style enum parsing
- [ ] Implement comprehensive validation logic

### 2.3 Enhanced MCP Tools
- [ ] Refactor `ImageGenerationTools` to use argument parser
- [ ] Add provider selection strategy integration
- [ ] Improve error response formatting
- [ ] Add request context tracking
- [ ] Implement retry logic with fallback
- [ ] Add telemetry hooks

## Phase 3: Comprehensive Testing

### 3.1 Test Infrastructure Setup
- [ ] Create new test project structure (Unit/Integration/E2E/Shared)
- [ ] Set up xUnit v3 test categories
- [ ] Create `McpServerFixture` for E2E tests
- [ ] Create `StdoutCapture` and `StderrCapture` utilities
- [ ] Set up test data builders
- [ ] Create provider test doubles/fakes

### 3.2 Unit Tests - Argument Parsing
- [ ] Test `McpArgumentParser.Parse` with valid arguments
- [ ] Test parsing with missing optional parameters
- [ ] Test parsing with invalid types
- [ ] Test size string parsing (valid formats)
- [ ] Test size string parsing (invalid formats)
- [ ] Test JSON conversation parsing
- [ ] Test validation rules for required fields
- [ ] Test validation for value ranges

### 3.3 Unit Tests - Provider Selection
- [ ] Test explicit provider selection
- [ ] Test model-based selection
- [ ] Test capability-based selection
- [ ] Test fallback chain behavior
- [ ] Test provider unavailability handling
- [ ] Test load balancing logic

### 3.4 E2E Tests - MCP Tools
- [ ] Test `generate_image` with all parameters
- [ ] Test `generate_image` with minimal parameters
- [ ] Test `generate_image` error handling
- [ ] Test `generate_image_from_conversation` with valid JSON
- [ ] Test `generate_image_from_conversation` with invalid JSON
- [ ] Test `edit_image` with base64 images
- [ ] Test `edit_image` with URL images
- [ ] Test `create_variation` functionality
- [ ] Test `list_providers` response format

### 3.5 E2E Tests - MCP Protocol
- [ ] Test JSON-RPC request/response format
- [ ] Test stderr logging (not stdout)
- [ ] Test concurrent request handling
- [ ] Test request timeout handling
- [ ] Test malformed request handling
- [ ] Test large payload handling

### 3.6 E2E Tests - Provider Scenarios
- [ ] Test provider fallback on failure
- [ ] Test multi-provider round-robin
- [ ] Test provider-specific model routing
- [ ] Test external provider loading
- [ ] Test provider hot-reload

### 3.7 Integration Tests
- [ ] Test OpenAI provider with mock adapter
- [ ] Test Google provider with mock adapter
- [ ] Test provider registry with real factories
- [ ] Test service layer integration
- [ ] Test configuration loading

## Phase 4: Performance & Observability

### 4.1 Telemetry Implementation
- [ ] Add OpenTelemetry packages
- [ ] Create `TelemetryProvider` decorator
- [ ] Add activity tracking to MCP tools
- [ ] Add metrics collection (success/failure rates)
- [ ] Add latency tracking
- [ ] Add provider usage metrics

### 4.2 Performance Optimization
- [ ] Implement provider connection pooling
- [ ] Add response caching layer
- [ ] Optimize request serialization
- [ ] Add batch request support
- [ ] Profile and optimize hot paths

### 4.3 Error Handling & Resilience
- [ ] Implement circuit breaker pattern
- [ ] Add retry policies with exponential backoff
- [ ] Improve error messages for users
- [ ] Add detailed error logging
- [ ] Implement graceful degradation

## Phase 5: Documentation & Polish

### 5.1 Documentation Updates
- [ ] Update README with new architecture
- [ ] Document provider factory pattern
- [ ] Document MCP tool parameters
- [ ] Create provider development guide
- [ ] Add E2E testing guide
- [ ] Update AGENTS.md with new patterns

### 5.2 Code Quality
- [ ] Run code analysis and fix issues
- [ ] Ensure XML documentation on public APIs
- [ ] Add nullable reference type annotations
- [ ] Review and optimize async/await usage
- [ ] Ensure proper disposal patterns

### 5.3 Final Validation
- [ ] Run full test suite
- [ ] Verify all E2E scenarios pass
- [ ] Performance benchmarking
- [ ] Load testing with concurrent requests
- [ ] Security review (API key handling)

## Implementation Order

### Week 1: Foundation
1. Provider Factory Pattern (1.1)
2. Remove Static Dependencies (1.3)
3. Provider Registry (1.2)
4. Unified Models (1.4)

### Week 2: MCP & Selection
1. Argument Parser (2.2)
2. Provider Selection Strategy (2.1)
3. Enhanced MCP Tools (2.3)

### Week 3: Testing
1. Test Infrastructure (3.1)
2. Unit Tests (3.2, 3.3)
3. E2E MCP Tests (3.4, 3.5)
4. Integration Tests (3.7)

### Week 4: Polish
1. Telemetry (4.1)
2. Performance (4.2)
3. Documentation (5.1)
4. Final Validation (5.3)

## Success Metrics

### Test Coverage
- [ ] Unit test coverage > 90%
- [ ] E2E test coverage for all MCP tools
- [ ] Integration test coverage for all providers

### Performance
- [ ] Request overhead < 100ms
- [ ] Concurrent request handling (10+ simultaneous)
- [ ] Memory usage stable under load

### Quality
- [ ] Zero critical code analysis warnings
- [ ] All public APIs documented
- [ ] Clean architecture principles followed

## Risk Items

### High Priority Issues to Address
1. **Parameterless constructor requirement** - Must be solved by factory pattern
2. **Static HttpClient** - Prevents proper testing
3. **No MCP E2E tests** - Critical for production readiness
4. **Hardcoded provider defaults** - Limits flexibility
5. **No argument validation** - Can cause runtime errors

### Technical Debt to Resolve
1. Mixed test types in same files
2. Adapter pattern inconsistently applied
3. Request/Response format duality
4. Tight coupling to concrete SDKs
5. No telemetry or observability

## Notes

- Each checkbox represents approximately 1-2 hours of work
- Items can be parallelized within phases
- E2E tests are critical - do not skip
- Performance targets are aspirational but important
- Documentation should be updated as we go, not at the end