# CI/CD Setup Guide

This document describes the Continuous Integration and Continuous Deployment setup for the AiGeekSquad.ImageGenerator project.

## Overview

The project uses GitHub Actions for automated build, test, code analysis, and deployment workflows.

## Workflows

### Build and Deploy Workflow

**File**: `.github/workflows/build-and-deploy.yml`

**Triggers**:
- Push to `main` branch
- Pull requests to `main` branch

**Steps**:
1. **Checkout Code**: Fetches the repository with full history (needed for SonarQube analysis)
2. **Setup Environment**:
   - JDK 17 (required for SonarQube scanner)
   - .NET 9.0
3. **Cache Management**:
   - SonarQube Cloud packages cache
   - SonarQube scanner cache
4. **Version Calculation**: Auto-increments version based on GitHub run number (1.0.{run_number})
5. **Restore Dependencies**: `dotnet restore`
6. **Update Project Versions**: Automatically updates version in all packable projects
7. **Build, Test, and Analyze**:
   - Runs SonarQube scanner
   - Builds solution in Release configuration
   - Runs all tests with code coverage collection (OpenCover format)
   - Uploads results to SonarQube Cloud
8. **Create NuGet Packages**: Packs both tool and core library with symbols
9. **Upload Artifacts**: Stores packages for 30 days
10. **Publish to NuGet.org**: Automatically publishes on main branch pushes (requires `NUGET_API_KEY` secret)

## SonarQube Cloud Integration

### Setup

1. **Create SonarQube Cloud Project**:
   - Go to [SonarCloud.io](https://sonarcloud.io)
   - Import your GitHub repository
   - Organization: `aigeeksquad`
   - Project key: `AIGeekSquad_image-generator`

2. **Configure GitHub Secret**:
   - Go to GitHub repository settings → Secrets and variables → Actions
   - Add new secret: `SONAR_TOKEN`
   - Value: Your SonarQube Cloud token (get from SonarCloud.io → My Account → Security)

3. **Coverage Exclusions**:
   - Test files: `**/*Tests.cs`
   - Acceptance criteria tests: `**/AcceptanceCriteria/**/*`

### SonarQube Analysis

The workflow runs the following SonarQube analysis:
```powershell
dotnet-sonarscanner begin /k:"AIGeekSquad_image-generator" /o:"aigeeksquad" \
  /d:sonar.token="$SONAR_TOKEN" \
  /d:sonar.host.url="https://sonarcloud.io" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
  /d:sonar.coverage.exclusions="**/*Tests.cs,**/AcceptanceCriteria/**/*"

dotnet build --configuration Release
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults/ \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura%2Copencover

dotnet-sonarscanner end /d:sonar.token="$SONAR_TOKEN"
```

### Viewing Results

View code quality metrics at: `https://sonarcloud.io/project/overview?id=AIGeekSquad_image-generator`

## NuGet Publishing

### Setup

1. **Get NuGet API Key**:
   - Go to [NuGet.org](https://www.nuget.org/)
   - Sign in → API Keys
   - Create new API key with "Push new packages and package versions" scope
   - Select packages: `AiGeekSquad.ImageGenerator`, `AiGeekSquad.ImageGenerator.Core`

2. **Configure GitHub Secret**:
   - Go to GitHub repository settings → Secrets and variables → Actions
   - Add new secret: `NUGET_API_KEY`
   - Value: Your NuGet.org API key

### Automatic Publishing

Packages are automatically published to NuGet.org when:
- Code is pushed to the `main` branch
- All tests pass
- SonarQube analysis completes

Published packages:
- `AiGeekSquad.ImageGenerator` (global .NET tool)
- `AiGeekSquad.ImageGenerator.Core` (extensibility library)

### Manual Publishing

If needed, you can manually publish:

```bash
# Build and pack
dotnet pack AiGeekSquad.ImageGenerator.slnx --configuration Release --output packages

# Publish tool package
dotnet nuget push packages/AiGeekSquad.ImageGenerator.1.0.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate

# Publish core library package
dotnet nuget push packages/AiGeekSquad.ImageGenerator.Core.1.0.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate
```

## Version Management

Versions are automatically calculated using the formula:
```
version = 1.0.{GITHUB_RUN_NUMBER}
```

This ensures:
- Every build has a unique version
- Versions increment monotonically
- Easy tracking of which CI/CD run produced a package

The version is applied to:
- `<Version>` - Package version
- `<PackageVersion>` - NuGet package version
- `<AssemblyVersion>` - Assembly version
- `<FileVersion>` - File version
- `<InformationalVersion>` - Informational version

## Testing

### Test Execution

Tests are executed with:
```bash
dotnet test --configuration Release --verbosity normal \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/ \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura%2Copencover
```

### Coverage Collection

Code coverage is collected in two formats:
- **Cobertura**: General coverage reporting
- **OpenCover**: SonarQube analysis

Coverage results are uploaded to SonarQube Cloud for tracking over time.

### Test Categories

- **Unit Tests**: 30+ tests covering core functionality
- **Acceptance Tests**: 8 tests validating acceptance criteria
- **Integration Tests**: 8 E2E tests (skipped in CI, require API keys)

## Artifacts

Build artifacts are uploaded to GitHub Actions with a 30-day retention:
- All `.nupkg` files (including symbols packages)
- Available in the workflow run summary

## Local Development

### Running CI/CD Checks Locally

```bash
# Restore dependencies
dotnet restore AiGeekSquad.ImageGenerator.slnx

# Build
dotnet build AiGeekSquad.ImageGenerator.slnx --configuration Release

# Run tests
dotnet test AiGeekSquad.ImageGenerator.slnx --configuration Release

# Create packages
dotnet pack AiGeekSquad.ImageGenerator.slnx --configuration Release --output packages
```

### Running SonarQube Analysis Locally

1. Install SonarQube scanner:
   ```bash
   dotnet tool install --global dotnet-sonarscanner
   ```

2. Run analysis:
   ```bash
   dotnet sonarscanner begin /k:"AIGeekSquad_image-generator" /o:"aigeeksquad" \
     /d:sonar.token="YOUR_SONAR_TOKEN" \
     /d:sonar.host.url="https://sonarcloud.io"
   
   dotnet build AiGeekSquad.ImageGenerator.slnx --configuration Release
   dotnet test AiGeekSquad.ImageGenerator.slnx --configuration Release
   
   dotnet sonarscanner end /d:sonar.token="YOUR_SONAR_TOKEN"
   ```

## Troubleshooting

### SonarQube Analysis Fails

- Verify `SONAR_TOKEN` secret is set correctly
- Check SonarQube Cloud project exists with correct key
- Ensure JDK 17 is available in the workflow

### NuGet Publishing Fails

- Verify `NUGET_API_KEY` secret is set correctly
- Check API key has correct permissions (Push)
- Ensure package names match your NuGet.org account permissions

### Tests Fail

- Check test output in GitHub Actions logs
- Integration tests (E2E) should be skipped automatically (require API keys)
- Verify xUnit v3 is being used correctly

### Version Conflicts

- Each GitHub Actions run gets a unique version number
- If publishing fails due to version conflict, re-run the workflow to get a new version

## Best Practices

1. **Always run tests locally** before pushing
2. **Review SonarQube reports** after each PR
3. **Monitor code coverage** trends
4. **Keep dependencies updated** using Dependabot
5. **Tag releases** in GitHub for major versions
6. **Document breaking changes** in release notes

## Future Enhancements

- [ ] Add release notes generation
- [ ] Implement semantic versioning based on commit messages
- [ ] Add deployment to preview NuGet feed for PRs
- [ ] Set up automated dependency updates
- [ ] Add performance benchmarks tracking
