using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notification.Domain;

namespace Notification.Infrastructure;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddTransactionalOutboxEntities();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<EmailLog>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.To).HasMaxLength(256).IsRequired();
            b.Property(e => e.Subject).HasMaxLength(500).IsRequired();
        });
    }
}
