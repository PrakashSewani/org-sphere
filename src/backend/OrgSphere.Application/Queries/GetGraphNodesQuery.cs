using DispatchR.Abstractions.Send;
using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Queries;

public record GetGraphNodeByIdQuery(Guid NodeId) : IRequest<GetGraphNodeByIdQuery, ValueTask<GraphNodeDto?>>;

public class GetGraphNodeByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetGraphNodeByIdQuery, ValueTask<GraphNodeDto?>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async ValueTask<GraphNodeDto?> Handle(GetGraphNodeByIdQuery request, CancellationToken cancellationToken)
    {
        var nodeId = new NodeId(request.NodeId);
        var node = await _unitOfWork.GraphNodes.GetByIdAsync(nodeId, new TenantId(Guid.Empty), cancellationToken);

        if (node is null)
            return null;

        return new GraphNodeDto
        {
            Id = node.Id.Value,
            Type = node.Type,
            Properties = node.Properties,
            CreatedAt = node.CreatedAt
        };
    }
}

public record GetAllGraphNodesQuery(NodeType? Type = null) : IRequest<GetAllGraphNodesQuery, ValueTask<IReadOnlyList<GraphNodeDto>>>;

public class GetAllGraphNodesHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllGraphNodesQuery, ValueTask<IReadOnlyList<GraphNodeDto>>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async ValueTask<IReadOnlyList<GraphNodeDto>> Handle(GetAllGraphNodesQuery request, CancellationToken cancellationToken)
    {
        var nodes = await _unitOfWork.GraphNodes.GetAllAsync(new TenantId(Guid.Empty), request.Type, cancellationToken);

        return nodes.Select(n => new GraphNodeDto
        {
            Id = n.Id.Value,
            Type = n.Type,
            Properties = n.Properties,
            CreatedAt = n.CreatedAt
        }).ToList().AsReadOnly();
    }
}
