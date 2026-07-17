using Basket.Infrastructure;
using EventBus.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Basket.Api.Consumers;

/// <summary>Keeps open baskets in sync when Inventory changes a price.</summary>
public class ProductPriceChangedConsumer(BasketDbContext db, ILogger<ProductPriceChangedConsumer> logger)
    : IConsumer<ProductPriceChangedEvent>
{
    public async Task Consume(ConsumeContext<ProductPriceChangedEvent> context)
    {
        var message = context.Message;

        var affected = await db.Baskets
            .Include(b => b.Items)
            .Where(b => b.Items.Any(i => i.ProductId == message.ProductId))
            .ToListAsync();

        foreach (var basket in affected)
            basket.UpdateItemPrice(message.ProductId, message.NewPrice);

        await db.SaveChangesAsync();

        logger.LogInformation("Re-priced {Count} basket(s) for product {ProductId} -> {Price}",
            affected.Count, message.ProductId, message.NewPrice);
    }
}
