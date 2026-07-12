using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Enums;

namespace OrgSphere.API.GraphQL;

public class OrganizationGraphQuery
{
    public async Task<CompanyDto?> GetCompanyAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetCompanyAsync(id, ct);
    }

    public async Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync(
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetAllCompaniesAsync(ct);
    }

    public async Task<RegionDto?> GetRegionAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetRegionAsync(id, ct);
    }

    public async Task<IReadOnlyList<RegionDto>> GetRegionsAsync(
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetAllRegionsAsync(ct);
    }

    public async Task<OfficeDto?> GetOfficeAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetOfficeAsync(id, ct);
    }

    public async Task<IReadOnlyList<OfficeDto>> GetOfficesAsync(
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetAllOfficesAsync(ct);
    }

    public async Task<DepartmentDto?> GetDepartmentAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetDepartmentAsync(id, ct);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetAllDepartmentsAsync(ct);
    }

    public async Task<TeamDto?> GetTeamAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetTeamAsync(id, ct);
    }

    public async Task<IReadOnlyList<TeamDto>> GetTeamsAsync(
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetAllTeamsAsync(ct);
    }

    public async Task<OrgEmployeeDto?> GetOrgEmployeeAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetEmployeeAsync(id, ct);
    }

    public async Task<IReadOnlyList<OrgEmployeeDto>> GetOrgEmployeesAsync(
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetAllEmployeesAsync(ct);
    }

    public async Task<IReadOnlyList<GraphEdgeDto>> GetNodeEdgesAsync(
        Guid nodeId,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.GetEdgesAsync(nodeId, ct);
    }
}
