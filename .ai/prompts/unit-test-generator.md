# Unit Test Generator - AI Agent Instructions

## Purpose

This document provides comprehensive instructions for AI agents (ChatGPT, GitHub Copilot, or any LLM-based assistant) to generate deterministic, standards-compliant unit tests for Azure Integration Platform components.

**Target Audience:** AI agents tasked with generating unit tests for .NET 8 Azure Functions, Logic Apps dependencies, and integration components.

**Expected Outcome:** Production-ready xUnit test classes that follow naming conventions, use Test Builder pattern, achieve >80% code coverage, and integrate seamlessly with CI/CD pipelines.

---

## Core Principles

### ALWAYS Ask — NEVER Guess

**This is a BLOCKING rule.** When generating tests, the AI agent MUST ask the user for clarification instead of guessing whenever:

- **Missing context:** The purpose of a parameter, return type, or business rule is unclear
- **Ambiguous behavior:** A method could be interpreted in multiple ways
- **Missing CSV mapping specifications:** If mapper/converter functions exist but no CSV mapping spec files are provided (see [Section 1.4](#14-csv-mapping-specification-discovery))
- **Missing source code:** A referenced interface, model, or dependency cannot be found in the workspace
- **Unknown conventions:** The project uses patterns or naming not covered by this document
- **Coverage gaps:** It is unclear which components need tests vs. which are intentionally excluded

**Examples of asking vs. guessing:**

| Situation | ❌ Guessing (WRONG) | ✅ Asking (CORRECT) |
|-----------|---------------------|---------------------|
| Unknown return type meaning | Assume `ProcessResult.Data` is a string | "What type does `ProcessResult.Data` contain? I see it used in line X but can't infer the expected values." |
| Missing CSV mapping spec | Generate generic assertion `result.Should().NotBeNull()` | "Do you have CSV mapping specification files for the `OrderMapper`? These define source-to-target field mappings and enable precise test assertions." |
| Unclear validation rule | Invent a regex pattern | "What validation rules apply to `OrderId`? I see it's checked but the expected format isn't clear from the code." |
| Unknown dependency behavior | Mock to return null | "What should `IExternalService.GetDataAsync` return when called with an invalid ID? Should it return null, throw, or return an empty result?" |

### Use Subagents for Heavy Workloads

When the task involves multiple components or the workload is substantial, **delegate to subagents** to parallelize and maintain quality:

| Scenario | Subagent Usage |
|----------|----------------|
| Scanning the full codebase to identify all testable components | Use an **Explore** subagent for codebase scanning |
| Generating tests for 5+ classes simultaneously | Delegate each class or folder to a separate subagent |
| Researching existing test patterns in the project | Use an **Explore** subagent to gather patterns |
| Validating all generated tests compile and pass | Run build/test commands sequentially after generation |

**Subagent delegation rules:**
1. Use subagents for **read-only exploration** (finding components, reading files, identifying patterns)
2. Use subagents when generating tests for **3 or more classes** to parallelize analysis
3. Always **consolidate subagent results** before writing any files
4. The main agent retains responsibility for **final validation** (build + test pass)

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
│       │   ├── <IntegrationId>/             # Per-integration subfolder (e.g., OrderImport)
│       │   │   └── MyFunction.cs
│       │   └── <IntegrationId>/
│       ├── Services/                        # Business logic services
│       ├── Models/                          # DTOs, domain models
│       └── Infrastructure/                  # Azure SDK clients, repositories
└── tests/
    └── unit/
        ├── test.runsettings                 # Code coverage config (for Azure DevOps CI/CD)
        └── <ProjectName>.UnitTests/         # Unit test project (THIS IS YOUR OUTPUT)
            ├── Functions/                   # Test classes for Functions/
            │   └── <IntegrationId>/         # Mirror source subfolder structure
            │       ├── HappyPathTests.cs    # Category-based split (not #region blocks)
            │       ├── ErrorHandlingTests.cs
            │       ├── FilteringRulesTests.cs
            │       ├── EdgeCaseTests.cs
            │       └── TestData/            # Large XML/JSON sample constants
            │           └── SampleData.cs
            ├── Services/                    # Test classes for Services/
            ├── Models/                      # Test classes for Models/
            ├── Helpers/                     # Shared test utilities (XML builders, request helpers)
            │   └── TestHelper.cs
            └── Builders/                    # Test data builders (Builder pattern)
```

**Key Identification Rules:**
- Scan `src/` to find the main project (typically `*.FunctionApp.csproj`)
- Identify all classes in `Functions/`, `Services/`, `Models/` folders
- **Mirror subfolder structure:** If source has `Functions/OrderImport/`, create `Functions/OrderImport/` in tests
- **Split by category:** When a function has many tests (>15), split eg. into `HappyPathTests`, `ErrorHandlingTests`, `FilteringRulesTests`, `EdgeCaseTests` — NOT `#region` blocks
- **Extract large data:** Store large XML/JSON sample constants in `TestData/SampleData.cs` to keep test classes clean
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

### 1.3 Comprehensive Component Discovery

**CRITICAL RULE:** Every testable component in the Function App MUST have unit tests. Do not generate tests for only the class the user points to — scan the entire `src/` tree and ensure full coverage.

**Step 1: Scan all source folders**

Use a subagent (Explore) to identify ALL classes in:
- `Functions/` — HTTP/Timer/Queue-triggered function classes
- `Services/` — Business logic services
- `Models/` — Domain models with logic (e.g., `IsValid()` methods)
- `Validators/` — Validation classes
- `Mappers/` or `Converters/` — Data transformation classes
- `Helpers/` or `Utilities/` — Shared utility classes
- `Infrastructure/` — Repository or client wrapper classes

**Step 2: Determine test eligibility**

For each discovered class, classify:

| Class Type | Testable? | Test Category |
|------------|-----------|---------------|
| Services with business logic | ✅ Yes | Full test suite (happy/error/edge) |
| Functions (HTTP/Timer triggers) | ✅ Yes | Mock dependencies, test orchestration |
| Models with methods (IsValid, etc.) | ✅ Yes | Unit tests for all logic methods |
| Validators | ✅ Yes | All validation rules + boundary cases |
| Mappers/Converters | ✅ Yes | Field-level mapping assertions (use CSV specs!) |
| Static helpers/utilities | ✅ Yes | Input/output testing |
| DTOs/POCOs (no logic) | ❌ No | Skip — auto-properties only |
| `Program.cs` / DI registration | ❌ No | Skip — integration test scope |
| Interfaces | ❌ No | Skip — no implementation to test |

**Step 3: Report findings to the user**

Before generating tests, present a summary:
```
I found the following testable components:
  - Functions/: ProcessOrderFunction.cs (1 class)
  - Services/: OrderService.cs, ValidationService.cs (2 classes)
  - Models/: Order.cs (1 class with IsValid() method)
  - Mappers/: OrderMapper.cs (1 class — CSV mapping spec needed)

Components without tests yet: [list]
Components with existing tests: [list]

Shall I proceed with generating tests for all components, or do you want to focus on specific ones?
```

### 1.4 CSV Mapping Specification Discovery

**BLOCKING STEP for Mappers/Converters:** If the codebase contains mapper or converter classes, you MUST check for CSV mapping specification files before generating tests.

CSV mapping specifications define source-to-target field mappings and enable **precise, field-level test assertions** instead of generic `result.Should().NotBeNull()` checks.

**Where to look for CSV specs:**
1. A `mapping-spec/` or `mappings/` folder at the repo root or near `src/`
2. A `docs/mappings/` folder
3. Files named `*-mapping.csv`, `*-spec.csv`, or `*-fields.csv` anywhere in the repo
4. Ask the user directly

**CSV Spec Format (expected):**
```csv
SourceField,SourceType,TargetField,TargetType,Required,DefaultValue,TransformRule
order_id,string,OrderId,string,true,,direct
customer_name,string,CustomerName,string,true,,direct
order_amount,decimal,Amount,decimal,true,0.00,direct
order_date,string,OrderDate,DateTime,true,,parse:yyyy-MM-dd
status_code,int,Status,string,false,"Pending",lookup:StatusMap
```

**MANDATORY prompt when CSV specs are missing:**

> "I found mapper/converter classes that would benefit from CSV mapping specification files. These specs define source-to-target field mappings and enable accurate, field-level test assertions.
>
> Do you have CSV mapping specification files for this project? You can provide:
> - A single CSV file path
> - Multiple paths separated by semicolons
> - A folder containing .csv files
>
> Without CSV specs, I'll generate basic structural tests. With CSV specs, I'll generate precise field-level assertions that verify each mapping rule."

**When CSV specs ARE available — test generation rules:**

For each row in the CSV mapping spec, generate assertions like:
```csharp
[Fact]
public void Map_WhenSourceHasOrderId_ShouldMapToTargetOrderId()
{
    // Arrange
    var source = new SourceOrder { order_id = "ORD-123" };

    // Act
    var result = sut.Map(source);

    // Assert
    result.OrderId.Should().Be("ORD-123");
}

[Fact]
public void Map_WhenSourceHasOrderDate_ShouldParseToDateTime()
{
    // Arrange
    var source = new SourceOrder { order_date = "2026-01-15" };

    // Act
    var result = sut.Map(source);

    // Assert
    result.OrderDate.Should().Be(new DateTime(2026, 1, 15));
}

[Fact]
public void Map_WhenStatusCodeHasLookupValue_ShouldMapViaStatusMap()
{
    // Arrange
    var source = new SourceOrder { status_code = 1 };

    // Act
    var result = sut.Map(source);

    // Assert
    result.Status.Should().Be("Active"); // Based on StatusMap lookup
}

[Fact]
public void Map_WhenOptionalFieldIsMissing_ShouldUseDefaultValue()
{
    // Arrange — status_code not set (default)
    var source = new SourceOrder();

    // Act
    var result = sut.Map(source);

    // Assert
    result.Status.Should().Be("Pending"); // Default from CSV spec
}
```

**When CSV specs are NOT available (user confirms):**
- Generate structural tests only (method exists, doesn't throw, returns non-null)
- Add `// TODO: Add field-level assertions when CSV mapping spec is available` comments
- Log a warning in the test generation summary

### 1.5 Dependency Graph Analysis

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

**Step 0: Discover ALL Testable Components (MANDATORY FIRST STEP)**
```
Actions:
  1. Use an Explore subagent to scan the entire src/ tree
  2. Identify ALL classes in Functions/, Services/, Models/, Mappers/,
     Validators/, Helpers/, Infrastructure/ folders
  3. Classify each class as testable or not (see Section 1.3)
  4. Check for existing test files in tests/unit/
  5. Identify components WITHOUT tests (these are gaps to fill)
  6. Present findings to the user and ask for confirmation before proceeding
  7. If the workload involves 3+ classes, plan subagent delegation
```

**Step 1: Check for CSV Mapping Specifications (BLOCKING for Mappers)**
```
Actions:
  1. Search for CSV mapping spec files in the repo (see Section 1.4)
  2. If mapper/converter classes exist but NO CSV specs are found:
     → STOP and ask the user to provide them
     → Only proceed without CSV specs if user explicitly confirms they don't have them
  3. If CSV specs are found, parse them to extract field mappings
  4. Match each CSV spec to its corresponding mapper class
```

**Step 2: Analyze Target Classes**
```
Input: All testable classes identified in Step 0
Actions:
  1. Read each file's content
  2. Identify class name, namespace, constructor dependencies
  3. Extract all public methods with signatures
  4. Note return types, parameters, async/sync patterns
  5. For mappers: load corresponding CSV spec field mappings
  6. ASK the user if any method's purpose or expected behavior is unclear
```

**Step 3: Check for Existing Tests & Builders**
```
Input: tests/unit/<Project>.UnitTests/
Actions:
  1. Search for existing test files — identify gaps vs. what already exists
  2. Search for existing builders (e.g., OrderBuilder.cs) — reuse them
  3. If builders are missing for complex models, generate new ones
  4. If tests already exist but are incomplete (stubbed / TODO comments):
     → Fill in the test logic using CSV mapping specs and source code analysis
     → Do NOT delete or overwrite working tests
```

**Step 4: Generate / Fill Test Methods**
```
For each public method in each source class:
  1. Happy path test (valid inputs, expected success)
  2. Null/invalid input tests (ArgumentNullException, ArgumentException)
  3. Dependency failure tests (when mocked service throws)
  4. Business logic branches (if/else paths)
  5. Edge cases (empty collections, boundary values)
  6. For mappers WITH CSV specs:
     → Field-level mapping assertions for EACH row in the CSV
     → Transform rule validation (parse, lookup, direct copy)
     → Default value assertions for optional fields
     → Required field validation tests
  7. For mappers WITHOUT CSV specs (user confirmed none available):
     → Structural tests only + TODO comments for field-level assertions
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

**Step 7: Maximize Code Coverage**
```
After generating initial tests, evaluate coverage:
  1. Count all branches (if/else, switch, ternary, null-coalescing)
  2. Ensure each branch has at least one test
  3. Add boundary value tests where applicable
  4. Add tests for exception handlers and catch blocks
  5. Target: >80% per class, strive for >90% on business logic
  6. If coverage is estimated below 80%, add additional tests:
     → Identify uncovered lines/branches
     → Generate targeted tests for each gap
     → ASK the user if the expected behavior of uncovered code is unclear
```

**Step 8: Build & Test Verification (MANDATORY)**
```
After generating/filling all test files:
  1. Run `dotnet restore` on the test project
  2. Run `dotnet build` on the test project
  3. Run `dotnet test` to verify all tests pass
  4. If build errors occur:
     → Fix simple issues (missing usings, wrong namespaces, type mismatches)
     → For complex issues, report to the user with clear fix instructions
  5. If tests fail:
     → Analyze the failure output
     → ASK the user about expected behavior if the failure suggests a misunderstanding
     → Fix the test if the source code behavior is clear
  6. ALL tests MUST be passing before marking the task as complete
```

**Step 9: Self-Review Checklist**
```
Before finalizing:
  ✓ ALL testable components have test classes (not just the one the user asked about)
  ✓ Naming conventions followed (MethodName_Scenario_ExpectedBehavior)
  ✓ AAA pattern clear (comments included)
  ✓ All dependencies mocked
  ✓ No hardcoded values (use builders or constants)
  ✓ Async tests use await properly
  ✓ CancellationToken.None used when required
  ✓ CSV mapping specs consumed for all mapper tests (or user confirmed none exist)
  ✓ Coverage > 80% estimated (>90% for business logic)
  ✓ All tests compile and pass (`dotnet test` succeeds)
  ✓ No guessed behavior — all unclear items were asked about
```

### 8.2 Automated Generation Template

**Prompt for AI agent self-invocation:**

```
Generate unit tests for the following project:

PROJECT ROOT: {projectRootPath}
SOURCE FOLDER: {srcFolderPath}
EXISTING TESTS FOLDER: {testsFolderPath} (may be empty or partially complete)

CSV MAPPING SPECS: {csvSpecPaths | "NONE — user confirmed" | "NOT YET ASKED — must ask user"}

REQUIREMENTS:
1. Scan ALL testable components in the source folder (see Section 1.3)
2. Test project name: {sourceProjectName}.UnitTests
3. Use existing builders from: tests/unit/{projectName}.UnitTests/Builders/
4. Follow naming: MethodName_Scenario_ExpectedBehavior
5. Target coverage: >80% per class, >90% for business logic
6. Use xUnit, Moq, FluentAssertions
7. Mock all dependencies
8. For mappers: use CSV mapping specs for field-level assertions
9. Fill in existing stubbed/TODO tests with real logic
10. ASK about any unclear behavior — do NOT guess
11. Verify all tests compile and pass before completing

WORKFLOW:
- Step 0: Discover all testable components (use Explore subagent)
- Step 1: Check for CSV mapping specs (BLOCKING for mappers)
- Step 2-9: Follow Generation Workflow in Section 8.1

OUTPUT:
- Test class files for ALL testable components
- Build verification results
- Coverage estimation summary
- List of questions asked to the user (if any)
```

### 8.3 Subagent Delegation Strategy

When the codebase has many testable components, delegate work to subagents:

```
Main Agent Responsibilities:
  - Orchestrate the overall workflow
  - Ask the user for CSV specs and clarifications
  - Make final decisions on test structure
  - Run build and test verification
  - Consolidate results and report to the user

Explore Subagent Tasks:
  - Scan src/ for all testable classes
  - Read existing test files to identify gaps
  - Search for CSV mapping spec files
  - Read builder classes to understand existing patterns
  - Analyze dependency graphs across classes

Generation Subagent Tasks (when 3+ classes need tests):
  - Generate test classes for assigned components
  - Each subagent handles one folder (e.g., Services/, Functions/, Models/)
  - Returns generated test code for main agent to write and validate
```

**Delegation decision tree:**
```
How many testable classes need new/updated tests?
├─ 1-2 classes → Handle directly (no subagents needed)
├─ 3-5 classes → Use Explore subagent for analysis, generate directly
└─ 6+ classes → Use Explore subagent for analysis + delegate generation
```

---

## 9. Validation Checklist

Before finalizing generated tests, verify:

### 9.1 Component Completeness
- [ ] ALL testable components in the Function App have been identified
- [ ] ALL testable components have corresponding test classes
- [ ] No testable class was skipped without explicit user confirmation
- [ ] Existing stubbed/TODO tests have been filled with real logic

### 9.2 CSV Mapping Specification
- [ ] Mapper/converter classes checked for corresponding CSV specs
- [ ] User was prompted for CSV specs if not found (BLOCKING)
- [ ] CSV-driven tests include field-level assertions for each mapping row
- [ ] Transform rules (parse, lookup, direct) are tested individually
- [ ] Default values for optional fields are tested
- [ ] Required field validation is tested
- [ ] If no CSV specs available, user explicitly confirmed and TODO comments added

### 9.3 Functional Requirements
- [ ] All public methods have at least one test
- [ ] Happy path covered
- [ ] Exception paths covered
- [ ] Null/invalid input handling tested
- [ ] All if/else branches covered

### 9.4 Code Quality
- [ ] Naming follows pattern: `MethodName_Scenario_ExpectedBehavior`
- [ ] AAA pattern used consistently
- [ ] No code duplication (use builders/helpers)
- [ ] FluentAssertions used for readability
- [ ] Comments explain complex setups

### 9.5 Isolation & Mocking
- [ ] All external dependencies mocked
- [ ] No real database connections
- [ ] No real HTTP calls
- [ ] No real file I/O
- [ ] ILogger mocked correctly

### 9.6 CI/CD Compatibility
- [ ] Tests run with `dotnet test` without errors
- [ ] No environment variable dependencies
- [ ] No local file path dependencies
- [ ] Execution time <100ms per test
- [ ] No flaky tests (deterministic outcomes)

### 9.7 Coverage Metrics
- [ ] Estimated line coverage >80% per class (>90% for business logic)
- [ ] All branches covered
- [ ] Exception handlers tested
- [ ] Return value validations included
- [ ] Additional tests added where coverage falls below threshold

### 9.8 Build & Test Verification
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build` succeeds with no errors
- [ ] `dotnet test` passes — ALL tests green
- [ ] No test was left in a failing state

### 9.9 Information Integrity
- [ ] No behavior was guessed — all ambiguities were asked about
- [ ] All user-provided answers are reflected in tests
- [ ] Questions log is available for audit

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
✅ Achieve >80% code coverage (>90% for business logic)  
✅ Use Test Builder pattern  
✅ Integrate with CI/CD pipelines  
✅ Align with 60-30-10 test pyramid  
✅ Are maintainable and readable  
✅ Cover ALL testable components in the Function App  
✅ Use CSV mapping specifications for precise mapper/converter tests  
✅ Never guess — always ask for clarification  
✅ Leverage subagents for heavy workloads  
✅ Verify all tests compile and pass before completing  

**Usage:** Point any AI agent (ChatGPT, Copilot, Claude) to this document before requesting test generation.