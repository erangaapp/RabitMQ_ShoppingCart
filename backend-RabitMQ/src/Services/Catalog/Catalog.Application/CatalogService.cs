using Catalog.Domain;

namespace Catalog.Application;

public class CatalogService(IProductRepository products)
{
    public async Task<List<ProductDto>> GetProductsAsync(string? category, CancellationToken ct = default) =>
        (await products.GetAllAsync(category, ct)).Select(ToDto).ToList();

    public async Task<ProductDto?> GetProductAsync(Guid id, CancellationToken ct = default)
    {
        var product = await products.GetByIdAsync(id, ct);
        return product is null ? null : ToDto(product);
    }

    public Task<List<string>> GetCategoriesAsync(CancellationToken ct = default) =>
        products.GetCategoriesAsync(ct);

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var product = Product.Create(request.Name, request.Description, request.Category, request.ImageUrl);
        await products.AddAsync(product, ct);
        await products.SaveChangesAsync(ct);
        return ToDto(product);
    }

    private static ProductDto ToDto(Product p) => new(p.Id, p.Name, p.Description, p.Category, p.ImageUrl);
}
