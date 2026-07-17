using Basket.Application;
using Basket.Domain;
using Microsoft.EntityFrameworkCore;

namespace Basket.Infrastructure;

public class BasketRepository(BasketDbContext db) : IBasketRepository
{
    public Task<ShoppingBasket?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        db.Baskets.Include(b => b.Items).FirstOrDefaultAsync(b => b.UserId == userId, ct);

    public async Task AddAsync(ShoppingBasket basket, CancellationToken ct = default) =>
        await db.Baskets.AddAsync(basket, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
