using Inventory.Domain;

namespace Inventory.Application;

public record StockDto(Guid ProductId, string ProductName, int Quantity, decimal Price);
public record UpdateStockRequest(decimal Price, int Quantity);
public record UpdateStockResult(StockDto Stock, bool PriceChanged);

public interface IStockRepository
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task<List<StockItem>> GetByProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken ct = default);
    Task<List<StockItem>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(StockItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
