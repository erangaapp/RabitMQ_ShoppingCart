using Basket.Domain;

namespace Basket.Application;

public class BasketService(IBasketRepository baskets)
{
    public async Task<BasketDto> GetAsync(Guid userId, CancellationToken ct = default) =>
        ToDto(await GetOrCreateAsync(userId, ct));

    public async Task<BasketDto> AddItemAsync(Guid userId, AddBasketItemRequest request, CancellationToken ct = default)
    {
        var basket = await GetOrCreateAsync(userId, ct);
        basket.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.Quantity);
        await baskets.SaveChangesAsync(ct);
        return ToDto(basket);
    }

    public async Task<BasketDto> SetQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken ct = default)
    {
        var basket = await GetOrCreateAsync(userId, ct);
        basket.SetQuantity(productId, quantity);
        await baskets.SaveChangesAsync(ct);
        return ToDto(basket);
    }

    public async Task<BasketDto> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var basket = await GetOrCreateAsync(userId, ct);
        basket.RemoveItem(productId);
        await baskets.SaveChangesAsync(ct);
        return ToDto(basket);
    }

    /// <summary>Snapshots totals, clears the basket, and returns the snapshot for event publishing.</summary>
    public async Task<CheckoutSnapshot?> CheckoutAsync(Guid userId, CancellationToken ct = default)
    {
        var basket = await baskets.GetByUserIdAsync(userId, ct);
        if (basket is null || basket.Items.Count == 0) return null;

        var snapshot = new CheckoutSnapshot(
            basket.UserId,
            basket.Items.Select(ToDto).ToList(),
            basket.Subtotal, basket.Discount, basket.Vat, basket.Total);

        basket.Clear();
        await baskets.SaveChangesAsync(ct);
        return snapshot;
    }

    private async Task<ShoppingBasket> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var basket = await baskets.GetByUserIdAsync(userId, ct);
        if (basket is null)
        {
            basket = ShoppingBasket.CreateFor(userId);
            await baskets.AddAsync(basket, ct);
            await baskets.SaveChangesAsync(ct);
        }
        return basket;
    }

    private static BasketDto ToDto(ShoppingBasket b) =>
        new(b.UserId, b.Items.Select(ToDto).ToList(), b.Subtotal, b.Discount, b.Vat, b.Total);

    private static BasketItemDto ToDto(BasketItem i) =>
        new(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity);
}
