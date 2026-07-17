using Identity.Application;

namespace Microsoft.AspNetCore.Builder;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, AuthService auth, CancellationToken ct) =>
        {
            var result = await auth.RegisterAsync(request, ct);
            return result.Succeeded
                ? Results.Ok(result.Response)
                : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/login", async (LoginRequest request, AuthService auth, CancellationToken ct) =>
        {
            var result = await auth.LoginAsync(request, ct);
            return result.Succeeded
                ? Results.Ok(result.Response)
                : Results.Json(new { error = result.Error }, statusCode: StatusCodes.Status401Unauthorized);
        });
    }
}
