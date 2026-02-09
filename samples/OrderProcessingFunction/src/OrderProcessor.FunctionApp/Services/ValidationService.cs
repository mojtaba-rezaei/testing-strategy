using OrderProcessor.FunctionApp.Models;

namespace OrderProcessor.FunctionApp.Services;

public class ValidationService : IValidationService
{
    private const decimal MinAmount = 0.01m;
    private const decimal MaxAmount = 1000000m;

    public Task<ValidationResult> ValidateAsync(Order order)
    {
        var errors = new List<string>();

        if (order == null)
        {
            return Task.FromResult(ValidationResult.Invalid("Order cannot be null"));
        }

        // Validate Order ID
        if (!IsValidOrderId(order.Id))
        {
            errors.Add("Order ID is required and must not be empty");
        }

        // Validate Customer ID
        if (string.IsNullOrWhiteSpace(order.CustomerId))
        {
            errors.Add("Customer ID is required");
        }

        // Validate Amount
        if (!IsValidAmount(order.Amount))
        {
            errors.Add($"Amount must be between {MinAmount:C} and {MaxAmount:C}");
        }

        // Validate Items
        if (order.Items == null || order.Items.Count == 0)
        {
            errors.Add("Order must contain at least one item");
        }
        else
        {
            // Validate each item
            for (int i = 0; i < order.Items.Count; i++)
            {
                var item = order.Items[i];
                
                if (string.IsNullOrWhiteSpace(item.ProductId))
                {
                    errors.Add($"Item {i + 1}: Product ID is required");
                }
                
                if (item.Quantity <= 0)
                {
                    errors.Add($"Item {i + 1}: Quantity must be greater than 0");
                }
                
                if (item.UnitPrice <= 0)
                {
                    errors.Add($"Item {i + 1}: Unit price must be greater than 0");
                }
            }

            // Validate total amount matches items
            var calculatedTotal = order.Items.Sum(i => i.TotalPrice);
            if (Math.Abs(order.Amount - calculatedTotal) > 0.01m)
            {
                errors.Add($"Order amount ({order.Amount:C}) does not match items total ({calculatedTotal:C})");
            }
        }

        var result = errors.Count == 0 
            ? ValidationResult.Valid() 
            : ValidationResult.Invalid(errors.ToArray());

        return Task.FromResult(result);
    }

    public bool IsValidOrderId(string orderId)
    {
        return !string.IsNullOrWhiteSpace(orderId);
    }

    public bool IsValidAmount(decimal amount)
    {
        return amount >= MinAmount && amount <= MaxAmount;
    }
}
