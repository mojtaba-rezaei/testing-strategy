# GitHub Workflows - Testing Examples

This folder contains complete GitHub Actions workflow examples for running automated tests in the Azure Integration Platform testing strategy.

## Available Workflows

### 1. [unit-tests.yml](.github/workflows/unit-tests.yml)
**Purpose:** Execute Phase 1 unit tests with code coverage reporting

**Triggers:**
- Push to `main` and `develop` branches
- Pull requests targeting `main` and `develop`
- Only when source code or unit tests change

**Features:**
- Multi-OS testing (Ubuntu, Windows, macOS)
- .NET 8 SDK installation
- NuGet package caching for faster builds
- Code coverage collection with Codecov integration
- Coverage badge generation
- Failure notifications

**Workflow Example:**
```yaml
name: Unit Tests
on:
  pull_request:
    branches: [main, develop]
    paths: ['src/**', 'tests/unit/**']
jobs:
  unit-tests:
    runs-on: ubuntu-latest
    # See unit-tests.yml for full implementation
```

### 2. [integration-tests.yml](.github/workflows/integration-tests.yml)
**Purpose:** Execute Phase 2 integration tests against Azure services or Azurite

**Triggers:**
- Pull requests with `integration-test` label
- Scheduled daily runs (2 AM UTC)
- Manual workflow dispatch

**Features:**
- Azurite Docker service for Azure Storage emulation
- Service Bus and Cosmos DB test container support
- Environment variable injection from GitHub Secrets
- Retry logic for flaky tests
- Test results artifact upload
- Slack/Teams notification on failure

**Workflow Example:**
```yaml
name: Integration Tests
on:
  schedule:
    - cron: '0 2 * * *'  # Daily at 2 AM UTC
services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    # See integration-tests.yml for full implementation
```

## Quick Start

### 1. Copy Workflows to Your Repository

```bash
# From repository root
mkdir -p .github/workflows
cp path/to/testing-strategy/.github/workflows/unit-tests.yml .github/workflows/
cp path/to/testing-strategy/.github/workflows/integration-tests.yml .github/workflows/
```

### 2. Configure GitHub Secrets

Navigate to **Settings → Secrets and variables → Actions** and add:

**For Unit Tests:**
- `CODECOV_TOKEN` (optional) - Codecov upload token for coverage reporting

**For Integration Tests:**
- `AZURE_STORAGE_CONNECTION_STRING` - Azure Storage connection string (or use Azurite default)
- `AZURE_SERVICE_BUS_CONNECTION_STRING` - Service Bus connection string
- `AZURE_KEY_VAULT_URI` - Azure Key Vault URI
- `SLACK_WEBHOOK_URL` (optional) - Slack webhook for failure notifications

**How to add secrets:**
1. Go to repository → Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Enter name (e.g., `AZURE_STORAGE_CONNECTION_STRING`)
4. Paste value (connection string from Azure Portal)
5. Click "Add secret"

### 3. Customize Workflow (Optional)

Edit the workflow files to match your project structure:

```yaml
# In unit-tests.yml
jobs:
  unit-tests:
    steps:
      - name: Run unit tests
        run: dotnet test tests/unit/**/*.UnitTests.csproj  # Adjust path
```

## Workflow Matrix Strategy

Run tests across multiple environments:

```yaml
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest, macos-latest]
    dotnet-version: ['8.0.x']
runs-on: ${{ matrix.os }}
steps:
  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: ${{ matrix.dotnet-version }}
```

**When to use:**
- Cross-platform compatibility testing
- Multiple .NET version support
- Different database versions

## Workflow Permissions

Workflows require specific permissions:

```yaml
permissions:
  contents: read        # Checkout code
  pull-requests: write  # Comment on PRs
  issues: write         # Create issues on failure
  checks: write         # Publish test results
```

Set in: **Settings → Actions → General → Workflow permissions**

## Coverage Reporting

### Codecov Integration

**Setup:**
1. Sign up at [codecov.io](https://codecov.io)
2. Add your repository
3. Copy the upload token
4. Add as secret: `CODECOV_TOKEN`

**Badge:**
Add to README.md:
```markdown
[![codecov](https://codecov.io/gh/YOUR_ORG/YOUR_REPO/branch/main/graph/badge.svg)](https://codecov.io/gh/YOUR_ORG/YOUR_REPO)
```

### Coveralls Alternative

Replace Codecov step in workflow:

```yaml
- name: Coveralls
  uses: coverallsapp/github-action@v2
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    path-to-lcov: coverage/lcov.info
```

## Caching Strategy

Speed up builds with dependency caching:

```yaml
- name: Cache NuGet packages
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

**Result:** 2-3x faster restore on subsequent runs.

## Notification Integrations

### Microsoft Teams Notifications

```yaml
- name: Notify Teams on Failure
  if: failure()
  run: |
    curl -H 'Content-Type: application/json' \
         -d '{"text":"❌ Tests failed in ${{ github.repository }}"}' \
         ${{ secrets.TEAMS_WEBHOOK_URL }}
```

## Troubleshooting

### Issue: "Workflow not triggering"
**Check:**
- Workflow file is in `.github/workflows/` (note the dot prefix)
- YAML syntax is valid (use YAML linter)
- Trigger paths match changed files

**Debug:**
```yaml
on:
  pull_request:
    paths:
      - 'src/**'   # Ensure this matches your structure
      - 'tests/unit/**'
```

### Issue: "Tests fail only in GitHub Actions"
**Common causes:**
- Timezone differences (use UTC in tests)
- File path separators (Windows `\` vs Linux `/`)
- Missing environment variables

**Solution:**
```yaml
env:
  TZ: 'UTC'
  DOTNET_CLI_TELEMETRY_OPTOUT: 1
```

### Issue: "Coverage report not uploaded"
**Check:**
- Coverage file path is correct
- `CODECOV_TOKEN` secret is set
- Coverage file exists after test run

**Debug:**
```yaml
- name: Debug coverage files
  run: find . -name "coverage.cobertura.xml"
```

### Issue: "Integration tests timeout"
**Solution:**
```yaml
- name: Run integration tests
  run: dotnet test --logger trx
  timeout-minutes: 30  # Increase from default 15
```

### Issue: "Azurite not connecting"
**Solution:**
Ensure service is ready before tests:
```yaml
services:
  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    ports:
      - 10000:10000
      - 10001:10001
      - 10002:10002
    options: >-
      --health-cmd "nc -z localhost 10000"
      --health-interval 10s
      --health-timeout 5s
      --health-retries 5
```

## Best Practices

1. **Separate Unit and Integration Workflows**
   - Unit tests run on every push (fast feedback)
   - Integration tests run on schedule or manual trigger (slower, resource-intensive)

2. **Use Path Filters**
   - Avoid running tests for documentation-only changes
   ```yaml
   paths-ignore:
     - '**/*.md'
     - 'docs/**'
   ```

3. **Fail Fast Strategy**
   ```yaml
   strategy:
     fail-fast: true  # Stop all jobs if one fails
     matrix:
       os: [ubuntu-latest, windows-latest]
   ```

4. **Artifact Retention**
   ```yaml
   - uses: actions/upload-artifact@v3
     with:
       name: test-results
       path: TestResults/
       retention-days: 7  # Keep for 1 week
   ```

5. **Concurrency Control**
   ```yaml
   concurrency:
     group: ${{ github.workflow }}-${{ github.ref }}
     cancel-in-progress: true  # Cancel old runs on new push
   ```

## Workflow Templates

### Minimal Unit Test Workflow

```yaml
name: Unit Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'
      - run: dotnet test tests/unit/**/*.UnitTests.csproj
```

### Advanced Multi-Stage Workflow

```yaml
name: CI/CD
on: [push]
jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps: [...]  # Unit tests
  
  integration-tests:
    needs: unit-tests
    runs-on: ubuntu-latest
    steps: [...]  # Integration tests
  
  deploy:
    needs: integration-tests
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps: [...]  # Deployment
```

## Security Considerations

1. **Never commit secrets to workflows**
   - Use GitHub Secrets for sensitive data
   - Use environments for production secrets

2. **Review third-party actions**
   ```yaml
   - uses: actions/checkout@v4  # Pin to specific version
     # Better: Pin to commit SHA
   - uses: actions/checkout@8e5e7e5ab8b370d6c329ec480221332ada57f0ab
   ```

3. **Limit workflow permissions**
   ```yaml
   permissions:
     contents: read  # Read-only by default
   ```

## Related Documentation

- [AUTOMATION_TESTING_STANDARD.md](../../architecture/docs/AUTOMATION_TESTING_STANDARD.md) - Overall strategy
- [PHASE_1_UNIT_TESTING.md](../../architecture/docs/PHASE_1_UNIT_TESTING.md) - Unit testing details
- [PHASE_2_INTEGRATION_TESTING.md](../../architecture/docs/PHASE_2_INTEGRATION_TESTING.md) - Integration testing details
- [Azure Pipelines](../pipelines/README.md) - Azure DevOps equivalents
