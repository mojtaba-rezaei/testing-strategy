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
# In azure-pipelines-unit-tests.yml
- script: |
    dotnet test --collect:"XPlat Code Coverage" /p:Threshold=80
  displayName: 'Run Unit Tests with 80% coverage requirement'
```

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
