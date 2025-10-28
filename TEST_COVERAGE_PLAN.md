# Test Coverage Improvement Plan

## Current State (10% Overall Coverage)

### What's Already Covered (100%)
- **Service Layer (ImageGenerationService)**: 12 tests
  - Provider discovery and routing
  - All 4 operations (Generate, GenerateFromConversation, Edit, Variation)
  - Error handling and validation

### What's Not Covered (0%)
1. **Provider Implementations** (OpenAIImageProvider, GoogleImageProvider)
2. **SDK Adapters** (OpenAIAdapter, GoogleImageAdapter)
3. **MCP Tool Layer** (ImageGenerationTools)
4. **Helper Methods** (ImageProviderBase protected methods)

## Target: 40% Overall Coverage

### Phase 1: Provider Business Logic Tests (Target: +20%)
Test provider orchestration logic using mocked adapters:

**OpenAIImageProvider (8 tests)**
- ✅ Provider name and capabilities
- ✅ GenerateImageAsync with different response formats
- ✅ EditImageAsync workflow
- ✅ CreateVariationAsync workflow
- ✅ Conversation support fallback
- ✅ Model parameter passing
- ✅ Error propagation

**GoogleImageProvider (6 tests)**
- ✅ Provider name and capabilities
- ✅ GenerateImageAsync with byte[] responses
- ✅ Multi-image generation
- ✅ Unsupported operations (Edit, Variation, Conversation)
- ✅ Model parameter passing

**ImageProviderBase (8 tests)**
- ✅ Operation support checks
- ✅ Response building helpers
- ✅ Message text extraction
- ✅ Base64/URL handling

**Total: 22 new tests**

### Phase 2: MCP Tool Layer Tests (Target: +10%)
Test MCP tool request/response serialization:

**ImageGenerationTools (10 tests)**
- ✅ ListProviders returns all registered providers
- ✅ GenerateImage with valid prompt
- ✅ GenerateImage with invalid provider
- ✅ GenerateImageFromConversation with multi-modal input
- ✅ EditImage with valid parameters
- ✅ CreateVariation with valid parameters
- ✅ Error response formatting
- ✅ Model parameter propagation
- ✅ Parameter validation

### Phase 3: Adapter Tests (Optional: +5%)
Only if adapters contain meaningful business logic:

**OpenAIAdapter (4 tests)**
- Only test parameter transformation, not SDK calls

**GoogleImageAdapter (4 tests)**
- Only test parameter transformation, not SDK calls

## Why Some Code Remains Untested

### SDK Wrapper Code (Not Tested)
- OpenAIAdapter SDK calls
- GoogleImageAdapter SDK calls
- Rationale: Testing would mock the entire SDK, validating nothing

### Integration Code (E2E Tests Only)
- Actual API communication
- Real image generation/editing/variation
- Multi-provider scenarios
- Rationale: Requires live credentials, tested via 8 E2E tests

### Thin Layers (Not Worth Testing)
- MCP JSON serialization (no logic)
- Simple property getters/setters
- Pass-through methods

## Expected Final Coverage

| Component | Tests | Coverage | Rationale |
|-----------|-------|----------|-----------|
| Service Layer | 12 | 100% | All business logic |
| Providers | 22 | 60% | Business logic only |
| MCP Tools | 10 | 70% | Request/response handling |
| Adapters | 0-8 | 0-20% | Thin wrappers |
| E2E | 8 | N/A | Integration validation |
| **Total** | **52-60** | **40-50%** | **All testable logic** |

## Success Criteria

✅ All business logic paths tested  
✅ All orchestration and routing tested  
✅ All error handling tested  
✅ All parameter validation tested  
✅ Integration scenarios validated via E2E  
✅ No untested decision points in service layer  
✅ Mocks used only for interfaces, not external SDKs  

The 40-50% target reflects an architecture where critical business logic is 100% covered, while integration code (adapters, SDK calls) is appropriately tested through E2E tests rather than unit tests with heavy mocking.
