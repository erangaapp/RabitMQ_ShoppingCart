using Azure.Identity;
using Catalog.Application;
using Catalog.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CatalogDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
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

app.MapCatalogEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    for (var attempt = 1; ; attempt++)
    {
        try { await db.Database.EnsureCreatedAsync(); await SeedData.SeedAsync(db); break; }
        catch when (attempt < 10) { await Task.Delay(3000); }
    }
}

app.Run();
