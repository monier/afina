using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Afina.Core.Interfaces;
using Afina.Infrastructure.Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null) return Results.Unauthorized();

        var req = new ListApiKeysRequest { UserId = Guid.Parse(userIdClaim) };
        var res = await mediator.CallAsync(req, ct);
        return Results.Ok(res);
    }
}
