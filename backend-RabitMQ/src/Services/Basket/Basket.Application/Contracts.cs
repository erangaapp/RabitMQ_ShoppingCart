using Basket.Domain;

namespace Basket.Application;

public record BasketItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record BasketDto(Guid UserId, List<BasketItemDto> Items, decimal Subtotal, decimal Discount, decimal Vat, decimal Total);
public record AddBasketItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record UpdateQuantityRequest(int Quantity);

/// <summary>Snapshot handed to the API layer so it can publish BasketCheckoutRequestedEvent.</summary>
public record CheckoutSnapshot(Guid UserId, List<BasketItemDto> Items, decimal Subtotal, decimal Discount, decimal Vat, decimal Total);

public interface IBasketRepository
{
    Task<ShoppingBasket?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(ShoppingBasket basket, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
