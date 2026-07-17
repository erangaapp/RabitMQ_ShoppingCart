using System.Text;
using EventBus.Messages;
using MassTransit;
using Notification.Application;
using Notification.Domain;

namespace Notification.Api.Consumers;

/// <summary>Sends the order confirmation email when an order is placed.</summary>
public class OrderPlacedConsumer(
    IEmailSender emailSender,
    IEmailLogRepository emailLogs,
    ILogger<OrderPlacedConsumer> logger) : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var message = context.Message;
        var subject = $"Order confirmation — {message.OrderId.ToString()[..8].ToUpper()}";

        var body = new StringBuilder()
            .Append($"<h2>Thanks for your order, {message.FullName}!</h2>")
            .Append($"<p>Order <b>{message.OrderId}</b> placed at {message.PlacedAtUtc:u}.</p>")
            .Append("<table border='1' cellpadding='6' cellspacing='0'><tr><th>Product</th><th>Qty</th><th>Unit Price</th></tr>");

        foreach (var item in message.Items)
            body.Append($"<tr><td>{item.ProductName}</td><td>{item.Quantity}</td><td>{item.UnitPrice:0.00}</td></tr>");

        body.Append("</table>")
            .Append($"<h3>Total: {message.Total:0.00}</h3>");

        await emailSender.SendAsync(message.Email, message.FullName, subject, body.ToString());

        await emailLogs.AddAsync(EmailLog.Record(message.OrderId, message.Email, subject));
        await emailLogs.SaveChangesAsync();

        logger.LogInformation("Confirmation email sent to {Email} for order {OrderId}", message.Email, message.OrderId);
    }
}
