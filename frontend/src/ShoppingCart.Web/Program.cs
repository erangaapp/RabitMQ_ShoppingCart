using ShoppingCart.Web.Components;
using ShoppingCart.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

// One named HttpClient per backend microservice.
var services = new (string Name, string ConfigKey)[]
{
    ("identity", "Services:Identity"),
    ("catalog", "Services:Catalog"),
    ("inventory", "Services:Inventory"),
    ("basket", "Services:Basket"),
    ("ordering", "Services:Ordering")
};
foreach (var (name, key) in services)
{
    var baseUrl = builder.Configuration[key]
        ?? throw new InvalidOperationException($"Missing config '{key}'.");
    builder.Services.AddHttpClient(name, c => c.BaseAddress = new Uri(baseUrl));
}

// Scoped = one instance per Blazor circuit (per browser session while connected).
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<AuthApi>();
builder.Services.AddScoped<CatalogApi>();
builder.Services.AddScoped<StockApi>();
builder.Services.AddScoped<BasketApi>();
builder.Services.AddScoped<OrdersApi>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
