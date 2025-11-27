using Afina.Core.Interfaces;
using Afina.Modules.Identity.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Afina.Modules.Identity.Endpoints.Register;

public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", HandleAsync)
            .WithTags("Authentication")
            .WithName("Register")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account and an individual tenant for the user")
            .Produces<RegisterResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        AuthService authService,
        CancellationToken ct)
    {
        try
        {
            var response = await authService.RegisterAsync(request, ct);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}
