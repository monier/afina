using Afina.Core.Interfaces;
using Afina.Modules.Tenant.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

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
        ILogger<CreateTenantEndpoint> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Create tenant endpoint called for tenant name: {TenantName}", request.Name);

        // TODO: Get current user ID from authentication context
        var currentUserId = Guid.NewGuid(); // Placeholder
        logger.LogWarning("Using placeholder user ID for tenant creation");

        try
        {
            var response = await tenantService.CreateTenantAsync(request, currentUserId, ct);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tenant {TenantName}", request.Name);
            throw;
        }
    }
}
