using AutoFixture;
using OrderProcessor.FunctionApp.Models;

namespace OrderProcessor.UnitTests.Builders;

public class OrderBuilder
{
    private readonly Fixture _fixture = new();
    private string _id;
    private string _customerId;
    private decimal _amount;
    private OrderStatus _status;
    private List<OrderItem> _items;

    public OrderBuilder()
    {
        // Set reasonable defaults
        _id = $"ORD-{Guid.NewGuid().ToString()[..8]}";
        _customerId = $"CUST-{Guid.NewGuid().ToString()[..8]}";
        _items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = "PROD-001",
                ProductName = "Sample Product",
                Quantity = 2,
                UnitPrice = 25.00m
            }
        };
        _amount = _items.Sum(i => i.TotalPrice);
        _status = OrderStatus.Pending;
    }

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

    public OrderBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }

    public OrderBuilder WithItems(params OrderItem[] items)
    {
        _items = items.ToList();
        _amount = _items.Sum(i => i.TotalPrice);
        return this;
    }

    public OrderBuilder WithSingleItem(string productId, int quantity, decimal unitPrice)
    {
        _items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = productId,
                ProductName = $"Product {productId}",
                Quantity = quantity,
                UnitPrice = unitPrice
            }
        };
        _amount = _items.Sum(i => i.TotalPrice);
        return this;
    }

    public OrderBuilder WithInvalidAmount()
    {
        _amount = -100;
        return this;
    }

    public OrderBuilder WithEmptyItems()
    {
        _items = new List<OrderItem>();
        _amount = 0;
        return this;
    }

    public OrderBuilder WithNullOrderId()
    {
        _id = null!;
        return this;
    }

    public OrderBuilder WithEmptyCustomerId()
    {
        _customerId = string.Empty;
        return this;
    }

    public Order Build()
    {
        return new Order
        {
            Id = _id,
            CustomerId = _customerId,
            Amount = _amount,
            Status = _status,
            Items = _items,
            CreatedAt = DateTime.UtcNow
        };
    }
}
