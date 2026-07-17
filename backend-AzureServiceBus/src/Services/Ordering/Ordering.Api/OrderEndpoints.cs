using System.Security.Claims;
using Ordering.Application;

namespace Microsoft.AspNetCore.Builder;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        group.MapGet("/", (ClaimsPrincipal user, OrderQueryService service, CancellationToken ct) =>
            service.GetMyOrdersAsync(UserId(user), ct));

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, OrderQueryService service, CancellationToken ct) =>
            await service.GetAsync(id, UserId(user), ct) is { } order ? Results.Ok(order) : Results.NotFound());
    }

    private static Guid UserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub")!);
}
