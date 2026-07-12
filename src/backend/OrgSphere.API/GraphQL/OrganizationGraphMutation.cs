using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Enums;

namespace OrgSphere.API.GraphQL;

public class OrganizationGraphMutation
{
    public async Task<CompanyDto> CreateCompanyAsync(
        CreateCompanyRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.CreateCompanyAsync(input, ct);
    }

    public async Task<CompanyDto> UpdateCompanyAsync(
        Guid id,
        UpdateCompanyRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.UpdateCompanyAsync(id, input, ct);
    }

    public async Task<bool> DeleteCompanyAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        await orgGraphService.DeleteCompanyAsync(id, ct);
        return true;
    }

    public async Task<RegionDto> CreateRegionAsync(
        CreateRegionRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.CreateRegionAsync(input, ct);
    }

    public async Task<RegionDto> UpdateRegionAsync(
        Guid id,
        UpdateRegionRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.UpdateRegionAsync(id, input, ct);
    }

    public async Task<bool> DeleteRegionAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        await orgGraphService.DeleteRegionAsync(id, ct);
        return true;
    }

    public async Task<OfficeDto> CreateOfficeAsync(
        CreateOfficeRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.CreateOfficeAsync(input, ct);
    }

    public async Task<OfficeDto> UpdateOfficeAsync(
        Guid id,
        UpdateOfficeRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.UpdateOfficeAsync(id, input, ct);
    }

    public async Task<bool> DeleteOfficeAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        await orgGraphService.DeleteOfficeAsync(id, ct);
        return true;
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(
        CreateDepartmentRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.CreateDepartmentAsync(input, ct);
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(
        Guid id,
        UpdateDepartmentRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.UpdateDepartmentAsync(id, input, ct);
    }

    public async Task<bool> DeleteDepartmentAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        await orgGraphService.DeleteDepartmentAsync(id, ct);
        return true;
    }

    public async Task<TeamDto> CreateTeamAsync(
        CreateTeamRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.CreateTeamAsync(input, ct);
    }

    public async Task<TeamDto> UpdateTeamAsync(
        Guid id,
        UpdateTeamRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.UpdateTeamAsync(id, input, ct);
    }

    public async Task<bool> DeleteTeamAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        await orgGraphService.DeleteTeamAsync(id, ct);
        return true;
    }

    public async Task<OrgEmployeeDto> CreateOrgEmployeeAsync(
        CreateOrgEmployeeRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.CreateEmployeeAsync(input, ct);
    }

    public async Task<OrgEmployeeDto> UpdateOrgEmployeeAsync(
        Guid id,
        UpdateOrgEmployeeRequest input,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.UpdateEmployeeAsync(id, input, ct);
    }

    public async Task<bool> DeleteOrgEmployeeAsync(
        Guid id,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        await orgGraphService.DeleteEmployeeAsync(id, ct);
        return true;
    }

    public async Task<GraphEdgeDto> CreateOrgEdgeAsync(
        EdgeType type,
        Guid sourceId,
        Guid targetId,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        return await orgGraphService.CreateEdgeAsync(type, sourceId, targetId, ct);
    }

    public async Task<bool> DeleteOrgEdgeAsync(
        Guid edgeId,
        IOrganizationGraphService orgGraphService,
        CancellationToken ct)
    {
        await orgGraphService.DeleteEdgeAsync(edgeId, ct);
        return true;
    }
}
