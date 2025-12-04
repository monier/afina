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

namespace Afina.Modules.Users.Features.DeleteApiKey;

public class DeleteApiKeyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/users/me/api-keys/{id}", HandleAsync)
            .WithTags("API Keys")
            .WithName("DeleteApiKey")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        Guid id,
        IMediator mediator,
        ILogger<DeleteApiKeyEndpoint> logger,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
        {
            logger.LogWarning("Delete API key called without user ID claim");
            return Results.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim);
        logger.LogInformation("Delete API key endpoint called for key {KeyId} by user {UserId}", id, userId);

        try
        {
            var req = new DeleteApiKeyRequest { KeyId = id };
            await mediator.CallAsync(req, ct);
            logger.LogInformation("API key {KeyId} deleted via endpoint by user {UserId}", id, userId);
            return Results.NoContent();
        }
        catch (ApiException ex)
        {
            logger.LogWarning(ex, "Error deleting API key {KeyId} for user {UserId} with code: {Code}", id, userId, ex.Code);
            var statusCode = ex.Code switch
            {
                ErrorCodes.VALIDATION_ERROR => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            return Results.Json(new ApiError(ex.Code, ex.Message), statusCode: statusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting API key {KeyId} for user {UserId}", id, userId);
            return Results.Json(new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred."),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
