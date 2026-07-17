using EventBus.Messages;
using Inventory.Application;
using MassTransit;

namespace Inventory.Api.Consumers;

/// <summary>Deducts stock for every line item of a placed order.</summary>
public class OrderPlacedConsumer(IStockRepository stock, ILogger<OrderPlacedConsumer> logger)
    : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        foreach (var item in context.Message.Items)
        {
            var stockItem = await stock.GetByProductIdAsync(item.ProductId);
            if (stockItem is null)
            {
                logger.LogWarning("No stock record for product {ProductId}", item.ProductId);
                continue;
            }
            stockItem.Deduct(item.Quantity);
        }

        await stock.SaveChangesAsync();
        logger.LogInformation("Stock deducted for order {OrderId}", context.Message.OrderId);
    }
}
