using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ShoppingCart.Web.Services;

public abstract class ApiClientBase(IHttpClientFactory factory, TokenProvider tokens, string clientName)
{
    protected HttpClient Client()
    {
        var client = factory.CreateClient(clientName);
        if (tokens.Session is { } session)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        return client;
    }
}

public class AuthApi(IHttpClientFactory factory, TokenProvider tokens)
    : ApiClientBase(factory, tokens, "identity")
{
    public async Task<(AuthResponse? Session, string? Error)> LoginAsync(LoginRequest request)
    {
        var response = await Client().PostAsJsonAsync("/api/auth/login", request);
        if (!response.IsSuccessStatusCode) return (null, "Invalid email or password.");
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(), null);
    }

    public async Task<(AuthResponse? Session, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var response = await Client().PostAsJsonAsync("/api/auth/register", request);
        if (!response.IsSuccessStatusCode) return (null, "Registration failed — email may already exist.");
        return (await response.Content.ReadFromJsonAsync<AuthResponse>(), null);
    }
}

public class CatalogApi(IHttpClientFactory factory, TokenProvider tokens)
    : ApiClientBase(factory, tokens, "catalog")
{
    public Task<List<ProductDto>?> GetProductsAsync(string? category = null) =>
        Client().GetFromJsonAsync<List<ProductDto>>(
            category is null ? "/api/products" : $"/api/products?category={Uri.EscapeDataString(category)}");

    public Task<List<string>?> GetCategoriesAsync() =>
        Client().GetFromJsonAsync<List<string>>("/api/categories");
}

public class StockApi(IHttpClientFactory factory, TokenProvider tokens)
    : ApiClientBase(factory, tokens, "inventory")
{
    public Task<List<StockDto>?> GetStockAsync(IEnumerable<Guid> productIds)
    {
        var query = string.Join("&", productIds.Select(id => $"ids={id}"));
        return Client().GetFromJsonAsync<List<StockDto>>($"/api/stock/?{query}");
    }
}

public class BasketApi(IHttpClientFactory factory, TokenProvider tokens)
    : ApiClientBase(factory, tokens, "basket")
{
    public Task<BasketDto?> GetAsync() =>
        Client().GetFromJsonAsync<BasketDto>("/api/basket/");

    public async Task<BasketDto?> AddItemAsync(AddBasketItemRequest request)
    {
        var response = await Client().PostAsJsonAsync("/api/basket/items", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BasketDto>();
    }

    public async Task<BasketDto?> SetQuantityAsync(Guid productId, int quantity)
    {
        var response = await Client().PutAsJsonAsync($"/api/basket/items/{productId}", new UpdateQuantityRequest(quantity));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BasketDto>();
    }

    public async Task<BasketDto?> RemoveItemAsync(Guid productId)
    {
        var response = await Client().DeleteAsync($"/api/basket/items/{productId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BasketDto>();
    }

    public async Task<bool> CheckoutAsync()
    {
        var response = await Client().PostAsync("/api/basket/checkout", null);
        return response.IsSuccessStatusCode;
    }
}

public class OrdersApi(IHttpClientFactory factory, TokenProvider tokens)
    : ApiClientBase(factory, tokens, "ordering")
{
    public Task<List<OrderDto>?> GetMyOrdersAsync() =>
        Client().GetFromJsonAsync<List<OrderDto>>("/api/orders/");
}
