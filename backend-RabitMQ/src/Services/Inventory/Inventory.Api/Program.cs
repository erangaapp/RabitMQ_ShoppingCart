using Inventory.Api.Consumers;
using Inventory.Application;
using Inventory.Infrastructure;
using MassTransit;
using MassTransit.SqlTransport;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<StockService>();
builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();
    x.AddConsumer<OrderPlacedConsumer>();

    x.AddEntityFrameworkOutbox<InventoryDbContext>(o =>
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
        cfg.UseEntityFrameworkOutbox<InventoryDbContext>(context);
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

app.MapStockEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    for (var attempt = 1; ; attempt++)
    {
        try { await db.Database.EnsureCreatedAsync(); await SeedData.SeedAsync(db); break; }
        catch when (attempt < 10) { await Task.Delay(3000); }
    }
}

app.Run();
