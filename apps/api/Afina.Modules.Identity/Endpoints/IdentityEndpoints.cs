using Afina.Modules.Identity.DTOs;
using Afina.Modules.Identity.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Afina.Modules.Identity.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication")
            ;

        group.MapPost("/register", async (RegisterRequest request, AuthService authService, CancellationToken ct) =>
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
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account and an individual tenant for the user")
        .Produces<RegisterResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginRequest request, AuthService authService, CancellationToken ct) =>
        {
            try
            {
                var response = await authService.LoginAsync(request, ct);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("Login")
        .WithSummary("Login a user")
        .WithDescription("Authenticates a user and returns user information")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
