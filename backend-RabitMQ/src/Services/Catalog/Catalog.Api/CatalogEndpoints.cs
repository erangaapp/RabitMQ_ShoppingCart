using Catalog.Application;
using EventBus.Messages;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Catalog");

        group.MapGet("/products", (string? category, CatalogService catalog, CancellationToken ct) =>
            catalog.GetProductsAsync(category, ct));

        group.MapGet("/products/{id:guid}", async (Guid id, CatalogService catalog, CancellationToken ct) =>
            await catalog.GetProductAsync(id, ct) is { } product ? Results.Ok(product) : Results.NotFound());

        group.MapGet("/categories", (CatalogService catalog, CancellationToken ct) =>
            catalog.GetCategoriesAsync(ct));

        group.MapPost("/products", async (
            CreateProductRequest request,
            CatalogService catalog,
            IPublishEndpoint publisher,
            CancellationToken ct) =>
        {
            var product = await catalog.CreateProductAsync(request, ct);

            // Choreography: Inventory listens and creates an empty stock record for the new product.
            await publisher.Publish(new ProductCreatedEvent(product.Id, product.Name), ct);

            return Results.Created($"/api/products/{product.Id}", product);
        });
    }
}
