# Azure Pipelines - Testing Examples

This folder contains complete Azure DevOps pipeline examples for running automated tests in the Azure Integration Platform testing strategy.

## Available Pipelines

### 1. [azure-pipelines-unit-tests.yml](azure-pipelines-unit-tests.yml)
**Purpose:** Execute Phase 1 unit tests with code coverage reporting

**Features:**
- Runs on every commit to `main` and `develop` branches
- Triggers only when source code or unit tests change
- Restores NuGet packages
- Executes all unit tests with code coverage
- Publishes test results and coverage reports
- Fails build if coverage < 80%

**Usage:**
```yaml
# Reference in your azure-pipelines.yml
resources:
  repositories:
    - repository: testing-strategy
      type: git
      name: testing-strategy

jobs:
  - template: samples/pipelines/azure-pipelines-unit-tests.yml@testing-strategy
```

### 2. [azure-pipelines-integration-tests.yml](azure-pipelines-integration-tests.yml)
**Purpose:** Execute Phase 2 integration tests against live Azure services or Azurite

**Features:**
- Runs on pull requests and scheduled builds
- Starts Azurite Docker container for local Azure emulation
- Configures service connections and connection strings
- Executes integration tests with longer timeout (30 min)
- Tags results for integration test category
- Handles flaky test retries

**Usage:**
```yaml
# Reference in your azure-pipelines.yml
resources:
  repositories:
    - repository: testing-strategy
      type: git
      name: testing-strategy

jobs:
  - template: samples/pipelines/azure-pipelines-integration-tests.yml@testing-strategy
```

## Quick Start

### Option 1: Use Templates Directly

Create `azure-pipelines.yml` in your repository root:

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

stages:
  - stage: UnitTests
    displayName: 'Phase 1 - Unit Tests'
    jobs:
      - job: RunUnitTests
        steps:
          - template: samples/pipelines/azure-pipelines-unit-tests.yml

  - stage: IntegrationTests
    displayName: 'Phase 2 - Integration Tests'
    dependsOn: UnitTests
    jobs:
      - job: RunIntegrationTests
        steps:
          - template: samples/pipelines/azure-pipelines-integration-tests.yml
```

### Option 2: Copy and Customize

1. Copy the desired YAML file to your `.azure-pipelines/` folder
2. Customize paths, triggers, and variables
3. Reference in your main pipeline:

```yaml
stages:
  - stage: Test
    jobs:
      - template: .azure-pipelines/unit-tests.yml
```

## Pipeline Variables

### Required for Unit Tests
- None (self-contained)

### Required for Integration Tests
```yaml
variables:
  - group: 'integration-test-secrets'  # Azure DevOps Variable Group
  # Contains:
  #   - AZURE_STORAGE_CONNECTION_STRING
  #   - AZURE_SERVICE_BUS_CONNECTION_STRING
  #   - AZURE_KEY_VAULT_URI
```

### How to Create Variable Group
1. Go to Azure DevOps → Pipelines → Library
2. Click "+ Variable group"
3. Name: `integration-test-secrets`
4. Add variables:
   - `AZURE_STORAGE_CONNECTION_STRING`: `UseDevelopmentStorage=true` (for Azurite)
   - `AZURE_SERVICE_BUS_CONNECTION_STRING`: Your Service Bus connection string
   - `AZURE_KEY_VAULT_URI`: Your Key Vault URI
5. Click "Save"
6. Update pipeline permissions to use this group

## Coverage Thresholds

**Unit Tests:** 80% minimum (configurable in YAML)

```yaml
# Use a .runsettings file for code coverage (see Troubleshooting for why)
- script: |
    dotnet test --settings tests/unit/test.runsettings
  displayName: 'Run Unit Tests with coverage'
```

> **Note:** Do NOT use `--collect:"XPlat Code Coverage"` in `DotNetCoreCLI@2` task arguments. See the [Troubleshooting](#issue-collectxplat-code-coverage-breaks-in-azure-devops) section for details.

**Integration Tests:** No coverage enforcement (focus on E2E scenarios)

## Path Triggers

### Unit Tests
```yaml
paths:
  include:
    - 'src/**'
    - 'tests/unit/**'
  exclude:
    - 'docs/**'
    - '**/*.md'
```

### Integration Tests
```yaml
paths:
  include:
    - 'src/**'
    - 'tests/integration/**'
    - 'infrastructure/**'  # Trigger when infra changes
```

## Parallel Execution

Both pipelines support parallel test execution:

```yaml
strategy:
  parallel: 3  # Run 3 test batches in parallel
```

**Note:** Integration tests may need sequential execution if they share resources.

## Artifacts Published

### Unit Tests
- Test results (`.trx` files)
- Code coverage reports (`coverage.cobertura.xml`)
- Coverage summary (HTML report)

### Integration Tests
- Test results (`.trx` files)
- Integration test logs
- Performance metrics (if collected)

## Troubleshooting

### Issue: "No tests found"
**Solution:** Verify test project path pattern:
```yaml
projects: '**/tests/unit/**/*.UnitTests.csproj'
```

### Issue: "Coverage threshold not met"
**Solution:** Check coverage report and add missing tests:
```bash
# Locally run:
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport
```

### Issue: `--collect:"XPlat Code Coverage"` breaks in Azure DevOps
**Symptom:** `dotnet test` fails with argument parsing errors like `Unrecognized command or argument 'Code'` or `Unrecognized command or argument 'Coverage"'`.

**Root Cause:** The `DotNetCoreCLI@2` task (and YAML processing) strips the surrounding quotes from `--collect:"XPlat Code Coverage"`, turning it into three separate tokens: `--collect:XPlat`, `Code`, `Coverage`. This corrupts the `dotnet test` command.

**Solution:** Use a `.runsettings` file instead of the `--collect` argument:

1. Create `tests/unit/test.runsettings`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

2. Reference it in the pipeline:
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests with Coverage'
  inputs:
    command: 'test'
    projects: '**/tests/unit/**/*.UnitTests.csproj'
    arguments: '--configuration $(buildConfiguration) --no-build --no-restore --settings tests/unit/test.runsettings --logger trx --results-directory $(Agent.TempDirectory)/TestResults'
    publishTestResults: false
```

> **Note:** This is a known limitation of the `DotNetCoreCLI@2` task. The `.runsettings` approach is more reliable and also allows configuring additional coverage options (exclusions, formats, etc.) without modifying the pipeline YAML.

### Issue: Duplicate `--logger` and `--results-directory` arguments
**Symptom:** `dotnet test` fails because it receives duplicate `--logger trx` or `--results-directory` arguments.

**Root Cause:** When `publishTestResults` is set to `true` (the default), the `DotNetCoreCLI@2` task automatically injects `--logger trx` and `--results-directory` arguments. If you also specify them in the `arguments` field, they appear twice.

**Solution:** Set `publishTestResults: false` and use a separate `PublishTestResults@2` task:
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests'
  inputs:
    command: 'test'
    projects: '**/tests/unit/**/*.UnitTests.csproj'
    arguments: '--no-build --no-restore --logger trx --results-directory $(Agent.TempDirectory)/TestResults'
    publishTestResults: false  # Prevents auto-injected arguments

- task: PublishTestResults@2
  displayName: 'Publish Test Results'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '$(Agent.TempDirectory)/TestResults/**/*.trx'
    mergeTestResults: true
    failTaskOnFailedTests: true
  condition: succeededOrFailed()
```

### Issue: NuGet restore fails for private feed packages during `dotnet test`
**Symptom:** `dotnet test` fails with errors like `error NU1101: Unable to find package <PackageName>. No packages exist with this id in source(s): nuget.org`.

**Root Cause:** `dotnet test` performs an implicit restore that only uses `nuget.org`. If your project references packages from a private Azure DevOps Artifacts feed, the implicit restore won't find them.

**Solution:** Add `--no-restore` to the test arguments and ensure a proper restore step runs beforehand with the private feed configured:
```yaml
# 1. Authenticate with private feed
- task: NuGetAuthenticate@1
  displayName: 'Authenticate NuGet'

# 2. Restore with private feed
- task: DotNetCoreCLI@2
  displayName: 'Restore NuGet Packages'
  inputs:
    command: 'restore'
    projects: '**/*.UnitTests.csproj'
    feedsToUse: 'select'
    vstsFeed: '<your-feed-id>'

# 3. Test with --no-restore
- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests'
  inputs:
    command: 'test'
    projects: '**/*.UnitTests.csproj'
    arguments: '--no-build --no-restore'
    publishTestResults: true
```

### Issue: Service connection validation fails on feature branches
**Symptom:** Pipeline fails at compile time with `There was a resource authorization problem` or service connection variable not found, even though the stage would never run on that branch.

**Root Cause:** Azure DevOps validates **all** service connection references at pipeline compile time, regardless of runtime conditions (`condition:`). If a deployment stage references a service connection via a variable group that only loads for specific branches (e.g., `main`, `release/*`), pipelines triggered from `feature/*` branches will fail because the variable group isn't loaded.

**Solution:** Use compile-time `${{ if }}` expressions instead of runtime `condition:` to conditionally include deployment stages:
```yaml
# BAD: Runtime condition - service connection still validated at compile time
- stage: DeployDev
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/dev'))
  jobs:
    - deployment: Deploy
      environment: 'dev'
      # Uses $(serviceConnection) from a variable group only loaded for 'dev' branch

# GOOD: Compile-time expression - stage excluded entirely for non-matching branches
- ${{ if eq(variables['Build.SourceBranchName'], 'dev') }}:
  - stage: DeployDev
    jobs:
      - deployment: Deploy
        environment: 'dev'
```

> **Important:** The `${{ if }}` expression must match the same branch conditions as your variable group loading logic. If variable groups load for `dev`, `release/*`, `main`, etc., each deployment stage must be wrapped in a corresponding `${{ if }}` expression.

### Issue: "Azurite not starting"
**Solution:** Ensure Docker service is available:
```yaml
services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    ports:
      - 10000:10000
      - 10001:10001
      - 10002:10002
```

### Issue: "Integration tests timing out"
**Solution:** Increase timeout or reduce test scope:
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    arguments: '--no-restore --testCaseFilter:"Category=Integration&Priority=1"'
  timeoutInMinutes: 60  # Increase from default 30
```

## Best Practices

1. **Separate Stages:** Run unit tests before integration tests to fail fast
2. **Cache Dependencies:** Use NuGet cache to speed up builds
3. **Conditional Execution:** Skip integration tests for documentation-only changes
4. **Secret Management:** Store connection strings in Azure Key Vault, reference in pipeline
5. **Test Categorization:** Use `[Trait("Category", "Integration")]` to filter tests

## Related Documentation

- [AUTOMATION_TESTING_STANDARD.md](../../architecture/docs/AUTOMATION_TESTING_STANDARD.md) - Overall strategy
- [PHASE_1_UNIT_TESTING.md](../../architecture/docs/PHASE_1_UNIT_TESTING.md) - Unit testing details
- [PHASE_2_INTEGRATION_TESTING.md](../../architecture/docs/PHASE_2_INTEGRATION_TESTING.md) - Integration testing details
- [GitHub Actions Workflows](../.github/workflows/README.md) - GitHub Actions equivalents
