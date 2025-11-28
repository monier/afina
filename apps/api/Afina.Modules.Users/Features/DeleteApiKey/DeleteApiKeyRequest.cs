using System;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.DeleteApiKey;

public sealed class DeleteApiKeyRequest : IRequest<EmptyResponse>
{
    public Guid KeyId { get; init; }
}
