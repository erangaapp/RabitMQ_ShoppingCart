using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Notification.Application;

namespace Notification.Infrastructure;

/// <summary>Sends via plain SMTP. In the POC this targets the Papercut container (UI at http://localhost:8080).</summary>
public class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            configuration["Smtp:FromName"] ?? "Shopping Cart POC",
            configuration["Smtp:FromAddress"] ?? "noreply@shop.local"));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            configuration["Smtp:Host"] ?? "localhost",
            int.Parse(configuration["Smtp:Port"] ?? "2525"),
            SecureSocketOptions.None, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
