using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notification.Api.Consumers;
using Notification.Application;
using Notification.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IEmailLogRepository, EmailLogRepository>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.AddEntityFrameworkOutbox<NotificationDbContext>(o =>
    {
        // Configures the background service to look for new outbox messages
        o.UseSqlServer(); // Or .UseNpgsql(), .UseMySql() depending on your DB provider

        // Automatically flushes outbox messages to RabbitMQ after DB save changes
        o.UseBusOutbox();

        o.DuplicateDetectionWindow = TimeSpan.FromDays(7); // How long to remember message IDs for deduplication
        o.QueryDelay = TimeSpan.FromMinutes(5);            // How often the background worker checks for old rows to delete
    });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<NotificationDbContext>(context);
    });

    

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:User"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Small diagnostic endpoint: last 50 emails sent by this service.
app.MapGet("/api/emails", async (IEmailLogRepository logs, CancellationToken ct) =>
    (await logs.GetRecentAsync(50, ct))
        .Select(e => new EmailLogDto(e.Id, e.OrderId, e.To, e.Subject, e.SentAtUtc)))
   .WithTags("Notifications");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    for (var attempt = 1; ; attempt++)
    {
        try { await db.Database.EnsureCreatedAsync(); break; }
        catch when (attempt < 10) { await Task.Delay(3000); }
    }
}

app.Run();
