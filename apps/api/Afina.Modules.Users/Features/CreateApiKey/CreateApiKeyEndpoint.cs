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

namespace Afina.Modules.Users.Features.CreateApiKey;

public class CreateApiKeyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/users/me/api-keys", HandleAsync)
            .WithTags("API Keys")
            .WithName("CreateApiKey")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        CreateApiKeyRequest request,
        IMediator mediator,
        ILogger<CreateApiKeyEndpoint> logger,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
        {
            logger.LogWarning("Create API key called without user ID claim");
            return Results.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim);
        logger.LogInformation("Create API key endpoint called for user {UserId} with key name {KeyName}", userId, request.Name);

        try
        {
            request.UserId = userId;
            var res = await mediator.CallAsync(request, ct);
            logger.LogInformation("API key {KeyId} created via endpoint for user {UserId}", res.Id, userId);
            return Results.Created($"/api/v1/users/me/api-keys/{res.Id}", res);
        }
        catch (ApiException ex)
        {
            logger.LogWarning(ex, "Error creating API key for user {UserId} with code: {Code}", userId, ex.Code);
            var statusCode = ex.Code switch
            {
                ErrorCodes.VALIDATION_ERROR => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            return Results.Json(new ApiError(ex.Code, ex.Message), statusCode: statusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating API key for user {UserId}", userId);
            return Results.Json(new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred."),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
