using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public class ApprovalService(
    IApprovalRequestRepository approvalRequestRepository,
    IApprovalChainRepository approvalChainRepository,
    IApprovalStepRepository approvalStepRepository,
    IApprovalStepInstanceRepository stepInstanceRepository,
    IApprovalDelegationRepository delegationRepository,
    IEmployeeRepository employeeRepository,
    IGraphService graphService,
    IEventBus eventBus,
    ITenantContext tenantContext) : IApprovalService
{
    private readonly IApprovalRequestRepository _approvalRequestRepository = approvalRequestRepository;
    private readonly IApprovalChainRepository _approvalChainRepository = approvalChainRepository;
    private readonly IApprovalStepRepository _approvalStepRepository = approvalStepRepository;
    private readonly IApprovalStepInstanceRepository _stepInstanceRepository = stepInstanceRepository;
    private readonly IApprovalDelegationRepository _delegationRepository = delegationRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IGraphService _graphService = graphService;
    private readonly IEventBus _eventBus = eventBus;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<ApprovalRequestDto> CreateApprovalRequestAsync(CreateApprovalRequestRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        _ = await _employeeRepository.GetByIdAsync(request.RequesterId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Employee {request.RequesterId} not found");

        var chain = await _approvalChainRepository.GetByTypeAsync(request.Type, tenantId, ct);

        var approvalRequest = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = request.Type,
            RequesterId = request.RequesterId,
            RelatedEntityId = request.RelatedEntityId,
            Title = request.Title,
            Description = request.Description,
            Metadata = request.Metadata,
            Status = ApprovalStatus.Pending,
            CurrentStep = 1,
            TotalSteps = chain?.StepCount ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _approvalRequestRepository.CreateAsync(approvalRequest, ct);

        if (chain is not null)
        {
            await CreateStepInstancesForRequestAsync(created, chain, tenantId, ct);
        }
        else
        {
            created.TotalSteps = 0;
            await _approvalRequestRepository.UpdateAsync(created, ct);
        }

        await _eventBus.PublishAsync(
            new ApprovalRequestedEvent(tenantId, created.Id, created.RequesterId, created.Type), ct);

        return await MapToDto(created, tenantId, ct);
    }

    public async Task<ApprovalRequestDto?> GetApprovalRequestByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _approvalRequestRepository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : await MapToDto(entity, tenantId, ct);
    }

    public async Task<IReadOnlyList<ApprovalRequestDto>> GetAllApprovalRequestsAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _approvalRequestRepository.GetAllAsync(tenantId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<IReadOnlyList<ApprovalRequestDto>> GetApprovalRequestsByRequesterAsync(Guid requesterId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _approvalRequestRepository.GetByRequesterAsync(requesterId, tenantId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<IReadOnlyList<ApprovalRequestDto>> GetApprovalRequestsByStatusAsync(ApprovalStatus status, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _approvalRequestRepository.GetByStatusAsync(status, tenantId, ct);
        var dtos = new List<ApprovalRequestDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<PendingApprovalsDto> GetPendingApprovalsForApproverAsync(Guid approverId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var delegations = await _delegationRepository.GetByDelegatorAsync(approverId, tenantId, ct);
        var activeDelegateIds = delegations
            .Where(d => d.IsActive && (!d.EndDate.HasValue || d.EndDate.Value > DateTime.UtcNow))
            .Select(d => d.DelegateId)
            .ToList();

        var allApproverIds = new List<Guid> { approverId };
        allApproverIds.AddRange(activeDelegateIds);

        var pendingSteps = new List<ApprovalStepInstance>();
        foreach (var id in allApproverIds)
        {
            var steps = await _stepInstanceRepository.GetPendingByApproverAsync(id, tenantId, ct);
            pendingSteps.AddRange(steps);
        }

        var requestIds = pendingSteps.Select(s => s.RequestId).Distinct().ToList();
        var requests = new List<ApprovalRequestDto>();
        foreach (var requestId in requestIds)
        {
            var request = await _approvalRequestRepository.GetByIdAsync(requestId, tenantId, ct);
            if (request is not null && request.Status == ApprovalStatus.Pending)
            {
                requests.Add(await MapToDto(request, tenantId, ct));
            }
        }

        return new PendingApprovalsDto
        {
            TotalPending = requests.Count,
            Requests = requests
        };
    }

    public async Task<ApprovalRequestDto> ApproveStepAsync(Guid requestId, ApproveStepRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var approvalRequest = await _approvalRequestRepository.GetByIdAsync(requestId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Approval request {requestId} not found");

        if (approvalRequest.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve request with status {approvalRequest.Status}");

        var currentStepInstance = await _stepInstanceRepository.GetPendingByRequestAndOrderAsync(
            requestId, approvalRequest.CurrentStep, tenantId, ct)
            ?? throw new InvalidOperationException($"No pending step found for step {approvalRequest.CurrentStep}");

        await ValidateApproverPermissionAsync(currentStepInstance, request.ApproverId, tenantId, ct);

        currentStepInstance.Status = ApprovalStepStatus.Approved;
        currentStepInstance.Comments = request.Comments;
        currentStepInstance.RespondedAt = DateTime.UtcNow;
        currentStepInstance.UpdatedAt = DateTime.UtcNow;
        await _stepInstanceRepository.UpdateAsync(currentStepInstance, ct);

        if (approvalRequest.CurrentStep >= approvalRequest.TotalSteps)
        {
            approvalRequest.Status = ApprovalStatus.Approved;
            approvalRequest.FinalApproverId = request.ApproverId;
            approvalRequest.FinalApprovedAt = DateTime.UtcNow;
            approvalRequest.UpdatedAt = DateTime.UtcNow;
            await _approvalRequestRepository.UpdateAsync(approvalRequest, ct);

            await _eventBus.PublishAsync(
                new ApprovalApprovedEvent(tenantId, requestId, request.ApproverId), ct);
        }
        else
        {
            approvalRequest.CurrentStep++;
            approvalRequest.UpdatedAt = DateTime.UtcNow;
            await _approvalRequestRepository.UpdateAsync(approvalRequest, ct);

            await ResolveNextStepApproversAsync(approvalRequest, tenantId, ct);
        }

        return await MapToDto(approvalRequest, tenantId, ct);
    }

    public async Task<ApprovalRequestDto> RejectStepAsync(Guid requestId, RejectStepRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var approvalRequest = await _approvalRequestRepository.GetByIdAsync(requestId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Approval request {requestId} not found");

        if (approvalRequest.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot reject request with status {approvalRequest.Status}");

        var currentStepInstance = await _stepInstanceRepository.GetPendingByRequestAndOrderAsync(
            requestId, approvalRequest.CurrentStep, tenantId, ct)
            ?? throw new InvalidOperationException($"No pending step found for step {approvalRequest.CurrentStep}");

        await ValidateApproverPermissionAsync(currentStepInstance, request.ApproverId, tenantId, ct);

        currentStepInstance.Status = ApprovalStepStatus.Rejected;
        currentStepInstance.Comments = request.Reason;
        currentStepInstance.RespondedAt = DateTime.UtcNow;
        currentStepInstance.UpdatedAt = DateTime.UtcNow;
        await _stepInstanceRepository.UpdateAsync(currentStepInstance, ct);

        approvalRequest.Status = ApprovalStatus.Rejected;
        approvalRequest.FinalApproverId = request.ApproverId;
        approvalRequest.FinalRejectionReason = request.Reason;
        approvalRequest.UpdatedAt = DateTime.UtcNow;
        await _approvalRequestRepository.UpdateAsync(approvalRequest, ct);

        await _eventBus.PublishAsync(
            new ApprovalRejectedEvent(tenantId, requestId, request.ApproverId, request.Reason), ct);

        return await MapToDto(approvalRequest, tenantId, ct);
    }

    public async Task<ApprovalRequestDto> EscalateStepAsync(Guid requestId, EscalateStepRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var approvalRequest = await _approvalRequestRepository.GetByIdAsync(requestId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Approval request {requestId} not found");

        if (approvalRequest.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot escalate request with status {approvalRequest.Status}");

        var currentStepInstance = await _stepInstanceRepository.GetPendingByRequestAndOrderAsync(
            requestId, approvalRequest.CurrentStep, tenantId, ct)
            ?? throw new InvalidOperationException($"No pending step found for step {approvalRequest.CurrentStep}");

        currentStepInstance.Status = ApprovalStepStatus.Escalated;
        currentStepInstance.Comments = request.Reason;
        currentStepInstance.EscalatedAt = DateTime.UtcNow;
        currentStepInstance.UpdatedAt = DateTime.UtcNow;
        await _stepInstanceRepository.UpdateAsync(currentStepInstance, ct);

        var escalatedTo = await FindEscalationTargetAsync(currentStepInstance, tenantId, ct);
        if (escalatedTo.HasValue)
        {
            var newStepInstance = new ApprovalStepInstance
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RequestId = requestId,
                StepId = currentStepInstance.StepId,
                StepOrder = currentStepInstance.StepOrder,
                ApproverId = escalatedTo.Value,
                Status = ApprovalStepStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _stepInstanceRepository.CreateAsync(newStepInstance, ct);

            approvalRequest.UpdatedAt = DateTime.UtcNow;
            await _approvalRequestRepository.UpdateAsync(approvalRequest, ct);
        }

        await _eventBus.PublishAsync(
            new ApprovalEscalatedEvent(tenantId, requestId, currentStepInstance.Id), ct);

        return await MapToDto(approvalRequest, tenantId, ct);
    }

    public async Task<ApprovalChainDto> CreateApprovalChainAsync(CreateApprovalChainRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var existing = await _approvalChainRepository.GetByTypeAsync(request.ApprovalType, tenantId, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Approval chain for {request.ApprovalType} already exists");

        var chain = new ApprovalChain
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            ApprovalType = request.ApprovalType,
            Description = request.Description,
            IsActive = true,
            StepCount = request.Steps.Count,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _approvalChainRepository.CreateAsync(chain, ct);

        foreach (var stepRequest in request.Steps)
        {
            var step = new ApprovalStep
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ChainId = created.Id,
                StepOrder = stepRequest.StepOrder,
                Name = stepRequest.Name,
                ApproverType = stepRequest.ApproverType,
                SpecificApproverId = stepRequest.SpecificApproverId,
                Condition = stepRequest.Condition,
                TimeoutHours = stepRequest.TimeoutHours,
                IsRequired = stepRequest.IsRequired,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _approvalStepRepository.CreateAsync(step, ct);
        }

        await _eventBus.PublishAsync(
            new ApprovalChainCreatedEvent(tenantId, created.Id, created.ApprovalType), ct);

        return await MapChainToDto(created, tenantId, ct);
    }

    public async Task<ApprovalChainDto?> GetApprovalChainByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _approvalChainRepository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : await MapChainToDto(entity, tenantId, ct);
    }

    public async Task<IReadOnlyList<ApprovalChainDto>> GetAllApprovalChainsAsync(CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _approvalChainRepository.GetAllAsync(tenantId, ct);
        var dtos = new List<ApprovalChainDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapChainToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task<ApprovalChainDto?> GetApprovalChainByTypeAsync(ApprovalType type, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _approvalChainRepository.GetByTypeAsync(type, tenantId, ct);
        return entity is null ? null : await MapChainToDto(entity, tenantId, ct);
    }

    public async Task<ApprovalChainDto> UpdateApprovalChainAsync(Guid id, UpdateApprovalChainRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        var chain = await _approvalChainRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Approval chain {id} not found");

        chain.Name = request.Name;
        chain.Description = request.Description;
        chain.IsActive = request.IsActive;
        chain.StepCount = request.Steps.Count;
        chain.UpdatedAt = DateTime.UtcNow;

        await _approvalChainRepository.UpdateAsync(chain, ct);
        await _approvalStepRepository.DeleteByChainIdAsync(id, tenantId, ct);

        foreach (var stepRequest in request.Steps)
        {
            var step = new ApprovalStep
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ChainId = id,
                StepOrder = stepRequest.StepOrder,
                Name = stepRequest.Name,
                ApproverType = stepRequest.ApproverType,
                SpecificApproverId = stepRequest.SpecificApproverId,
                Condition = stepRequest.Condition,
                TimeoutHours = stepRequest.TimeoutHours,
                IsRequired = stepRequest.IsRequired,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _approvalStepRepository.CreateAsync(step, ct);
        }

        return await MapChainToDto(chain, tenantId, ct);
    }

    public async Task DeleteApprovalChainAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        _ = await _approvalChainRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Approval chain {id} not found");

        await _approvalStepRepository.DeleteByChainIdAsync(id, tenantId, ct);
        await _approvalChainRepository.DeleteAsync(id, tenantId, ct);
    }

    public async Task<ApprovalDelegationDto> CreateDelegationAsync(CreateDelegationRequest request, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;

        _ = await _employeeRepository.GetByIdAsync(request.DelegatorId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Delegator employee {request.DelegatorId} not found");

        _ = await _employeeRepository.GetByIdAsync(request.DelegateId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Delegate employee {request.DelegateId} not found");

        if (request.DelegatorId == request.DelegateId)
            throw new InvalidOperationException("Cannot delegate to yourself");

        if (request.EndDate.HasValue && request.StartDate.HasValue && request.EndDate.Value < request.StartDate.Value)
            throw new InvalidOperationException("End date must be after start date");

        var delegation = new ApprovalDelegation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DelegatorId = request.DelegatorId,
            DelegateId = request.DelegateId,
            Scope = request.Scope,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _delegationRepository.CreateAsync(delegation, ct);
        return await MapDelegationToDto(created, tenantId, ct);
    }

    public async Task<ApprovalDelegationDto?> GetDelegationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entity = await _delegationRepository.GetByIdAsync(id, tenantId, ct);
        return entity is null ? null : await MapDelegationToDto(entity, tenantId, ct);
    }

    public async Task<IReadOnlyList<ApprovalDelegationDto>> GetDelegationsByDelegatorAsync(Guid delegatorId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var entities = await _delegationRepository.GetByDelegatorAsync(delegatorId, tenantId, ct);
        var dtos = new List<ApprovalDelegationDto>();
        foreach (var entity in entities)
        {
            dtos.Add(await MapDelegationToDto(entity, tenantId, ct));
        }
        return dtos;
    }

    public async Task DeleteDelegationAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        _ = await _delegationRepository.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Delegation {id} not found");

        await _delegationRepository.DeleteAsync(id, tenantId, ct);
    }

    private async Task CreateStepInstancesForRequestAsync(
        ApprovalRequest request, ApprovalChain chain, TenantId tenantId, CancellationToken ct)
    {
        var steps = await _approvalStepRepository.GetByChainIdAsync(chain.Id, tenantId, ct);
        var orderedSteps = steps.OrderBy(s => s.StepOrder).ToList();

        foreach (var step in orderedSteps)
        {
            var approverId = await ResolveApproverForStepAsync(step, request.RequesterId, tenantId, ct);

            var stepInstance = new ApprovalStepInstance
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RequestId = request.Id,
                StepId = step.Id,
                StepOrder = step.StepOrder,
                ApproverId = approverId,
                Status = step.StepOrder == 1 ? ApprovalStepStatus.Pending : ApprovalStepStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _stepInstanceRepository.CreateAsync(stepInstance, ct);
        }
    }

    private async Task<Guid?> ResolveApproverForStepAsync(ApprovalStep step, Guid requesterId, TenantId tenantId, CancellationToken ct)
    {
        if (step.SpecificApproverId.HasValue)
        {
            return await CheckDelegationAsync(step.SpecificApproverId.Value, tenantId, ct);
        }

        return step.ApproverType?.ToLowerInvariant() switch
        {
            "manager" => await FindManagerAsync(requesterId, tenantId, ct),
            "departmenthead" => await FindDepartmentHeadAsync(requesterId, tenantId, ct),
            "hr" => await FindRoleApproverAsync("HR", tenantId, ct),
            "finance" => await FindRoleApproverAsync("Finance", tenantId, ct),
            "it" => await FindRoleApproverAsync("IT", tenantId, ct),
            _ => null
        };
    }

    private async Task ResolveNextStepApproversAsync(ApprovalRequest request, TenantId tenantId, CancellationToken ct)
    {
        var steps = await _approvalStepRepository.GetByChainIdAsync(
            (await _approvalChainRepository.GetByTypeAsync(request.Type, tenantId, ct))?.Id ?? Guid.Empty,
            tenantId, ct);

        var currentStep = steps.FirstOrDefault(s => s.StepOrder == request.CurrentStep);
        if (currentStep is not null)
        {
            var approverId = await ResolveApproverForStepAsync(currentStep, request.RequesterId, tenantId, ct);

            var existingStep = await _stepInstanceRepository.GetPendingByRequestAndOrderAsync(
                request.Id, request.CurrentStep, tenantId, ct);

            if (existingStep is not null)
            {
                existingStep.ApproverId = approverId;
                existingStep.UpdatedAt = DateTime.UtcNow;
                await _stepInstanceRepository.UpdateAsync(existingStep, ct);
            }
        }
    }

    private async Task<Guid?> FindManagerAsync(Guid employeeId, TenantId tenantId, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, tenantId, ct);
        if (employee?.ManagerId.HasValue == true)
        {
            return await CheckDelegationAsync(employee.ManagerId.Value, tenantId, ct);
        }

        return null;
    }

    private async Task<Guid?> FindDepartmentHeadAsync(Guid employeeId, TenantId tenantId, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, tenantId, ct);
        if (employee is null) return null;

        var departmentEmployees = await _employeeRepository.GetByDepartmentAsync(employee.DepartmentId, tenantId, ct);
        var departmentHead = departmentEmployees.FirstOrDefault(e =>
            e.Title.Contains("Head", StringComparison.OrdinalIgnoreCase) ||
            e.Title.Contains("Director", StringComparison.OrdinalIgnoreCase) ||
            e.Title.Contains("Manager", StringComparison.OrdinalIgnoreCase));

        if (departmentHead is not null && departmentHead.Id != employeeId)
        {
            return await CheckDelegationAsync(departmentHead.Id, tenantId, ct);
        }

        return await FindManagerAsync(employeeId, tenantId, ct);
    }

    private async Task<Guid?> FindRoleApproverAsync(string role, TenantId tenantId, CancellationToken ct)
    {
        var employees = await _employeeRepository.GetAllAsync(tenantId, ct);
        var roleEmployee = employees.FirstOrDefault(e =>
            e.Title.Contains(role, StringComparison.OrdinalIgnoreCase));

        if (roleEmployee is not null)
        {
            return await CheckDelegationAsync(roleEmployee.Id, tenantId, ct);
        }

        return null;
    }

    private async Task<Guid> CheckDelegationAsync(Guid approverId, TenantId tenantId, CancellationToken ct)
    {
        var delegation = await _delegationRepository.GetActiveByDelegatorAsync(approverId, tenantId, ct);
        if (delegation is not null &&
            (!delegation.StartDate.HasValue || delegation.StartDate.Value <= DateTime.UtcNow) &&
            (!delegation.EndDate.HasValue || delegation.EndDate.Value >= DateTime.UtcNow))
        {
            return delegation.DelegateId;
        }

        return approverId;
    }

    private async Task ValidateApproverPermissionAsync(ApprovalStepInstance stepInstance, Guid approverId, TenantId tenantId, CancellationToken ct)
    {
        if (stepInstance.ApproverId != approverId)
        {
            var delegation = await _delegationRepository.GetActiveByDelegatorAsync(stepInstance.ApproverId!.Value, tenantId, ct);
            if (delegation is null || delegation.DelegateId != approverId)
            {
                throw new InvalidOperationException("You are not authorized to approve this step");
            }
        }
    }

    private async Task<Guid?> FindEscalationTargetAsync(ApprovalStepInstance currentStep, TenantId tenantId, CancellationToken ct)
    {
        if (currentStep.ApproverId.HasValue)
        {
            var managerId = await FindManagerAsync(currentStep.ApproverId.Value, tenantId, ct);
            if (managerId.HasValue && managerId.Value != currentStep.ApproverId.Value)
            {
                return await CheckDelegationAsync(managerId.Value, tenantId, ct);
            }
        }

        return null;
    }

    private async Task<ApprovalRequestDto> MapToDto(ApprovalRequest entity, TenantId tenantId, CancellationToken ct)
    {
        var requester = await _employeeRepository.GetByIdAsync(entity.RequesterId, tenantId, ct);
        var requesterName = requester is not null ? $"{requester.FirstName} {requester.LastName}" : "Unknown";

        string? finalApproverName = null;
        if (entity.FinalApproverId.HasValue)
        {
            var approver = await _employeeRepository.GetByIdAsync(entity.FinalApproverId.Value, tenantId, ct);
            finalApproverName = approver is not null ? $"{approver.FirstName} {approver.LastName}" : "Unknown";
        }

        var stepInstances = await _stepInstanceRepository.GetByRequestIdAsync(entity.Id, tenantId, ct);
        var steps = new List<ApprovalStepInstanceDto>();
        foreach (var step in stepInstances.OrderBy(s => s.StepOrder))
        {
            steps.Add(await MapStepInstanceToDto(step, tenantId, ct));
        }

        return new ApprovalRequestDto
        {
            Id = entity.Id,
            Type = entity.Type,
            TypeName = entity.Type.ToString(),
            RequesterId = entity.RequesterId,
            RequesterName = requesterName,
            RelatedEntityId = entity.RelatedEntityId,
            Title = entity.Title,
            Description = entity.Description,
            Metadata = entity.Metadata,
            Status = entity.Status,
            StatusName = entity.Status.ToString(),
            CurrentStep = entity.CurrentStep,
            TotalSteps = entity.TotalSteps,
            Steps = steps,
            FinalApproverId = entity.FinalApproverId,
            FinalApproverName = finalApproverName,
            FinalApprovedAt = entity.FinalApprovedAt,
            FinalRejectionReason = entity.FinalRejectionReason,
            CreatedAt = entity.CreatedAt
        };
    }

    private async Task<ApprovalStepInstanceDto> MapStepInstanceToDto(ApprovalStepInstance step, TenantId tenantId, CancellationToken ct)
    {
        var approverName = "Unknown";
        if (step.ApproverId.HasValue)
        {
            var approver = await _employeeRepository.GetByIdAsync(step.ApproverId.Value, tenantId, ct);
            approverName = approver is not null ? $"{approver.FirstName} {approver.LastName}" : "Unknown";
        }

        string? delegatedToName = null;
        if (step.DelegatedToId.HasValue)
        {
            var delegatee = await _employeeRepository.GetByIdAsync(step.DelegatedToId.Value, tenantId, ct);
            delegatedToName = delegatee is not null ? $"{delegatee.FirstName} {delegatee.LastName}" : "Unknown";
        }

        return new ApprovalStepInstanceDto
        {
            Id = step.Id,
            StepOrder = step.StepOrder,
            Name = step.Comments ?? $"Step {step.StepOrder}",
            ApproverId = step.ApproverId,
            ApproverName = approverName,
            DelegatedToId = step.DelegatedToId,
            DelegatedToName = delegatedToName,
            Status = step.Status,
            StatusName = step.Status.ToString(),
            Comments = step.Comments,
            RespondedAt = step.RespondedAt,
            EscalatedAt = step.EscalatedAt,
            CreatedAt = step.CreatedAt
        };
    }

    private async Task<ApprovalChainDto> MapChainToDto(ApprovalChain entity, TenantId tenantId, CancellationToken ct)
    {
        var steps = await _approvalStepRepository.GetByChainIdAsync(entity.Id, tenantId, ct);
        var stepDtos = steps.OrderBy(s => s.StepOrder).Select(s => new ApprovalStepDto
        {
            Id = s.Id,
            StepOrder = s.StepOrder,
            Name = s.Name,
            ApproverType = s.ApproverType,
            SpecificApproverId = s.SpecificApproverId,
            Condition = s.Condition,
            TimeoutHours = s.TimeoutHours,
            IsRequired = s.IsRequired
        }).ToList();

        return new ApprovalChainDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.ApprovalType,
            TypeName = entity.ApprovalType.ToString(),
            Description = entity.Description,
            IsActive = entity.IsActive,
            StepCount = entity.StepCount,
            Steps = stepDtos,
            CreatedAt = entity.CreatedAt
        };
    }

    private async Task<ApprovalDelegationDto> MapDelegationToDto(ApprovalDelegation entity, TenantId tenantId, CancellationToken ct)
    {
        var delegator = await _employeeRepository.GetByIdAsync(entity.DelegatorId, tenantId, ct);
        var delegatorName = delegator is not null ? $"{delegator.FirstName} {delegator.LastName}" : "Unknown";

        var delegatee = await _employeeRepository.GetByIdAsync(entity.DelegateId, tenantId, ct);
        var delegateName = delegatee is not null ? $"{delegatee.FirstName} {delegatee.LastName}" : "Unknown";

        return new ApprovalDelegationDto
        {
            Id = entity.Id,
            DelegatorId = entity.DelegatorId,
            DelegatorName = delegatorName,
            DelegateId = entity.DelegateId,
            DelegateName = delegateName,
            Scope = entity.Scope,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }
}
