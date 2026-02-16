# Naming Conventions for Automated Tests

## Purpose
This document defines the standard naming conventions for all automated tests in Azure Integration Platform projects. Consistent naming improves code readability, maintainability, and helps developers quickly understand test intent and scope.

## Reference
This is extracted from [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) Section 5.

## Test Project Naming

### Unit Test Projects
**Pattern:** `<Component>.UnitTests`

**Examples:**
- ✅ `OrderProcessor.UnitTests`
- ✅ `PaymentService.UnitTests`
- ✅ `InventoryManager.UnitTests`
- ✅ `CustomerValidator.UnitTests`

**Anti-Patterns:**
- ❌ `OrderProcessorTests` (missing .UnitTests suffix)
- ❌ `UnitTests.OrderProcessor` (inverted structure)
- ❌ `OrderProcessor.Tests` (ambiguous - could be any test type)
- ❌ `OrderProcessorUnitTests` (missing dot separator)

### Integration Test Projects
**Pattern:** `<Component>.IntegrationTests`

**Examples:**
- ✅ `OrderProcessor.IntegrationTests`
- ✅ `PaymentService.IntegrationTests`
- ✅ `CustomerApi.IntegrationTests`

**Anti-Patterns:**
- ❌ `OrderProcessorIntegrationTests` (missing dot separator)
- ❌ `IntegrationTests.OrderProcessor` (inverted structure)
- ❌ `OrderProcessor.Integration` (incomplete suffix)

## Test File Naming

### C# Test Files
**Pattern:** `<TargetClass>Tests.cs`

**Examples:**
- ✅ `OrderServiceTests.cs`
- ✅ `ValidationServiceTests.cs`
- ✅ `ProcessOrderFunctionTests.cs`
- ✅ `OrderTests.cs` (for model/entity tests)

**Anti-Patterns:**
- ❌ `TestOrderService.cs` (prefix instead of suffix)
- ❌ `OrderService.Test.cs` (incorrect separator)
- ❌ `OrderServiceTest.cs` (singular "Test" instead of "Tests")

### JavaScript/TypeScript Test Files
**Pattern:** `<target>.tests.js` or `<target>.test.ts`

**Examples:**
- ✅ `orderService.tests.js`
- ✅ `validator.test.ts`
- ✅ `orderProcessor.tests.ts`

**Anti-Patterns:**
- ❌ `orderService.spec.js` (use .tests.js instead)
- ❌ `test.orderService.js` (prefix instead of suffix)

## Test Method/Function Naming

### Recommended Pattern
**Pattern:** `<MethodName>_<Scenario>_<ExpectedBehavior>`

This three-part naming convention clearly communicates:
1. **What** is being tested (method/function name)
2. **When** it's being tested (scenario/condition)
3. **What should happen** (expected result)

### Examples - C# (xUnit)

#### Good Examples ✅

```csharp
[Fact]
public async Task ProcessOrder_WithValidOrder_ReturnsSuccess()
{
    // Test implementation
}

[Fact]
public async Task ProcessOrder_WithInvalidData_ThrowsValidationException()
{
    // Test implementation
}

[Fact]
public async Task ProcessOrder_WithNullOrder_ThrowsArgumentNullException()
{
    // Test implementation
}

[Fact]
public async Task ValidateOrder_WithMissingCustomerId_ReturnsFalse()
{
    // Test implementation
}

[Fact]
public async Task CalculateTotal_WithDiscountApplied_ReturnsReducedAmount()
{
    // Test implementation
}

[Fact]
public async Task SendNotification_WhenServiceBusFails_RetriesThreeTimes()
{
    // Test implementation
}
```

#### Anti-Patterns ❌

```csharp
// Too vague - what scenario? what behavior?
[Fact]
public async Task TestProcessOrder() { }

// Unclear expected behavior
[Fact]
public async Task ProcessOrderTest() { }

// Missing scenario context
[Fact]
public async Task ProcessOrder_Test() { }

// Not descriptive enough
[Fact]
public async Task Test1() { }

// Using "Should" is acceptable but less clear than expected behavior
[Fact]
public async Task ProcessOrderShouldReturnSuccess() { }
```

### Theory Tests (Data-Driven Tests)

**Pattern:** `<MethodName>_<GeneralScenario>_<ExpectedBehavior>`

```csharp
[Theory]
[InlineData(0)]
[InlineData(-10)]
[InlineData(-100.50)]
public async Task ProcessOrder_WithNegativeOrZeroAmount_ThrowsArgumentException(decimal amount)
{
    // Arrange
    var order = new Order { Id = "123", Amount = amount };

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => _function.Run(order));
}

[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public async Task ValidateCustomerId_WithInvalidInput_ReturnsFalse(string customerId)
{
    // Test implementation
}
```

### Alternative Naming Patterns (Acceptable)

#### Pattern: MethodName_Should_Behavior_When_Condition

```csharp
[Fact]
public async Task ProcessOrder_Should_ReturnSuccess_When_OrderIsValid()
{
    // Test implementation
}

[Fact]
public async Task SendEmail_Should_Retry_When_SmtpConnectionFails()
{
    // Test implementation
}
```

#### Pattern: Given_When_Then (BDD Style)

```csharp
[Fact]
public async Task Given_ValidOrder_When_Processing_Then_ReturnsSuccess()
{
    // Test implementation
}

[Fact]
public async Task Given_InvalidData_When_Validating_Then_ThrowsException()
{
    // Test implementation
}
```

**Note:** While BDD-style naming is acceptable, the recommended `MethodName_Scenario_ExpectedBehavior` pattern is preferred for consistency across the organization.

## Test Class Naming

### Pattern
**Pattern:** `<TargetClass>Tests`

### Examples

```csharp
// Testing OrderService class
public class OrderServiceTests
{
    // Test methods
}

// Testing ValidationService class
public class ValidationServiceTests
{
    // Test methods
}

// Testing ProcessOrderFunction (Azure Function)
public class ProcessOrderFunctionTests
{
    // Test methods
}

// Testing Order model/entity
public class OrderTests
{
    // Test methods
}
```

### Nested Test Classes (Optional)

For better organization of related tests, you can use nested classes:

```csharp
public class OrderServiceTests
{
    public class ProcessOrderMethod
    {
        [Fact]
        public async Task WithValidOrder_ReturnsSuccess() { }

        [Fact]
        public async Task WithInvalidData_ThrowsException() { }
    }

    public class ValidateOrderMethod
    {
        [Fact]
        public async Task WithMissingFields_ReturnsFalse() { }

        [Fact]
        public async Task WithValidData_ReturnsTrue() { }
    }
}
```

## Test Builder Naming

### Pattern
**Pattern:** `<TargetType>Builder`

### Implementation Example

```csharp
// Builder for Order objects
public class OrderBuilder
{
    private string _id;
    private string _customerId;
    private decimal _amount;

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
            Amount = _amount
        };
    }
}
```

### Usage in Tests

```csharp
[Fact]
public async Task ProcessOrder_WithValidOrder_ReturnsSuccess()
{
    // Arrange - Using builder pattern for clarity
    var order = new OrderBuilder()
        .WithId("TEST-123")
        .WithCustomerId("C-456")
        .WithAmount(99.99m)
        .Build();

    // Act
    var result = await _orderService.ProcessOrder(order);

    // Assert
    result.Success.Should().BeTrue();
}
```

### Builder Examples

- ✅ `OrderBuilder.cs`
- ✅ `CustomerBuilder.cs`
- ✅ `PaymentRequestBuilder.cs`
- ✅ `ServiceBusMessageBuilder.cs`

**Anti-Patterns:**
- ❌ `BuildOrder.cs` (verb instead of noun)
- ❌ `OrderTestBuilder.cs` (unnecessary "Test" in name)
- ❌ `TestOrderBuilder.cs` (prefix instead of suffix)

## Test Category/Trait Naming

### Unit Tests
```csharp
[Trait("Category", "Unit")]
public class OrderServiceTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ProcessOrder_WithValidOrder_ReturnsSuccess() { }
}
```

### Integration Tests
```csharp
[Trait("Category", "Integration")]
public class OrderProcessingIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task ProcessOrder_EndToEnd_MessageProcessedSuccessfully() { }
}
```

### Subcategories
```csharp
[Trait("Category", "Unit")]
[Trait("Subcategory", "Validation")]
public async Task ValidateOrder_WithMissingFields_ReturnsFalse() { }

[Trait("Category", "Integration")]
[Trait("Subcategory", "ServiceBus")]
public async Task SendMessage_ToQueue_DeliversSuccessfully() { }
```

## Namespace Naming

### Pattern
```
<Component>.<TestType>.<FolderStructure>
```

### Examples

```csharp
// Unit tests
namespace OrderProcessor.UnitTests.Functions
{
    public class ProcessOrderFunctionTests { }
}

namespace OrderProcessor.UnitTests.Services
{
    public class OrderServiceTests { }
}

namespace OrderProcessor.UnitTests.Models
{
    public class OrderTests { }
}

// Integration tests
namespace OrderProcessor.IntegrationTests.Scenarios
{
    public class OrderProcessingTests { }
}

namespace OrderProcessor.IntegrationTests.Contracts
{
    public class OrderApiContractTests { }
}

// Shared test utilities
namespace TestUtilities.Builders
{
    public class OrderBuilder { }
}
```

## Folder Structure Naming

```
/tests
  /unit
    /OrderProcessor.UnitTests      ← Project matches component
      /Functions                    ← Matches source folder structure
        ProcessOrderFunctionTests.cs
      /Services
        OrderServiceTests.cs
        ValidationServiceTests.cs
      /Models
        OrderTests.cs
      /Builders                     ← Test data builders
        OrderBuilder.cs
  /integration
    /OrderProcessor.IntegrationTests
      /Scenarios                    ← End-to-end scenarios
        OrderProcessingTests.cs
      /Contracts                    ← Contract tests
        OrderApiContractTests.cs
      /Fixtures                     ← Test fixtures and helpers
        ServiceBusFixture.cs
  /shared
    /TestUtilities                  ← Shared across all test projects
      /Builders
      /Fixtures
      /Helpers
```

## Variable Naming in Tests

### Test Setup Variables

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IValidationService> _validationServiceMock;
    private readonly OrderService _orderService;  // System Under Test (SUT)
    private readonly ITestOutputHelper _output;

    public OrderServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _validationServiceMock = new Mock<IValidationService>();
        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _validationServiceMock.Object
        );
    }
}
```

### Test Method Variables

```csharp
[Fact]
public async Task ProcessOrder_WithValidOrder_ReturnsSuccess()
{
    // Arrange
    var order = new OrderBuilder().Build();
    var expectedResult = new ProcessResult { Success = true };

    _orderRepositoryMock
        .Setup(x => x.SaveAsync(It.IsAny<Order>()))
        .ReturnsAsync(expectedResult);

    // Act
    var actualResult = await _orderService.ProcessOrder(order);

    // Assert
    actualResult.Should().BeEquivalentTo(expectedResult);
}
```

## Summary - Quick Reference

| Element | Pattern | Example |
|---------|---------|---------|
| **Unit Test Project** | `<Component>.UnitTests` | `OrderProcessor.UnitTests` |
| **Integration Test Project** | `<Component>.IntegrationTests` | `OrderProcessor.IntegrationTests` |
| **Test File (C#)** | `<TargetClass>Tests.cs` | `OrderServiceTests.cs` |
| **Test File (JS/TS)** | `<target>.tests.js` | `orderService.tests.js` |
| **Test Method** | `<MethodName>_<Scenario>_<ExpectedBehavior>` | `ProcessOrder_WithValidOrder_ReturnsSuccess` |
| **Test Class** | `<TargetClass>Tests` | `OrderServiceTests` |
| **Builder Class** | `<TargetType>Builder` | `OrderBuilder` |
| **Mock Variable** | `_<dependency>Mock` | `_orderServiceMock` |
| **System Under Test** | `_<className>` | `_orderService` |

## Benefits of Consistent Naming

1. **Readability:** Tests are self-documenting and easy to understand
2. **Organization:** Tests are easy to locate and group
3. **Maintenance:** Consistent patterns reduce cognitive load
4. **Automation:** Naming conventions support test filtering and CI/CD integration
5. **Team Alignment:** Everyone follows the same conventions
6. **Test Reports:** Clear, meaningful test result reports

## Related Documentation

- [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) - Complete testing standard
- [PHASE_1_UNIT_TESTING.md](PHASE_1_UNIT_TESTING.md) - Unit testing guidelines
- [PHASE_2_INTEGRATION_TESTING.md](PHASE_2_INTEGRATION_TESTING.md) - Integration testing guidelines
