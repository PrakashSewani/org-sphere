using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(IAttendanceService attendanceService) : ControllerBase
{
    private readonly IAttendanceService _attendanceService = attendanceService;

    [HttpPost("check-in")]
    public async Task<ActionResult<AttendanceRecordDto>> CheckIn([FromBody] CheckInRequest request)
    {
        var result = await _attendanceService.CheckInAsync(request);
        return Ok(result);
    }

    [HttpPost("check-out")]
    public async Task<ActionResult<AttendanceRecordDto>> CheckOut([FromBody] CheckOutRequest request)
    {
        var result = await _attendanceService.CheckOutAsync(request);
        return Ok(result);
    }

    [HttpGet("records")]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetAllRecords()
    {
        return Ok(await _attendanceService.GetAllAttendanceRecordsAsync());
    }

    [HttpGet("records/{id:guid}")]
    public async Task<ActionResult<AttendanceRecordDto>> GetRecordById(Guid id)
    {
        var result = await _attendanceService.GetAttendanceRecordByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("records/employee/{employeeId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetRecordsByEmployee(Guid employeeId)
    {
        return Ok(await _attendanceService.GetAttendanceRecordsByEmployeeAsync(employeeId));
    }

    [HttpGet("records/employee/{employeeId:guid}/date/{date:datetime}")]
    public async Task<ActionResult<AttendanceRecordDto>> GetRecordByEmployeeAndDate(Guid employeeId, DateTime date)
    {
        var result = await _attendanceService.GetAttendanceRecordByEmployeeAndDateAsync(employeeId, date);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("records/date-range")]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> GetRecordsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return Ok(await _attendanceService.GetAttendanceRecordsByDateRangeAsync(startDate, endDate));
    }

    [HttpGet("summary/employee/{employeeId:guid}/year/{year:int}/month/{month:int}")]
    public async Task<ActionResult<AttendanceSummaryDto>> GetAttendanceSummary(Guid employeeId, int year, int month)
    {
        return Ok(await _attendanceService.GetAttendanceSummaryAsync(employeeId, year, month));
    }

    [HttpPost("policies")]
    public async Task<ActionResult<AttendancePolicyDto>> CreatePolicy([FromBody] CreateAttendancePolicyRequest request)
    {
        var result = await _attendanceService.CreateAttendancePolicyAsync(request);
        return CreatedAtAction(nameof(GetPolicyById), new { id = result.Id }, result);
    }

    [HttpGet("policies")]
    public async Task<ActionResult<IReadOnlyList<AttendancePolicyDto>>> GetAllPolicies()
    {
        return Ok(await _attendanceService.GetAllAttendancePoliciesAsync());
    }

    [HttpGet("policies/{id:guid}")]
    public async Task<ActionResult<AttendancePolicyDto>> GetPolicyById(Guid id)
    {
        var result = await _attendanceService.GetAttendancePolicyByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("policies/active")]
    public async Task<ActionResult<AttendancePolicyDto>> GetActivePolicy()
    {
        var result = await _attendanceService.GetActiveAttendancePolicyAsync();
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("policies/{id:guid}")]
    public async Task<ActionResult<AttendancePolicyDto>> UpdatePolicy(Guid id, [FromBody] UpdateAttendancePolicyRequest request)
    {
        return Ok(await _attendanceService.UpdateAttendancePolicyAsync(id, request));
    }

    [HttpDelete("policies/{id:guid}")]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        await _attendanceService.DeleteAttendancePolicyAsync(id);
        return NoContent();
    }
}
