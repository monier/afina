using Afina.Modules.Vault.DTOs;
using Afina.Modules.Vault.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Afina.Modules.Vault.Endpoints;

public static class VaultEndpoints
{
    public static IEndpointRouteBuilder MapVaultEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants/{tenantId:guid}/vault")
            .WithTags("Vault")
            ;

        group.MapPost("/", async (Guid tenantId, CreateVaultItemRequest request, VaultService vaultService, CancellationToken ct) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.NewGuid(); // Placeholder

            var response = await vaultService.CreateVaultItemAsync(tenantId, request, currentUserId, ct);
            return Results.Ok(response);
        })
        .WithName("CreateVaultItem")
        .WithSummary("Create a vault item")
        .WithDescription("Creates a new encrypted vault item for a tenant")
        .Produces<CreateVaultItemResponse>(StatusCodes.Status200OK);

        group.MapGet("/", async (Guid tenantId, VaultService vaultService, CancellationToken ct) =>
        {
            var response = await vaultService.ListVaultItemsAsync(tenantId, ct);
            return Results.Ok(response);
        })
        .WithName("ListVaultItems")
        .WithSummary("List vault items")
        .WithDescription("Retrieves all vault items for a tenant (with decrypted data)")
        .Produces<ListVaultItemsResponse>(StatusCodes.Status200OK);

        return app;
    }
}
