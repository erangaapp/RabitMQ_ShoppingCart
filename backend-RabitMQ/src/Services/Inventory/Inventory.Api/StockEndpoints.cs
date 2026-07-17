using EventBus.Messages;
using Inventory.Application;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

public static class StockEndpoints
{
    public static void MapStockEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/stock").WithTags("Stock & Price");

        // GET /api/stock            -> all stock items
        // GET /api/stock?ids=a&ids=b -> filtered
        group.MapGet("/", (Guid[]? ids, StockService service, CancellationToken ct) =>
            service.GetManyAsync(ids ?? [], ct));

        group.MapGet("/{productId:guid}", async (Guid productId, StockService service, CancellationToken ct) =>
            await service.GetAsync(productId, ct) is { } stock ? Results.Ok(stock) : Results.NotFound());

        group.MapPut("/{productId:guid}", async (
            Guid productId,
            UpdateStockRequest request,
            StockService service,
            IPublishEndpoint publisher,
            CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(productId, request, ct);
            if (result is null) return Results.NotFound();

            if (result.PriceChanged)
            {
                // Basket service listens and re-prices open baskets.
                await publisher.Publish(new ProductPriceChangedEvent(productId, result.Stock.Price), ct);
            }

            return Results.Ok(result.Stock);
        });
    }
}
