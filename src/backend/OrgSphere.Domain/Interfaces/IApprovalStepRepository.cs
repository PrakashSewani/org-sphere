using OrgSphere.Domain.Entities;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IApprovalStepRepository
{
    Task<ApprovalStep?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalStep>> GetByChainIdAsync(Guid chainId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalStep> CreateAsync(ApprovalStep step, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApprovalStep step, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task DeleteByChainIdAsync(Guid chainId, TenantId tenantId, CancellationToken cancellationToken = default);
}
