using Afina.Core.Interfaces;
using Afina.Modules.Identity.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Afina.Modules.Identity.Endpoints.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", HandleAsync)
            .WithTags("Authentication")
            .WithName("Login")
            .WithSummary("Login a user")
            .WithDescription("Authenticates a user and returns user information")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        AuthService authService,
        CancellationToken ct)
    {
        try
        {
            var response = await authService.LoginAsync(request, ct);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }
}
