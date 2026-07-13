# Module Development Skill

Load this skill when creating new modules or modifying existing modules in OrgSphere.

---

## When to Use

- Creating a new module
- Adding features to existing modules
- Refactoring module structure
- Integrating modules with the graph

---

## Tech Stack

- **Backend**: C# / .NET 10, Clean Architecture
- **Database**: Neo4j (graph), Redis (cache)
- **API**: REST controllers with ASP.NET Core
- **Real-time**: SignalR for graph updates
- **CQRS**: DispatchR.Mediator
- **Validation**: FluentValidation
- **Events**: InMemoryEventBus (domain events)

---

## Module Structure

### Clean Architecture Layout

Each module spans multiple projects following Clean Architecture:

```
src/backend/
├── OrgSphere.Domain/              # Entities, interfaces, enums (no dependencies)
│   ├── Entities/
│   │   └── LeaveRequest.cs
│   ├── Enums/
│   │   └── LeaveType.cs
│   └── Interfaces/
│       └── ILeaveRepository.cs
│
├── OrgSphere.Application/         # CQRS handlers, validators, DTOs
│   ├── Commands/
│   │   └── CreateLeaveRequestCommand.cs
│   ├── Queries/
│   │   └── GetLeaveRequestsQuery.cs
│   ├── Services/
│   │   └── LeaveService.cs
│   ├── DTOs/
│   │   └── LeaveDtos.cs
│   └── Validators/
│       └── CreateLeaveRequestValidator.cs
│
├── OrgSphere.Infrastructure/      # Neo4j repositories, external services
│   └── Repositories/
│       └── LeaveRepository.cs
│
└── OrgSphere.API/                 # Controllers, middleware
    └── Controllers/
        └── LeaveController.cs
```

---

## Creating a New Module

### Step 1: Define Domain Entities

```csharp
// OrgSphere.Domain/Enums/LeaveType.cs
namespace OrgSphere.Domain.Enums;

public enum LeaveType
{
    Vacation,
    Sick,
    Personal,
    Parental
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}
```

```csharp
// OrgSphere.Domain/Entities/LeaveRequest.cs
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class LeaveRequest
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public LeaveType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaveStatus Status { get; set; }
    public string? Reason { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Step 2: Define Repository Interface

```csharp
// OrgSphere.Domain/Interfaces/ILeaveRepository.cs
using OrgSphere.Domain.Entities;

namespace OrgSphere.Domain.Interfaces;

public interface ILeaveRepository
{
    Task<LeaveRequest> CreateAsync(LeaveRequest request, CancellationToken ct = default);
    Task<LeaveRequest?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetAllAsync(TenantId tenantId, CancellationToken ct = default);
    Task<LeaveRequest> UpdateAsync(LeaveRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken ct = default);
}
```

### Step 3: Create DTOs

```csharp
// OrgSphere.Application/DTOs/LeaveDtos.cs
using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.DTOs;

public record LeaveRequestDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public LeaveType Type { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public LeaveStatus Status { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateLeaveRequestCommand(
    Guid EmployeeId,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason);
```

### Step 4: Create Service with CQRS

```csharp
// OrgSphere.Application/Services/LeaveService.cs
using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;

namespace OrgSphere.Application.Services;

public class LeaveService(
    ILeaveRepository repository,
    IGraphService graphService,
    IEventBus eventBus,
    ITenantContext tenantContext)
{
    public async Task<LeaveRequestDto> CreateRequestAsync(
        CreateLeaveRequestCommand command,
        CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId!;

        // 1. Validate employee exists in graph
        var employee = await graphService.GetNodeAsync(command.EmployeeId, tenantId, ct)
            ?? throw new KeyNotFoundException("Employee not found");

        // 2. Validate leave balance
        await ValidateBalanceAsync(command, tenantId, ct);

        // 3. Create entity
        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = command.EmployeeId,
            Type = command.Type,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Status = LeaveStatus.Pending,
            Reason = command.Reason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await repository.CreateAsync(entity, ct);

        // 4. Publish domain event
        await eventBus.PublishAsync(
            new LeaveRequestedEvent(tenantId, created.Id, command.EmployeeId), ct);

        return MapToDto(created);
    }

    public async Task<LeaveRequestDto> ApproveRequestAsync(
        Guid requestId,
        Guid approvedBy,
        CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId!;
        var entity = await repository.GetByIdAsync(requestId, tenantId, ct)
            ?? throw new KeyNotFoundException("Leave request not found");

        entity.Status = LeaveStatus.Approved;
        entity.ApprovedBy = approvedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(entity, ct);

        await eventBus.PublishAsync(
            new LeaveApprovedEvent(tenantId, requestId, entity.EmployeeId), ct);

        return MapToDto(updated);
    }

    private async Task ValidateBalanceAsync(
        CreateLeaveRequestCommand command,
        Domain.ValueObjects.TenantId tenantId,
        CancellationToken ct)
    {
        var days = (command.EndDate - command.StartDate).Days + 1;
        // Check balance via repository or graph query
        // Throw if insufficient
    }

    private static LeaveRequestDto MapToDto(LeaveRequest entity) => new()
    {
        Id = entity.Id,
        EmployeeId = entity.EmployeeId,
        Type = entity.Type,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        Reason = entity.Reason,
        CreatedAt = entity.CreatedAt
    };
}
```

### Step 5: Create REST Controller

```csharp
// OrgSphere.API/Controllers/LeaveController.cs
using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController(ILeaveService leaveService) : ControllerBase
{
    [HttpPost("requests")]
    public async Task<ActionResult<LeaveRequestDto>> CreateRequest(
        [FromBody] CreateLeaveRequestCommand command)
    {
        var result = await leaveService.CreateRequestAsync(command);
        return CreatedAtAction(nameof(GetRequest), new { id = result.Id }, result);
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetRequests()
    {
        return Ok(await leaveService.GetRequestsAsync());
    }

    [HttpGet("requests/{id:guid}")]
    public async Task<ActionResult<LeaveRequestDto>> GetRequest(Guid id)
    {
        var result = await leaveService.GetRequestAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("requests/{id:guid}/approve")]
    public async Task<ActionResult<LeaveRequestDto>> ApproveRequest(Guid id)
    {
        return Ok(await leaveService.ApproveRequestAsync(id, GetCurrentUserId()));
    }

    private Guid GetCurrentUserId()
    {
        // Extract from JWT claims
        return Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
    }
}
```

### Step 6: Define Permissions

```csharp
// OrgSphere.Domain/Permissions/LeavePermissions.cs
using OrgSphere.Domain.Permissions;

namespace OrgSphere.Domain.Permissions;

public static class LeavePermissions
{
    public static readonly Permission CreateRequest = new(
        "leave:request:create", "Create leave request", PermissionScope.Own);

    public static readonly Permission ReadRequests = new(
        "leave:request:read", "Read leave requests", PermissionScope.Team);

    public static readonly Permission ApproveRequests = new(
        "leave:request:approve", "Approve leave requests", PermissionScope.Team);

    public static readonly Permission ManagePolicies = new(
        "leave:policy:manage", "Manage leave policies", PermissionScope.Company);
}
```

### Step 7: Create FluentValidation Validator

```csharp
// OrgSphere.Application/Validators/CreateLeaveRequestValidator.cs
using FluentValidation;

namespace OrgSphere.Application.Validators;

public class CreateLeaveRequestValidator : AbstractValidator<Commands.CreateLeaveRequestCommand>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("End date cannot be in the past");
    }
}
```

---

## Integration Checklist

When creating or modifying a module, ensure:

- [ ] Domain entities defined in `OrgSphere.Domain`
- [ ] Repository interface in `OrgSphere.Domain/Interfaces`
- [ ] DTOs defined in `OrgSphere.Application/DTOs`
- [ ] Service implements business logic with CQRS pattern
- [ ] FluentValidation validators created
- [ ] REST controller with proper HTTP verbs
- [ ] Permissions defined with `RequirePermission` attribute
- [ ] Domain events published for state changes
- [ ] Multi-tenant isolation enforced via `ITenantContext`
- [ ] Tests written
- [ ] Documentation updated

---

## Common Patterns

### Repository Pattern

```csharp
public interface IGraphRepository
{
    Task<T?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(TenantId tenantId, CancellationToken ct = default);
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken ct = default);
}
```

### Event Pattern

```csharp
// Always publish events after successful operations
await _eventBus.PublishAsync(
    new EntityCreatedEvent(_tenantContext.TenantId!, entity.Id), ct);
```

### MediatR Pipeline (CQRS)

```csharp
// Command handler registration in Program.cs
builder.Services.AddScoped<IRequestHandler<CreateLeaveRequestCommand, LeaveRequestDto>,
    CreateLeaveRequestHandler>();
```
