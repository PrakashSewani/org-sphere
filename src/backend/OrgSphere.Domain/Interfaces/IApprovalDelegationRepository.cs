using OrgSphere.Domain.Entities;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IApprovalDelegationRepository
{
    Task<ApprovalDelegation?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalDelegation>> GetByDelegatorAsync(Guid delegatorId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalDelegation?> GetActiveByDelegatorAsync(Guid delegatorId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalDelegation> CreateAsync(ApprovalDelegation delegation, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApprovalDelegation delegation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
