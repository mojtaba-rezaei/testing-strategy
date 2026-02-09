using FluentAssertions;
using OrderProcessor.FunctionApp.Models;
using Xunit;

namespace OrderProcessor.UnitTests.Models;

public class OrderTests
{
    [Fact]
    public void Order_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var order = new Order();

        // Assert
        order.Id.Should().BeEmpty();
        order.CustomerId.Should().BeEmpty();
        order.Amount.Should().Be(0);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().NotBeNull().And.BeEmpty();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Order_WithProperties_StoresValuesCorrectly()
    {
        // Arrange
        var expectedId = "ORD-123";
        var expectedCustomerId = "CUST-456";
        var expectedAmount = 99.99m;
        var expectedStatus = OrderStatus.Processing;

        // Act
        var order = new Order
        {
            Id = expectedId,
            CustomerId = expectedCustomerId,
            Amount = expectedAmount,
            Status = expectedStatus
        };

        // Assert
        order.Id.Should().Be(expectedId);
        order.CustomerId.Should().Be(expectedCustomerId);
        order.Amount.Should().Be(expectedAmount);
        order.Status.Should().Be(expectedStatus);
    }
}

public class OrderItemTests
{
    [Fact]
    public void OrderItem_TotalPrice_CalculatesCorrectly()
    {
        // Arrange
        var item = new OrderItem
        {
            ProductId = "PROD-001",
            ProductName = "Test Product",
            Quantity = 3,
            UnitPrice = 25.50m
        };

        // Act
        var totalPrice = item.TotalPrice;

        // Assert
        totalPrice.Should().Be(76.50m);
    }

    [Fact]
    public void OrderItem_TotalPrice_WithZeroQuantity_ReturnsZero()
    {
        // Arrange
        var item = new OrderItem
        {
            Quantity = 0,
            UnitPrice = 25.50m
        };

        // Act
        var totalPrice = item.TotalPrice;

        // Assert
        totalPrice.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(5, 20.00, 100.00)]
    [InlineData(10, 9.99, 99.90)]
    public void OrderItem_TotalPrice_WithVariousInputs_CalculatesCorrectly(
        int quantity, decimal unitPrice, decimal expectedTotal)
    {
        // Arrange
        var item = new OrderItem
        {
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        // Act
        var totalPrice = item.TotalPrice;

        // Assert
        totalPrice.Should().Be(expectedTotal);
    }
}
