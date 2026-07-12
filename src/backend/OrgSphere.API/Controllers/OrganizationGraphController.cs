using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Enums;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationGraphController(IOrganizationGraphService orgGraphService) : ControllerBase
{
    private readonly IOrganizationGraphService _orgGraph = orgGraphService;

    [HttpPost("companies")]
    public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        var result = await _orgGraph.CreateCompanyAsync(request);
        return CreatedAtAction(nameof(GetCompany), new { id = result.Id }, result);
    }

    [HttpGet("companies")]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies()
    {
        return Ok(await _orgGraph.GetAllCompaniesAsync());
    }

    [HttpGet("companies/{id:guid}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid id)
    {
        var result = await _orgGraph.GetCompanyAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("companies/{id:guid}")]
    public async Task<ActionResult<CompanyDto>> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        return Ok(await _orgGraph.UpdateCompanyAsync(id, request));
    }

    [HttpDelete("companies/{id:guid}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        await _orgGraph.DeleteCompanyAsync(id);
        return NoContent();
    }

    [HttpPost("regions")]
    public async Task<ActionResult<RegionDto>> CreateRegion([FromBody] CreateRegionRequest request)
    {
        var result = await _orgGraph.CreateRegionAsync(request);
        return CreatedAtAction(nameof(GetRegion), new { id = result.Id }, result);
    }

    [HttpGet("regions")]
    public async Task<ActionResult<IReadOnlyList<RegionDto>>> GetRegions()
    {
        return Ok(await _orgGraph.GetAllRegionsAsync());
    }

    [HttpGet("regions/{id:guid}")]
    public async Task<ActionResult<RegionDto>> GetRegion(Guid id)
    {
        var result = await _orgGraph.GetRegionAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("regions/{id:guid}")]
    public async Task<ActionResult<RegionDto>> UpdateRegion(Guid id, [FromBody] UpdateRegionRequest request)
    {
        return Ok(await _orgGraph.UpdateRegionAsync(id, request));
    }

    [HttpDelete("regions/{id:guid}")]
    public async Task<IActionResult> DeleteRegion(Guid id)
    {
        await _orgGraph.DeleteRegionAsync(id);
        return NoContent();
    }

    [HttpPost("offices")]
    public async Task<ActionResult<OfficeDto>> CreateOffice([FromBody] CreateOfficeRequest request)
    {
        var result = await _orgGraph.CreateOfficeAsync(request);
        return CreatedAtAction(nameof(GetOffice), new { id = result.Id }, result);
    }

    [HttpGet("offices")]
    public async Task<ActionResult<IReadOnlyList<OfficeDto>>> GetOffices()
    {
        return Ok(await _orgGraph.GetAllOfficesAsync());
    }

    [HttpGet("offices/{id:guid}")]
    public async Task<ActionResult<OfficeDto>> GetOffice(Guid id)
    {
        var result = await _orgGraph.GetOfficeAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("offices/{id:guid}")]
    public async Task<ActionResult<OfficeDto>> UpdateOffice(Guid id, [FromBody] UpdateOfficeRequest request)
    {
        return Ok(await _orgGraph.UpdateOfficeAsync(id, request));
    }

    [HttpDelete("offices/{id:guid}")]
    public async Task<IActionResult> DeleteOffice(Guid id)
    {
        await _orgGraph.DeleteOfficeAsync(id);
        return NoContent();
    }

    [HttpPost("departments")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var result = await _orgGraph.CreateDepartmentAsync(request);
        return CreatedAtAction(nameof(GetDepartment), new { id = result.Id }, result);
    }

    [HttpGet("departments")]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetDepartments()
    {
        return Ok(await _orgGraph.GetAllDepartmentsAsync());
    }

    [HttpGet("departments/{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(Guid id)
    {
        var result = await _orgGraph.GetDepartmentAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("departments/{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequest request)
    {
        return Ok(await _orgGraph.UpdateDepartmentAsync(id, request));
    }

    [HttpDelete("departments/{id:guid}")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        await _orgGraph.DeleteDepartmentAsync(id);
        return NoContent();
    }

    [HttpPost("teams")]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var result = await _orgGraph.CreateTeamAsync(request);
        return CreatedAtAction(nameof(GetTeam), new { id = result.Id }, result);
    }

    [HttpGet("teams")]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetTeams()
    {
        return Ok(await _orgGraph.GetAllTeamsAsync());
    }

    [HttpGet("teams/{id:guid}")]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid id)
    {
        var result = await _orgGraph.GetTeamAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("teams/{id:guid}")]
    public async Task<ActionResult<TeamDto>> UpdateTeam(Guid id, [FromBody] UpdateTeamRequest request)
    {
        return Ok(await _orgGraph.UpdateTeamAsync(id, request));
    }

    [HttpDelete("teams/{id:guid}")]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        await _orgGraph.DeleteTeamAsync(id);
        return NoContent();
    }

    [HttpPost("employees")]
    public async Task<ActionResult<OrgEmployeeDto>> CreateEmployee([FromBody] CreateOrgEmployeeRequest request)
    {
        var result = await _orgGraph.CreateEmployeeAsync(request);
        return CreatedAtAction(nameof(GetEmployee), new { id = result.Id }, result);
    }

    [HttpGet("employees")]
    public async Task<ActionResult<IReadOnlyList<OrgEmployeeDto>>> GetEmployees()
    {
        return Ok(await _orgGraph.GetAllEmployeesAsync());
    }

    [HttpGet("employees/{id:guid}")]
    public async Task<ActionResult<OrgEmployeeDto>> GetEmployee(Guid id)
    {
        var result = await _orgGraph.GetEmployeeAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("employees/{id:guid}")]
    public async Task<ActionResult<OrgEmployeeDto>> UpdateEmployee(Guid id, [FromBody] UpdateOrgEmployeeRequest request)
    {
        return Ok(await _orgGraph.UpdateEmployeeAsync(id, request));
    }

    [HttpDelete("employees/{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        await _orgGraph.DeleteEmployeeAsync(id);
        return NoContent();
    }

    [HttpGet("nodes/{nodeId:guid}/edges")]
    public async Task<ActionResult<IReadOnlyList<GraphEdgeDto>>> GetEdges(Guid nodeId)
    {
        return Ok(await _orgGraph.GetEdgesAsync(nodeId));
    }

    [HttpPost("edges")]
    public async Task<ActionResult<GraphEdgeDto>> CreateEdge([FromBody] CreateEdgeRequest request)
    {
        var result = await _orgGraph.CreateEdgeAsync(request.Type, request.SourceId, request.TargetId);
        return Ok(result);
    }

    [HttpDelete("edges/{edgeId:guid}")]
    public async Task<IActionResult> DeleteEdge(Guid edgeId)
    {
        await _orgGraph.DeleteEdgeAsync(edgeId);
        return NoContent();
    }
}

public record CreateEdgeRequest(EdgeType Type, Guid SourceId, Guid TargetId);
