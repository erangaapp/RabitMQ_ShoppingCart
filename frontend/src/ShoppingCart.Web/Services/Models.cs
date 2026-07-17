namespace ShoppingCart.Web.Services;

// Auth (Identity service)
public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(Guid UserId, string Email, string FullName, string Token);

// Catalog service
public record ProductDto(Guid Id, string Name, string Description, string Category, string ImageUrl);

// Inventory (Stock & Price) service
public record StockDto(Guid ProductId, string ProductName, int Quantity, decimal Price);
public record UpdateStockRequest(decimal Price, int Quantity);

// Basket service
public record BasketItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record BasketDto(Guid UserId, List<BasketItemDto> Items, decimal Subtotal, decimal Discount, decimal Vat, decimal Total);
public record AddBasketItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record UpdateQuantityRequest(int Quantity);

// Ordering service
public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record OrderDto(
    Guid Id, string Status, decimal Subtotal, decimal Discount, decimal Vat, decimal Total,
    DateTime PlacedAtUtc, List<OrderItemDto> Items);
