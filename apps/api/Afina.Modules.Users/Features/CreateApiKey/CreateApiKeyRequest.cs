using System;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Users.Features.CreateApiKey;

public sealed class CreateApiKeyRequest : IRequest<CreateApiKeyResponse>
{
    public Guid UserId { get; set; }
    public string Name { get; init; } = string.Empty;
}
