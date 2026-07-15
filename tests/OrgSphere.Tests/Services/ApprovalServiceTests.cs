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

public class ApprovalServiceTests : IDisposable
{
    private readonly Mock<IApprovalRequestRepository> _approvalRequestRepository = new();
    private readonly Mock<IApprovalChainRepository> _approvalChainRepository = new();
    private readonly Mock<IApprovalStepRepository> _approvalStepRepository = new();
    private readonly Mock<IApprovalStepInstanceRepository> _stepInstanceRepository = new();
    private readonly Mock<IApprovalDelegationRepository> _delegationRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IGraphService> _graphService = new();
    private readonly Mock<IEventBus> _eventBus;
    private readonly MockTenantContextFixture _tenantContext = new();
    private readonly ApprovalService _sut;

    public ApprovalServiceTests()
    {
        _eventBus = MockEventBusFixture.Create();
        _tenantContext.SetTenantContext(TenantId.New(), UserId.New(), UserRole.Admin);
        _sut = new ApprovalService(
            _approvalRequestRepository.Object,
            _approvalChainRepository.Object,
            _approvalStepRepository.Object,
            _stepInstanceRepository.Object,
            _delegationRepository.Object,
            _employeeRepository.Object,
            _graphService.Object,
            _eventBus.Object,
            _tenantContext);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region Approval Request Tests

    [Fact]
    public async Task CreateApprovalRequestAsync_ShouldCreateRequestAndPublishEvent()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _approvalChainRepository.Setup(r => r.GetByTypeAsync(It.IsAny<ApprovalType>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalChain?)null);

        _approvalRequestRepository.Setup(r => r.CreateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalRequest ar, CancellationToken _) => ar);

        _approvalRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new CreateApprovalRequestRequest(
            Type: ApprovalType.LeaveRequest,
            RequesterId: employee.Id,
            RelatedEntityId: Guid.NewGuid(),
            Title: "Vacation Request",
            Description: "Summer vacation",
            Metadata: null);

        var result = await _sut.CreateApprovalRequestAsync(request);

        result.Should().NotBeNull();
        result.Type.Should().Be(ApprovalType.LeaveRequest);
        result.Status.Should().Be(ApprovalStatus.Pending);
        result.RequesterId.Should().Be(employee.Id);
        _approvalRequestRepository.Verify(r => r.CreateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<ApprovalRequestedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateApprovalRequestAsync_ShouldThrowWhenEmployeeNotFound()
    {
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var request = new CreateApprovalRequestRequest(
            Type: ApprovalType.LeaveRequest,
            RequesterId: Guid.NewGuid(),
            RelatedEntityId: null,
            Title: "Test",
            Description: null,
            Metadata: null);

        var act = () => _sut.CreateApprovalRequestAsync(request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateApprovalRequestAsync_ShouldCreateStepInstances_WhenChainExists()
    {
        var employee = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var chain = new ApprovalChain
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Leave Approval",
            ApprovalType = ApprovalType.LeaveRequest,
            IsActive = true,
            StepCount = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _approvalChainRepository.Setup(r => r.GetByTypeAsync(ApprovalType.LeaveRequest, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chain);

        var steps = new List<ApprovalStep>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                ChainId = chain.Id,
                StepOrder = 1,
                Name = "Manager Approval",
                ApproverType = "Manager",
                IsRequired = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                ChainId = chain.Id,
                StepOrder = 2,
                Name = "HR Approval",
                ApproverType = "HR",
                IsRequired = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _approvalStepRepository.Setup(r => r.GetByChainIdAsync(chain.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        _stepInstanceRepository.Setup(r => r.CreateAsync(It.IsAny<ApprovalStepInstance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalStepInstance si, CancellationToken _) => si);

        _approvalRequestRepository.Setup(r => r.CreateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalRequest ar, CancellationToken _) => ar);

        _approvalRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var manager = CreateTestEmployee();
        _employeeRepository.Setup(r => r.GetByDepartmentAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { manager });

        var request = new CreateApprovalRequestRequest(
            Type: ApprovalType.LeaveRequest,
            RequesterId: employee.Id,
            RelatedEntityId: null,
            Title: "Vacation",
            Description: null,
            Metadata: null);

        var result = await _sut.CreateApprovalRequestAsync(request);

        result.Should().NotBeNull();
        result.TotalSteps.Should().Be(2);
        _stepInstanceRepository.Verify(r => r.CreateAsync(It.IsAny<ApprovalStepInstance>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ApproveStepAsync_ShouldApproveAndAdvanceStep()
    {
        var approver = CreateTestEmployee();
        var requester = CreateTestEmployee();

        var approvalRequest = CreateTestApprovalRequest(ApprovalStatus.Pending, requester.Id);
        approvalRequest.CurrentStep = 1;
        approvalRequest.TotalSteps = 2;

        _approvalRequestRepository.Setup(r => r.GetByIdAsync(approvalRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRequest);

        var stepInstance = new ApprovalStepInstance
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            RequestId = approvalRequest.Id,
            StepId = Guid.NewGuid(),
            StepOrder = 1,
            ApproverId = approver.Id,
            Status = ApprovalStepStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _stepInstanceRepository.Setup(r => r.GetPendingByRequestAndOrderAsync(approvalRequest.Id, 1, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stepInstance);

        _stepInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalStepInstance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _approvalRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var chain = new ApprovalChain { Id = Guid.NewGuid(), ApprovalType = approvalRequest.Type };
        _approvalChainRepository.Setup(r => r.GetByTypeAsync(approvalRequest.Type, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chain);

        var steps = new List<ApprovalStep>
        {
            new() { Id = Guid.NewGuid(), ChainId = chain.Id, StepOrder = 2, Name = "HR" }
        };
        _approvalStepRepository.Setup(r => r.GetByChainIdAsync(chain.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(steps);

        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);

        _delegationRepository.Setup(r => r.GetActiveByDelegatorAsync(approver.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalDelegation?)null);

        var request = new ApproveStepRequest(ApproverId: approver.Id, Comments: "Approved");

        var result = await _sut.ApproveStepAsync(approvalRequest.Id, request);

        result.Should().NotBeNull();
        result.CurrentStep.Should().Be(2);
        stepInstance.Status.Should().Be(ApprovalStepStatus.Approved);
    }

    [Fact]
    public async Task ApproveStepAsync_ShouldCompleteApproval_WhenLastStep()
    {
        var approver = CreateTestEmployee();
        var requester = CreateTestEmployee();

        var approvalRequest = CreateTestApprovalRequest(ApprovalStatus.Pending, requester.Id);
        approvalRequest.CurrentStep = 2;
        approvalRequest.TotalSteps = 2;

        _approvalRequestRepository.Setup(r => r.GetByIdAsync(approvalRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRequest);

        var stepInstance = new ApprovalStepInstance
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            RequestId = approvalRequest.Id,
            StepId = Guid.NewGuid(),
            StepOrder = 2,
            ApproverId = approver.Id,
            Status = ApprovalStepStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _stepInstanceRepository.Setup(r => r.GetPendingByRequestAndOrderAsync(approvalRequest.Id, 2, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stepInstance);

        _stepInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalStepInstance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _approvalRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);

        _delegationRepository.Setup(r => r.GetActiveByDelegatorAsync(approver.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalDelegation?)null);

        var request = new ApproveStepRequest(ApproverId: approver.Id, Comments: "Final approval");

        var result = await _sut.ApproveStepAsync(approvalRequest.Id, request);

        result.Should().NotBeNull();
        result.Status.Should().Be(ApprovalStatus.Approved);
        result.FinalApproverId.Should().Be(approver.Id);
        result.FinalApprovedAt.Should().NotBeNull();
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<ApprovalApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveStepAsync_ShouldThrow_WhenNotPending()
    {
        var approvalRequest = CreateTestApprovalRequest(ApprovalStatus.Approved, Guid.NewGuid());
        _approvalRequestRepository.Setup(r => r.GetByIdAsync(approvalRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRequest);

        var request = new ApproveStepRequest(ApproverId: Guid.NewGuid(), Comments: null);

        var act = () => _sut.ApproveStepAsync(approvalRequest.Id, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot approve request with status*");
    }

    [Fact]
    public async Task RejectStepAsync_ShouldRejectAndPublishEvent()
    {
        var approver = CreateTestEmployee();
        var requester = CreateTestEmployee();

        var approvalRequest = CreateTestApprovalRequest(ApprovalStatus.Pending, requester.Id);
        approvalRequest.CurrentStep = 1;
        approvalRequest.TotalSteps = 2;

        _approvalRequestRepository.Setup(r => r.GetByIdAsync(approvalRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRequest);

        var stepInstance = new ApprovalStepInstance
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            RequestId = approvalRequest.Id,
            StepId = Guid.NewGuid(),
            StepOrder = 1,
            ApproverId = approver.Id,
            Status = ApprovalStepStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _stepInstanceRepository.Setup(r => r.GetPendingByRequestAndOrderAsync(approvalRequest.Id, 1, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stepInstance);

        _stepInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalStepInstance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _approvalRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);

        _delegationRepository.Setup(r => r.GetActiveByDelegatorAsync(approver.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalDelegation?)null);

        var request = new RejectStepRequest(ApproverId: approver.Id, Reason: "Not enough justification");

        var result = await _sut.RejectStepAsync(approvalRequest.Id, request);

        result.Should().NotBeNull();
        result.Status.Should().Be(ApprovalStatus.Rejected);
        result.FinalRejectionReason.Should().Be("Not enough justification");
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<ApprovalRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EscalateStepAsync_ShouldEscalateAndPublishEvent()
    {
        var approver = CreateTestEmployee();
        var requester = CreateTestEmployee();
        var manager = CreateTestEmployee();

        var approvalRequest = CreateTestApprovalRequest(ApprovalStatus.Pending, requester.Id);
        approvalRequest.CurrentStep = 1;
        approvalRequest.TotalSteps = 2;

        _approvalRequestRepository.Setup(r => r.GetByIdAsync(approvalRequest.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRequest);

        var stepInstance = new ApprovalStepInstance
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            RequestId = approvalRequest.Id,
            StepId = Guid.NewGuid(),
            StepOrder = 1,
            ApproverId = approver.Id,
            Status = ApprovalStepStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _stepInstanceRepository.Setup(r => r.GetPendingByRequestAndOrderAsync(approvalRequest.Id, 1, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stepInstance);

        _stepInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalStepInstance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _stepInstanceRepository.Setup(r => r.CreateAsync(It.IsAny<ApprovalStepInstance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalStepInstance si, CancellationToken _) => si);

        _approvalRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _employeeRepository.Setup(r => r.GetByIdAsync(approver.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);

        _employeeRepository.Setup(r => r.GetByIdAsync(manager.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(manager);

        var approverWithManager = new Employee
        {
            Id = approver.Id,
            TenantId = _tenantContext.TenantId!,
            ManagerId = manager.Id,
            FirstName = "Test",
            LastName = "Approver",
            Email = "approver@test.com",
            Title = "Engineer",
            DepartmentId = Guid.NewGuid(),
            EmploymentType = EmploymentType.FullTime,
            StartDate = DateTime.UtcNow,
            Status = EmployeeStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _employeeRepository.Setup(r => r.GetByIdAsync(approver.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approverWithManager);

        _delegationRepository.Setup(r => r.GetActiveByDelegatorAsync(manager.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalDelegation?)null);

        var request = new EscalateStepRequest(Reason: "Timeout - need manager review");

        var result = await _sut.EscalateStepAsync(approvalRequest.Id, request);

        result.Should().NotBeNull();
        stepInstance.Status.Should().Be(ApprovalStepStatus.Escalated);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<ApprovalEscalatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Approval Chain Tests

    [Fact]
    public async Task CreateApprovalChainAsync_ShouldCreateChainWithSteps()
    {
        _approvalChainRepository.Setup(r => r.GetByTypeAsync(It.IsAny<ApprovalType>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalChain?)null);

        _approvalChainRepository.Setup(r => r.CreateAsync(It.IsAny<ApprovalChain>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalChain c, CancellationToken _) => c);

        _approvalStepRepository.Setup(r => r.CreateAsync(It.IsAny<ApprovalStep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalStep s, CancellationToken _) => s);

        var request = new CreateApprovalChainRequest(
            Name: "Standard Leave Approval",
            ApprovalType: ApprovalType.LeaveRequest,
            Description: "Two-step approval for leave requests",
            Steps:
            [
                new CreateApprovalStepRequest(1, "Manager", "Manager", null, null, 48, true),
                new CreateApprovalStepRequest(2, "HR", "HR", null, null, 24, true)
            ]);

        var result = await _sut.CreateApprovalChainAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Standard Leave Approval");
        result.StepCount.Should().Be(2);
        _approvalStepRepository.Verify(r => r.CreateAsync(It.IsAny<ApprovalStep>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<ApprovalChainCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateApprovalChainAsync_ShouldThrow_WhenAlreadyExists()
    {
        var existingChain = new ApprovalChain
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Existing",
            ApprovalType = ApprovalType.LeaveRequest,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _approvalChainRepository.Setup(r => r.GetByTypeAsync(ApprovalType.LeaveRequest, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingChain);

        var request = new CreateApprovalChainRequest(
            Name: "New Chain",
            ApprovalType: ApprovalType.LeaveRequest,
            Description: null,
            Steps: []);

        var act = () => _sut.CreateApprovalChainAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task DeleteApprovalChainAsync_ShouldDeleteChainAndSteps()
    {
        var chain = new ApprovalChain
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Name = "Test Chain",
            ApprovalType = ApprovalType.LeaveRequest,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _approvalChainRepository.Setup(r => r.GetByIdAsync(chain.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chain);

        _approvalStepRepository.Setup(r => r.DeleteByChainIdAsync(chain.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _approvalChainRepository.Setup(r => r.DeleteAsync(chain.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteApprovalChainAsync(chain.Id);

        _approvalStepRepository.Verify(r => r.DeleteByChainIdAsync(chain.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()), Times.Once);
        _approvalChainRepository.Verify(r => r.DeleteAsync(chain.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Delegation Tests

    [Fact]
    public async Task CreateDelegationAsync_ShouldCreateDelegation()
    {
        var delegator = CreateTestEmployee();
        var delegatee = CreateTestEmployee();

        _employeeRepository.Setup(r => r.GetByIdAsync(delegator.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(delegator);

        _employeeRepository.Setup(r => r.GetByIdAsync(delegatee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(delegatee);

        _delegationRepository.Setup(r => r.CreateAsync(It.IsAny<ApprovalDelegation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalDelegation d, CancellationToken _) => d);

        var request = new CreateDelegationRequest(
            DelegatorId: delegator.Id,
            DelegateId: delegatee.Id,
            Scope: "Leave",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(7));

        var result = await _sut.CreateDelegationAsync(request);

        result.Should().NotBeNull();
        result.DelegatorId.Should().Be(delegator.Id);
        result.DelegateId.Should().Be(delegatee.Id);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDelegationAsync_ShouldThrow_WhenDelegatingToSelf()
    {
        var employee = CreateTestEmployee();

        _employeeRepository.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var request = new CreateDelegationRequest(
            DelegatorId: employee.Id,
            DelegateId: employee.Id,
            Scope: null,
            StartDate: null,
            EndDate: null);

        var act = () => _sut.CreateDelegationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot delegate to yourself*");
    }

    [Fact]
    public async Task CreateDelegationAsync_ShouldThrow_WhenEndBeforeStart()
    {
        var delegator = CreateTestEmployee();
        var delegatee = CreateTestEmployee();

        _employeeRepository.Setup(r => r.GetByIdAsync(delegator.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(delegator);

        _employeeRepository.Setup(r => r.GetByIdAsync(delegatee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(delegatee);

        var request = new CreateDelegationRequest(
            DelegatorId: delegator.Id,
            DelegateId: delegatee.Id,
            Scope: null,
            StartDate: new DateTime(2025, 12, 31),
            EndDate: new DateTime(2025, 1, 1));

        var act = () => _sut.CreateDelegationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*End date must be after start date*");
    }

    [Fact]
    public async Task GetPendingApprovalsForApproverAsync_ShouldIncludeDelegatedApprovals()
    {
        var approver = CreateTestEmployee();
        var delegatee = CreateTestEmployee();

        var delegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            DelegatorId = approver.Id,
            DelegateId = delegatee.Id,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _delegationRepository.Setup(r => r.GetByDelegatorAsync(approver.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApprovalDelegation> { delegation });

        var delegatedSteps = new List<ApprovalStepInstance>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                RequestId = Guid.NewGuid(),
                StepId = Guid.NewGuid(),
                StepOrder = 1,
                ApproverId = delegatee.Id,
                Status = ApprovalStepStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _stepInstanceRepository.Setup(r => r.GetPendingByApproverAsync(delegatee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(delegatedSteps);

        var approvalRequest = CreateTestApprovalRequest(ApprovalStatus.Pending, Guid.NewGuid());
        _approvalRequestRepository.Setup(r => r.GetByIdAsync(delegatedSteps[0].RequestId, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalRequest);

        _employeeRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestEmployee());

        var result = await _sut.GetPendingApprovalsForApproverAsync(approver.Id);

        result.Should().NotBeNull();
        result.TotalPending.Should().Be(1);
    }

    #endregion

    private ApprovalRequest CreateTestApprovalRequest(ApprovalStatus status, Guid requesterId)
    {
        return new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Type = ApprovalType.LeaveRequest,
            RequesterId = requesterId,
            Title = "Test Request",
            Status = status,
            CurrentStep = 1,
            TotalSteps = 1,
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
