using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Interfaces;
using Afina.Infrastructure.Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.ListApiKeys;

public class ListApiKeysEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/users/me/api-keys", HandleAsync)
            .WithTags("API Keys")
            .WithName("ListApiKeys")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IMediator mediator,
        ILogger<ListApiKeysEndpoint> logger,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
        {
            logger.LogWarning("List API keys called without user ID claim");
            return Results.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim);
        logger.LogDebug("List API keys endpoint called for user {UserId}", userId);

        try
        {
            var req = new ListApiKeysRequest { UserId = userId };
            var res = await mediator.CallAsync(req, ct);
            return Results.Ok(res);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing API keys for user {UserId}", userId);
            throw;
        }
    }
}
