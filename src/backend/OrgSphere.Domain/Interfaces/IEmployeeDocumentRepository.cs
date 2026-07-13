using OrgSphere.Domain.Entities;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IEmployeeDocumentRepository
{
    Task<EmployeeDocument?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<EmployeeDocument> CreateAsync(EmployeeDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
