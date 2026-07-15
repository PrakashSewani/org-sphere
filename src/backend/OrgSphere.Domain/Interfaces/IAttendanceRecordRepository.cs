using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IAttendanceRecordRepository
{
    Task<AttendanceRecord?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetByEmployeeAndDateAsync(Guid employeeId, DateTime date, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetByEmployeeAsync(Guid employeeId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startDate, DateTime endDate, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<AttendanceRecord> CreateAsync(AttendanceRecord attendanceRecord, CancellationToken cancellationToken = default);
    Task UpdateAsync(AttendanceRecord attendanceRecord, CancellationToken cancellationToken = default);
}
