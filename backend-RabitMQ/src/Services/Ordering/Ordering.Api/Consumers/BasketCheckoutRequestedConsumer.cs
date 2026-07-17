using EventBus.Messages;
using MassTransit;
using Ordering.Application;
using Ordering.Domain;

namespace Ordering.Api.Consumers;

/// <summary>
/// Checkout handler: turns a checkout request into a persisted Order,
/// then announces it via OrderPlacedEvent (consumed by Inventory + Notification).
/// NOTE: For production, wrap persist+publish with MassTransit's EF Outbox
/// (AddEntityFrameworkOutbox) so the DB write and the publish are atomic.
/// </summary>
public class BasketCheckoutRequestedConsumer(IOrderRepository orders, ILogger<BasketCheckoutRequestedConsumer> logger)
    : IConsumer<BasketCheckoutRequestedEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutRequestedEvent> context)
    {
        var message = context.Message;

        var order = Order.Place(
            message.UserId, message.Email, message.FullName,
            message.Items.Select(i => (i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)),
            message.Subtotal, message.Discount, message.Vat, message.Total);

        await orders.AddAsync(order);

        await context.Publish(new OrderPlacedEvent(
            order.Id, order.UserId, order.Email, order.FullName,
            message.Items, order.Total, order.PlacedAtUtc));

        await orders.SaveChangesAsync();

        logger.LogInformation("Order {OrderId} placed for user {UserId}, total {Total}",
            order.Id, order.UserId, order.Total);
    }
}
