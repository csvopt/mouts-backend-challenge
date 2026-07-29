using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.IoC.Messaging;

public sealed class RebusEventPublisher : IEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<RebusEventPublisher> _logger;

    public RebusEventPublisher(IBus bus, ILogger<RebusEventPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(
            "Publishing domain event {EventType}: {@DomainEvent}",
            typeof(TEvent).Name,
            domainEvent);
        await _bus.Publish(domainEvent);
    }
}
