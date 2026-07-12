using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public interface IGraphService
{
    Task<GraphNodeDto> CreateNodeAsync(NodeType type, Dictionary<string, object> properties, CancellationToken ct = default);
    Task<GraphEdgeDto> CreateEdgeAsync(EdgeType type, NodeId sourceId, NodeId targetId, CancellationToken ct = default);
    Task<GraphNodeDto?> GetNodeAsync(NodeId id, CancellationToken ct = default);
    Task<IReadOnlyList<GraphNodeDto>> GetAllNodesAsync(NodeType? type = null, CancellationToken ct = default);
    Task<IReadOnlyList<GraphEdgeDto>> GetEdgesBySourceAsync(NodeId sourceId, CancellationToken ct = default);
    Task<IReadOnlyList<GraphEdgeDto>> GetEdgesByTargetAsync(NodeId targetId, CancellationToken ct = default);
    Task<IReadOnlyList<GraphNodeDto>> TraverseAsync(NodeId startNodeId, EdgeType edgeType, CancellationToken ct = default);
    Task DeleteNodeAsync(NodeId id, CancellationToken ct = default);
    Task DeleteEdgeAsync(EdgeId id, CancellationToken ct = default);
}
