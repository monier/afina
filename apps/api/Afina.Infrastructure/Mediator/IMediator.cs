using System.Threading;
using System.Threading.Tasks;

namespace Afina.Infrastructure.Mediator;

public interface IMediator
{
    Task<TResponse> CallAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
