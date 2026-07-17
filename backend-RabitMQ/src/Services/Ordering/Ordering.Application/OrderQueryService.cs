using Ordering.Domain;

namespace Ordering.Application;

public class OrderQueryService(IOrderRepository orders)
{
    public async Task<List<OrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken ct = default) =>
        (await orders.GetByUserAsync(userId, ct)).Select(ToDto).ToList();

    public async Task<OrderDto?> GetAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var order = await orders.GetByIdAsync(id, ct);
        return order is null || order.UserId != userId ? null : ToDto(order);
    }

    public static OrderDto ToDto(Order o) => new(
        o.Id, o.Status.ToString(), o.Subtotal, o.Discount, o.Vat, o.Total, o.PlacedAtUtc,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList());
}
