using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    private readonly IEmployeeService _employeeService = employeeService;

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeRequest request)
    {
        var result = await _employeeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll()
    {
        return Ok(await _employeeService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var result = await _employeeService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<EmployeeDto>> GetByEmail(string email)
    {
        var result = await _employeeService.GetByEmailAsync(email);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-department/{departmentId:guid}")]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetByDepartment(Guid departmentId)
    {
        return Ok(await _employeeService.GetByDepartmentAsync(departmentId));
    }

    [HttpGet("by-manager/{managerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetByManager(Guid managerId)
    {
        return Ok(await _employeeService.GetByManagerAsync(managerId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        return Ok(await _employeeService.UpdateAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _employeeService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/profile")]
    public async Task<ActionResult<EmployeeDto>> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request)
    {
        return Ok(await _employeeService.UpdateProfileAsync(id, request));
    }

    [HttpPatch("{id:guid}/preferences")]
    public async Task<ActionResult<EmployeeDto>> UpdatePreferences(Guid id, [FromBody] UpdatePreferencesRequest request)
    {
        return Ok(await _employeeService.UpdatePreferencesAsync(id, request));
    }

    [HttpPost("{id:guid}/documents")]
    public async Task<ActionResult<EmployeeDocumentDto>> UploadDocument(
        Guid id,
        IFormFile file,
        [FromQuery] string? description = null)
    {
        if (file.Length == 0)
            return BadRequest("No file uploaded");

        await using var stream = file.OpenReadStream();
        var result = await _employeeService.UploadDocumentAsync(id, stream, file.FileName, file.ContentType, description);
        return Ok(result);
    }

    [HttpGet("{id:guid}/documents")]
    public async Task<ActionResult<IReadOnlyList<EmployeeDocumentDto>>> GetDocuments(Guid id)
    {
        return Ok(await _employeeService.GetDocumentsAsync(id));
    }

    [HttpDelete("documents/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        await _employeeService.DeleteDocumentAsync(documentId);
        return NoContent();
    }
}
