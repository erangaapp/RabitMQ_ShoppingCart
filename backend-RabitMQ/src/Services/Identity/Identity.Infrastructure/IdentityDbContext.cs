using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Email).HasMaxLength(256).IsRequired();
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            b.Property(u => u.PasswordHash).IsRequired();
        });
    }
}
