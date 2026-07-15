using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IApprovalStepInstanceRepository
{
    Task<ApprovalStepInstance?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalStepInstance>> GetByRequestIdAsync(Guid requestId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApprovalStepInstance>> GetPendingByApproverAsync(Guid approverId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalStepInstance?> GetPendingByRequestAndOrderAsync(Guid requestId, int stepOrder, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<ApprovalStepInstance> CreateAsync(ApprovalStepInstance stepInstance, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApprovalStepInstance stepInstance, CancellationToken cancellationToken = default);
}
