using Azure.Identity;
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

        // Automatically flushes outbox messages to the broker after DB save changes
        o.UseBusOutbox();

        o.DuplicateDetectionWindow = TimeSpan.FromDays(7); // How long to remember message IDs for deduplication
        o.QueryDelay = TimeSpan.FromMinutes(5);            // How often the background worker checks for old rows to delete
    });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<InventoryDbContext>(context);
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
