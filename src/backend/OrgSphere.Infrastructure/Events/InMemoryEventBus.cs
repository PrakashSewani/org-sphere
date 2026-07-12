using Microsoft.Extensions.Logging;
using OrgSphere.Domain.Events;

namespace OrgSphere.Infrastructure.Events;

public class InMemoryEventBus(ILogger<InMemoryEventBus> logger) : IEventBus
{
    private readonly List<Func<IDomainEvent, CancellationToken, Task>> _handlers = [];

    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var eventType = domainEvent.EventType;
        var eventId = domainEvent.EventId;
        var tenantId = domainEvent.TenantId;
        logger.LogInformation("Publishing event {EventType} ({EventId}) for tenant {TenantId}",
            eventType, eventId, tenantId);

        foreach (var handler in _handlers)
        {
            _ = Task.Run(() => handler(domainEvent, cancellationToken), cancellationToken);
        }

        return Task.CompletedTask;
    }

    public void Subscribe(Func<IDomainEvent, CancellationToken, Task> handler)
    {
        _handlers.Add(handler);
    }
}
