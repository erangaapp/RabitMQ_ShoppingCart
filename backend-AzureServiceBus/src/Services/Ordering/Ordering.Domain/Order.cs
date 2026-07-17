using SharedKernel;

namespace Ordering.Domain;

public enum OrderStatus { Placed, Confirmed, Shipped, Cancelled }

public class Order : Entity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Vat { get; private set; }
    public decimal Total { get; private set; }
    public DateTime PlacedAtUtc { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // EF Core

    public static Order Place(
        Guid userId, string email, string fullName,
        IEnumerable<(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity)> items,
        decimal subtotal, decimal discount, decimal vat, decimal total)
    {
        var order = new Order
        {
            UserId = userId,
            Email = email,
            FullName = fullName,
            Status = OrderStatus.Placed,
            Subtotal = subtotal,
            Discount = discount,
            Vat = vat,
            Total = total,
            PlacedAtUtc = DateTime.UtcNow
        };

        foreach (var (productId, productName, unitPrice, quantity) in items)
            order._items.Add(OrderItem.Create(productId, productName, unitPrice, quantity));

        if (order._items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one item.");

        return order;
    }

    public void Confirm() => Status = OrderStatus.Confirmed;
}
