using SharedKernel;

namespace Basket.Domain;

public class BasketItem : Entity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private BasketItem() { } // EF Core

    internal static BasketItem Create(Guid productId, string productName, decimal unitPrice, int quantity) =>
        new() { ProductId = productId, ProductName = productName, UnitPrice = unitPrice, Quantity = quantity };

    internal void IncreaseQuantity(int by) => Quantity += by;
    internal void SetQuantity(int quantity) => Quantity = quantity;
    internal void SetUnitPrice(decimal unitPrice) => UnitPrice = unitPrice;
}
