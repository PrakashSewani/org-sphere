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

public class LeaveServiceTests : IDisposable
{
    private readonly Mock<ILeaveRequestRepository> _leaveRequestRepository = new();
    private readonly Mock<ILeavePolicyRepository> _leavePolicyRepository = new();
    private readonly Mock<ILeaveBalanceRepository> _leaveBalanceRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IEventBus> _eventBus;
    private readonly MockTenantContextFixture _tenantContext = new();
    private readonly LeaveService _sut;

    public LeaveServiceTests()
    {
        _eventBus = MockEventBusFixture.Create();
        _tenantContext.SetTenantContext(TenantId.New(), UserId.New(), UserRole.Admin);
        _sut = new LeaveService(
            _leaveRequestRepository.Object,
            _leavePolicyRepository.Object,
            _leaveBalanceRepository.Object,
            _employeeRepository.Object,
            _eventBus.Object,
            _tenantContext);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region Leave Request Tests

    [Fact]
    public async Task CreateLeaveRequestAsync_ShouldCreateRequestAndPublishEvent()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _leavePolicyRepository.Setup(r => r.GetByLeaveTypeAsync(It.IsAny<LeaveType>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeavePolicy?)null);

        _leaveRequestRepository.Setup(r => r.CreateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest lr, CancellationToken _) => lr);

        _leaveRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new CreateLeaveRequestRequest(
            EmployeeId: employee.Id,
            LeaveType: LeaveType.Annual,
            StartDate: new DateTime(2025, 3, 10),
            EndDate: new DateTime(2025, 3, 14),
            Reason: "Vacation");

        var result = await _sut.CreateLeaveRequestAsync(request);

        result.Should().NotBeNull();
        result.LeaveType.Should().Be(LeaveType.Annual);
        result.TotalDays.Should().Be(5);
        result.Status.Should().Be(LeaveStatus.Approved);
        _leaveRequestRepository.Verify(r => r.CreateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<LeaveRequestCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_ShouldThrowWhenEndDateBeforeStartDate()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var request = new CreateLeaveRequestRequest(
            EmployeeId: employee.Id,
            LeaveType: LeaveType.Annual,
            StartDate: new DateTime(2025, 3, 14),
            EndDate: new DateTime(2025, 3, 10),
            Reason: "Vacation");

        var act = () => _sut.CreateLeaveRequestAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*End date must be after start date*");
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_ShouldSetPendingStatus_WhenApprovalRequired()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var policy = new LeavePolicy
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Annual Leave",
            LeaveType = LeaveType.Annual,
            DefaultDaysPerYear = 20,
            RequireApproval = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _leavePolicyRepository.Setup(r => r.GetByLeaveTypeAsync(LeaveType.Annual, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        _leaveBalanceRepository.Setup(r => r.GetByEmployeeAndTypeAsync(It.IsAny<Guid>(), It.IsAny<LeaveType>(), It.IsAny<int>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeaveBalance
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                EmployeeId = employee.Id,
                LeaveType = LeaveType.Annual,
                Year = 2025,
                TotalDays = 20,
                UsedDays = 0,
                PendingDays = 0,
                CarriedForwardDays = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        _leaveRequestRepository.Setup(r => r.CreateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest lr, CancellationToken _) => lr);

        _leaveBalanceRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveBalance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new CreateLeaveRequestRequest(
            EmployeeId: employee.Id,
            LeaveType: LeaveType.Annual,
            StartDate: new DateTime(2025, 3, 10),
            EndDate: new DateTime(2025, 3, 14),
            Reason: "Vacation");

        var result = await _sut.CreateLeaveRequestAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be(LeaveStatus.Pending);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_ShouldApproveAndPublishEvent()
    {
        var leaveRequest = CreateTestLeaveRequest(LeaveStatus.Pending);
        _leaveRequestRepository.Setup(r => r.GetByIdAsync(leaveRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        _leaveRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _leaveBalanceRepository.Setup(r => r.GetByEmployeeAndTypeAsync(It.IsAny<Guid>(), It.IsAny<LeaveType>(), It.IsAny<int>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeaveBalance
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                EmployeeId = leaveRequest.EmployeeId,
                LeaveType = leaveRequest.LeaveType,
                Year = 2025,
                TotalDays = 20,
                UsedDays = 0,
                PendingDays = 5,
                CarriedForwardDays = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        _leaveBalanceRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveBalance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestEmployee());

        var request = new ApproveLeaveRequestRequest(ApprovedById: Guid.NewGuid(), Notes: "Approved");

        var result = await _sut.ApproveLeaveRequestAsync(leaveRequest.Id, request);

        result.Should().NotBeNull();
        result.Status.Should().Be(LeaveStatus.Approved);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<LeaveRequestApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_ShouldThrow_WhenNotPending()
    {
        var leaveRequest = CreateTestLeaveRequest(LeaveStatus.Approved);
        _leaveRequestRepository.Setup(r => r.GetByIdAsync(leaveRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        var request = new ApproveLeaveRequestRequest(ApprovedById: Guid.NewGuid(), Notes: null);

        var act = () => _sut.ApproveLeaveRequestAsync(leaveRequest.Id, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot approve leave request with status*");
    }

    [Fact]
    public async Task RejectLeaveRequestAsync_ShouldRejectAndPublishEvent()
    {
        var leaveRequest = CreateTestLeaveRequest(LeaveStatus.Pending);
        _leaveRequestRepository.Setup(r => r.GetByIdAsync(leaveRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        _leaveRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _leaveBalanceRepository.Setup(r => r.GetByEmployeeAndTypeAsync(It.IsAny<Guid>(), It.IsAny<LeaveType>(), It.IsAny<int>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeaveBalance
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                EmployeeId = leaveRequest.EmployeeId,
                LeaveType = leaveRequest.LeaveType,
                Year = 2025,
                TotalDays = 20,
                UsedDays = 0,
                PendingDays = 5,
                CarriedForwardDays = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        _leaveBalanceRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveBalance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestEmployee());

        var request = new RejectLeaveRequestRequest(RejectedById: Guid.NewGuid(), Reason: "Not enough coverage");

        var result = await _sut.RejectLeaveRequestAsync(leaveRequest.Id, request);

        result.Should().NotBeNull();
        result.Status.Should().Be(LeaveStatus.Rejected);
        result.RejectionReason.Should().Be("Not enough coverage");
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<LeaveRequestRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelLeaveRequestAsync_ShouldCancelPendingRequest()
    {
        var leaveRequest = CreateTestLeaveRequest(LeaveStatus.Pending);
        _leaveRequestRepository.Setup(r => r.GetByIdAsync(leaveRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        _leaveRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _leaveBalanceRepository.Setup(r => r.GetByEmployeeAndTypeAsync(It.IsAny<Guid>(), It.IsAny<LeaveType>(), It.IsAny<int>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeaveBalance
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                EmployeeId = leaveRequest.EmployeeId,
                LeaveType = leaveRequest.LeaveType,
                Year = 2025,
                TotalDays = 20,
                UsedDays = 0,
                PendingDays = 5,
                CarriedForwardDays = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        _leaveBalanceRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveBalance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CancelLeaveRequestAsync(leaveRequest.Id);

        result.Should().NotBeNull();
        result.Status.Should().Be(LeaveStatus.Cancelled);
    }

    #endregion

    #region Leave Policy Tests

    [Fact]
    public async Task CreateLeavePolicyAsync_ShouldCreatePolicy()
    {
        _leavePolicyRepository.Setup(r => r.GetByLeaveTypeAsync(It.IsAny<LeaveType>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeavePolicy?)null);
        _leavePolicyRepository.Setup(r => r.CreateAsync(It.IsAny<LeavePolicy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeavePolicy p, CancellationToken _) => p);

        var request = new CreateLeavePolicyRequest(
            Name: "Annual Leave Policy",
            LeaveType: LeaveType.Annual,
            DefaultDaysPerYear: 20,
            CarryForward: true,
            MaxCarryForwardDays: 5,
            RequireApproval: true,
            MinServiceDays: 0);

        var result = await _sut.CreateLeavePolicyAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Annual Leave Policy");
        result.LeaveType.Should().Be(LeaveType.Annual);
        result.DefaultDaysPerYear.Should().Be(20);
        result.CarryForward.Should().BeTrue();
    }

    [Fact]
    public async Task CreateLeavePolicyAsync_ShouldThrow_WhenAlreadyExists()
    {
        var existingPolicy = new LeavePolicy
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Existing Policy",
            LeaveType = LeaveType.Annual,
            DefaultDaysPerYear = 15,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _leavePolicyRepository.Setup(r => r.GetByLeaveTypeAsync(LeaveType.Annual, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPolicy);

        var request = new CreateLeavePolicyRequest(
            Name: "New Policy",
            LeaveType: LeaveType.Annual,
            DefaultDaysPerYear: 20,
            CarryForward: false,
            MaxCarryForwardDays: 0,
            RequireApproval: true,
            MinServiceDays: 0);

        var act = () => _sut.CreateLeavePolicyAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    #endregion

    #region Leave Balance Tests

    [Fact]
    public async Task InitializeLeaveBalanceAsync_ShouldCreateBalance()
    {
        _leaveBalanceRepository.Setup(r => r.GetByEmployeeAndTypeAsync(It.IsAny<Guid>(), It.IsAny<LeaveType>(), It.IsAny<int>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveBalance?)null);
        _leaveBalanceRepository.Setup(r => r.CreateAsync(It.IsAny<LeaveBalance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveBalance b, CancellationToken _) => b);

        var request = new InitializeLeaveBalanceRequest(
            EmployeeId: Guid.NewGuid(),
            LeaveType: LeaveType.Annual,
            Year: 2025,
            TotalDays: 20,
            CarriedForwardDays: 5);

        var result = await _sut.InitializeLeaveBalanceAsync(request);

        result.Should().NotBeNull();
        result.TotalDays.Should().Be(20);
        result.CarriedForwardDays.Should().Be(5);
        result.RemainingDays.Should().Be(25);
    }

    [Fact]
    public async Task InitializeLeaveBalanceAsync_ShouldThrow_WhenAlreadyExists()
    {
        var existingBalance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            Year = 2025,
            TotalDays = 20,
            UsedDays = 0,
            PendingDays = 0,
            CarriedForwardDays = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _leaveBalanceRepository.Setup(r => r.GetByEmployeeAndTypeAsync(It.IsAny<Guid>(), It.IsAny<LeaveType>(), It.IsAny<int>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBalance);

        var request = new InitializeLeaveBalanceRequest(
            EmployeeId: existingBalance.EmployeeId,
            LeaveType: LeaveType.Annual,
            Year: 2025,
            TotalDays: 20,
            CarriedForwardDays: 0);

        var act = () => _sut.InitializeLeaveBalanceAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    #endregion

    private LeaveRequest CreateTestLeaveRequest(LeaveStatus status = LeaveStatus.Pending)
    {
        return new LeaveRequest
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            StartDate = new DateTime(2025, 3, 10),
            EndDate = new DateTime(2025, 3, 14),
            TotalDays = 5,
            Reason = "Vacation",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

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
