using OrderProcessor.FunctionApp.Models;

namespace OrderProcessor.FunctionApp.Services;

public interface IValidationService
{
    Task<ValidationResult> ValidateAsync(Order order);
    bool IsValidOrderId(string orderId);
    bool IsValidAmount(decimal amount);
}
