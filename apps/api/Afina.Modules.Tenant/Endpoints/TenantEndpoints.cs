using Afina.Modules.Tenant.DTOs;
using Afina.Modules.Tenant.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Afina.Modules.Tenant.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants")
            .WithTags("Tenants")
            ;

        group.MapPost("/", async (CreateTenantRequest request, TenantService tenantService, CancellationToken ct) =>
        {
            // TODO: Get current user ID from authentication context
            var currentUserId = Guid.NewGuid(); // Placeholder

            var response = await tenantService.CreateTenantAsync(request, currentUserId, ct);
            return Results.Ok(response);
        })
        .WithName("CreateTenant")
        .WithSummary("Create a new tenant (organization)")
        .WithDescription("Creates a new organization tenant")
        .Produces<CreateTenantResponse>(StatusCodes.Status200OK);

        return app;
    }
}
