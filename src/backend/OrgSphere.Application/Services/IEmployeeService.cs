using OrgSphere.Application.DTOs;

namespace OrgSphere.Application.Services;

public interface IEmployeeService
{
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EmployeeDto?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDto>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDto>> GetByManagerAsync(Guid managerId, CancellationToken ct = default);
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<EmployeeDto> UpdateProfileAsync(Guid employeeId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<EmployeeDto> UpdatePreferencesAsync(Guid employeeId, UpdatePreferencesRequest request, CancellationToken ct = default);
    Task<EmployeeDocumentDto> UploadDocumentAsync(Guid employeeId, Stream fileStream, string fileName, string contentType, string? description, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDocumentDto>> GetDocumentsAsync(Guid employeeId, CancellationToken ct = default);
    Task DeleteDocumentAsync(Guid documentId, CancellationToken ct = default);
}
