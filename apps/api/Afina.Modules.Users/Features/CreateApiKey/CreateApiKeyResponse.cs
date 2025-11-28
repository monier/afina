using System;

namespace Afina.Modules.Users.Features.CreateApiKey;

public sealed class CreateApiKeyResponse
{
    public Guid Id { get; init; }
    public string KeyPrefix { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
}
