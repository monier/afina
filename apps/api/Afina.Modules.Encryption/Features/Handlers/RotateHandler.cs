using System;
using System.Threading;
using System.Threading.Tasks;
using Afina.Infrastructure.Mediator;

namespace Afina.Modules.Encryption.Features.Handlers;

public sealed class RotateRequest : IRequest<RotateResponse>
{
    public Guid EncryptionId { get; init; }
}

public sealed class RotateResponse
{
    public Guid NewVersionId { get; init; }
}

public sealed class RotateHandler : IRequestHandler<RotateRequest, RotateResponse>
{
    public Task<RotateResponse> HandleAsync(RotateRequest request, CancellationToken ct)
    {
        // TODO: call external or internal encryption service to rotate keys
        return Task.FromResult(new RotateResponse { NewVersionId = Guid.NewGuid() });
    }
}
