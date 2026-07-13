using Microsoft.AspNetCore.SignalR;
using OrgSphere.API.Hubs;
using OrgSphere.Domain.Events;

namespace OrgSphere.API.Hubs;

public class GraphEventRelay : IHostedService
{
    private readonly IEventBus _eventBus;
    private readonly IHubContext<GraphHub> _hubContext;

    public GraphEventRelay(IEventBus eventBus, IHubContext<GraphHub> hubContext)
    {
        _eventBus = eventBus;
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventBus.Subscribe(HandleEventAsync);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task HandleEventAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var tenantGroup = $"tenant:{domainEvent.TenantId}";

        var message = new GraphEventMessage
        {
            EventType = domainEvent.EventType,
            EventId = domainEvent.EventId,
            OccurredAt = domainEvent.OccurredAt,
            Data = ExtractPayload(domainEvent)
        };

        await _hubContext.Clients.Group(tenantGroup).SendAsync("GraphEvent", message, ct);
    }

    private static object ExtractPayload(IDomainEvent domainEvent) => domainEvent switch
    {
        NodeCreatedEvent e => new { NodeType = e.Type.ToString(), NodeId = e.NodeId.Value },
        NodeUpdatedEvent e => new { NodeId = e.NodeId.Value },
        NodeDeletedEvent e => new { NodeId = e.NodeId.Value },
        EdgeCreatedEvent e => new { EdgeId = e.EdgeId.Value, Type = e.Type.ToString(), SourceId = e.SourceId.Value, TargetId = e.TargetId.Value },
        EdgeDeletedEvent e => new { EdgeId = e.EdgeId.Value },
        _ => new { }
    };
}

public record GraphEventMessage
{
    public string EventType { get; init; } = string.Empty;
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public object Data { get; init; } = new();
}
