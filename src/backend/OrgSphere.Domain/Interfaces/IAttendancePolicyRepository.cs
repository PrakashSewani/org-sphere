using OrgSphere.Domain.Entities;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IAttendancePolicyRepository
{
    Task<AttendancePolicy?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendancePolicy>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<AttendancePolicy?> GetActiveAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<AttendancePolicy> CreateAsync(AttendancePolicy attendancePolicy, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendancePolicy attendancePolicy, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
