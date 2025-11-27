using Afina.Core.Interfaces;
using Afina.Modules.Vault.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Afina.Modules.Vault.Endpoints.CreateVaultItem;

public class CreateVaultItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/tenants/{tenantId:guid}/vault", HandleAsync)
            .WithTags("Vault")
            .WithName("CreateVaultItem")
            .WithSummary("Create a vault item")
            .WithDescription("Creates a new encrypted vault item for a tenant")
            .Produces<CreateVaultItemResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        Guid tenantId,
        CreateVaultItemRequest request,
        VaultService vaultService,
        CancellationToken ct)
    {
        // TODO: Get current user ID from authentication context
        var currentUserId = Guid.NewGuid(); // Placeholder

        var response = await vaultService.CreateVaultItemAsync(tenantId, request, currentUserId, ct);
        return Results.Ok(response);
    }
}
