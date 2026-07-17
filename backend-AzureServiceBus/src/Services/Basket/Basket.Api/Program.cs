using Azure.Identity;
using System.Text;
using Basket.Api.Consumers;
using Basket.Application;
using Basket.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BasketDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddOpenApi();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep raw "sub", "email", "name" claim types
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductPriceChangedConsumer>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapBasketEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
    for (var attempt = 1; ; attempt++)
    {
        try { await db.Database.EnsureCreatedAsync(); break; }
        catch when (attempt < 10) { await Task.Delay(3000); }
    }
}

app.Run();
