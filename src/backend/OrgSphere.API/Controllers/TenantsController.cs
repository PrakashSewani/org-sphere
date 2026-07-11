using DispatchR;
using DispatchR.Abstractions.Send;
using OrgSphere.Application.Commands;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Validators;
using Microsoft.AspNetCore.Mvc;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequest request)
    {
        var command = new CreateTenantCommand(request.Name, request.Slug);
        var result = await _mediator.Send(command, CancellationToken.None);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id)
    {
        // TODO: Implement GetTenantByIdQuery
        return NotFound();
    }
}
