using Microsoft.EntityFrameworkCore;
using Ordering.Application;
using Ordering.Domain;

namespace Ordering.Infrastructure;

public class OrderRepository(OrderingDbContext db) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await db.Orders.AddAsync(order, ct);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Orders.Include(o => o.Items).AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<List<Order>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        db.Orders.Include(o => o.Items).AsNoTracking()
          .Where(o => o.UserId == userId)
          .OrderByDescending(o => o.PlacedAtUtc)
          .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
