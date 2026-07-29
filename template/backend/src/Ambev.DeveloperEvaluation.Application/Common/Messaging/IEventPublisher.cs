namespace Ambev.DeveloperEvaluation.Application.Common.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class;
}
