using Identity.Application;
using Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IdentityDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(); // interactive API docs at /scalar/v1

app.MapAuthEndpoints();

// POC only: EnsureCreated + seed (use EF migrations for anything beyond a POC).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    for (var attempt = 1; ; attempt++)
    {
        try { await db.Database.EnsureCreatedAsync(); await SeedData.SeedAsync(db, hasher); break; }
        catch when (attempt < 10) { await Task.Delay(3000); } // wait for SQL container
    }
}

app.Run();
