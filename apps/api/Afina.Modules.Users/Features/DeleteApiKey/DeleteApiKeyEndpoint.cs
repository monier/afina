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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting API key {KeyId} for user {UserId}", id, userId);
            throw;
        }
    }
}
