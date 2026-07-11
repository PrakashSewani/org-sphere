using DispatchR;
using DispatchR.Abstractions.Send;
using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.Commands;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Validators;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequest request)
    {
        var command = new CreateTenantCommand(request.Name, request.Slug);
        var result = await _mediator.Send(command, CancellationToken.None);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
#pragma warning disable IDE0060 // Remove unused parameter
    public async Task<ActionResult<TenantDto>> GetById(Guid id)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        // TODO: Implement GetTenantByIdQuery
        return NotFound();
    }
}
