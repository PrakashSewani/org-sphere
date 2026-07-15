using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
