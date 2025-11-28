using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Encryption.Features.Handlers;

public sealed class EncryptRequest : IRequest<EncryptResponse>
{
    public Guid EncryptionId { get; init; }
    public string Data { get; init; } = string.Empty;
    public Guid? EncryptionVersionId { get; init; }
}

public sealed class EncryptResponse
{
    public string CipherText { get; init; } = string.Empty;
    public Guid EncryptionId { get; init; }
    public Guid EncryptionVersionId { get; init; }
    public string Algorithm { get; init; } = string.Empty;
    public object Metadata { get; init; } = default!;
}

public sealed class EncryptHandler : IRequestHandler<EncryptRequest, EncryptResponse>
{
    public Task<EncryptResponse> HandleAsync(EncryptRequest request, CancellationToken ct)
    {
        // TODO: call external or internal encryption service per tenant config
        return Task.FromResult(new EncryptResponse
        {
            CipherText = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Data)),
            EncryptionId = request.EncryptionId,
            EncryptionVersionId = request.EncryptionVersionId ?? Guid.NewGuid(),
            Algorithm = "AES-256-GCM",
            Metadata = new { iv = "demo" }
        });
    }
}
