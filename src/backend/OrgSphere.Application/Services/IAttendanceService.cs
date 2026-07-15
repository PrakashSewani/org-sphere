using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.Services;

public interface IAttendanceService
{
    Task<AttendanceRecordDto> CheckInAsync(CheckInRequest request, CancellationToken ct = default);
    Task<AttendanceRecordDto> CheckOutAsync(CheckOutRequest request, CancellationToken ct = default);
    Task<AttendanceRecordDto?> GetAttendanceRecordByIdAsync(Guid id, CancellationToken ct = default);
    Task<AttendanceRecordDto?> GetAttendanceRecordByEmployeeAndDateAsync(Guid employeeId, DateTime date, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceRecordDto>> GetAllAttendanceRecordsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceRecordDto>> GetAttendanceRecordsByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceRecordDto>> GetAttendanceRecordsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(Guid employeeId, int year, int month, CancellationToken ct = default);

    Task<AttendancePolicyDto> CreateAttendancePolicyAsync(CreateAttendancePolicyRequest request, CancellationToken ct = default);
    Task<AttendancePolicyDto?> GetAttendancePolicyByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AttendancePolicyDto>> GetAllAttendancePoliciesAsync(CancellationToken ct = default);
    Task<AttendancePolicyDto?> GetActiveAttendancePolicyAsync(CancellationToken ct = default);
    Task<AttendancePolicyDto> UpdateAttendancePolicyAsync(Guid id, UpdateAttendancePolicyRequest request, CancellationToken ct = default);
    Task DeleteAttendancePolicyAsync(Guid id, CancellationToken ct = default);
}
