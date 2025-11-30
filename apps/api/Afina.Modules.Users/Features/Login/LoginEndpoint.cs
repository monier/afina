using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Interfaces;
using Afina.Infrastructure.Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.Login;

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
        IMediator mediator,
        ILogger<LoginEndpoint> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Login endpoint called for username: {Username}", request.Username);

        try
        {
            var response = await mediator.CallAsync(request, ct);
            logger.LogInformation("Login successful for username: {Username}", request.Username);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Login failed for username: {Username}", request.Username);
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login for username: {Username}", request.Username);
            throw;
        }
    }
}
