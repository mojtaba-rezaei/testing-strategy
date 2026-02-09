using Microsoft.Extensions.Logging;
using OrderProcessor.FunctionApp.Models;

namespace OrderProcessor.FunctionApp.Services;

public class OrderService : IOrderService
{
    private readonly IValidationService _validationService;
    private readonly ILogger<OrderService> _logger;
    private readonly Dictionary<string, Order> _orderStore = new(); // In-memory for demo

    public OrderService(IValidationService validationService, ILogger<OrderService> logger)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ValidationResult> ValidateOrderAsync(Order order)
    {
        if (order == null)
        {
            return ValidationResult.Invalid("Order cannot be null");
        }

        var validationResult = await _validationService.ValidateAsync(order);
        
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Order validation failed for OrderId: {OrderId}. Errors: {Errors}", 
                order.Id, string.Join(", ", validationResult.Errors));
        }

        return validationResult;
    }

    public async Task<ProcessResult> ProcessOrderAsync(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        _logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", 
            order.Id, order.CustomerId);

        try
        {
            // Validate order first
            var validationResult = await ValidateOrderAsync(order);
            if (!validationResult.IsValid)
            {
                return new ProcessResult
                {
                    Success = false,
                    Message = "Validation failed",
                    ErrorCode = "VALIDATION_ERROR",
                    Metadata = new Dictionary<string, object>
                    {
                        ["Errors"] = validationResult.Errors
                    }
                };
            }

            // Update status
            order.Status = OrderStatus.Processing;

            // Simulate processing logic
            await Task.Delay(10); // Simulate async work

            // Store order
            _orderStore[order.Id] = order;

            order.Status = OrderStatus.Completed;

            _logger.LogInformation("Order {OrderId} processed successfully", order.Id);

            return new ProcessResult
            {
                Success = true,
                Message = "Order processed successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["OrderId"] = order.Id,
                    ["ProcessedAt"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
            
            order.Status = OrderStatus.Failed;
            
            return new ProcessResult
            {
                Success = false,
                Message = "Processing failed",
                ErrorCode = "PROCESSING_ERROR",
                Metadata = new Dictionary<string, object>
                {
                    ["Exception"] = ex.Message
                }
            };
        }
    }

    public Task<Order?> GetOrderByIdAsync(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order ID cannot be null or empty", nameof(orderId));
        }

        _orderStore.TryGetValue(orderId, out var order);
        return Task.FromResult(order);
    }

    public async Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status)
    {
        var order = await GetOrderByIdAsync(orderId);
        
        if (order == null)
        {
            _logger.LogWarning("Cannot update status. Order {OrderId} not found", orderId);
            return false;
        }

        order.Status = status;
        _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);
        
        return true;
    }
}
