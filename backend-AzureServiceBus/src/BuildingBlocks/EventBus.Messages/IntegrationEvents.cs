namespace EventBus.Messages;

/// <summary>Published by Catalog when a product is created. Inventory reacts by creating an (empty) stock record.</summary>
public record ProductCreatedEvent(Guid ProductId, string ProductName);

/// <summary>Published by Inventory when a price changes. Basket reacts by re-pricing open baskets.</summary>
public record ProductPriceChangedEvent(Guid ProductId, decimal NewPrice);

/// <summary>Line item snapshot carried through checkout messages.</summary>
public record CheckoutItem(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

/// <summary>Published by Basket at checkout. Ordering consumes it and creates the Order.</summary>
public record BasketCheckoutRequestedEvent(
    Guid UserId,
    string Email,
    string FullName,
    List<CheckoutItem> Items,
    decimal Subtotal,
    decimal Discount,
    decimal Vat,
    decimal Total);

/// <summary>Published by Ordering after the order is persisted.
/// Inventory consumes it (stock decrement); Notification consumes it (confirmation email).</summary>
public record OrderPlacedEvent(
    Guid OrderId,
    Guid UserId,
    string Email,
    string FullName,
    List<CheckoutItem> Items,
    decimal Total,
    DateTime PlacedAtUtc);
