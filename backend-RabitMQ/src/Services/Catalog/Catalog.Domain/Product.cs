using SharedKernel;

namespace Catalog.Domain;

/// <summary>
/// Catalog owns product identity and descriptive data only.
/// Price and stock belong to the Inventory bounded context.
/// </summary>
public class Product : Entity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Category { get; private set; } = default!;
    public string ImageUrl { get; private set; } = default!;

    private Product() { } // EF Core

    public static Product Create(string name, string description, string category, string imageUrl, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

        var product = new Product
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
            ImageUrl = imageUrl ?? string.Empty
        };
        if (id.HasValue) product.Id = id.Value;
        return product;
    }
}
