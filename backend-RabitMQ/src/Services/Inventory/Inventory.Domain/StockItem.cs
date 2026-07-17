using SharedKernel;

namespace Inventory.Domain;

/// <summary>
/// Stock &amp; Price bounded context: this service is the single source of truth for
/// how many units are available and what a product costs.
/// </summary>
public class StockItem : Entity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

    private StockItem() { } // EF Core

    public static StockItem Create(Guid productId, string productName, int quantity, decimal price) =>
        new()
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = Math.Max(0, quantity),
            Price = Math.Max(0, price)
        };

    /// <summary>Returns true when the price actually changed (caller publishes ProductPriceChangedEvent).</summary>
    public bool Update(decimal newPrice, int newQuantity)
    {
        if (newPrice < 0) throw new ArgumentOutOfRangeException(nameof(newPrice));
        if (newQuantity < 0) throw new ArgumentOutOfRangeException(nameof(newQuantity));

        var priceChanged = Price != newPrice;
        Price = newPrice;
        Quantity = newQuantity;
        return priceChanged;
    }

    public void Deduct(int quantity) => Quantity = Math.Max(0, Quantity - quantity);
}
