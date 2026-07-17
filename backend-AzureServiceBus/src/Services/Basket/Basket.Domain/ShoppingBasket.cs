using SharedKernel;

namespace Basket.Domain;

/// <summary>
/// Aggregate root for the shopping cart. All basket calculation rules
/// (discount, VAT, totals) live here — the domain layer, not the UI or the API.
/// </summary>
public class ShoppingBasket : Entity, IAggregateRoot
{
    public const decimal DiscountThreshold = 100m;
    public const decimal DiscountRate = 0.10m; // 10% off subtotals of 100+
    public const decimal VatRate = 0.05m;      // 5% VAT (UAE)

    public Guid UserId { get; private set; }

    private readonly List<BasketItem> _items = [];
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();

    public decimal Subtotal => _items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal Discount => Subtotal >= DiscountThreshold ? Math.Round(Subtotal * DiscountRate, 2) : 0m;
    public decimal Vat => Math.Round((Subtotal - Discount) * VatRate, 2);
    public decimal Total => Subtotal - Discount + Vat;

    private ShoppingBasket() { } // EF Core

    public static ShoppingBasket CreateFor(Guid userId) => new() { UserId = userId };

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
            existing.IncreaseQuantity(quantity);
        else
            _items.Add(BasketItem.Create(productId, productName, unitPrice, quantity));
    }

    public void SetQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return;

        if (quantity <= 0) _items.Remove(item);
        else item.SetQuantity(quantity);
    }

    public void RemoveItem(Guid productId) =>
        _items.RemoveAll(i => i.ProductId == productId);

    /// <summary>Applied when Inventory publishes ProductPriceChangedEvent.</summary>
    public void UpdateItemPrice(Guid productId, decimal newPrice) =>
        _items.FirstOrDefault(i => i.ProductId == productId)?.SetUnitPrice(newPrice);

    public void Clear() => _items.Clear();
}
