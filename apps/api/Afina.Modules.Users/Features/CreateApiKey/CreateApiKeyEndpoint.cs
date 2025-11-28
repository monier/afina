using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Interfaces;
using Afina.Infrastructure.Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null) return Results.Unauthorized();

        request.UserId = Guid.Parse(userIdClaim);
        var res = await mediator.CallAsync(request, ct);
        return Results.Created($"/api/v1/users/me/api-keys/{res.Id}", res);
    }
}
