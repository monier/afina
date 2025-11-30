using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Interfaces;
using Afina.Infrastructure.Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/refresh", HandleAsync)
            .WithTags("Authentication")
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .WithDescription("Exchange a refresh token for a new access token and refresh token")
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        RefreshTokenRequest request,
        IMediator mediator,
        ILogger<RefreshTokenEndpoint> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Refresh token endpoint called");

        try
        {
            var response = await mediator.CallAsync(request, ct);
            logger.LogInformation("Token refreshed successfully");
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Token refresh failed - unauthorized");
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during token refresh");
            throw;
        }
    }
}
