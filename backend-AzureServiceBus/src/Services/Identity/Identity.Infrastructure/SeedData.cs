using Identity.Application;
using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

public static class SeedData
{
    public static async Task SeedAsync(IdentityDbContext db, IPasswordHasher hasher)
    {
        if (await db.Users.AnyAsync()) return;

        db.Users.Add(User.Register("demo@shop.local", hasher.Hash("Pass@123"), "Demo User"));
        await db.SaveChangesAsync();
    }
}
