using Catalog.Domain;

namespace Catalog.Application;

public record ProductDto(Guid Id, string Name, string Description, string Category, string ImageUrl);
public record CreateProductRequest(string Name, string Description, string Category, string ImageUrl);

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(string? category, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
