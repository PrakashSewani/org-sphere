using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Enums;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalController(IApprovalService approvalService) : ControllerBase
{
    private readonly IApprovalService _approvalService = approvalService;

    [HttpPost("requests")]
    public async Task<ActionResult<ApprovalRequestDto>> CreateApprovalRequest([FromBody] CreateApprovalRequestRequest request)
    {
        var result = await _approvalService.CreateApprovalRequestAsync(request);
        return CreatedAtAction(nameof(GetApprovalRequestById), new { id = result.Id }, result);
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRequestDto>>> GetAllApprovalRequests()
    {
        return Ok(await _approvalService.GetAllApprovalRequestsAsync());
    }

    [HttpGet("requests/{id:guid}")]
    public async Task<ActionResult<ApprovalRequestDto>> GetApprovalRequestById(Guid id)
    {
        var result = await _approvalService.GetApprovalRequestByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("requests/requester/{requesterId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRequestDto>>> GetApprovalRequestsByRequester(Guid requesterId)
    {
        return Ok(await _approvalService.GetApprovalRequestsByRequesterAsync(requesterId));
    }

    [HttpGet("requests/status/{status}")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRequestDto>>> GetApprovalRequestsByStatus(ApprovalStatus status)
    {
        return Ok(await _approvalService.GetApprovalRequestsByStatusAsync(status));
    }

    [HttpGet("pending/{approverId:guid}")]
    public async Task<ActionResult<PendingApprovalsDto>> GetPendingApprovals(Guid approverId)
    {
        return Ok(await _approvalService.GetPendingApprovalsForApproverAsync(approverId));
    }

    [HttpPost("requests/{id:guid}/approve")]
    public async Task<ActionResult<ApprovalRequestDto>> ApproveStep(Guid id, [FromBody] ApproveStepRequest request)
    {
        return Ok(await _approvalService.ApproveStepAsync(id, request));
    }

    [HttpPost("requests/{id:guid}/reject")]
    public async Task<ActionResult<ApprovalRequestDto>> RejectStep(Guid id, [FromBody] RejectStepRequest request)
    {
        return Ok(await _approvalService.RejectStepAsync(id, request));
    }

    [HttpPost("requests/{id:guid}/escalate")]
    public async Task<ActionResult<ApprovalRequestDto>> EscalateStep(Guid id, [FromBody] EscalateStepRequest request)
    {
        return Ok(await _approvalService.EscalateStepAsync(id, request));
    }

    [HttpPost("chains")]
    public async Task<ActionResult<ApprovalChainDto>> CreateApprovalChain([FromBody] CreateApprovalChainRequest request)
    {
        var result = await _approvalService.CreateApprovalChainAsync(request);
        return CreatedAtAction(nameof(GetApprovalChainById), new { id = result.Id }, result);
    }

    [HttpGet("chains")]
    public async Task<ActionResult<IReadOnlyList<ApprovalChainDto>>> GetAllApprovalChains()
    {
        return Ok(await _approvalService.GetAllApprovalChainsAsync());
    }

    [HttpGet("chains/{id:guid}")]
    public async Task<ActionResult<ApprovalChainDto>> GetApprovalChainById(Guid id)
    {
        var result = await _approvalService.GetApprovalChainByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("chains/type/{type}")]
    public async Task<ActionResult<ApprovalChainDto>> GetApprovalChainByType(ApprovalType type)
    {
        var result = await _approvalService.GetApprovalChainByTypeAsync(type);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("chains/{id:guid}")]
    public async Task<ActionResult<ApprovalChainDto>> UpdateApprovalChain(Guid id, [FromBody] UpdateApprovalChainRequest request)
    {
        return Ok(await _approvalService.UpdateApprovalChainAsync(id, request));
    }

    [HttpDelete("chains/{id:guid}")]
    public async Task<IActionResult> DeleteApprovalChain(Guid id)
    {
        await _approvalService.DeleteApprovalChainAsync(id);
        return NoContent();
    }

    [HttpPost("delegations")]
    public async Task<ActionResult<ApprovalDelegationDto>> CreateDelegation([FromBody] CreateDelegationRequest request)
    {
        var result = await _approvalService.CreateDelegationAsync(request);
        return CreatedAtAction(nameof(GetDelegationById), new { id = result.Id }, result);
    }

    [HttpGet("delegations/{id:guid}")]
    public async Task<ActionResult<ApprovalDelegationDto>> GetDelegationById(Guid id)
    {
        var result = await _approvalService.GetDelegationByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("delegations/delegator/{delegatorId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ApprovalDelegationDto>>> GetDelegationsByDelegator(Guid delegatorId)
    {
        return Ok(await _approvalService.GetDelegationsByDelegatorAsync(delegatorId));
    }

    [HttpDelete("delegations/{id:guid}")]
    public async Task<IActionResult> DeleteDelegation(Guid id)
    {
        await _approvalService.DeleteDelegationAsync(id);
        return NoContent();
    }
}
