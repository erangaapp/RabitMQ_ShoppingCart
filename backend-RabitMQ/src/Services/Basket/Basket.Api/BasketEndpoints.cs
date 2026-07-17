using System.Security.Claims;
using Basket.Application;
using EventBus.Messages;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

public static class BasketEndpoints
{
    public static void MapBasketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/basket").WithTags("Basket").RequireAuthorization();

        group.MapGet("/", (ClaimsPrincipal user, BasketService service, CancellationToken ct) =>
            service.GetAsync(UserId(user), ct));

        group.MapPost("/items", (ClaimsPrincipal user, AddBasketItemRequest request, BasketService service, CancellationToken ct) =>
            service.AddItemAsync(UserId(user), request, ct));

        group.MapPut("/items/{productId:guid}", (ClaimsPrincipal user, Guid productId, UpdateQuantityRequest request, BasketService service, CancellationToken ct) =>
            service.SetQuantityAsync(UserId(user), productId, request.Quantity, ct));

        group.MapDelete("/items/{productId:guid}", (ClaimsPrincipal user, Guid productId, BasketService service, CancellationToken ct) =>
            service.RemoveItemAsync(UserId(user), productId, ct));

        group.MapPost("/checkout", async (
            ClaimsPrincipal user,
            BasketService service,
            IPublishEndpoint publisher,
            CancellationToken ct) =>
        {
            var snapshot = await service.CheckoutAsync(UserId(user), ct);
            if (snapshot is null)
                return Results.BadRequest(new { error = "Basket is empty." });

            await publisher.Publish(new BasketCheckoutRequestedEvent(
                snapshot.UserId,
                user.FindFirstValue("email") ?? string.Empty,
                user.FindFirstValue("name") ?? string.Empty,
                snapshot.Items.Select(i => new CheckoutItem(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
                snapshot.Subtotal, snapshot.Discount, snapshot.Vat, snapshot.Total), ct);

            return Results.Accepted(value: new { message = "Checkout accepted. Order is being processed." });
        });
    }

    private static Guid UserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub")!);
}
