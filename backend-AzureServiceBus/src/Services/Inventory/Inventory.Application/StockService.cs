using Inventory.Domain;

namespace Inventory.Application;

public class StockService(IStockRepository stock)
{
    public async Task<StockDto?> GetAsync(Guid productId, CancellationToken ct = default)
    {
        var item = await stock.GetByProductIdAsync(productId, ct);
        return item is null ? null : ToDto(item);
    }

    public async Task<List<StockDto>> GetManyAsync(Guid[] productIds, CancellationToken ct = default)
    {
        var items = productIds.Length == 0
            ? await stock.GetAllAsync(ct)
            : await stock.GetByProductIdsAsync(productIds, ct);
        return items.Select(ToDto).ToList();
    }

    public async Task<UpdateStockResult?> UpdateAsync(Guid productId, UpdateStockRequest request, CancellationToken ct = default)
    {
        var item = await stock.GetByProductIdAsync(productId, ct);
        if (item is null) return null;

        var priceChanged = item.Update(request.Price, request.Quantity);
        await stock.SaveChangesAsync(ct);
        return new UpdateStockResult(ToDto(item), priceChanged);
    }

    private static StockDto ToDto(StockItem i) => new(i.ProductId, i.ProductName, i.Quantity, i.Price);
}
