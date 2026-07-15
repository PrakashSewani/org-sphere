using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public class AttendanceService(
    IAttendanceRecordRepository attendanceRecordRepository,
    IAttendancePolicyRepository attendancePolicyRepository,
    IEmployeeRepository employeeRepository,
    IEventBus eventBus,
    ITenantContext tenantContext) : IAttendanceService
{
    private readonly IAttendanceRecordRepository _attendanceRecordRepository = attendanceRecordRepository;
    private readonly IAttendancePolicyRepository _attendancePolicyRepository = attendancePolicyRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEventBus _eventBus = eventBus;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<AttendanceRecordDto> CheckInAsync(CheckInRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        _ = await _employeeRepository.GetByIdAsync(request.EmployeeId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {request.EmployeeId} not found");

        var today = DateTime.UtcNow.Date;
        var existing = await _attendanceRecordRepository.GetByEmployeeAndDateAsync(request.EmployeeId, today, tenantId, ct);

        if (existing is not null)
            throw new InvalidOperationException("Employee has already checked in today");

        var policy = await _attendancePolicyRepository.GetActiveAsync(tenantId, ct);
        var status = AttendanceStatus.Present;

        if (policy is not null)
        {
            var checkInTime = TimeOnly.FromDateTime(DateTime.UtcNow);
            var gracePeriod = policy.WorkStartTime.AddMinutes(policy.GracePeriodMinutes);

            if (checkInTime > gracePeriod)
                status = AttendanceStatus.Late;
        }

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = request.EmployeeId,
            Date = today,
            CheckInTime = DateTime.UtcNow,
            Status = status,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _attendanceRecordRepository.CreateAsync(record, ct);

        await _eventBus.PublishAsync(
            new AttendanceCheckedInEvent(tenantId, created.EmployeeId, created.CheckInTime!.Value), ct);

        return await MapToDto(created, tenantId, ct);
    }

    public async Task<AttendanceRecordDto> CheckOutAsync(CheckOutRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        _ = await _employeeRepository.GetByIdAsync(request.EmployeeId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {request.EmployeeId} not found");

        var today = DateTime.UtcNow.Date;
        var record = await _attendanceRecordRepository.GetByEmployeeAndDateAsync(request.EmployeeId, today, tenantId, ct)
            ?? throw new InvalidOperationException("Employee has not checked in today");

        if (record.CheckOutTime.HasValue)
            throw new InvalidOperationException("Employee has already checked out today");

        record.CheckOutTime = DateTime.UtcNow;
        record.Notes = request.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        await _attendanceRecordRepository.UpdateAsync(record, ct);

        await _eventBus.PublishAsync(
            new AttendanceCheckedOutEvent(tenantId, record.EmployeeId, record.CheckOutTime.Value), ct);

        return await MapToDto(record, tenantId, ct);
    }

    public async Task<AttendanceRecordDto?> GetAttendanceRecordByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _attendanceRecordRepository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : await MapToDto(entity, tenantId, ct);
    }

    public async Task<AttendanceRecordDto?> GetAttendanceRecordByEmployeeAndDateAsync(Guid employeeId, DateTime date, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _attendanceRecordRepository.GetByEmployeeAndDateAsync(employeeId, date, tenantId, ct);
        return entity is null ? null : await MapToDto(entity, tenantId, ct);
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> GetAllAttendanceRecordsAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _attendanceRecordRepository.GetAllAsync(tenantId, ct);
        var dtos = new List<AttendanceRecordDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> GetAttendanceRecordsByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _attendanceRecordRepository.GetByEmployeeAsync(employeeId, tenantId, ct);
        var dtos = new List<AttendanceRecordDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> GetAttendanceRecordsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _attendanceRecordRepository.GetByDateRangeAsync(startDate, endDate, tenantId, ct);
        var dtos = new List<AttendanceRecordDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(Guid employeeId, int year, int month, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var records = await _attendanceRecordRepository.GetByEmployeeAndDateRangeAsync(employeeId, startDate, endDate, tenantId, ct);
        var recordsList = records.ToList();

        var totalWorkingDays = CalculateWorkingDays(startDate, endDate);
        var daysPresent = recordsList.Count(r => r.Status == AttendanceStatus.Present);
        var daysLate = recordsList.Count(r => r.Status == AttendanceStatus.Late);
        var daysOnLeave = recordsList.Count(r => r.Status == AttendanceStatus.Leave);
        var daysAbsent = totalWorkingDays - daysPresent - daysLate - daysOnLeave;

        double totalHours = 0;
        foreach (var record in recordsList)
        {
            if (record.CheckInTime.HasValue && record.CheckOutTime.HasValue)
            {
                totalHours += (record.CheckOutTime.Value - record.CheckInTime.Value).TotalHours;
            }
        }

        var daysWorked = daysPresent + daysLate;
        var averageHours = daysWorked > 0 ? totalHours / daysWorked : 0;

        return new AttendanceSummaryDto
        {
            EmployeeId = employeeId,
            Year = year,
            Month = month,
            TotalWorkingDays = totalWorkingDays,
            DaysPresent = daysPresent,
            DaysAbsent = daysAbsent,
            DaysLate = daysLate,
            DaysOnLeave = daysOnLeave,
            TotalHoursWorked = Math.Round(totalHours, 2),
            AverageHoursPerDay = Math.Round(averageHours, 2)
        };
    }

    public async Task<AttendancePolicyDto> CreateAttendancePolicyAsync(CreateAttendancePolicyRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var policy = new AttendancePolicy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            WorkStartTime = request.WorkStartTime,
            WorkEndTime = request.WorkEndTime,
            GracePeriodMinutes = request.GracePeriodMinutes,
            RequireCheckIn = request.RequireCheckIn,
            RequireCheckOut = request.RequireCheckOut,
            AllowRemoteCheckIn = request.AllowRemoteCheckIn,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _attendancePolicyRepository.CreateAsync(policy, ct);
        return MapPolicyToDto(created);
    }

    public async Task<AttendancePolicyDto?> GetAttendancePolicyByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _attendancePolicyRepository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : MapPolicyToDto(entity);
    }

    public async Task<IReadOnlyList<AttendancePolicyDto>> GetAllAttendancePoliciesAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _attendancePolicyRepository.GetAllAsync(tenantId, ct);
        return [.. entities.Select(MapPolicyToDto)];
    }

    public async Task<AttendancePolicyDto?> GetActiveAttendancePolicyAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _attendancePolicyRepository.GetActiveAsync(tenantId, ct);
        return entity is null ? null : MapPolicyToDto(entity);
    }

    public async Task<AttendancePolicyDto> UpdateAttendancePolicyAsync(Guid id, UpdateAttendancePolicyRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var policy = await _attendancePolicyRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Attendance policy {id} not found");

        policy.Name = request.Name;
        policy.WorkStartTime = request.WorkStartTime;
        policy.WorkEndTime = request.WorkEndTime;
        policy.GracePeriodMinutes = request.GracePeriodMinutes;
        policy.RequireCheckIn = request.RequireCheckIn;
        policy.RequireCheckOut = request.RequireCheckOut;
        policy.AllowRemoteCheckIn = request.AllowRemoteCheckIn;
        policy.IsActive = request.IsActive;
        policy.UpdatedAt = DateTime.UtcNow;

        await _attendancePolicyRepository.UpdateAsync(policy, ct);
        return MapPolicyToDto(policy);
    }

    public async Task DeleteAttendancePolicyAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        _ = await _attendancePolicyRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Attendance policy {id} not found");

        await _attendancePolicyRepository.DeleteAsync(id, tenantId, ct);
    }

    private static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
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

    private async Task<AttendanceRecordDto> MapToDto(AttendanceRecord entity, TenantId tenantId, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(entity.EmployeeId, tenantId, ct);
        var employeeName = employee is not null ? $"{employee.FirstName} {employee.LastName}" : "Unknown";

        double? totalHours = null;
        if (entity.CheckInTime.HasValue && entity.CheckOutTime.HasValue)
        {
            totalHours = Math.Round((entity.CheckOutTime.Value - entity.CheckInTime.Value).TotalHours, 2);
        }

        return new AttendanceRecordDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = employeeName,
            Date = entity.Date,
            CheckInTime = entity.CheckInTime,
            CheckOutTime = entity.CheckOutTime,
            TotalHours = totalHours,
            Status = entity.Status,
            StatusName = entity.Status.ToString(),
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt
        };
    }

    private static AttendancePolicyDto MapPolicyToDto(AttendancePolicy entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        WorkStartTime = entity.WorkStartTime,
        WorkEndTime = entity.WorkEndTime,
        GracePeriodMinutes = entity.GracePeriodMinutes,
        RequireCheckIn = entity.RequireCheckIn,
        RequireCheckOut = entity.RequireCheckOut,
        AllowRemoteCheckIn = entity.AllowRemoteCheckIn,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt
    };
}
