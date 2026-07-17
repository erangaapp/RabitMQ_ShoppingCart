using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure;

public static class SeedData
{
    // Well-known IDs shared with Inventory seed data (POC convenience only).
    public static readonly Guid Mouse    = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Keyboard = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Espresso = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid GreenTea = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Shoes    = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid TShirt   = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static async Task SeedAsync(CatalogDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        db.Products.AddRange(
            Product.Create("Wireless Mouse", "Ergonomic 2.4GHz wireless mouse.", "Electronics", "", Mouse),
            Product.Create("Mechanical Keyboard", "Tenkeyless mechanical keyboard, brown switches.", "Electronics", "", Keyboard),
            Product.Create("Espresso Beans 1kg", "Dark roast arabica beans.", "Grocery", "", Espresso),
            Product.Create("Green Tea Box", "25 bags of premium green tea.", "Grocery", "", GreenTea),
            Product.Create("Running Shoes", "Lightweight road running shoes.", "Fashion", "", Shoes),
            Product.Create("Cotton T-Shirt", "100% cotton, regular fit.", "Fashion", "", TShirt));

        await db.SaveChangesAsync();
    }
}
