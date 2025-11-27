using Microsoft.AspNetCore.Routing;

namespace Afina.Core.Interfaces;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
