using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IApprovalChainRepository
{
    Task<ApprovalChain?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalChain>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalChain?> GetByTypeAsync(ApprovalType type, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalChain> CreateAsync(ApprovalChain chain, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApprovalChain chain, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
