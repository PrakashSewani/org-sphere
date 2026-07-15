using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.DTOs;

public record AttendanceRecordDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public double? TotalHours { get; init; }
    public AttendanceStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CheckInRequest(
    Guid EmployeeId,
    string? Notes);

public record CheckOutRequest(
    Guid EmployeeId,
    string? Notes);

public record AttendancePolicyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TimeOnly WorkStartTime { get; init; }
    public TimeOnly WorkEndTime { get; init; }
    public int GracePeriodMinutes { get; init; }
    public bool RequireCheckIn { get; init; }
    public bool RequireCheckOut { get; init; }
    public bool AllowRemoteCheckIn { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateAttendancePolicyRequest(
    string Name,
    TimeOnly WorkStartTime,
    TimeOnly WorkEndTime,
    int GracePeriodMinutes,
    bool RequireCheckIn,
    bool RequireCheckOut,
    bool AllowRemoteCheckIn);

public record UpdateAttendancePolicyRequest(
    string Name,
    TimeOnly WorkStartTime,
    TimeOnly WorkEndTime,
    int GracePeriodMinutes,
    bool RequireCheckIn,
    bool RequireCheckOut,
    bool AllowRemoteCheckIn,
    bool IsActive);

public record AttendanceSummaryDto
{
    public Guid EmployeeId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public int TotalWorkingDays { get; init; }
    public int DaysPresent { get; init; }
    public int DaysAbsent { get; init; }
    public int DaysLate { get; init; }
    public int DaysOnLeave { get; init; }
    public double TotalHoursWorked { get; init; }
    public double AverageHoursPerDay { get; init; }
}
