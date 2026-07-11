using DispatchR.Abstractions.Send;
using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Commands;

public record CreateGraphNodeCommand(NodeType Type, Dictionary<string, object> Properties) : IRequest<CreateGraphNodeCommand, ValueTask<GraphNodeDto>>;

public class CreateGraphNodeHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateGraphNodeCommand, ValueTask<GraphNodeDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async ValueTask<GraphNodeDto> Handle(CreateGraphNodeCommand request, CancellationToken cancellationToken)
    {
        var node = new GraphNode
        {
            Type = request.Type,
            Properties = request.Properties
        };

        var created = await _unitOfWork.GraphNodes.CreateAsync(node, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GraphNodeDto
        {
            Id = created.Id.Value,
            Type = created.Type,
            Properties = created.Properties,
            CreatedAt = created.CreatedAt
        };
    }
}
