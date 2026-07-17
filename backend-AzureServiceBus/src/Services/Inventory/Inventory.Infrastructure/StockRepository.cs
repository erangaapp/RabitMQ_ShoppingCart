using Inventory.Application;
using Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure;

public class StockRepository(InventoryDbContext db) : IStockRepository
{
    public Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        db.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId, ct);

    public Task<List<StockItem>> GetByProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken ct = default) =>
        db.StockItems.Where(s => productIds.Contains(s.ProductId)).AsNoTracking().ToListAsync(ct);

    public Task<List<StockItem>> GetAllAsync(CancellationToken ct = default) =>
        db.StockItems.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(StockItem item, CancellationToken ct = default) =>
        await db.StockItems.AddAsync(item, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
