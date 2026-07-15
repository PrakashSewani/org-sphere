using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface ILeavePolicyRepository
{
    Task<LeavePolicy?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeavePolicy>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<LeavePolicy?> GetByLeaveTypeAsync(LeaveType leaveType, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<LeavePolicy> CreateAsync(LeavePolicy leavePolicy, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeavePolicy leavePolicy, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
