using DispatchR;
using DispatchR.Abstractions.Send;
using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.Commands;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Queries;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using NodeType = OrgSphere.Domain.Enums.NodeType;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphNodesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GraphNodeDto>>> GetAll([FromQuery] NodeType? type)
    {
        var query = new GetAllGraphNodesQuery(type);
        var result = await _mediator.Send(query, CancellationToken.None);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GraphNodeDto>> GetById(Guid id)
    {
        var query = new GetGraphNodeByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken.None);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GraphNodeDto>> Create([FromBody] CreateGraphNodeRequest request)
    {
        var command = new CreateGraphNodeCommand(request.Type, request.Properties);
        var result = await _mediator.Send(command, CancellationToken.None);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}

public record CreateGraphNodeRequest(NodeType Type, Dictionary<string, object> Properties);
