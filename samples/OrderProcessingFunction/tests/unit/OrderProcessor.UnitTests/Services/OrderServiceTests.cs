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
    private readonly Mock<IValidationService> _validationServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _validationServiceMock = new Mock<IValidationService>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _orderService = new OrderService(_validationServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ValidateOrderAsync_WithNullOrder_ReturnsInvalidResult()
    {
        // Act
        var result = await _orderService.ValidateOrderAsync(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Order cannot be null");
    }

    [Fact]
    public async Task ValidateOrderAsync_WithValidOrder_CallsValidationService()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(ValidationResult.Valid());

        // Act
        var result = await _orderService.ValidateOrderAsync(order);

        // Assert
        result.IsValid.Should().BeTrue();
        _validationServiceMock.Verify(v => v.ValidateAsync(order), Times.Once);
    }

    [Fact]
    public async Task ValidateOrderAsync_WithInvalidOrder_LogsWarning()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var validationResult = ValidationResult.Invalid("Error 1", "Error 2");
        
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(validationResult);

        // Act
        await _orderService.ValidateOrderAsync(order);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessOrderAsync_WithNullOrder_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _orderService.ProcessOrderAsync(null!));
    }

    [Fact]
    public async Task ProcessOrderAsync_WithValidOrder_ReturnsSuccessResult()
    {
        // Arrange
        var order = new OrderBuilder().WithId("ORD-123").Build();
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(ValidationResult.Valid());

        // Act
        var result = await _orderService.ProcessOrderAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order processed successfully");
        result.Metadata.Should().ContainKey("OrderId");
        result.Metadata.Should().ContainKey("ProcessedAt");
    }

    [Fact]
    public async Task ProcessOrderAsync_WithValidOrder_UpdatesStatusToCompleted()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(ValidationResult.Valid());

        // Act
        await _orderService.ProcessOrderAsync(order);

        // Assert
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task ProcessOrderAsync_WithInvalidOrder_ReturnsFailureResult()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var validationErrors = new[] { "Error 1", "Error 2" };
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(ValidationResult.Invalid(validationErrors));

        // Act
        var result = await _orderService.ProcessOrderAsync(order);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.Metadata.Should().ContainKey("Errors");
    }

    [Fact]
    public async Task ProcessOrderAsync_WithValidOrder_LogsInformation()
    {
        // Arrange
        var order = new OrderBuilder().WithId("ORD-123").WithCustomerId("CUST-456").Build();
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(ValidationResult.Valid());

        // Act
        await _orderService.ProcessOrderAsync(order);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing order")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNullOrEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _orderService.GetOrderByIdAsync(string.Empty));
        
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _orderService.GetOrderByIdAsync(null!));
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistentOrder_ReturnsNull()
    {
        // Act
        var result = await _orderService.GetOrderByIdAsync("NON-EXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderByIdAsync_AfterProcessing_ReturnsOrder()
    {
        // Arrange
        var order = new OrderBuilder().WithId("ORD-123").Build();
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(ValidationResult.Valid());

        await _orderService.ProcessOrderAsync(order);

        // Act
        var retrievedOrder = await _orderService.GetOrderByIdAsync("ORD-123");

        // Assert
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Id.Should().Be("ORD-123");
        retrievedOrder.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithNonExistentOrder_ReturnsFalse()
    {
        // Act
        var result = await _orderService.UpdateOrderStatusAsync("NON-EXISTENT", OrderStatus.Cancelled);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithExistingOrder_UpdatesStatusAndReturnsTrue()
    {
        // Arrange
        var order = new OrderBuilder().WithId("ORD-123").Build();
        _validationServiceMock
            .Setup(v => v.ValidateAsync(It.IsAny<Order>()))
            .ReturnsAsync(ValidationResult.Valid());

        await _orderService.ProcessOrderAsync(order);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync("ORD-123", OrderStatus.Cancelled);
        var updatedOrder = await _orderService.GetOrderByIdAsync("ORD-123");

        // Assert
        result.Should().BeTrue();
        updatedOrder!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Constructor_WithNullValidationService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OrderService(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OrderService(_validationServiceMock.Object, null!));
    }
}
