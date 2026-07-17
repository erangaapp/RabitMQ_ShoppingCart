using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain;

namespace Ordering.Infrastructure;

public class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddTransactionalOutboxEntities();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.HasIndex(o => o.UserId);
            b.Property(o => o.Email).HasMaxLength(256).IsRequired();
            b.Property(o => o.FullName).HasMaxLength(200).IsRequired();
            b.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            b.Property(o => o.Subtotal).HasPrecision(18, 2);
            b.Property(o => o.Discount).HasPrecision(18, 2);
            b.Property(o => o.Vat).HasPrecision(18, 2);
            b.Property(o => o.Total).HasPrecision(18, 2);

            b.HasMany(o => o.Items)
             .WithOne()
             .HasForeignKey("OrderId")
             .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderItem>(i =>
        {
            i.HasKey(x => x.Id);
            i.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            i.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });
    }
}
