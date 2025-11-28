using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Afina.Infrastructure.Mediator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] additionalAssemblies)
    {
        services.AddSingleton<IMediator, Mediator>();

        var assemblies = additionalAssemblies is { Length: > 0 }
            ? AppDomain.CurrentDomain.GetAssemblies().Concat(additionalAssemblies).Distinct().ToArray()
            : AppDomain.CurrentDomain.GetAssemblies();

        var handlerInterfaceType = typeof(IRequestHandler<,>);
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(i => (Implementation: t, Service: i)))
            .ToList();

        foreach (var (implementation, service) in handlerTypes)
            services.AddScoped(service, implementation);

        return services;
    }
}
