using Notification.Domain;

namespace Notification.Application;

public record EmailLogDto(Guid Id, Guid OrderId, string To, string Subject, DateTime SentAtUtc);

public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default);
}

public interface IEmailLogRepository
{
    Task AddAsync(EmailLog log, CancellationToken ct = default);
    Task<List<EmailLog>> GetRecentAsync(int take, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
