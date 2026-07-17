using Inventory.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddTransactionalOutboxEntities();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<StockItem>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasIndex(s => s.ProductId).IsUnique();
            b.Property(s => s.ProductName).HasMaxLength(200).IsRequired();
            b.Property(s => s.Price).HasPrecision(18, 2);
        });
    }
}
