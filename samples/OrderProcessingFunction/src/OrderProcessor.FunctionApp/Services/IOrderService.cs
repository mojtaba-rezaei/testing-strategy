using OrderProcessor.FunctionApp.Models;

namespace OrderProcessor.FunctionApp.Services;

public interface IOrderService
{
    Task<ValidationResult> ValidateOrderAsync(Order order);
    Task<ProcessResult> ProcessOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(string orderId);
    Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status);
}
