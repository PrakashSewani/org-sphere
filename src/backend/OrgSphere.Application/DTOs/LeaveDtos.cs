using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.DTOs;

public record LeaveRequestDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public LeaveType LeaveType { get; init; }
    public string LeaveTypeName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalDays { get; init; }
    public string? Reason { get; init; }
    public LeaveStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public Guid? ApprovedById { get; init; }
    public string? ApprovedByName { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateLeaveRequestRequest(
    Guid EmployeeId,
    LeaveType LeaveType,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason);

public record ApproveLeaveRequestRequest(
    Guid ApprovedById,
    string? Notes);

public record RejectLeaveRequestRequest(
    Guid RejectedById,
    string Reason);

public record LeavePolicyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public LeaveType LeaveType { get; init; }
    public string LeaveTypeName { get; init; } = string.Empty;
    public int DefaultDaysPerYear { get; init; }
    public bool CarryForward { get; init; }
    public int MaxCarryForwardDays { get; init; }
    public bool RequireApproval { get; init; }
    public int MinServiceDays { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateLeavePolicyRequest(
    string Name,
    LeaveType LeaveType,
    int DefaultDaysPerYear,
    bool CarryForward,
    int MaxCarryForwardDays,
    bool RequireApproval,
    int MinServiceDays);

public record UpdateLeavePolicyRequest(
    string Name,
    int DefaultDaysPerYear,
    bool CarryForward,
    int MaxCarryForwardDays,
    bool RequireApproval,
    int MinServiceDays,
    bool IsActive);

public record LeaveBalanceDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public LeaveType LeaveType { get; init; }
    public string LeaveTypeName { get; init; } = string.Empty;
    public int Year { get; init; }
    public int TotalDays { get; init; }
    public int UsedDays { get; init; }
    public int PendingDays { get; init; }
    public int CarriedForwardDays { get; init; }
    public int RemainingDays { get; init; }
}

public record InitializeLeaveBalanceRequest(
    Guid EmployeeId,
    LeaveType LeaveType,
    int Year,
    int TotalDays,
    int CarriedForwardDays);
