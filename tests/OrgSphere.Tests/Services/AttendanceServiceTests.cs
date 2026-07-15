using FluentAssertions;
using Moq;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Tests.Fixtures;

namespace OrgSphere.Tests.Services;

public class AttendanceServiceTests : IDisposable
{
    private readonly Mock<IAttendanceRecordRepository> _attendanceRecordRepository = new();
    private readonly Mock<IAttendancePolicyRepository> _attendancePolicyRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IEventBus> _eventBus;
    private readonly MockTenantContextFixture _tenantContext = new();
    private readonly AttendanceService _sut;

    public AttendanceServiceTests()
    {
        _eventBus = MockEventBusFixture.Create();
        _tenantContext.SetTenantContext(TenantId.New(), UserId.New(), UserRole.Admin);
        _sut = new AttendanceService(
            _attendanceRecordRepository.Object,
            _attendancePolicyRepository.Object,
            _employeeRepository.Object,
            _eventBus.Object,
            _tenantContext);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region Check-In Tests

    [Fact]
    public async Task CheckInAsync_ShouldCreateRecordAndPublishEvent()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _attendanceRecordRepository.Setup(r => r.GetByEmployeeAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);

        _attendancePolicyRepository.Setup(r => r.GetActiveAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendancePolicy?)null);

        _attendanceRecordRepository.Setup(r => r.CreateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord ar, CancellationToken _) => ar);

        var request = new CheckInRequest(EmployeeId: employee.Id, Notes: "On time");

        var result = await _sut.CheckInAsync(request);

        result.Should().NotBeNull();
        result.CheckInTime.Should().NotBeNull();
        result.Status.Should().Be(AttendanceStatus.Present);
        _attendanceRecordRepository.Verify(r => r.CreateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<AttendanceCheckedInEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_ShouldThrow_WhenAlreadyCheckedIn()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var existingRecord = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            EmployeeId = employee.Id,
            Date = DateTime.UtcNow.Date,
            CheckInTime = DateTime.UtcNow.AddHours(-1),
            Status = AttendanceStatus.Present,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _attendanceRecordRepository.Setup(r => r.GetByEmployeeAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecord);

        var request = new CheckInRequest(EmployeeId: employee.Id, Notes: null);

        var act = () => _sut.CheckInAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already checked in today*");
    }

    [Fact]
    public async Task CheckInAsync_ShouldSetLateStatus_WhenAfterGracePeriod()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _attendanceRecordRepository.Setup(r => r.GetByEmployeeAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);

        var policy = new AttendancePolicy
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Standard Policy",
            WorkStartTime = new TimeOnly(9, 0),
            WorkEndTime = new TimeOnly(17, 0),
            GracePeriodMinutes = 15,
            RequireCheckIn = true,
            RequireCheckOut = true,
            AllowRemoteCheckIn = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _attendancePolicyRepository.Setup(r => r.GetActiveAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        _attendanceRecordRepository.Setup(r => r.CreateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord ar, CancellationToken _) => ar);

        var request = new CheckInRequest(EmployeeId: employee.Id, Notes: "Late");

        var result = await _sut.CheckInAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be(AttendanceStatus.Late);
    }

    #endregion

    #region Check-Out Tests

    [Fact]
    public async Task CheckOutAsync_ShouldUpdateRecordAndPublishEvent()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            EmployeeId = employee.Id,
            Date = DateTime.UtcNow.Date,
            CheckInTime = DateTime.UtcNow.AddHours(-8),
            Status = AttendanceStatus.Present,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _attendanceRecordRepository.Setup(r => r.GetByEmployeeAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        _attendanceRecordRepository.Setup(r => r.UpdateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new CheckOutRequest(EmployeeId: employee.Id, Notes: "Going home");

        var result = await _sut.CheckOutAsync(request);

        result.Should().NotBeNull();
        result.CheckOutTime.Should().NotBeNull();
        result.TotalHours.Should().BeGreaterThan(0);
        _attendanceRecordRepository.Verify(r => r.UpdateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<AttendanceCheckedOutEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckOutAsync_ShouldThrow_WhenNotCheckedIn()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _attendanceRecordRepository.Setup(r => r.GetByEmployeeAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);

        var request = new CheckOutRequest(EmployeeId: employee.Id, Notes: null);

        var act = () => _sut.CheckOutAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*has not checked in today*");
    }

    [Fact]
    public async Task CheckOutAsync_ShouldThrow_WhenAlreadyCheckedOut()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            EmployeeId = employee.Id,
            Date = DateTime.UtcNow.Date,
            CheckInTime = DateTime.UtcNow.AddHours(-8),
            CheckOutTime = DateTime.UtcNow.AddHours(-1),
            Status = AttendanceStatus.Present,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _attendanceRecordRepository.Setup(r => r.GetByEmployeeAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var request = new CheckOutRequest(EmployeeId: employee.Id, Notes: null);

        var act = () => _sut.CheckOutAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already checked out today*");
    }

    #endregion

    #region Attendance Summary Tests

    [Fact]
    public async Task GetAttendanceSummaryAsync_ShouldCalculateSummary()
    {
        var employeeId = Guid.NewGuid();
        var records = new List<AttendanceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                EmployeeId = employeeId,
                Date = new DateTime(2025, 3, 3),
                CheckInTime = new DateTime(2025, 3, 3, 9, 0, 0),
                CheckOutTime = new DateTime(2025, 3, 3, 17, 0, 0),
                Status = AttendanceStatus.Present,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                EmployeeId = employeeId,
                Date = new DateTime(2025, 3, 4),
                CheckInTime = new DateTime(2025, 3, 4, 9, 30, 0),
                CheckOutTime = new DateTime(2025, 3, 4, 17, 0, 0),
                Status = AttendanceStatus.Late,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _attendanceRecordRepository.Setup(r => r.GetByEmployeeAndDateRangeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var result = await _sut.GetAttendanceSummaryAsync(employeeId, 2025, 3);

        result.Should().NotBeNull();
        result.DaysPresent.Should().Be(1);
        result.DaysLate.Should().Be(1);
        result.TotalHoursWorked.Should().Be(15.5);
        result.AverageHoursPerDay.Should().Be(7.75);
    }

    #endregion

    #region Attendance Policy Tests

    [Fact]
    public async Task CreateAttendancePolicyAsync_ShouldCreatePolicy()
    {
        _attendancePolicyRepository.Setup(r => r.CreateAsync(It.IsAny<AttendancePolicy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendancePolicy p, CancellationToken _) => p);

        var request = new CreateAttendancePolicyRequest(
            Name: "Standard Policy",
            WorkStartTime: new TimeOnly(9, 0),
            WorkEndTime: new TimeOnly(17, 0),
            GracePeriodMinutes: 15,
            RequireCheckIn: true,
            RequireCheckOut: true,
            AllowRemoteCheckIn: false);

        var result = await _sut.CreateAttendancePolicyAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Standard Policy");
        result.WorkStartTime.Should().Be(new TimeOnly(9, 0));
        result.WorkEndTime.Should().Be(new TimeOnly(17, 0));
    }

    [Fact]
    public async Task UpdateAttendancePolicyAsync_ShouldUpdatePolicy()
    {
        var policy = new AttendancePolicy
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Old Policy",
            WorkStartTime = new TimeOnly(8, 0),
            WorkEndTime = new TimeOnly(16, 0),
            GracePeriodMinutes = 10,
            RequireCheckIn = true,
            RequireCheckOut = true,
            AllowRemoteCheckIn = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _attendancePolicyRepository.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        _attendancePolicyRepository.Setup(r => r.UpdateAsync(It.IsAny<AttendancePolicy>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateAttendancePolicyRequest(
            Name: "Updated Policy",
            WorkStartTime: new TimeOnly(9, 0),
            WorkEndTime: new TimeOnly(17, 0),
            GracePeriodMinutes: 15,
            RequireCheckIn: true,
            RequireCheckOut: true,
            AllowRemoteCheckIn: true,
            IsActive: true);

        var result = await _sut.UpdateAttendancePolicyAsync(policy.Id, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Policy");
        result.AllowRemoteCheckIn.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAttendancePolicyAsync_ShouldDeletePolicy()
    {
        var policy = new AttendancePolicy
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Policy to Delete",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _attendancePolicyRepository.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);
        _attendancePolicyRepository.Setup(r => r.DeleteAsync(policy.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAttendancePolicyAsync(policy.Id);

        _attendancePolicyRepository.Verify(r => r.DeleteAsync(policy.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAttendancePolicyAsync_ShouldThrow_WhenNotFound()
    {
        _attendancePolicyRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendancePolicy?)null);

        var act = () => _sut.DeleteAttendancePolicyAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    private Employee CreateTestEmployee()
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            EmployeeId = "EMP-001",
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@acme.com",
            Title = "Engineer",
            DepartmentId = Guid.NewGuid(),
            EmploymentType = EmploymentType.FullTime,
            StartDate = new DateTime(2025, 1, 15),
            Status = EmployeeStatus.Active,
            Skills = [],
            Preferences = new EmployeePreferences(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
