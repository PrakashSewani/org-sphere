using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.Services;

public interface IApprovalService
{
    Task<ApprovalRequestDto> CreateApprovalRequestAsync(CreateApprovalRequestRequest request, CancellationToken ct = default);
    Task<ApprovalRequestDto?> GetApprovalRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalRequestDto>> GetAllApprovalRequestsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalRequestDto>> GetApprovalRequestsByRequesterAsync(Guid requesterId, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalRequestDto>> GetApprovalRequestsByStatusAsync(ApprovalStatus status, CancellationToken ct = default);
    Task<PendingApprovalsDto> GetPendingApprovalsForApproverAsync(Guid approverId, CancellationToken ct = default);
    Task<ApprovalRequestDto> ApproveStepAsync(Guid requestId, ApproveStepRequest request, CancellationToken ct = default);
    Task<ApprovalRequestDto> RejectStepAsync(Guid requestId, RejectStepRequest request, CancellationToken ct = default);
    Task<ApprovalRequestDto> EscalateStepAsync(Guid requestId, EscalateStepRequest request, CancellationToken ct = default);

    Task<ApprovalChainDto> CreateApprovalChainAsync(CreateApprovalChainRequest request, CancellationToken ct = default);
    Task<ApprovalChainDto?> GetApprovalChainByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalChainDto>> GetAllApprovalChainsAsync(CancellationToken ct = default);
    Task<ApprovalChainDto?> GetApprovalChainByTypeAsync(ApprovalType type, CancellationToken ct = default);
    Task<ApprovalChainDto> UpdateApprovalChainAsync(Guid id, UpdateApprovalChainRequest request, CancellationToken ct = default);
    Task DeleteApprovalChainAsync(Guid id, CancellationToken ct = default);

    Task<ApprovalDelegationDto> CreateDelegationAsync(CreateDelegationRequest request, CancellationToken ct = default);
    Task<ApprovalDelegationDto?> GetDelegationByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalDelegationDto>> GetDelegationsByDelegatorAsync(Guid delegatorId, CancellationToken ct = default);
    Task DeleteDelegationAsync(Guid id, CancellationToken ct = default);
}
