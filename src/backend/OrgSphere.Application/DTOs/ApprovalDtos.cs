using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.DTOs;

public record ApprovalRequestDto
{
    public Guid Id { get; init; }
    public ApprovalType Type { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public Guid RequesterId { get; init; }
    public string RequesterName { get; init; } = string.Empty;
    public Guid? RelatedEntityId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Metadata { get; init; }
    public ApprovalStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public int CurrentStep { get; init; }
    public int TotalSteps { get; init; }
    public IReadOnlyList<ApprovalStepInstanceDto> Steps { get; init; } = [];
    public Guid? FinalApproverId { get; init; }
    public string? FinalApproverName { get; init; }
    public DateTime? FinalApprovedAt { get; init; }
    public string? FinalRejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ApprovalStepInstanceDto
{
    public Guid Id { get; init; }
    public int StepOrder { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid? ApproverId { get; init; }
    public string? ApproverName { get; init; }
    public Guid? DelegatedToId { get; init; }
    public string? DelegatedToName { get; init; }
    public ApprovalStepStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public string? Comments { get; init; }
    public DateTime? RespondedAt { get; init; }
    public DateTime? EscalatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateApprovalRequestRequest(
    ApprovalType Type,
    Guid RequesterId,
    Guid? RelatedEntityId,
    string? Title,
    string? Description,
    string? Metadata);

public record ApproveStepRequest(
    Guid ApproverId,
    string? Comments);

public record RejectStepRequest(
    Guid ApproverId,
    string Reason);

public record EscalateStepRequest(
    string? Reason);

public record ApprovalChainDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ApprovalType Type { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int StepCount { get; init; }
    public IReadOnlyList<ApprovalStepDto> Steps { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public record ApprovalStepDto
{
    public Guid Id { get; init; }
    public int StepOrder { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ApproverType { get; init; }
    public Guid? SpecificApproverId { get; init; }
    public string? Condition { get; init; }
    public int TimeoutHours { get; init; }
    public bool IsRequired { get; init; }
}

public record CreateApprovalChainRequest(
    string Name,
    ApprovalType ApprovalType,
    string? Description,
    IReadOnlyList<CreateApprovalStepRequest> Steps);

public record CreateApprovalStepRequest(
    int StepOrder,
    string Name,
    string? ApproverType,
    Guid? SpecificApproverId,
    string? Condition,
    int TimeoutHours,
    bool IsRequired);

public record UpdateApprovalChainRequest(
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<CreateApprovalStepRequest> Steps);

public record ApprovalDelegationDto
{
    public Guid Id { get; init; }
    public Guid DelegatorId { get; init; }
    public string DelegatorName { get; init; } = string.Empty;
    public Guid DelegateId { get; init; }
    public string DelegateName { get; init; } = string.Empty;
    public string? Scope { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateDelegationRequest(
    Guid DelegatorId,
    Guid DelegateId,
    string? Scope,
    DateTime? StartDate,
    DateTime? EndDate);

public record PendingApprovalsDto
{
    public int TotalPending { get; init; }
    public IReadOnlyList<ApprovalRequestDto> Requests { get; init; } = [];
}
