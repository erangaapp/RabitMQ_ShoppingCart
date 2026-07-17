using Azure.Identity;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ordering.Api.Consumers;
using Ordering.Application;
using Ordering.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderingDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderQueryService>();
builder.Services.AddOpenApi();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
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
    x.AddConsumer<BasketCheckoutRequestedConsumer>();

    x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
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
        cfg.UseEntityFrameworkOutbox<OrderingDbContext>(context);
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
        cfg.UseMessageRetry(r => r.Interval(2, TimeSpan.FromSeconds(5)));
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapOrderEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    for (var attempt = 1; ; attempt++)
    {
        try { await db.Database.EnsureCreatedAsync(); break; }
        catch when (attempt < 10) { await Task.Delay(3000); }
    }
}

app.Run();
