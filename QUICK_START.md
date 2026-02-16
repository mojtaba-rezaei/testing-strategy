# Quick Start Guide - Automation Testing

Get your Azure integration project set up with automated testing in 5-10 minutes.

## Prerequisites

- .NET 8 SDK installed
- Azure subscription (for integration tests in Phase 2)
- Git repository for your project
- CI/CD pipeline (Azure DevOps or GitHub Actions)

## Step 1: Create Test Projects (2 minutes)

### For a .NET Azure Function Project

```bash
# Navigate to your project root
cd your-project-root

# Create unit test project
dotnet new xunit -n YourFunctionApp.UnitTests -o tests/unit/YourFunctionApp.UnitTests

# Add necessary packages
cd tests/unit/YourFunctionApp.UnitTests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package AutoFixture

# Reference your function app
dotnet add reference ../../../src/YourFunctionApp/YourFunctionApp.csproj

# Create folder structure
mkdir Functions
mkdir Services
mkdir Models
```

### Project Structure Should Look Like:

```
/your-project-root
  /src
    /YourFunctionApp
      /Functions
      /Services
      /Models
  /tests
    /unit
      /YourFunctionApp.UnitTests
        /Functions
        /Services
        /Models
```

## Step 2: Write Your First Unit Test (3 minutes)

Create a test file: `tests/unit/YourFunctionApp.UnitTests/Services/OrderServiceTests.cs`

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using YourFunctionApp.Services;
using YourFunctionApp.Models;

namespace YourFunctionApp.UnitTests.Services;

public class OrderServiceTests
{
    [Fact]
    public async Task ValidateOrder_WithValidOrder_ReturnsTrue()
    {
        // Arrange
        var orderService = new OrderService();
        var order = new Order 
        { 
            Id = "123", 
            Amount = 100.00m 
        };

        // Act
        var result = await orderService.ValidateOrder(order);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateOrder_WithNullOrder_ReturnsFalse()
    {
        // Arrange
        var orderService = new OrderService();

        // Act
        var result = await orderService.ValidateOrder(null);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task ValidateOrder_WithInvalidAmount_ReturnsFalse(decimal amount)
    {
        // Arrange
        var orderService = new OrderService();
        var order = new Order { Id = "123", Amount = amount };

        // Act
        var result = await orderService.ValidateOrder(order);

        // Assert
        result.Should().BeFalse();
    }
}
```

## Step 3: Run Tests Locally (1 minute)

```bash
# From project root
dotnet test tests/unit/YourFunctionApp.UnitTests

# With code coverage
dotnet test tests/unit/YourFunctionApp.UnitTests --collect:"XPlat Code Coverage"

# View results
# Coverage report will be in: tests/unit/YourFunctionApp.UnitTests/TestResults/
```

## Step 4: Add CI Pipeline (2-3 minutes)

### For Azure Pipelines

Create `azure-pipelines.yml` in your repo root:

```yaml
trigger:
  - main
  - develop

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '8.x'

  - task: DotNetCoreCLI@2
    displayName: 'Restore'
    inputs:
      command: 'restore'

  - task: DotNetCoreCLI@2
    displayName: 'Build'
    inputs:
      command: 'build'
      arguments: '--configuration Release'

  - task: DotNetCoreCLI@2
    displayName: 'Unit Tests'
    inputs:
      command: 'test'
      projects: '**/tests/unit/**/*.csproj'
      arguments: '--configuration Release --collect:"XPlat Code Coverage"'

  - task: PublishCodeCoverageResults@1
    inputs:
      codeCoverageTool: 'Cobertura'
      summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
```

### For GitHub Actions

Create `.github/workflows/tests.yml`:

```yaml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Test
        run: dotnet test tests/unit/**/*.csproj --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.cobertura.xml
```

## Step 5: Verify (1 minute)

1. Commit and push your changes
2. Verify pipeline runs successfully
3. Check test results in pipeline output
4. Review code coverage report

## Phase 1 Checklist

Use this checklist to track Phase 1 compliance:

- [ ] Test project created following naming convention
- [ ] Folder structure matches standard
- [ ] At least one unit test written and passing
- [ ] Tests run locally successfully
- [ ] CI pipeline configured and running tests
- [ ] Code coverage reporting enabled
- [ ] All tests passing in CI
- [ ] Coverage ≥ 80% (or path to get there documented)

## Next Steps

### To Continue with Phase 1:
1. Add tests for all new code you write
2. Target 80% code coverage for critical components
3. Review [AUTOMATION_TESTING_STANDARD.md](architecture/docs/AUTOMATION_TESTING_STANDARD.md) Section 6 for Definition of Done
4. Use test builders from Section 8.3 for complex test data

### To Move to Phase 2 (Integration Testing):
1. Complete Phase 1 exit criteria (see Section 7 of standard)
2. Set up test Azure environment
3. Review integration test examples (Section 8.1)
4. Configure integration test pipeline (Section 8.2)

## Common Issues

### Tests not found in CI
- Ensure test project paths match glob pattern in pipeline
- Verify test framework (xUnit) is referenced
- Check project targets .NET 8

### Low code coverage
- Focus on business logic first
- Don't worry about hitting 80% immediately
- Incrementally improve with each PR

### Slow tests
- Ensure tests are truly isolated (no I/O, no Azure calls)
- Use mocks for external dependencies
- Each unit test should be < 100ms

## Getting Help

- 📚 **Full Standard:** [AUTOMATION_TESTING_STANDARD.md](architecture/docs/AUTOMATION_TESTING_STANDARD.md)

## Templates

Find ready-to-use templates in the `/samples` folder:

---

**Congratulations!** 🎉 You've set up your first automated tests. Keep building on this foundation!
