using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevSource.Foundation.Application;

/// <summary>
/// Registers domain event dispatching with an ASP.NET Core dependency injection container.
/// </summary>
public static class DomainEventServiceCollectionExtensions
{
    /// <summary>
    /// Adds the domain event dispatcher to the service collection.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for further configuration.</returns>
    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IDomainEventDispatcher>(serviceProvider =>
        {
            var dispatcher = new DomainEventDispatcher();

            foreach (var registration in serviceProvider
                .GetServices<Action<DomainEventDispatcher>>())
            {
                registration(dispatcher);
            }

            return dispatcher;
        });

        return services;
    }

    /// <summary>
    /// Registers a domain event handler and associates it with its event type.
    /// </summary>
    /// <typeparam name="TEvent">The domain event type.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection for further configuration.</returns>
    public static IServiceCollection AddHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : DomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<THandler>();
        services.AddScoped<Action<DomainEventDispatcher>>(serviceProvider =>
            dispatcher => dispatcher.Register<TEvent>(
                (domainEvent, cancellationToken) =>
                    serviceProvider.GetRequiredService<THandler>()
                        .HandleAsync(domainEvent, cancellationToken)));

        return services;
    }
}
