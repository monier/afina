using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Afina.Infrastructure.Mediator;

internal sealed class Mediator : IMediator
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Mediator> _logger;
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    public Mediator(IServiceScopeFactory scopeFactory, ILogger<Mediator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<TResponse> CallAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var handlerType = HandlerTypeCache.GetOrAdd(requestType, ResolveHandlerType);
        var requestName = requestType.Name;

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Executing handler for {RequestType}",
            requestName
        );

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is null)
            {
                _logger.LogError(
                    "No handler registered for request type {RequestType}",
                    requestType.FullName
                );
                throw new InvalidOperationException($"No handler registered for request type {requestType.FullName}");
            }

            // Invoke HandleAsync via reflection (generic constraints prevent direct cast without MakeGenericType)
            var method = handlerType.GetMethod("HandleAsync")!;
            var task = (Task)method.Invoke(handler, new object[] { request, ct })!;
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            if (resultProperty is null)
            {
                _logger.LogError("Handler {HandlerType} did not return a result", handlerType.Name);
                throw new InvalidOperationException("Handler did not return a result.");
            }

            stopwatch.Stop();
            _logger.LogInformation(
                "Handler {RequestType} completed successfully in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds
            );

            return (TResponse)resultProperty.GetValue(task)!;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Handler {RequestType} failed after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }

    private static Type ResolveHandlerType(Type requestType)
    {
        var responseInterface = requestType.GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
        if (responseInterface == null)
            throw new InvalidOperationException($"Type {requestType.FullName} does not implement IRequest<T>.");
        var responseType = responseInterface.GetGenericArguments()[0];
        var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        // Handler types implement handlerInterface; we rely on DI registration to match interface
        return handlerInterface;
    }
}
