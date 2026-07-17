using Ordering.Domain;

namespace Ordering.Application;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record OrderDto(
    Guid Id, string Status, decimal Subtotal, decimal Discount, decimal Vat, decimal Total,
    DateTime PlacedAtUtc, List<OrderItemDto> Items);

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Order>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
