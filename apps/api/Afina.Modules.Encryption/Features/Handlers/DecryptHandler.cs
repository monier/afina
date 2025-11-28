using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Encryption.Features.Handlers;

public sealed class DecryptRequest : IRequest<DecryptResponse>
{
    public Guid EncryptionId { get; init; }
    public string CipherText { get; init; } = string.Empty;
    public Guid EncryptionVersionId { get; init; }
    public object Metadata { get; init; } = default!;
}

public sealed class DecryptResponse
{
    public string Data { get; init; } = string.Empty;
}

public sealed class DecryptHandler : IRequestHandler<DecryptRequest, DecryptResponse>
{
    public Task<DecryptResponse> HandleAsync(DecryptRequest request, CancellationToken ct)
    {
        // TODO: call external or internal encryption service per tenant config
        var bytes = Convert.FromBase64String(request.CipherText);
        return Task.FromResult(new DecryptResponse { Data = System.Text.Encoding.UTF8.GetString(bytes) });
    }
}
