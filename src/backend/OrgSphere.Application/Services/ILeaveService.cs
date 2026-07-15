using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.Services;

public interface ILeaveService
{
    Task<LeaveRequestDto> CreateLeaveRequestAsync(CreateLeaveRequestRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequestDto>> GetAllLeaveRequestsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(LeaveStatus status, CancellationToken ct = default);
    Task<LeaveRequestDto> ApproveLeaveRequestAsync(Guid id, ApproveLeaveRequestRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto> RejectLeaveRequestAsync(Guid id, RejectLeaveRequestRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto> CancelLeaveRequestAsync(Guid id, CancellationToken ct = default);

    Task<LeavePolicyDto> CreateLeavePolicyAsync(CreateLeavePolicyRequest request, CancellationToken ct = default);
    Task<LeavePolicyDto?> GetLeavePolicyByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LeavePolicyDto>> GetAllLeavePoliciesAsync(CancellationToken ct = default);
    Task<LeavePolicyDto> UpdateLeavePolicyAsync(Guid id, UpdateLeavePolicyRequest request, CancellationToken ct = default);
    Task DeleteLeavePolicyAsync(Guid id, CancellationToken ct = default);

    Task<LeaveBalanceDto?> GetLeaveBalanceAsync(Guid employeeId, LeaveType leaveType, int year, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveBalanceDto>> GetLeaveBalancesByEmployeeAsync(Guid employeeId, int year, CancellationToken ct = default);
    Task<LeaveBalanceDto> InitializeLeaveBalanceAsync(InitializeLeaveBalanceRequest request, CancellationToken ct = default);
}
