using SharedKernel;

namespace Notification.Domain;

public class EmailLog : Entity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string To { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public DateTime SentAtUtc { get; private set; }

    private EmailLog() { } // EF Core

    public static EmailLog Record(Guid orderId, string to, string subject) =>
        new() { OrderId = orderId, To = to, Subject = subject, SentAtUtc = DateTime.UtcNow };
}
