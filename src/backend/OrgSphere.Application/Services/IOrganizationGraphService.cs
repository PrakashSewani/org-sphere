using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.Services;

public interface IOrganizationGraphService
{
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request, CancellationToken ct = default);
    Task<CompanyDto?> GetCompanyAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CompanyDto>> GetAllCompaniesAsync(CancellationToken ct = default);
    Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request, CancellationToken ct = default);
    Task DeleteCompanyAsync(Guid id, CancellationToken ct = default);

    Task<RegionDto> CreateRegionAsync(CreateRegionRequest request, CancellationToken ct = default);
    Task<RegionDto?> GetRegionAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RegionDto>> GetAllRegionsAsync(CancellationToken ct = default);
    Task<RegionDto> UpdateRegionAsync(Guid id, UpdateRegionRequest request, CancellationToken ct = default);
    Task DeleteRegionAsync(Guid id, CancellationToken ct = default);

    Task<OfficeDto> CreateOfficeAsync(CreateOfficeRequest request, CancellationToken ct = default);
    Task<OfficeDto?> GetOfficeAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OfficeDto>> GetAllOfficesAsync(CancellationToken ct = default);
    Task<OfficeDto> UpdateOfficeAsync(Guid id, UpdateOfficeRequest request, CancellationToken ct = default);
    Task DeleteOfficeAsync(Guid id, CancellationToken ct = default);

    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken ct = default);
    Task<DepartmentDto?> GetDepartmentAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken ct = default);
    Task<DepartmentDto> UpdateDepartmentAsync(Guid id, UpdateDepartmentRequest request, CancellationToken ct = default);
    Task DeleteDepartmentAsync(Guid id, CancellationToken ct = default);

    Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, CancellationToken ct = default);
    Task<TeamDto?> GetTeamAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TeamDto>> GetAllTeamsAsync(CancellationToken ct = default);
    Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamRequest request, CancellationToken ct = default);
    Task DeleteTeamAsync(Guid id, CancellationToken ct = default);

    Task<OrgEmployeeDto> CreateEmployeeAsync(CreateOrgEmployeeRequest request, CancellationToken ct = default);
    Task<OrgEmployeeDto?> GetEmployeeAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OrgEmployeeDto>> GetAllEmployeesAsync(CancellationToken ct = default);
    Task<OrgEmployeeDto> UpdateEmployeeAsync(Guid id, UpdateOrgEmployeeRequest request, CancellationToken ct = default);
    Task DeleteEmployeeAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<GraphEdgeDto>> GetEdgesAsync(Guid nodeId, CancellationToken ct = default);
    Task<GraphEdgeDto> CreateEdgeAsync(EdgeType type, Guid sourceId, Guid targetId, CancellationToken ct = default);
    Task DeleteEdgeAsync(Guid edgeId, CancellationToken ct = default);
}
