using Microsoft.EntityFrameworkCore;
using Notification.Application;
using Notification.Domain;

namespace Notification.Infrastructure;

public class EmailLogRepository(NotificationDbContext db) : IEmailLogRepository
{
    public async Task AddAsync(EmailLog log, CancellationToken ct = default) =>
        await db.EmailLogs.AddAsync(log, ct);

    public Task<List<EmailLog>> GetRecentAsync(int take, CancellationToken ct = default) =>
        db.EmailLogs.AsNoTracking().OrderByDescending(e => e.SentAtUtc).Take(take).ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
