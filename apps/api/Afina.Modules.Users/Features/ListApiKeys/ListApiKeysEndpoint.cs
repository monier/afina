using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Api;
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
        catch (ApiException ex)
        {
            logger.LogWarning(ex, "Error listing API keys for user {UserId} with code: {Code}", userId, ex.Code);
            var statusCode = ex.Code switch
            {
                ErrorCodes.VALIDATION_ERROR => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            return Results.Json(new ApiError(ex.Code, ex.Message), statusCode: statusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error listing API keys for user {UserId}", userId);
            return Results.Json(new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred."),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
