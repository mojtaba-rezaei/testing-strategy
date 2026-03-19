# Phase 1: Unit Testing Foundation

## Purpose
This document defines the mandatory first phase of the automation testing strategy: **Unit Testing**. Phase 1 establishes the foundation for all testing practices and must be completed before moving to integration or E2E testing.

## Reference
This is extracted from [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) covering Phase 1 unit testing requirements, examples, and best practices.

## Overview

### What is Phase 1?
Phase 1 focuses exclusively on **unit testing** - testing individual components in isolation with mocked dependencies. This phase builds the testing culture, establishes patterns, and achieves high code coverage before moving to more complex testing types.

### Phase Duration
**2-3 months** for initial adoption

### Phase Objective
Establish a robust unit testing foundation that:
- Covers 80% of business logic
- Runs fast (< 5 minutes total)
- Provides rapid feedback to developers
- Prevents defects from reaching production
- Creates a culture of test-first development

## Testing Scope - Step 1 (Unit Testing First)

### Required

- ✅ Unit tests for all custom code:
  - Azure Functions (business logic, triggers, bindings)
  - Logic App inline code
  - Helper utilities
  - Transformers
  - Validators
  - Services
  - Models/Entities

- ✅ Test Characteristics:
  - **Fast:** < 100ms per test
  - **Isolated:** No external dependencies
  - **Deterministic:** Same result every time
  - **Focused:** Test one thing at a time

- ✅ Coverage Requirements:
  - Minimum 80% code coverage for business logic
  - Edge cases covered
  - Error handling tested
  - Validation logic verified

### Intentionally Deferred (Not in Phase 1)

- ❌ Integration tests (between Azure components)
- ❌ Contract tests (API/schema validation)
- ❌ Performance tests
- ❌ End-to-end tests
- ❌ System tests
- ❌ Tests requiring real Azure resources
- ❌ Tests requiring network calls
- ❌ Tests requiring database connections
- ❌ Tests requiring external services

## Test Pyramid Architecture - Phase 1 Focus

### 60% Unit Tests (Foundation Layer)

In Phase 1, you'll build the foundation of the test pyramid:

```
                     /\
                    /  \
                   /    \
                  / E2E  \ ← Not in Phase 1
                 /_______ \
                /          \
               /Integration \ ← Not in Phase 1
              /_____________ \
             /                \
            /   Unit Tests     \ ← FOCUS HERE (60% of future total)
           /___________________ \
```

**Why 60% Unit Tests?**
- **Speed:** Fastest feedback loop (milliseconds)
- **Cost:** Cheapest to write and maintain
- **Isolation:** Test one thing at a time, easy to debug
- **Coverage:** High coverage of business logic
- **Reliability:** Most stable (no external dependencies)
- **Developer Experience:** Run locally without infrastructure

## Azure-Specific Testing Strategies (Phase 1)

### Azure Functions (Unit Testing)

**What to Test:**
- Business logic and processing code
- Input validation
- Output formatting
- Error handling
- Data transformations

**How to Test:**
- Mock triggers (HTTP, Timer, Queue, Event Grid)
- Mock bindings (CosmosDB, Blob, Table)
- Mock dependencies (services, repositories)
- Use dependency injection for testability

#### Pattern A: Functions with DI Dependencies

When the function has constructor-injected dependencies, mock them:

**Example:**
```csharp
public class ProcessOrderTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly ProcessOrderFunction _function;

    public ProcessOrderTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _function = new ProcessOrderFunction(_orderServiceMock.Object);
    }

    [Fact]
    public async Task ProcessOrder_WithValidOrder_ReturnsSuccess()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        _orderServiceMock
            .Setup(s => s.ProcessOrder(It.IsAny<Order>()))
            .ReturnsAsync(new ProcessResult { Success = true });

        // Act
        var result = await _function.Run(order);

        // Assert
        result.Success.Should().BeTrue();
    }
}
```

#### Pattern B: Stateless Functions (No DI Dependencies)

Many integration functions (e.g., XML-to-JSON mapping, data transformations) are stateless and have no constructor dependencies. These can be instantiated directly without mocks. The focus is on testing the transformation logic with real input/output data.

**Example:**
```csharp
public class XmlToCanonicalFunctionTests
{
    private readonly XmlToCanonicalFunction _sut = new();

    [Fact]
    public async Task Run_WithValidXml_ReturnsOkWithMappedJson()
    {
        // Arrange
        var xml = TestHelper.BuildMinimalValidXml(
            sourceParty: "WAREHOUSE01",
            destinationParty: "STORE001",
            referenceNumber: "REF-123");
        var request = TestHelper.CreateHttpRequest(xml);

        // Act
        var response = await _sut.Run(request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var result = DeserializeResult(response);
        result.Header.SourceParty.Should().Be("WAREHOUSE01");
        result.Header.DestinationParty.Should().Be("STORE001");
    }

    [Fact]
    public async Task Run_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var request = TestHelper.CreateHttpRequest("");

        // Act
        var response = await _sut.Run(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
    }
}
```

**Key points for stateless function tests:**
- **No mocks needed** - instantiate the function class directly (`new()`)
- **Focus on input/output mapping** - validate that XML/JSON/CSV transformations produce correct output
- **Use helper methods** for building test data (e.g., `TestHelper.BuildMinimalValidXml()`) to keep tests readable
- **Test filtering rules** - verify that invalid/excluded records are correctly filtered out
- **Test edge cases** - empty collections, missing optional fields, boundary values

> **Gotcha: Serializer behavior with empty collections**
> Some serialization libraries (e.g., custom JSON serializers, certain Newtonsoft.Json configurations) omit empty arrays and serialize them as `null`. If your tests assert on empty collections, use `.BeNullOrEmpty()` instead of `.BeEmpty()` to avoid false failures.

### Logic Apps Standard (Unit Testing)

**What to Test:**
- Inline JavaScript/C# code
- Custom expressions
- Data transformation logic
- Validation functions

**How to Test:**
- Extract inline code to testable functions
- Test functions independently
- Mock workflow context

### Async/Retries/Failures (Unit Testing)

**What to Test:**
- Retry logic with mocked failures
- Timeout handling
- Error propagation
- Exception handling

**How to Test:**
```csharp
[Fact]
public async Task SendMessage_WhenServiceFails_RetriesThreeTimes()
{
    // Arrange
    var callCount = 0;
    _serviceBusMock
        .Setup(x => x.SendAsync(It.IsAny<Message>()))
        .Callback(() => callCount++)
        .ThrowsAsync(new ServiceBusException("Transient failure"));

    // Act & Assert
    await Assert.ThrowsAsync<ServiceBusException>(() => _service.SendWithRetry(message));
    callCount.Should().Be(3); // Verify retry attempts
}
```

## Tooling and Frameworks (Phase 1)

### Azure Functions (.NET)

**Testing Framework:**
- **xUnit** - Primary testing framework
  - Modern, extensible, community-driven
  - Excellent parallel execution support
  - Clean syntax with `[Fact]` and `[Theory]`

**Mocking/Stubbing:**
- **Moq** - Mock objects and verify interactions
  - Easy setup/verification syntax
  - Support for async methods
  - Interface and virtual method mocking

**Assertions:**
- **FluentAssertions** - Readable, expressive assertions
  - Natural language syntax
  - Detailed failure messages
  - Support for complex object comparisons

**Test Data:**
- **AutoFixture** - Generate test data automatically
  - Reduce boilerplate code
  - Create realistic test objects
  - Support for customization

**Example Setup:**
```xml
<!-- OrderProcessor.UnitTests.csproj -->
<ItemGroup>
  <PackageReference Include="xunit" Version="2.6.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
  <PackageReference Include="Moq" Version="4.20.0" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="AutoFixture" Version="4.18.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
</ItemGroup>
```

### Logic Apps Inline Code

**Testing Framework:**
- **Jest** (JavaScript/TypeScript)

### Code Coverage

**Tool:** Coverlet (.NET)
```xml
<PackageReference Include="coverlet.collector" Version="6.0.0" />
<PackageReference Include="coverlet.msbuild" Version="6.0.0" />
```

**Local Usage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Azure DevOps Pipeline Usage:**

> **Warning:** Do NOT use `--collect:"XPlat Code Coverage"` in `DotNetCoreCLI@2` task arguments — the quotes get stripped by YAML processing, breaking the command. Use a `.runsettings` file instead:

```xml
<!-- tests/unit/test.runsettings -->
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[SharedLibrary]*,[*.Views]*</Exclude>
          <ExcludeByFile>**/obj/**,**/bin/**</ExcludeByFile>
          <ExcludeByAttribute>ObsoleteAttribute,GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

> **Coverage Exclusions:** The `.runsettings` file supports several exclusion directives that are **essential** for accurate coverage reporting:
> - `<Exclude>` — Exclude entire assemblies by name using `[AssemblyName]*` syntax. This is critical when your project uses `<ProjectReference>` to a shared library (see pitfall below).
> - `<ExcludeByFile>` — Exclude files by path pattern (e.g., auto-generated code in `obj/`).
> - `<ExcludeByAttribute>` — Exclude code decorated with specific attributes (e.g., `GeneratedCodeAttribute`, `CompilerGeneratedAttribute`).

```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    arguments: '--no-build --no-restore --settings tests/unit/test.runsettings --logger trx'
    publishTestResults: false
```

See [pipelines/README.md](../../samples/pipelines/README.md#troubleshooting) for more details.

### InternalsVisibleTo for Testing Internal Helpers

Azure Functions often contain `internal` helper methods (e.g., parsing utilities, formatting helpers) that are not accessible from the test project. To test these without changing their visibility to `public`, add an `<InternalsVisibleTo>` directive to the source project's `.csproj`:

```xml
<!-- MyFunctionApp.csproj -->
<ItemGroup>
    <InternalsVisibleTo Include="MyFunctionApp.UnitTests" />
</ItemGroup>
```

This allows the test project to access `internal` members while keeping them hidden from other consumers.

### CI/CD Integration

- **GitHub Actions** - Workflow automation
- **Azure Pipelines** - Enterprise CI/CD

### Local Execution

```bash
# Run all unit tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/unit/OrderProcessor.UnitTests

# Run tests matching filter
dotnet test --filter "Category=Unit"
```

## Folder Structure (Phase 1)

```
/project-root
  /src
    /OrderProcessor.FunctionApp
      /Functions
        ProcessOrderFunction.cs
      /Services
        OrderService.cs
        ValidationService.cs
      /Models
        Order.cs
        ProcessResult.cs
      Program.cs
      host.json
  
  /tests
    /unit
      /OrderProcessor.UnitTests
        /Functions
          ProcessOrderFunctionTests.cs
        /Services
          OrderServiceTests.cs
          ValidationServiceTests.cs
        /Models
          OrderTests.cs
        /Builders                    ← Test data builders
          OrderBuilder.cs
        OrderProcessor.UnitTests.csproj
    
    /integration                     ← Not used in Phase 1
    
    /shared
      /TestUtilities
        /Builders
        /Helpers
  
  /pipelines
    ci-unit-tests.yml
```

## Test Builder Pattern (Phase 1 Best Practice)

### Purpose
Test builders create test data in a readable, maintainable way using the Builder pattern.

### Implementation

```csharp
// tests/shared/TestUtilities/Builders/OrderBuilder.cs
using AutoFixture;
using OrderProcessor.Models;

namespace TestUtilities.Builders
{
    public class OrderBuilder
    {
        private readonly Fixture _fixture = new();
        private string _id;
        private string _customerId;
        private decimal _amount;
        private OrderStatus _status;

        public OrderBuilder()
        {
            // Set reasonable defaults using AutoFixture
            _id = _fixture.Create<string>();
            _customerId = _fixture.Create<string>();
            _amount = _fixture.Create<decimal>();
            _status = OrderStatus.Pending;
        }

        public OrderBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public OrderBuilder WithCustomerId(string customerId)
        {
            _customerId = customerId;
            return this;
        }

        public OrderBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public OrderBuilder WithStatus(OrderStatus status)
        {
            _status = status;
            return this;
        }

        public OrderBuilder WithInvalidAmount()
        {
            _amount = -100;
            return this;
        }

        public Order Build()
        {
            return new Order
            {
                Id = _id,
                CustomerId = _customerId,
                Amount = _amount,
                Status = _status
            };
        }
    }
}
```

### Usage in Tests

```csharp
[Fact]
public async Task ProcessOrder_WithValidOrder_ReturnsSuccess()
{
    // Arrange - Clean, readable test data creation
    var order = new OrderBuilder()
        .WithId("TEST-123")
        .WithCustomerId("C-456")
        .WithAmount(99.99m)
        .Build();

    _orderServiceMock
        .Setup(s => s.ProcessOrder(order))
        .ReturnsAsync(new ProcessResult { Success = true });

    // Act
    var result = await _function.Run(order);

    // Assert
    result.Success.Should().BeTrue();
}

[Fact]
public async Task ProcessOrder_WithInvalidAmount_ThrowsException()
{
    // Arrange - Builder makes invalid scenarios clear
    var order = new OrderBuilder()
        .WithInvalidAmount()
        .Build();

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() => _function.Run(order));
}
```

### Benefits
- ✅ Readable test setup
- ✅ Reusable across tests
- ✅ Easy to create valid/invalid scenarios
- ✅ Reduces boilerplate code
- ✅ Centralized test data creation

## Code Coverage Requirements (Phase 1)

### Minimum Targets

| Metric | Target | Minimum Acceptable | Blocker |
|--------|--------|-------------------|---------|
| **Overall Code Coverage** | ≥ 80% | ≥ 70% | < 60% |
| **Business Logic Coverage** | ≥ 90% | ≥ 80% | < 70% |
| **Branch Coverage** | ≥ 75% | ≥ 65% | < 55% |
| **New Code Coverage** | 100% | ≥ 90% | < 80% |

### What to Cover

**High Priority (Must Cover):**
- ✅ Business logic
- ✅ Validation rules
- ✅ Data transformations
- ✅ Error handling
- ✅ Edge cases

**Lower Priority:**
- ⚠️ DTOs/Models (if simple)
- ⚠️ Configuration classes
- ⚠️ Auto-generated code

**Don't Waste Time On:**
- ❌ Third-party libraries
- ❌ Framework code
- ❌ Getters/setters with no logic

> **Watch Out: Hidden Branches in Model Properties**
>
> C# model classes can generate more IL-level branches than expected, impacting branch coverage even when they appear to have no logic:
> - **Nullable value-type wrappers:** Properties like `get => Specified ? _field : null` generate true/false branches in IL.
> - **XML `ShouldSerialize*()` methods:** Methods like `public bool ShouldSerializeMyProperty() => MyProperty.HasValue;` generate branches for the `HasValue` check.
>
> These branches are easy to miss because the source code looks trivial, but they can prevent reaching the 75% branch coverage target. Writing simple tests that set and read these properties (or call ShouldSerialize with and without values) is low-effort and can close branch coverage gaps quickly.

### Measuring Coverage

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## Unit Test Examples (Phase 1)

### Example 1: Azure Function with Validation

```csharp
// src/OrderProcessor.FunctionApp/Functions/ProcessOrderFunction.cs
public class ProcessOrderFunction
{
    private readonly IOrderService _orderService;
    private readonly IValidationService _validationService;

    public ProcessOrderFunction(
        IOrderService orderService,
        IValidationService validationService)
    {
        _orderService = orderService;
        _validationService = validationService;
    }

    [Function("ProcessOrder")]
    public async Task<ProcessResult> Run(
        [ServiceBusTrigger("orders-queue")] Order order)
    {
        if (!await _validationService.ValidateOrder(order))
        {
            throw new ValidationException("Invalid order data");
        }

        return await _orderService.ProcessOrder(order);
    }
}

// tests/unit/OrderProcessor.UnitTests/Functions/ProcessOrderFunctionTests.cs
public class ProcessOrderFunctionTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IValidationService> _validationServiceMock;
    private readonly ProcessOrderFunction _function;

    public ProcessOrderFunctionTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _validationServiceMock = new Mock<IValidationService>();
        _function = new ProcessOrderFunction(
            _orderServiceMock.Object,
            _validationServiceMock.Object
        );
    }

    [Fact]
    public async Task ProcessOrder_WithValidOrder_ReturnsSuccess()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        
        _validationServiceMock
            .Setup(s => s.ValidateOrder(It.IsAny<Order>()))
            .ReturnsAsync(true);
        
        _orderServiceMock
            .Setup(s => s.ProcessOrder(It.IsAny<Order>()))
            .ReturnsAsync(new ProcessResult { Success = true });

        // Act
        var result = await _function.Run(order);

        // Assert
        result.Success.Should().BeTrue();
        _validationServiceMock.Verify(s => s.ValidateOrder(order), Times.Once);
        _orderServiceMock.Verify(s => s.ProcessOrder(order), Times.Once);
    }

    [Fact]
    public async Task ProcessOrder_WithInvalidOrder_ThrowsValidationException()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        
        _validationServiceMock
            .Setup(s => s.ValidateOrder(It.IsAny<Order>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _function.Run(order));
        
        _orderServiceMock.Verify(
            s => s.ProcessOrder(It.IsAny<Order>()), 
            Times.Never
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100.50)]
    public async Task ProcessOrder_WithInvalidAmount_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithAmount(amount)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _function.Run(order));
    }
}
```

### Example 2: Service with Business Logic

```csharp
// src/OrderProcessor.FunctionApp/Services/OrderService.cs
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProcessResult> ProcessOrder(Order order)
    {
        try
        {
            // Business logic
            var total = CalculateTotal(order);
            order.Total = total;

            await _repository.SaveAsync(order);
            
            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            
            return new ProcessResult { Success = true, OrderId = order.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            return new ProcessResult { Success = false, Error = ex.Message };
        }
    }

    private decimal CalculateTotal(Order order)
    {
        var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
        var tax = subtotal * 0.1m;
        return subtotal + tax;
    }
}

// tests/unit/OrderProcessor.UnitTests/Services/OrderServiceTests.cs
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _service = new OrderService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessOrder_WithValidOrder_SavesAndReturnsSuccess()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId("TEST-123")
            .Build();

        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        result.Success.Should().BeTrue();
        result.OrderId.Should().Be("TEST-123");
        _repositoryMock.Verify(r => r.SaveAsync(order), Times.Once);
    }

    [Fact]
    public async Task ProcessOrder_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var expectedException = new Exception("Database error");

        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<Order>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _service.ProcessOrder(order);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Database error");
    }
}
```

### Example 3: Validation Service

See [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md) for naming pattern details.

```csharp
// tests/unit/OrderProcessor.UnitTests/Services/ValidationServiceTests.cs
public class ValidationServiceTests
{
    private readonly ValidationService _service;

    public ValidationServiceTests()
    {
        _service = new ValidationService();
    }

    [Fact]
    public async Task ValidateOrder_WithValidData_ReturnsTrue()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId("ORDER-123")
            .WithCustomerId("C-456")
            .WithAmount(99.99m)
            .Build();

        // Act
        var result = await _service.ValidateOrder(order);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null, "C-456", 99.99)]
    [InlineData("", "C-456", 99.99)]
    [InlineData("ORDER-123", null, 99.99)]
    [InlineData("ORDER-123", "", 99.99)]
    [InlineData("ORDER-123", "C-456", 0)]
    [InlineData("ORDER-123", "C-456", -10)]
    public async Task ValidateOrder_WithInvalidData_ReturnsFalse(
        string orderId, 
        string customerId, 
        decimal amount)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId(orderId)
            .WithCustomerId(customerId)
            .WithAmount(amount)
            .Build();

        // Act
        var result = await _service.ValidateOrder(order);

        // Assert
        result.Should().BeFalse();
    }
}
```

## CI/CD Integration (Phase 1)

### Unit Test Pipeline Requirements

**Trigger:** Every PR and commit to all branches

**Execution Time:** < 5 minutes total

**Quality Gates:**
- ✅ All unit tests must pass (0 failures)
- ✅ Minimum code coverage: 80%
- ✅ No critical/high security vulnerabilities
- ✅ Build must succeed

**Pipeline Actions:**
1. Restore dependencies
2. Build solution
3. Run unit tests with coverage
4. Publish test results
5. Publish coverage report
6. Block PR if gates fail

### Azure Pipelines Example

```yaml
# pipelines/ci-unit-tests.yml
trigger:
  branches:
    include:
      - main
      - develop
      - feature/*

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  coverageThreshold: 80

steps:
  - task: UseDotNet@2
    displayName: 'Install .NET SDK'
    inputs:
      version: '8.x'

  - task: DotNetCoreCLI@2
    displayName: 'Restore Dependencies'
    inputs:
      command: 'restore'

  - task: DotNetCoreCLI@2
    displayName: 'Build Solution'
    inputs:
      command: 'build'
      arguments: '--configuration $(buildConfiguration)'

  - task: DotNetCoreCLI@2
    displayName: 'Run Unit Tests'
    inputs:
      command: 'test'
      projects: '**/tests/unit/**/*.csproj'
      arguments: >
        --configuration $(buildConfiguration)
        --collect:"XPlat Code Coverage"
        --logger trx
        --no-build

  - task: PublishTestResults@2
    displayName: 'Publish Test Results'
    condition: succeededOrFailed()
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '**/*.trx'
      failTaskOnFailedTests: true

  - task: PublishCodeCoverageResults@1
    displayName: 'Publish Code Coverage'
    inputs:
      codeCoverageTool: 'Cobertura'
      summaryFileLocation: '**/coverage.cobertura.xml'

  - task: BuildQualityChecks@8
    displayName: 'Check Coverage Threshold'
    inputs:
      checkCoverage: true
      coverageThreshold: '$(coverageThreshold)'
```

### GitHub Actions Example

```yaml
# .github/workflows/unit-tests.yml
name: Unit Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Run Unit Tests
        run: |
          dotnet test tests/unit/**/*.csproj \
            --configuration Release \
            --no-build \
            --collect:"XPlat Code Coverage" \
            --logger "trx"

      - name: Publish Test Results
        uses: EnricoMi/publish-unit-test-result-action@v2
        if: always()
        with:
          files: '**/*.trx'

      - name: Code Coverage Report
        uses: codecov/codecov-action@v3
        with:
          files: '**/coverage.cobertura.xml'
          fail_ci_if_error: true
```

## Phase 1 Success Criteria

### Entry Criteria
- [ ] Team commitment to testing culture
- [ ] Basic CI/CD pipeline in place
- [ ] Development environment setup complete
- [ ] Testing frameworks and tools installed

### Success Criteria (During Phase)
- ✅ All new code has unit tests (100% compliance for 1 month)
- ✅ Unit test pipeline runs on every PR
- ✅ Code coverage ≥ 80% for new code
- ✅ Average test execution time < 5 minutes
- ✅ Zero critical bugs escaped to production from untested code
- ✅ Team comfortable writing and maintaining tests

### Exit Criteria (Ready for Phase 2)
- [ ] 100% of active projects have unit tests
- [ ] Unit test coverage ≥ 80% across codebase
- [ ] Pipeline reliability ≥ 95% (minimal flaky tests)
- [ ] Team has established testing patterns/practices
- [ ] No significant defects in production from untested code for 2 months
- [ ] Definition of Done includes unit tests
- [ ] Code review process includes test review

## Definition of Done (Phase 1)

Every PR must meet these criteria:

- [ ] All new/modified code has corresponding unit tests
- [ ] Unit tests pass locally and in CI pipeline
- [ ] Code coverage ≥ 80% for business logic
- [ ] No skipped or ignored tests without justification
- [ ] Test code reviewed as part of PR
- [ ] All edge cases and error paths tested
- [ ] Tests follow naming conventions (see [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md))
- [ ] Test builders used for complex object creation
- [ ] Mocks used appropriately (not over-mocked)

## Best Practices (Phase 1)

### Test Organization

**Arrange-Act-Assert Pattern:**
```csharp
[Fact]
public async Task ProcessOrder_WithValidOrder_ReturnsSuccess()
{
    // Arrange - Set up test data and mocks
    var order = new OrderBuilder().Build();
    _orderServiceMock.Setup(/* ... */);

    // Act - Execute the method being tested
    var result = await _function.Run(order);

    // Assert - Verify expected behavior
    result.Success.Should().BeTrue();
}
```

### Test Isolation

**Each test should be independent:**
```csharp
// ✅ Good - Each test creates its own data
[Fact]
public async Task Test1()
{
    var order = new OrderBuilder().Build();
    // ...
}

[Fact]
public async Task Test2()
{
    var order = new OrderBuilder().Build();
    // ...
}

// ❌ Bad - Shared state between tests
private Order _sharedOrder;

[Fact]
public async Task Test1()
{
    _sharedOrder = new Order(); // Modifies shared state
}
```

### Meaningful Assertions

```csharp
// ✅ Good - Specific assertions with FluentAssertions
result.Success.Should().BeTrue();
result.OrderId.Should().Be("TEST-123");
result.Items.Should().HaveCount(3);

// ❌ Bad - Generic assertions
Assert.True(result.Success);
Assert.Equal("TEST-123", result.OrderId);
```

### Avoid Over-Mocking

```csharp
// ✅ Good - Mock external dependencies only
_orderRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<Order>()));

// ❌ Bad - Mocking simple objects or value types
var mockString = new Mock<string>(); // Don't do this
```

## Common Pitfalls (Phase 1)

| Pitfall | Impact | Solution |
|---------|--------|----------|
| **Skipping tests for "simple" code** | Bugs in "simple" code | Test everything with logic |
| **Testing implementation details** | Brittle tests | Test behavior, not internals |
| **Over-mocking** | False confidence | Mock only external dependencies |
| **Slow tests** | Reduced productivity | Keep tests < 100ms, remove I/O |
| **Hardcoded test data** | Brittle tests | Use builders, randomize data |
| **Ignoring test failures** | Degraded quality | Zero-tolerance policy |
| **No test maintenance** | Obsolete tests | Refactor tests with code |
| **Shared library inflating coverage denominator** | Coverage drops dramatically (e.g., to ~10%) when a shared library is included via `<ProjectReference>` | Add `<Exclude>[LibraryName]*</Exclude>` in `.runsettings` to exclude external assemblies from coverage instrumentation |
| **`ProjectReference` vs `PackageReference` coverage difference** | Switching a shared library from NuGet package to local project reference causes Coverlet to instrument it, inflating uncovered code | Always update `.runsettings` exclusions when changing a dependency from `<PackageReference>` to `<ProjectReference>` |
| **Overlooking model property branches** | Branch coverage blocked below target by trivial-looking nullable wrappers and `ShouldSerialize*` methods | Write targeted tests for nullable value-type properties and `ShouldSerialize*()` methods in model classes |

## Next Steps

After completing Phase 1:

1. **Validate Exit Criteria:** Ensure all exit criteria are met
2. **Team Retrospective:** Discuss what worked, what didn't
3. **Document Patterns:** Capture team-specific patterns
4. **Plan Phase 2:** Schedule Phase 2 kick-off
5. **Continue Excellence:** Maintain Phase 1 standards while adding Phase 2

## Related Documentation

- [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) - Complete testing standard
- [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md) - Test naming patterns
- [PHASE_2_INTEGRATION_TESTING.md](PHASE_2_INTEGRATION_TESTING.md) - Next phase guidance
- [QUICK_START.md](QUICK_START.md) - Getting started guide
