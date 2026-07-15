using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public class LeaveService(
    ILeaveRequestRepository leaveRequestRepository,
    ILeavePolicyRepository leavePolicyRepository,
    ILeaveBalanceRepository leaveBalanceRepository,
    IEmployeeRepository employeeRepository,
    IEventBus eventBus,
    ITenantContext tenantContext) : ILeaveService
{
    private readonly ILeaveRequestRepository _leaveRequestRepository = leaveRequestRepository;
    private readonly ILeavePolicyRepository _leavePolicyRepository = leavePolicyRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository = leaveBalanceRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEventBus _eventBus = eventBus;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<LeaveRequestDto> CreateLeaveRequestAsync(CreateLeaveRequestRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {request.EmployeeId} not found");

        if (request.EndDate < request.StartDate)
            throw new InvalidOperationException("End date must be after start date");

        var totalDays = CalculateBusinessDays(request.StartDate, request.EndDate);
        if (totalDays <= 0)
            throw new InvalidOperationException("Leave request must include at least one business day");

        var policy = await _leavePolicyRepository.GetByLeaveTypeAsync(request.LeaveType, tenantId, ct);
        if (policy is not null && policy.RequireApproval)
        {
            var balance = await _leaveBalanceRepository.GetByEmployeeAndTypeAsync(
                request.EmployeeId, request.LeaveType, request.StartDate.Year, tenantId, ct);

            if (balance is not null && balance.RemainingDays < totalDays)
                throw new InvalidOperationException($"Insufficient leave balance. Remaining: {balance.RemainingDays} days, Requested: {totalDays} days");
        }

        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _leaveRequestRepository.CreateAsync(leaveRequest, ct);

        if (policy is null || !policy.RequireApproval)
        {
            created.Status = LeaveStatus.Approved;
            created.UpdatedAt = DateTime.UtcNow;
            await _leaveRequestRepository.UpdateAsync(created, ct);

            await UpdateBalanceOnApproval(created, tenantId, ct);
        }
        else
        {
            await UpdateBalanceOnPending(created, tenantId, ct);
        }

        await _eventBus.PublishAsync(
            new LeaveRequestCreatedEvent(tenantId, created.Id, created.EmployeeId), ct);

        return await MapToDto(created, tenantId, ct);
    }

    public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _leaveRequestRepository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : await MapToDto(entity, tenantId, ct);
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetAllLeaveRequestsAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _leaveRequestRepository.GetAllAsync(tenantId, ct);
        var dtos = new List<LeaveRequestDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _leaveRequestRepository.GetByEmployeeAsync(employeeId, tenantId, ct);
        var dtos = new List<LeaveRequestDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(LeaveStatus status, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _leaveRequestRepository.GetByStatusAsync(status, tenantId, ct);
        var dtos = new List<LeaveRequestDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<LeaveRequestDto> ApproveLeaveRequestAsync(Guid id, ApproveLeaveRequestRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Leave request {id} not found");

        if (leaveRequest.Status != LeaveStatus.Pending)
            throw new InvalidOperationException($"Cannot approve leave request with status {leaveRequest.Status}");

        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedById = request.ApprovedById;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _leaveRequestRepository.UpdateAsync(leaveRequest, ct);

        await RevertBalanceOnPending(leaveRequest, tenantId, ct);
        await UpdateBalanceOnApproval(leaveRequest, tenantId, ct);

        await _eventBus.PublishAsync(
            new LeaveRequestApprovedEvent(tenantId, leaveRequest.Id, leaveRequest.EmployeeId), ct);

        return await MapToDto(leaveRequest, tenantId, ct);
    }

    public async Task<LeaveRequestDto> RejectLeaveRequestAsync(Guid id, RejectLeaveRequestRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Leave request {id} not found");

        if (leaveRequest.Status != LeaveStatus.Pending)
            throw new InvalidOperationException($"Cannot reject leave request with status {leaveRequest.Status}");

        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.ApprovedById = request.RejectedById;
        leaveRequest.RejectionReason = request.Reason;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _leaveRequestRepository.UpdateAsync(leaveRequest, ct);

        await RevertBalanceOnPending(leaveRequest, tenantId, ct);

        await _eventBus.PublishAsync(
            new LeaveRequestRejectedEvent(tenantId, leaveRequest.Id, leaveRequest.EmployeeId), ct);

        return await MapToDto(leaveRequest, tenantId, ct);
    }

    public async Task<LeaveRequestDto> CancelLeaveRequestAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Leave request {id} not found");

        if (leaveRequest.Status == LeaveStatus.Cancelled)
            throw new InvalidOperationException("Leave request is already cancelled");

        var previousStatus = leaveRequest.Status;
        leaveRequest.Status = LeaveStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _leaveRequestRepository.UpdateAsync(leaveRequest, ct);

        if (previousStatus == LeaveStatus.Pending)
        {
            await RevertBalanceOnPending(leaveRequest, tenantId, ct);
        }
        else if (previousStatus == LeaveStatus.Approved)
        {
            await RevertBalanceOnApproval(leaveRequest, tenantId, ct);
        }

        return await MapToDto(leaveRequest, tenantId, ct);
    }

    public async Task<LeavePolicyDto> CreateLeavePolicyAsync(CreateLeavePolicyRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var existing = await _leavePolicyRepository.GetByLeaveTypeAsync(request.LeaveType, tenantId, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Leave policy for {request.LeaveType} already exists");

        var policy = new LeavePolicy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            LeaveType = request.LeaveType,
            DefaultDaysPerYear = request.DefaultDaysPerYear,
            CarryForward = request.CarryForward,
            MaxCarryForwardDays = request.MaxCarryForwardDays,
            RequireApproval = request.RequireApproval,
            MinServiceDays = request.MinServiceDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _leavePolicyRepository.CreateAsync(policy, ct);
        return MapPolicyToDto(created);
    }

    public async Task<LeavePolicyDto?> GetLeavePolicyByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _leavePolicyRepository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : MapPolicyToDto(entity);
    }

    public async Task<IReadOnlyList<LeavePolicyDto>> GetAllLeavePoliciesAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _leavePolicyRepository.GetAllAsync(tenantId, ct);
        return [.. entities.Select(MapPolicyToDto)];
    }

    public async Task<LeavePolicyDto> UpdateLeavePolicyAsync(Guid id, UpdateLeavePolicyRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var policy = await _leavePolicyRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Leave policy {id} not found");

        policy.Name = request.Name;
        policy.DefaultDaysPerYear = request.DefaultDaysPerYear;
        policy.CarryForward = request.CarryForward;
        policy.MaxCarryForwardDays = request.MaxCarryForwardDays;
        policy.RequireApproval = request.RequireApproval;
        policy.MinServiceDays = request.MinServiceDays;
        policy.IsActive = request.IsActive;
        policy.UpdatedAt = DateTime.UtcNow;

        await _leavePolicyRepository.UpdateAsync(policy, ct);
        return MapPolicyToDto(policy);
    }

    public async Task DeleteLeavePolicyAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        _ = await _leavePolicyRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Leave policy {id} not found");

        await _leavePolicyRepository.DeleteAsync(id, tenantId, ct);
    }

    public async Task<LeaveBalanceDto?> GetLeaveBalanceAsync(Guid employeeId, LeaveType leaveType, int year, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _leaveBalanceRepository.GetByEmployeeAndTypeAsync(employeeId, leaveType, year, tenantId, ct);
        return entity is null ? null : MapBalanceToDto(entity);
    }

    public async Task<IReadOnlyList<LeaveBalanceDto>> GetLeaveBalancesByEmployeeAsync(Guid employeeId, int year, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _leaveBalanceRepository.GetByEmployeeAsync(employeeId, year, tenantId, ct);
        return [.. entities.Select(MapBalanceToDto)];
    }

    public async Task<LeaveBalanceDto> InitializeLeaveBalanceAsync(InitializeLeaveBalanceRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var existing = await _leaveBalanceRepository.GetByEmployeeAndTypeAsync(
            request.EmployeeId, request.LeaveType, request.Year, tenantId, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Leave balance already exists for employee {request.EmployeeId}, type {request.LeaveType}, year {request.Year}");

        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            Year = request.Year,
            TotalDays = request.TotalDays,
            UsedDays = 0,
            PendingDays = 0,
            CarriedForwardDays = request.CarriedForwardDays,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _leaveBalanceRepository.CreateAsync(balance, ct);
        return MapBalanceToDto(created);
    }

    private async Task UpdateBalanceOnPending(LeaveRequest leaveRequest, TenantId tenantId, CancellationToken ct)
    {
        var balance = await _leaveBalanceRepository.GetByEmployeeAndTypeAsync(
            leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.StartDate.Year, tenantId, ct);

        if (balance is not null)
        {
            balance.PendingDays += leaveRequest.TotalDays;
            balance.UpdatedAt = DateTime.UtcNow;
            await _leaveBalanceRepository.UpdateAsync(balance, ct);
        }
    }

    private async Task RevertBalanceOnPending(LeaveRequest leaveRequest, TenantId tenantId, CancellationToken ct)
    {
        var balance = await _leaveBalanceRepository.GetByEmployeeAndTypeAsync(
            leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.StartDate.Year, tenantId, ct);

        if (balance is not null && balance.PendingDays >= leaveRequest.TotalDays)
        {
            balance.PendingDays -= leaveRequest.TotalDays;
            balance.UpdatedAt = DateTime.UtcNow;
            await _leaveBalanceRepository.UpdateAsync(balance, ct);
        }
    }

    private async Task UpdateBalanceOnApproval(LeaveRequest leaveRequest, TenantId tenantId, CancellationToken ct)
    {
        var balance = await _leaveBalanceRepository.GetByEmployeeAndTypeAsync(
            leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.StartDate.Year, tenantId, ct);

        if (balance is not null)
        {
            balance.UsedDays += leaveRequest.TotalDays;
            balance.UpdatedAt = DateTime.UtcNow;
            await _leaveBalanceRepository.UpdateAsync(balance, ct);
        }
    }

    private async Task RevertBalanceOnApproval(LeaveRequest leaveRequest, TenantId tenantId, CancellationToken ct)
    {
        var balance = await _leaveBalanceRepository.GetByEmployeeAndTypeAsync(
            leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.StartDate.Year, tenantId, ct);

        if (balance is not null && balance.UsedDays >= leaveRequest.TotalDays)
        {
            balance.UsedDays -= leaveRequest.TotalDays;
            balance.UpdatedAt = DateTime.UtcNow;
            await _leaveBalanceRepository.UpdateAsync(balance, ct);
        }
    }

    private static int CalculateBusinessDays(DateTime startDate, DateTime endDate)
    {
        int totalDays = 0;
        var current = startDate.Date;
        while (current <= endDate.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                totalDays++;
            current = current.AddDays(1);
        }
        return totalDays;
    }

    private async Task<LeaveRequestDto> MapToDto(LeaveRequest entity, TenantId tenantId, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(entity.EmployeeId, tenantId, ct);
        var employeeName = employee is not null ? $"{employee.FirstName} {employee.LastName}" : "Unknown";

        string? approvedByName = null;
        if (entity.ApprovedById.HasValue)
        {
            var approver = await _employeeRepository.GetByIdAsync(entity.ApprovedById.Value, tenantId, ct);
            approvedByName = approver is not null ? $"{approver.FirstName} {approver.LastName}" : "Unknown";
        }

        return new LeaveRequestDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName,
            LeaveType = entity.LeaveType,
            LeaveTypeName = entity.LeaveType.ToString(),
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            TotalDays = entity.TotalDays,
            Reason = entity.Reason,
            Status = entity.Status,
            StatusName = entity.Status.ToString(),
            ApprovedById = entity.ApprovedById,
            ApprovedByName = approvedByName,
            ApprovedAt = entity.ApprovedAt,
            RejectionReason = entity.RejectionReason,
            CreatedAt = entity.CreatedAt
        };
    }

    private static LeavePolicyDto MapPolicyToDto(LeavePolicy entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        LeaveType = entity.LeaveType,
        LeaveTypeName = entity.LeaveType.ToString(),
        DefaultDaysPerYear = entity.DefaultDaysPerYear,
        CarryForward = entity.CarryForward,
        MaxCarryForwardDays = entity.MaxCarryForwardDays,
        RequireApproval = entity.RequireApproval,
        MinServiceDays = entity.MinServiceDays,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };

    private static LeaveBalanceDto MapBalanceToDto(LeaveBalance entity) => new()
    {
        Id = entity.Id,
        EmployeeId = entity.EmployeeId,
        LeaveType = entity.LeaveType,
        LeaveTypeName = entity.LeaveType.ToString(),
        Year = entity.Year,
        TotalDays = entity.TotalDays,
        UsedDays = entity.UsedDays,
        PendingDays = entity.PendingDays,
        CarriedForwardDays = entity.CarriedForwardDays,
        RemainingDays = entity.RemainingDays
    };
}
