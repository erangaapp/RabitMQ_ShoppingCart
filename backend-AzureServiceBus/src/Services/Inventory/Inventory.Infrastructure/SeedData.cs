using Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure;

public static class SeedData
{
    public static async Task SeedAsync(InventoryDbContext db)
    {
        if (await db.StockItems.AnyAsync()) return;

        // Same well-known product IDs as Catalog seed data (POC convenience).
        db.StockItems.AddRange(
            StockItem.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Wireless Mouse", 25, 49.00m),
            StockItem.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Mechanical Keyboard", 15, 179.00m),
            StockItem.Create(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Espresso Beans 1kg", 40, 35.00m),
            StockItem.Create(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Green Tea Box", 60, 12.00m),
            StockItem.Create(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Running Shoes", 10, 220.00m),
            StockItem.Create(Guid.Parse("66666666-6666-6666-6666-666666666666"), "Cotton T-Shirt", 50, 25.00m));

        await db.SaveChangesAsync();
    }
}
