using EventBus.Messages;
using Inventory.Application;
using Inventory.Domain;
using MassTransit;

namespace Inventory.Api.Consumers;

/// <summary>When Catalog creates a product, create an empty stock record so it can be priced later.</summary>
public class ProductCreatedConsumer(IStockRepository stock, ILogger<ProductCreatedConsumer> logger)
    : IConsumer<ProductCreatedEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var message = context.Message;
        // Idempotent handled by Outbox pattern, so we don't need to check if the stock record already exists

        await stock.AddAsync(StockItem.Create(message.ProductId, message.ProductName, 0, 0));
        await stock.SaveChangesAsync();

        logger.LogInformation("Stock record created for new product {ProductId} ({Name})",
            message.ProductId, message.ProductName);
    }
}
