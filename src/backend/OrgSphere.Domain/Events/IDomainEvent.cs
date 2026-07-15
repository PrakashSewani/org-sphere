using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    string EventType { get; }
    TenantId TenantId { get; }
    DateTime OccurredAt { get; }
}

public record NodeCreatedEvent(TenantId TenantId, NodeId NodeId, NodeType Type) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Node.Created";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record NodeUpdatedEvent(TenantId TenantId, NodeId NodeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Node.Updated";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record NodeDeletedEvent(TenantId TenantId, NodeId NodeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Node.Deleted";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record EdgeCreatedEvent(TenantId TenantId, EdgeId EdgeId, EdgeType Type, NodeId SourceId, NodeId TargetId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Edge.Created";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record EdgeDeletedEvent(TenantId TenantId, EdgeId EdgeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Edge.Deleted";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record UserCreatedEvent(TenantId TenantId, UserId UserId, string Email) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "User.Created";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record UserLoginEvent(TenantId TenantId, UserId UserId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "User.Login";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record TenantCreatedEvent(TenantId TenantId, string Name) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Tenant.Created";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record EmployeeCreatedEvent(TenantId TenantId, Guid EmployeeId, string Email) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Employee.Created";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record EmployeeUpdatedEvent(TenantId TenantId, Guid EmployeeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Employee.Updated";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record EmployeeDeletedEvent(TenantId TenantId, Guid EmployeeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Employee.Deleted";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record LeaveRequestCreatedEvent(TenantId TenantId, Guid LeaveRequestId, Guid EmployeeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "LeaveRequest.Created";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record LeaveRequestApprovedEvent(TenantId TenantId, Guid LeaveRequestId, Guid EmployeeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "LeaveRequest.Approved";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record LeaveRequestRejectedEvent(TenantId TenantId, Guid LeaveRequestId, Guid EmployeeId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "LeaveRequest.Rejected";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record AttendanceCheckedInEvent(TenantId TenantId, Guid EmployeeId, DateTime CheckTime) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Attendance.CheckedIn";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record AttendanceCheckedOutEvent(TenantId TenantId, Guid EmployeeId, DateTime CheckTime) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "Attendance.CheckedOut";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
