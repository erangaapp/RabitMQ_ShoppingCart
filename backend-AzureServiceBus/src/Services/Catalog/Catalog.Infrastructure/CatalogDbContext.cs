using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).HasMaxLength(200).IsRequired();
            b.Property(p => p.Category).HasMaxLength(100).IsRequired();
            b.Property(p => p.Description).HasMaxLength(2000);
            b.Property(p => p.ImageUrl).HasMaxLength(500);
        });
    }
}
