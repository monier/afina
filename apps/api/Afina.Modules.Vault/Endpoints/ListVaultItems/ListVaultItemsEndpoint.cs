using Afina.Core.Interfaces;
using Afina.Modules.Vault.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Vault.Endpoints.ListVaultItems;

public class ListVaultItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/tenants/{tenantId:guid}/vault", HandleAsync)
            .WithTags("Vault")
            .WithName("ListVaultItems")
            .WithSummary("List vault items")
            .WithDescription("Retrieves all vault items for a tenant (with decrypted data)")
            .Produces<ListVaultItemsResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        Guid tenantId,
        VaultService vaultService,
        ILogger<ListVaultItemsEndpoint> logger,
        CancellationToken ct)
    {
        logger.LogInformation("List vault items endpoint called for tenant {TenantId}", tenantId);

        try
        {
            var response = await vaultService.ListVaultItemsAsync(tenantId, ct);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing vault items for tenant {TenantId}", tenantId);
            throw;
        }
    }
}
