using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Api;
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
        catch (ApiException ex)
        {
            logger.LogWarning(ex, "Login failed for username: {Username} with code: {Code}", request.Username, ex.Code);
            var statusCode = ex.Code switch
            {
                ErrorCodes.INVALID_CREDENTIALS => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            return Results.Json(new ApiError(ex.Code, ex.Message), statusCode: statusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login for username: {Username}", request.Username);
            return Results.Json(new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred."),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
