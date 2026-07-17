using SharedKernel;

namespace Ordering.Domain;

public class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { } // EF Core

    internal static OrderItem Create(Guid productId, string productName, decimal unitPrice, int quantity) =>
        new() { ProductId = productId, ProductName = productName, UnitPrice = unitPrice, Quantity = quantity };
}
