using FluentAssertions;
using OrderProcessor.FunctionApp.Models;
using OrderProcessor.FunctionApp.Services;
using OrderProcessor.UnitTests.Builders;
using Xunit;

namespace OrderProcessor.UnitTests.Services;

public class ValidationServiceTests
{
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _validationService = new ValidationService();
    }

    [Fact]
    public async Task ValidateAsync_WithValidOrder_ReturnsValid()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId("ORD-123")
            .WithCustomerId("CUST-456")
            .WithSingleItem("PROD-001", 2, 25.00m)
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithNullOrder_ReturnsInvalid()
    {
        // Act
        var result = await _validationService.ValidateAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Order cannot be null");
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyOrderId_ReturnsInvalid()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId(string.Empty)
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Order ID is required and must not be empty");
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyCustomerId_ReturnsInvalid()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithEmptyCustomerId()
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Customer ID is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-0.01)]
    public async Task ValidateAsync_WithInvalidAmount_ReturnsInvalid(decimal amount)
    {
        // Arrange
        var order = new OrderBuilder()
            .WithAmount(amount)
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Amount must be between"));
    }

    [Fact]
    public async Task ValidateAsync_WithAmountExceedingMaximum_ReturnsInvalid()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithAmount(2000000m) // Exceeds max
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Amount must be between"));
    }

    [Fact]
    public async Task ValidateAsync_WithNoItems_ReturnsInvalid()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithEmptyItems()
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Order must contain at least one item");
    }

    [Fact]
    public async Task ValidateAsync_WithItemHavingInvalidQuantity_ReturnsInvalid()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithSingleItem("PROD-001", 0, 25.00m) // Invalid quantity
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Quantity must be greater than 0"));
    }

    [Fact]
    public async Task ValidateAsync_WithAmountMismatch_ReturnsInvalid()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithSingleItem("PROD-001", 2, 25.00m) // Total should be 50
            .WithAmount(100m) // But we set it to 100
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("does not match items total"));
    }

    [Theory]
    [InlineData("ORD-123", true)]
    [InlineData("ORDER-456", true)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    public void IsValidOrderId_WithVariousInputs_ReturnsExpectedResult(string orderId, bool expected)
    {
        // Act
        var result = _validationService.IsValidOrderId(orderId);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.01, true)]
    [InlineData(100, true)]
    [InlineData(1000000, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1000001, false)]
    public void IsValidAmount_WithVariousAmounts_ReturnsExpectedResult(decimal amount, bool expected)
    {
        // Act
        var result = _validationService.IsValidAmount(amount);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task ValidateAsync_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId(string.Empty)
            .WithEmptyCustomerId()
            .WithInvalidAmount()
            .WithEmptyItems()
            .Build();

        // Act
        var result = await _validationService.ValidateAsync(order);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(2);
        result.Errors.Should().Contain("Order ID is required and must not be empty");
        result.Errors.Should().Contain("Customer ID is required");
    }
}
