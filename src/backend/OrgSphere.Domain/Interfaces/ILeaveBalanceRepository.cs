using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<LeaveBalance?> GetByEmployeeAndTypeAsync(Guid employeeId, LeaveType leaveType, int year, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveBalance>> GetByEmployeeAsync(Guid employeeId, int year, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<LeaveBalance> CreateAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default);
}
