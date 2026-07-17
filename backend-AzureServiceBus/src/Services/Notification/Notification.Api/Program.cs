using Azure.Identity;
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

        // Automatically flushes outbox messages to the broker after DB save changes
        o.UseBusOutbox();

        o.DuplicateDetectionWindow = TimeSpan.FromDays(7); // How long to remember message IDs for deduplication
        o.QueryDelay = TimeSpan.FromMinutes(5);            // How often the background worker checks for old rows to delete
    });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<NotificationDbContext>(context);
    });

    

    x.UsingAzureServiceBus((context, cfg) =>
    {
        var connectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            // Local development / Service Bus emulator: SAS connection string.
            cfg.Host(connectionString);
        }
        else
        {
            // Azure-hosted (or az login locally): Managed Identity via DefaultAzureCredential.
            cfg.Host(new Uri(builder.Configuration["AzureServiceBus:Namespace"]!), h =>
            {
                h.TokenCredential = new DefaultAzureCredential();
            });
        }
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
