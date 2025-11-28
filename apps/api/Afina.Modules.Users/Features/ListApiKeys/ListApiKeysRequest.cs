using System;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.ListApiKeys;

public sealed class ListApiKeysRequest : IRequest<ListApiKeysResponse>
{
    public Guid UserId { get; init; }
}
