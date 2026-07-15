using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IApprovalRequestRepository
{
    Task<ApprovalRequest?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalRequest>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalRequest>> GetByRequesterAsync(Guid requesterId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalRequest>> GetByApproverAsync(Guid approverId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalRequest>> GetByStatusAsync(ApprovalStatus status, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalRequest>> GetByTypeAsync(ApprovalType type, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalRequest> CreateAsync(ApprovalRequest approvalRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApprovalRequest approvalRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
