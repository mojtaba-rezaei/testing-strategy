# Unit Test Generator - AI Agent Instructions

## Purpose

This document provides comprehensive instructions for AI agents (ChatGPT, GitHub Copilot, or any LLM-based assistant) to generate deterministic, standards-compliant unit tests for Azure Integration Platform components.

**Target Audience:** AI agents tasked with generating unit tests for .NET 8 Azure Functions, Logic Apps dependencies, and integration components.

**Expected Outcome:** Production-ready xUnit test classes that follow naming conventions, use Test Builder pattern, achieve >80% code coverage, and integrate seamlessly with CI/CD pipelines.

---

## Table of Contents

1. [Codebase Understanding](#codebase-understanding)
2. [Test Architecture & Pyramid](#test-architecture--pyramid)
3. [Naming Conventions](#naming-conventions)
4. [Test Structure & Patterns](#test-structure--patterns)
5. [Dependencies & Test Doubles](#dependencies--test-doubles)
6. [Code Coverage Requirements](#code-coverage-requirements)
7. [CI/CD Integration](#cicd-integration)
8. [Generation Workflow](#generation-workflow)
9. [Validation Checklist](#validation-checklist)
10. [Examples](#examples)

---

## 1. Codebase Understanding

### 1.1 Project Structure

Before generating tests, analyze the codebase structure:

```
<Solution>/
├── src/
│   └── <ProjectName>.FunctionApp/          # Azure Functions project
│       ├── Functions/                       # HTTP/Timer/Queue triggered functions
│       ├── Services/                        # Business logic services
│       ├── Models/                          # DTOs, domain models
│       └── Infrastructure/                  # Azure SDK clients, repositories
└── tests/
    └── unit/
        └── <ProjectName>.UnitTests/         # Unit test project (THIS IS YOUR OUTPUT)
            ├── Functions/                   # Test classes for Functions/
            ├── Services/                    # Test classes for Services/
            ├── Models/                      # Test classes for Models/
            ├── Builders/                    # Test data builders
            └── Infrastructure/              # Test classes for Infrastructure/
```

**Key Identification Rules:**
- Scan `src/` to find the main project (typically `*.FunctionApp.csproj`)
- Identify all classes in `Functions/`, `Services/`, `Models/` folders
- Check for existing `Builders/` folder in tests - reuse existing builders
- Verify test project naming: `<ProjectName>.UnitTests.csproj`

### 1.2 Technology Stack Detection

**Required NuGet Packages (must be present in test project):**
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xUnit" Version="2.6.2" />
<PackageReference Include="xUnit.runner.visualstudio" Version="2.5.4" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="AutoFixture" Version="4.18.0" />
<PackageReference Include="Coverlet.collector" Version="6.0.0" />
```

**Azure-Specific Dependencies (install if needed):**
- `Microsoft.Azure.Functions.Worker` (for function testing)
- `Microsoft.Extensions.Logging.Abstractions` (for ILogger mocking)

### 1.3 Dependency Graph Analysis

**For each class under test:**
1. Identify constructor dependencies (interfaces to mock)
2. Find external dependencies (Azure clients, databases, HTTP clients)
3. Locate domain models and DTOs used in method signatures
4. Check for static dependencies or DateTime.Now usage (needs abstraction)

**Example Analysis for `OrderService`:**
```csharp
// Source: src/OrderProcessor.FunctionApp/Services/OrderService.cs
public class OrderService : IOrderService
{
    private readonly IValidationService _validationService;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(IValidationService validationService, ILogger<OrderService> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }
    
    public async Task<ProcessResult> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
    {
        // ... implementation
    }
}
```

**AI Agent Takeaway:**
- Mock: `IValidationService`, `ILogger<OrderService>`
- Test data: `Order` (create builder), `ProcessResult` (assert on)
- Method signature: async Task, requires cancellation token

---

## 2. Test Architecture & Pyramid

### 2.1 Test Distribution Pattern

**CRITICAL RULE:** Follow the 60-30-10 test pyramid architecture.

**Unit Tests (60% of total tests):**
- Focus: Single class/method isolation
- Scope: Services, models, validators, helpers, utilities
- Mocking: All external dependencies
- Performance: <100ms per test
- Coverage target: >80% per class

**What NOT to test in unit tests:**
- ❌ Actual Azure services (Service Bus, Cosmos DB, Key Vault)
- ❌ Database queries
- ❌ HTTP calls to external APIs
- ❌ File system I/O
- ❌ Multi-component workflows

### 2.2 Test Scope Decision Tree

When generating tests for a class, ask:

```
Is the class a pure service/model with no external I/O?
├─ YES → Generate UNIT tests with full mocking
└─ NO  → Is it an Azure Function or repository with external dependencies?
    └─ YES → Generate UNIT tests that mock Azure clients
               (Integration tests belong in Phase 2, not your concern)
```

**Unit Test Eligibility:**
- ✅ Business logic services (`*Service.cs`)
- ✅ Validators (`*Validator.cs`)
- ✅ Domain models with methods (`Order.cs` with validation logic)
- ✅ Mappers/converters
- ✅ In-memory utilities (string formatters, calculators)

---

## 3. Naming Conventions

### 3.1 Test Project Naming

**Pattern:** `<SourceProjectName>.UnitTests`

**Examples:**
- `OrderProcessor.FunctionApp` → `OrderProcessor.UnitTests`
- `LogicAppConnector.Library` → `LogicAppConnector.UnitTests`

**File Location:** Always under `tests/unit/` directory.

### 3.2 Test Class Naming

**Pattern:** `<ClassUnderTest>Tests`

**Examples:**
- Class under test: `OrderService` → Test class: `OrderServiceTests`
- Class under test: `ValidationService` → Test class: `ValidationServiceTests`
- Class under test: `Order` (model) → Test class: `OrderTests`

**Namespace:** Must mirror source project with `.UnitTests` suffix.

```csharp
// Source: OrderProcessor.FunctionApp.Services.OrderService
namespace OrderProcessor.FunctionApp.Services;

// Test: OrderProcessor.UnitTests.Services.OrderServiceTests
namespace OrderProcessor.UnitTests.Services;
```

### 3.3 Test Method Naming

**Pattern (STRICT):** `MethodName_Scenario_ExpectedBehavior`

**Components:**
- **MethodName:** Exact name of method being tested
- **Scenario:** Specific condition or input state (e.g., `WhenOrderIsValid`, `WhenDependencyThrows`)
- **ExpectedBehavior:** Outcome verification (e.g., `ShouldReturnSuccess`, `ShouldThrowException`)

**Examples:**
```csharp
[Fact]
public async Task ProcessOrderAsync_WhenOrderIsValid_ShouldReturnSuccessResult()

[Fact]
public async Task ProcessOrderAsync_WhenValidationFails_ShouldReturnFailureResult()

[Fact]
public async Task ProcessOrderAsync_WhenCancellationRequested_ShouldThrowOperationCanceledException()

[Theory]
[InlineData(null)]
[InlineData("")]
public void ValidateOrder_WhenOrderIdIsNullOrEmpty_ShouldThrowArgumentException(string invalidId)
```

### 3.4 Builder Naming

**Pattern:** `<EntityName>Builder`

**File location:** `tests/unit/<Project>.UnitTests/Builders/`

**Example:**
```csharp
// File: OrderBuilder.cs
namespace OrderProcessor.UnitTests.Builders;

public class OrderBuilder
{
    private Order _order = new Order
    {
        OrderId = "ORD-12345",
        CustomerId = "CUST-001",
        Amount = 100.00m,
        Status = "Pending"
    };

    public OrderBuilder WithOrderId(string orderId)
    {
        _order.OrderId = orderId;
        return this;
    }

    public OrderBuilder WithInvalidAmount()
    {
        _order.Amount = -10.00m;
        return this;
    }

    public Order Build() => _order;
}
```

**Usage in tests:**
```csharp
var order = new OrderBuilder()
    .WithOrderId("TEST-001")
    .Build();
```

---

## 4. Test Structure & Patterns

### 4.1 AAA Pattern (Mandatory)

Every test MUST follow Arrange-Act-Assert with clear sections:

```csharp
[Fact]
public async Task ProcessOrderAsync_WhenOrderIsValid_ShouldReturnSuccessResult()
{
    // Arrange
    var mockValidationService = new Mock<IValidationService>();
    mockValidationService
        .Setup(x => x.ValidateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);
    
    var mockLogger = new Mock<ILogger<OrderService>>();
    var sut = new OrderService(mockValidationService.Object, mockLogger.Object);
    var order = new OrderBuilder().Build();
    var cancellationToken = CancellationToken.None;

    // Act
    var result = await sut.ProcessOrderAsync(order, cancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
    result.Message.Should().Be("Order processed successfully");
    
    // Verify interactions
    mockValidationService.Verify(
        x => x.ValidateAsync(order, cancellationToken),
        Times.Once
    );
}
```

### 4.2 System Under Test (SUT) Pattern

**Always name the instance under test `sut`:**
```csharp
var sut = new OrderService(mockValidationService.Object, mockLogger.Object);
```

**Benefits:**
- Clear identification of what's being tested
- Consistent across all test classes
- Aligns with industry standards

### 4.3 Test Class Structure Template

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessor.FunctionApp.Models;
using OrderProcessor.FunctionApp.Services;
using OrderProcessor.UnitTests.Builders;
using Xunit;

namespace OrderProcessor.UnitTests.Services;

public class OrderServiceTests
{
    // Private fields for commonly used mocks (optional pattern)
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;

    public OrderServiceTests()
    {
        // Constructor for shared setup (use when multiple tests share setup)
        _mockValidationService = new Mock<IValidationService>();
        _mockLogger = new Mock<ILogger<OrderService>>();
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        // ... test-specific setup

        // Act
        // ... invoke method

        // Assert
        // ... verify outcome
    }

    // More test methods...
}
```

---

## 5. Dependencies & Test Doubles

### 5.1 Moq Setup Patterns

**Return value mocking:**
```csharp
mockService
    .Setup(x => x.GetDataAsync(It.IsAny<string>()))
    .ReturnsAsync(expectedData);
```

**Exception throwing:**
```csharp
mockService
    .Setup(x => x.ProcessAsync(It.IsAny<Request>()))
    .ThrowsAsync(new InvalidOperationException("Processing failed"));
```

**Conditional returns (using It.Is):**
```csharp
mockValidator
    .Setup(x => x.ValidateAsync(It.Is<Order>(o => o.Amount > 0), It.IsAny<CancellationToken>()))
    .ReturnsAsync(true);

mockValidator
    .Setup(x => x.ValidateAsync(It.Is<Order>(o => o.Amount <= 0), It.IsAny<CancellationToken>()))
    .ReturnsAsync(false);
```

**Verify method calls:**
```csharp
// Verify called exactly once
mockService.Verify(x => x.LogEvent(It.IsAny<string>()), Times.Once);

// Verify called with specific argument
mockService.Verify(x => x.LogEvent("Order processed"), Times.Once);

// Verify never called
mockService.Verify(x => x.SendEmail(It.IsAny<string>()), Times.Never);
```

### 5.2 ILogger Mocking Pattern

**Standard approach for ILogger<T>:**
```csharp
var mockLogger = new Mock<ILogger<OrderService>>();

// No need to setup - logger methods return void
// For verification (optional):
mockLogger.Verify(
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Order processed")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
    Times.Once);
```

### 5.3 Azure Client Mocking

**Do NOT mock sealed Azure SDK classes directly.** Instead:

**Option 1: Create abstraction interfaces**
```csharp
// In source code: Create IServiceBusClient interface
public interface IServiceBusClient
{
    Task SendMessageAsync(string queueName, string message);
}

// Mock in tests
var mockServiceBusClient = new Mock<IServiceBusClient>();
mockServiceBusClient
    .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
    .Returns(Task.CompletedTask);
```

**Option 2: Use Azure SDK's mockable patterns (when available)**
```csharp
// Azure SDK provides mockable Response<T> patterns
var mockResponse = new Mock<Response<SecretBundle>>();
mockResponse.Setup(r => r.Value).Returns(new SecretBundle { Value = "secret-value" });
```

---

## 6. Code Coverage Requirements

### 6.1 Coverage Targets

**Per-Class Minimum:** 80% line coverage

**What to cover:**
- ✅ All public methods
- ✅ All branches (if/else, switch cases)
- ✅ Exception handling paths
- ✅ Validation logic
- ✅ Business rules

**What can be excluded:**
- ❌ Auto-properties (get; set;)
- ❌ Constructors with only assignments
- ❌ DTOs/POCOs without logic

### 6.2 Branch Coverage Strategy

**Example: Cover all conditional paths**

```csharp
// Source code
public class ValidationService
{
    public ValidationResult Validate(Order order)
    {
        if (order == null)
            return ValidationResult.Fail("Order is null");
        
        if (order.Amount <= 0)
            return ValidationResult.Fail("Amount must be positive");
        
        if (string.IsNullOrEmpty(order.OrderId))
            return ValidationResult.Fail("OrderId is required");
        
        return ValidationResult.Success();
    }
}

// Tests - Cover all branches
[Fact]
public void Validate_WhenOrderIsNull_ShouldReturnFailure()
{
    // Covers: order == null branch
}

[Fact]
public void Validate_WhenAmountIsZero_ShouldReturnFailure()
{
    // Covers: order.Amount <= 0 branch
}

[Fact]
public void Validate_WhenOrderIdIsEmpty_ShouldReturnFailure()
{
    // Covers: string.IsNullOrEmpty branch
}

[Fact]
public void Validate_WhenOrderIsValid_ShouldReturnSuccess()
{
    // Covers: happy path (no failures)
}
```

### 6.3 Coverage Reporting

Tests will be executed with:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Verification command (AI agents can suggest this to users):**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
```

---

## 7. CI/CD Integration

### 7.1 Azure Pipelines Integration

Generated tests MUST work with this pipeline configuration:

```yaml
# From samples/pipelines/azure-pipelines-unit-tests.yml
trigger:
  branches:
    include:
      - main
      - develop
  paths:
    include:
      - 'src/**'
      - 'tests/unit/**'

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  displayName: 'Install .NET 8 SDK'
  inputs:
    version: '8.x'

- task: DotNetCoreCLI@2
  displayName: 'Restore NuGet packages'
  inputs:
    command: 'restore'
    projects: '**/*.UnitTests.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests with Coverage'
  inputs:
    command: 'test'
    projects: '**/tests/unit/**/*.UnitTests.csproj'
    arguments: '--no-restore --configuration Release --collect:"XPlat Code Coverage" --logger trx'
    publishTestResults: true

- task: PublishCodeCoverageResults@1
  displayName: 'Publish Code Coverage'
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
    failIfCoverageEmpty: true
```

**Requirements for AI-generated tests:**
- Must target .NET 8 (`<TargetFramework>net8.0</TargetFramework>`)
- Must use xUnit test framework
- Must restore without errors
- Must execute in `dotnet test` without requiring additional setup

### 7.2 GitHub Actions Integration

```yaml
# From .github/workflows/unit-tests.yml
name: Unit Tests

on:
  pull_request:
    branches: [main, develop]
    paths:
      - 'src/**'
      - 'tests/unit/**'

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Run unit tests
        run: dotnet test tests/unit/**/*.UnitTests.csproj --no-restore --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: '**/coverage.cobertura.xml'
          flags: unittests
          fail_ci_if_error: true
```

**Test execution expectations:**
- Fast execution (<5 minutes for 100 tests)
- No flaky tests (deterministic outcomes)
- No external dependencies (all mocked)
- Parallel execution safe (no shared state)

---

## 8. Generation Workflow

### 8.1 Step-by-Step Process

**Step 1: Analyze Target Class**
```
Input: Path to source code file (e.g., src/Services/OrderService.cs)
Actions:
  1. Read file content
  2. Identify class name, namespace, constructor dependencies
  3. Extract all public methods with signatures
  4. Note return types, parameters, async/sync patterns
```

**Step 2: Check for Existing Builders**
```
Input: tests/unit/<Project>.UnitTests/Builders/
Actions:
  1. Search for existing builders (e.g., OrderBuilder.cs)
  2. If found, use them in generated tests
  3. If missing, generate new builder for complex models
```

**Step 3: Generate Test Class Skeleton**
```
Output file: tests/unit/<Project>.UnitTests/<Folder>/<ClassName>Tests.cs
Content:
  - Namespace matching source + .UnitTests
  - Using statements (xUnit, Moq, FluentAssertions, Builders)
  - Class declaration: public class <ClassName>Tests
  - Mock fields if shared across tests
```

**Step 4: Generate Test Methods**
```
For each public method in source class:
  1. Happy path test (valid inputs, expected success)
  2. Null/invalid input tests (ArgumentNullException, ArgumentException)
  3. Dependency failure tests (when mocked service throws)
  4. Business logic branches (if/else paths)
  5. Edge cases (empty collections, boundary values)
```

**Step 5: Add FluentAssertions**
```
Replace xUnit assertions with FluentAssertions:
  - Assert.NotNull(result) → result.Should().NotBeNull()
  - Assert.Equal(expected, actual) → actual.Should().Be(expected)
  - Assert.True(condition) → condition.Should().BeTrue()
  - Assert.Throws<Exception>(() => ...) → Action act = () => ...; act.Should().Throw<Exception>()
```

**Step 6: Verify Moq Calls**
```
Add verification for critical dependencies:
  mockService.Verify(x => x.MethodName(It.IsAny<Type>()), Times.Once);
```

**Step 7: Self-Review Checklist**
```
Before finalizing:
  ✓ Naming conventions followed (MethodName_Scenario_ExpectedBehavior)
  ✓ AAA pattern clear (comments included)
  ✓ All dependencies mocked
  ✓ No hardcoded values (use builders or constants)
  ✓ Async tests use await properly
  ✓ CancellationToken.None used when required
  ✓ Coverage > 80% estimated
```

### 8.2 Automated Generation Template

**Prompt for AI agent self-invocation:**

```
Generate unit tests for the following class:

SOURCE FILE: {filePath}
SOURCE CODE:
```csharp
{sourceCodeContent}
```

REQUIREMENTS:
1. Test project name: {sourceProjectName}.UnitTests
2. Test class name: {className}Tests
3. Use existing builders from: tests/unit/{projectName}.UnitTests/Builders/
4. Follow naming: MethodName_Scenario_ExpectedBehavior
5. Target coverage: >80%
6. Use xUnit, Moq, FluentAssertions
7. Mock all dependencies: {listOfDependencies}

OUTPUT:
- Full test class code ready to save to: tests/unit/{projectName}.UnitTests/{folder}/{className}Tests.cs
```

---

## 9. Validation Checklist

Before finalizing generated tests, verify:

### 9.1 Functional Requirements
- [ ] All public methods have at least one test
- [ ] Happy path covered
- [ ] Exception paths covered
- [ ] Null/invalid input handling tested
- [ ] All if/else branches covered

### 9.2 Code Quality
- [ ] Naming follows pattern: `MethodName_Scenario_ExpectedBehavior`
- [ ] AAA pattern used consistently
- [ ] No code duplication (use builders/helpers)
- [ ] FluentAssertions used for readability
- [ ] Comments explain complex setups

### 9.3 Isolation & Mocking
- [ ] All external dependencies mocked
- [ ] No real database connections
- [ ] No real HTTP calls
- [ ] No real file I/O
- [ ] ILogger mocked correctly

### 9.4 CI/CD Compatibility
- [ ] Tests run with `dotnet test` without errors
- [ ] No environment variable dependencies
- [ ] No local file path dependencies
- [ ] Execution time <100ms per test
- [ ] No flaky tests (deterministic outcomes)

### 9.5 Coverage Metrics
- [ ] Estimated line coverage >80%
- [ ] All branches covered
- [ ] Exception handlers tested
- [ ] Return value validations included

---

## 10. Examples

### 10.1 Complete Service Test Example

**Source Code:**
```csharp
// File: src/OrderProcessor.FunctionApp/Services/OrderService.cs
namespace OrderProcessor.FunctionApp.Services;

public class OrderService : IOrderService
{
    private readonly IValidationService _validationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IValidationService validationService, ILogger<OrderService> logger)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcessResult> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _logger.LogInformation("Processing order {OrderId}", order.OrderId);

        var isValid = await _validationService.ValidateAsync(order, cancellationToken);
        if (!isValid)
        {
            _logger.LogWarning("Order {OrderId} validation failed", order.OrderId);
            return new ProcessResult { Success = false, Message = "Validation failed" };
        }

        _logger.LogInformation("Order {OrderId} processed successfully", order.OrderId);
        return new ProcessResult { Success = true, Message = "Order processed successfully" };
    }
}
```

**Generated Test Class:**
```csharp
// File: tests/unit/OrderProcessor.UnitTests/Services/OrderServiceTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessor.FunctionApp.Models;
using OrderProcessor.FunctionApp.Services;
using OrderProcessor.UnitTests.Builders;
using Xunit;

namespace OrderProcessor.UnitTests.Services;

public class OrderServiceTests
{
    [Fact]
    public void Constructor_WhenValidationServiceIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OrderService>>();

        // Act
        Action act = () => new OrderService(null, mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("validationService");
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockValidationService = new Mock<IValidationService>();

        // Act
        Action act = () => new OrderService(mockValidationService.Object, null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task ProcessOrderAsync_WhenOrderIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockValidationService = new Mock<IValidationService>();
        var mockLogger = new Mock<ILogger<OrderService>>();
        var sut = new OrderService(mockValidationService.Object, mockLogger.Object);

        // Act
        Func<Task> act = async () => await sut.ProcessOrderAsync(null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("order");
    }

    [Fact]
    public async Task ProcessOrderAsync_WhenOrderIsValid_ShouldReturnSuccessResult()
    {
        // Arrange
        var mockValidationService = new Mock<IValidationService>();
        mockValidationService
            .Setup(x => x.ValidateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockLogger = new Mock<ILogger<OrderService>>();
        var sut = new OrderService(mockValidationService.Object, mockLogger.Object);
        var order = new OrderBuilder().Build();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.ProcessOrderAsync(order, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order processed successfully");

        // Verify validation was called
        mockValidationService.Verify(
            x => x.ValidateAsync(order, cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessOrderAsync_WhenValidationFails_ShouldReturnFailureResult()
    {
        // Arrange
        var mockValidationService = new Mock<IValidationService>();
        mockValidationService
            .Setup(x => x.ValidateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var mockLogger = new Mock<ILogger<OrderService>>();
        var sut = new OrderService(mockValidationService.Object, mockLogger.Object);
        var order = new OrderBuilder().Build();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await sut.ProcessOrderAsync(order, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");

        // Verify validation was called
        mockValidationService.Verify(
            x => x.ValidateAsync(order, cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessOrderAsync_WhenCancellationRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var mockValidationService = new Mock<IValidationService>();
        mockValidationService
            .Setup(x => x.ValidateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var mockLogger = new Mock<ILogger<OrderService>>();
        var sut = new OrderService(mockValidationService.Object, mockLogger.Object);
        var order = new OrderBuilder().Build();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        Func<Task> act = async () => await sut.ProcessOrderAsync(order, cancellationTokenSource.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
```

### 10.2 Model Test Example

**Source Code:**
```csharp
// File: src/OrderProcessor.FunctionApp/Models/Order.cs
namespace OrderProcessor.FunctionApp.Models;

public class Order
{
    public string OrderId { get; set; }
    public string CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(OrderId) 
            && !string.IsNullOrEmpty(CustomerId) 
            && Amount > 0;
    }
}
```

**Generated Test Class:**
```csharp
// File: tests/unit/OrderProcessor.UnitTests/Models/OrderTests.cs
using FluentAssertions;
using OrderProcessor.FunctionApp.Models;
using Xunit;

namespace OrderProcessor.UnitTests.Models;

public class OrderTests
{
    [Fact]
    public void IsValid_WhenAllFieldsAreValid_ShouldReturnTrue()
    {
        // Arrange
        var order = new Order
        {
            OrderId = "ORD-12345",
            CustomerId = "CUST-001",
            Amount = 100.00m,
            Status = "Pending"
        };

        // Act
        var result = order.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WhenOrderIdIsNullOrEmpty_ShouldReturnFalse(string invalidOrderId)
    {
        // Arrange
        var order = new Order
        {
            OrderId = invalidOrderId,
            CustomerId = "CUST-001",
            Amount = 100.00m,
            Status = "Pending"
        };

        // Act
        var result = order.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_WhenCustomerIdIsNullOrEmpty_ShouldReturnFalse(string invalidCustomerId)
    {
        // Arrange
        var order = new Order
        {
            OrderId = "ORD-12345",
            CustomerId = invalidCustomerId,
            Amount = 100.00m,
            Status = "Pending"
        };

        // Act
        var result = order.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.00)]
    [InlineData(-0.01)]
    public void IsValid_WhenAmountIsZeroOrNegative_ShouldReturnFalse(decimal invalidAmount)
    {
        // Arrange
        var order = new Order
        {
            OrderId = "ORD-12345",
            CustomerId = "CUST-001",
            Amount = invalidAmount,
            Status = "Pending"
        };

        // Act
        var result = order.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}
```

---

## Conclusion

This guide provides deterministic, comprehensive instructions for AI agents to generate production-ready unit tests. By following these rules, any AI assistant can generate tests that:

✅ Follow naming conventions  
✅ Achieve >80% code coverage  
✅ Use Test Builder pattern  
✅ Integrate with CI/CD pipelines  
✅ Align with 60-30-10 test pyramid  
✅ Are maintainable and readable  

**Usage:** Point any AI agent (ChatGPT, Copilot, Claude) to this document before requesting test generation.