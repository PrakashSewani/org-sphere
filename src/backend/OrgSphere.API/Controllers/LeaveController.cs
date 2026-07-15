using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Enums;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController(ILeaveService leaveService) : ControllerBase
{
    private readonly ILeaveService _leaveService = leaveService;

    [HttpPost("requests")]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest([FromBody] CreateLeaveRequestRequest request)
    {
        var result = await _leaveService.CreateLeaveRequestAsync(request);
        return CreatedAtAction(nameof(GetLeaveRequestById), new { id = result.Id }, result);
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetAllLeaveRequests()
    {
        return Ok(await _leaveService.GetAllLeaveRequestsAsync());
    }

    [HttpGet("requests/{id:guid}")]
    public async Task<ActionResult<LeaveRequestDto>> GetLeaveRequestById(Guid id)
    {
        var result = await _leaveService.GetLeaveRequestByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("requests/employee/{employeeId:guid}")]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetLeaveRequestsByEmployee(Guid employeeId)
    {
        return Ok(await _leaveService.GetLeaveRequestsByEmployeeAsync(employeeId));
    }

    [HttpGet("requests/status/{status}")]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetLeaveRequestsByStatus(LeaveStatus status)
    {
        return Ok(await _leaveService.GetLeaveRequestsByStatusAsync(status));
    }

    [HttpPost("requests/{id:guid}/approve")]
    public async Task<ActionResult<LeaveRequestDto>> ApproveLeaveRequest(Guid id, [FromBody] ApproveLeaveRequestRequest request)
    {
        return Ok(await _leaveService.ApproveLeaveRequestAsync(id, request));
    }

    [HttpPost("requests/{id:guid}/reject")]
    public async Task<ActionResult<LeaveRequestDto>> RejectLeaveRequest(Guid id, [FromBody] RejectLeaveRequestRequest request)
    {
        return Ok(await _leaveService.RejectLeaveRequestAsync(id, request));
    }

    [HttpPost("requests/{id:guid}/cancel")]
    public async Task<ActionResult<LeaveRequestDto>> CancelLeaveRequest(Guid id)
    {
        return Ok(await _leaveService.CancelLeaveRequestAsync(id));
    }

    [HttpPost("policies")]
    public async Task<ActionResult<LeavePolicyDto>> CreateLeavePolicy([FromBody] CreateLeavePolicyRequest request)
    {
        var result = await _leaveService.CreateLeavePolicyAsync(request);
        return CreatedAtAction(nameof(GetLeavePolicyById), new { id = result.Id }, result);
    }

    [HttpGet("policies")]
    public async Task<ActionResult<IReadOnlyList<LeavePolicyDto>>> GetAllLeavePolicies()
    {
        return Ok(await _leaveService.GetAllLeavePoliciesAsync());
    }

    [HttpGet("policies/{id:guid}")]
    public async Task<ActionResult<LeavePolicyDto>> GetLeavePolicyById(Guid id)
    {
        var result = await _leaveService.GetLeavePolicyByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("policies/{id:guid}")]
    public async Task<ActionResult<LeavePolicyDto>> UpdateLeavePolicy(Guid id, [FromBody] UpdateLeavePolicyRequest request)
    {
        return Ok(await _leaveService.UpdateLeavePolicyAsync(id, request));
    }

    [HttpDelete("policies/{id:guid}")]
    public async Task<IActionResult> DeleteLeavePolicy(Guid id)
    {
        await _leaveService.DeleteLeavePolicyAsync(id);
        return NoContent();
    }

    [HttpGet("balances/employee/{employeeId:guid}/year/{year:int}")]
    public async Task<ActionResult<IReadOnlyList<LeaveBalanceDto>>> GetLeaveBalancesByEmployee(Guid employeeId, int year)
    {
        return Ok(await _leaveService.GetLeaveBalancesByEmployeeAsync(employeeId, year));
    }

    [HttpGet("balances/employee/{employeeId:guid}/type/{leaveType}/year/{year:int}")]
    public async Task<ActionResult<LeaveBalanceDto>> GetLeaveBalance(Guid employeeId, LeaveType leaveType, int year)
    {
        var result = await _leaveService.GetLeaveBalanceAsync(employeeId, leaveType, year);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("balances")]
    public async Task<ActionResult<LeaveBalanceDto>> InitializeLeaveBalance([FromBody] InitializeLeaveBalanceRequest request)
    {
        var result = await _leaveService.InitializeLeaveBalanceAsync(request);
        return Ok(result);
    }
}
