using Afina.Core.Interfaces;
using Afina.Modules.Tenant.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Afina.Modules.Tenant.Endpoints.CreateTenant;

public class CreateTenantEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/tenants", HandleAsync)
            .WithTags("Tenants")
            .WithName("CreateTenant")
            .WithSummary("Create a new tenant (organization)")
            .WithDescription("Creates a new organization tenant")
            .Produces<CreateTenantResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        CreateTenantRequest request,
        TenantService tenantService,
        CancellationToken ct)
    {
        // TODO: Get current user ID from authentication context
        var currentUserId = Guid.NewGuid(); // Placeholder

        var response = await tenantService.CreateTenantAsync(request, currentUserId, ct);
        return Results.Ok(response);
    }
}
