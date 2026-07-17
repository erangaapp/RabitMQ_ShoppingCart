using Catalog.Application;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure;

public class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public Task<List<Product>> GetAllAsync(string? category, CancellationToken ct = default) =>
        db.Products
          .Where(p => category == null || p.Category == category)
          .OrderBy(p => p.Name)
          .AsNoTracking()
          .ToListAsync(ct);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<string>> GetCategoriesAsync(CancellationToken ct = default) =>
        db.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await db.Products.AddAsync(product, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
