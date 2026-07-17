using Basket.Domain;
using Microsoft.EntityFrameworkCore;

namespace Basket.Infrastructure;

public class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
{
    public DbSet<ShoppingBasket> Baskets => Set<ShoppingBasket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingBasket>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.UserId).IsUnique();

            b.HasMany(x => x.Items)
             .WithOne()
             .HasForeignKey(x => x.Id)
             .HasConstraintName("FK_BasketItems_BasketId")
             .OnDelete(DeleteBehavior.Cascade);

            // Map through the private _items backing field so the aggregate stays encapsulated.
            b.Navigation(x => x.Items).
              UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<BasketItem>(i =>
        {
            i.HasKey(x => x.Id);
            i.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            i.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });
    }
}
