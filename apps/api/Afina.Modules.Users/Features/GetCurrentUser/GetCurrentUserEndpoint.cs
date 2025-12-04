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

namespace Afina.Modules.Users.Features.GetCurrentUser;

public class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/users/me", HandleAsync)
            .RequireAuthorization()
            .WithTags("Users")
            .WithName("GetCurrentUser")
            .Produces<GetCurrentUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IMediator mediator,
        ILogger<GetCurrentUserEndpoint> logger,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
        {
            logger.LogWarning("Get current user called without user ID claim");
            return Results.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim);
        logger.LogDebug("Get current user endpoint called for user {UserId}", userId);

        try
        {
            var req = new GetCurrentUserRequest { UserId = userId };
            var res = await mediator.CallAsync(req, ct);
            return Results.Ok(res);
        }
        catch (ApiException ex)
        {
            logger.LogWarning(ex, "Error getting current user {UserId} with code: {Code}", userId, ex.Code);
            var statusCode = ex.Code switch
            {
                ErrorCodes.USER_DELETED => StatusCodes.Status401Unauthorized,
                ErrorCodes.UNAUTHORIZED => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            return Results.Json(new ApiError(ex.Code, ex.Message), statusCode: statusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error getting current user {UserId}", userId);
            return Results.Json(new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred."),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
