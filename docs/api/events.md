# Events

## Event System

Event-driven architecture for decoupled communication between services.

---

## Event Types

### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| employee.created | New employee added | Analytics, Notifications |
| employee.updated | Employee info changed | Analytics, Search |
| employee.deleted | Employee removed | Analytics, Search |
| department.created | New department | Graph, Analytics |
| department.updated | Department changed | Graph, Analytics |
| team.created | New team | Graph, Analytics |
| team.member.added | Member joined team | Notifications, Analytics |
| team.member.removed | Member left team | Notifications, Analytics |

### Workflow Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| leave.requested | Leave request submitted | Notifications, Approvals |
| leave.approved | Leave approved | Notifications, Attendance |
| leave.rejected | Leave rejected | Notifications |
| attendance.checked.in | Employee checked in | Analytics |
| attendance.checked.out | Employee checked out | Analytics |
| approval.requested | Approval needed | Notifications |
| approval.approved | Approval granted | Notifications, Workflow |
| approval.rejected | Approval denied | Notifications, Workflow |
| approval.escalated | Approval escalated | Notifications |

### System Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| user.login | User logged in | Audit, Analytics |
| user.logout | User logged out | Audit |
| settings.updated | Settings changed | All services |
| integration.synced | Integration synced | Analytics |

---

## Event Structure

### Event Envelope

```json
{
  "id": "evt-123",
  "type": "employee.created",
  "tenantId": "tenant-456",
  "timestamp": "2025-01-15T10:30:00Z",
  "version": "1.0",
  "data": {
    "employeeId": "emp-789",
    "name": "John Doe",
    "department": "Engineering"
  },
  "metadata": {
    "userId": "user-012",
    "source": "employee-service"
  }
}
```

### Event Properties

| Property | Type | Description |
|----------|------|-------------|
| id | string | Unique event ID |
| type | string | Event type |
| tenantId | string | Tenant identifier |
| timestamp | datetime | When event occurred |
| version | string | Schema version |
| data | object | Event payload |
| metadata | object | Additional context |

---

## Event Publishing

### Publishing Pattern

```csharp
await _eventBus.PublishAsync(
    new EmployeeCreatedEvent(
        TenantId: _tenantContext.TenantId!,
        EmployeeId: employee.Id,
        Name: employee.Name),
    cancellationToken);
```

### Guaranteed Delivery

- At-least-once delivery
- Retry with exponential backoff
- Dead letter queue for failures
- Delivery confirmation

---

## Event Consumption

### Consumer Pattern

```csharp
// Register handler in DI
_eventBus.Subscribe(async (IDomainEvent domainEvent, CancellationToken ct) =>
{
    if (domainEvent is EmployeeCreatedEvent e)
    {
        await updateSearchIndexAsync(e, ct);
        await sendWelcomeNotificationAsync(e, ct);
        await updateAnalyticsAsync(e, ct);
    }
});
```

### Consumer Groups

- Each service has its own consumer group
- Independent offset tracking
- Parallel processing within group
- Ordered processing within partition

---

## Event Routing

### Topic-Based

```
domain.employee.created → Employee Service, Analytics Service
domain.leave.requested → Leave Service, Notification Service
system.user.logged → Audit Service, Analytics Service
```

### Pattern-Based

```
domain.* → Analytics Service
*.created → Search Service
workflow.* → Notification Service
```

---

## Event Store

### Storage

- Append-only log
- Partitioned by tenant
- Configurable retention
- Replay capability

### Query

- Query by event type
- Query by tenant
- Query by time range
- Query by aggregate

---

## Integration Patterns

### Saga Pattern

For distributed transactions:

```
1. Start saga
2. Execute step 1
3. Execute step 2
4. Complete saga
   OR
   Compensate step 1
   Compensate step 0
```

### CQRS

Command Query Responsibility Segregation:

- Commands produce events
- Queries read from projections
- Events update projections
- Projections optimized for reads

---

## Event Schema Evolution

### Versioning

- Event version in envelope
- Backward compatible changes
- Forward compatible consumers
- Migration strategy for breaking changes

### Compatibility

- Additive changes: No version bump
- Breaking changes: New version
- Deprecated fields: Marked, not removed
- Consumer tolerance: Ignore unknown fields
