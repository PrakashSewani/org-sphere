namespace OrgSphere.Domain.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    void Subscribe(Func<IDomainEvent, CancellationToken, Task> handler);
}
