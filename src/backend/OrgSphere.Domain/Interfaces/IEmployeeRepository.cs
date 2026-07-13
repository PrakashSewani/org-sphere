using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<Employee?> GetByEmailAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByManagerAsync(Guid managerId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);
}
