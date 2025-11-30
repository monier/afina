using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Interfaces;
using Afina.Infrastructure.Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Afina.Modules.Users.Features.Register;

public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", HandleAsync)
            .WithTags("Authentication")
            .WithName("Register")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account and returns authentication tokens")
            .Produces<RegisterResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        RegisterRequest request,
        IMediator mediator,
        ILogger<RegisterEndpoint> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Registration endpoint called for username: {Username}", request.Username);

        try
        {
            var response = await mediator.CallAsync(request, ct);
            logger.LogInformation("Registration successful for username: {Username}", request.Username);
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Registration validation failed for username: {Username}", request.Username);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Registration failed for username: {Username}", request.Username);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during registration for username: {Username}", request.Username);
            throw;
        }
    }
}
