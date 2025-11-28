using System.Collections.Generic;

namespace Afina.Modules.Users.Features.ListApiKeys;

public sealed class ListApiKeysResponse
{
    public List<object> Keys { get; init; } = new();
}
