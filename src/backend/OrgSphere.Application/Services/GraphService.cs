using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public class GraphService(
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IEventBus eventBus) : IGraphService
{
    public async Task<GraphNodeDto> CreateNodeAsync(NodeType type, Dictionary<string, object> properties, CancellationToken ct = default)
    {
        var node = new GraphNode
        {
            TenantId = tenantContext.TenantId!,
            Type = type,
            Properties = properties
        };

        var created = await unitOfWork.GraphNodes.CreateAsync(node, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new NodeCreatedEvent(tenantContext.TenantId!, created.Id, type), ct);

        return MapToDto(created);
    }

    public async Task<GraphEdgeDto> CreateEdgeAsync(EdgeType type, NodeId sourceId, NodeId targetId, CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId!;

        _ = await unitOfWork.GraphNodes.GetByIdAsync(sourceId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Source node {sourceId} not found");

        _ = await unitOfWork.GraphNodes.GetByIdAsync(targetId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Target node {targetId} not found");

        var edge = new GraphEdge
        {
            TenantId = tenantId,
            Type = type,
            SourceId = sourceId,
            TargetId = targetId,
            Properties = []
        };

        var created = await unitOfWork.GraphEdges.CreateAsync(edge, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new EdgeCreatedEvent(tenantId, created.Id, type, sourceId, targetId), ct);

        return MapToDto(created);
    }

    public async Task<GraphNodeDto?> GetNodeAsync(NodeId id, CancellationToken ct = default)
    {
        var node = await unitOfWork.GraphNodes.GetByIdAsync(id, tenantContext.TenantId!, ct);
        return node is null ? null : MapToDto(node);
    }

    public async Task<IReadOnlyList<GraphNodeDto>> GetAllNodesAsync(NodeType? type = null, CancellationToken ct = default)
    {
        var nodes = await unitOfWork.GraphNodes.GetAllAsync(tenantContext.TenantId!, type, ct);
        return [.. nodes.Select(MapToDto)];
    }

    public async Task<IReadOnlyList<GraphEdgeDto>> GetEdgesBySourceAsync(NodeId sourceId, CancellationToken ct = default)
    {
        var edges = await unitOfWork.GraphEdges.GetBySourceAsync(sourceId, tenantContext.TenantId!, ct);
        return [.. edges.Select(MapToDto)];
    }

    public async Task<IReadOnlyList<GraphEdgeDto>> GetEdgesByTargetAsync(NodeId targetId, CancellationToken ct = default)
    {
        var edges = await unitOfWork.GraphEdges.GetByTargetAsync(targetId, tenantContext.TenantId!, ct);
        return [.. edges.Select(MapToDto)];
    }

    public async Task<IReadOnlyList<GraphNodeDto>> TraverseAsync(NodeId startNodeId, EdgeType edgeType, CancellationToken ct = default)
    {
        var nodes = await unitOfWork.GraphNodes.TraverseAsync(startNodeId, edgeType, ct);
        return [.. nodes.Select(MapToDto)];
    }

    public async Task DeleteNodeAsync(NodeId id, CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId!;

        _ = await unitOfWork.GraphNodes.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Node {id} not found");

        await unitOfWork.GraphNodes.DeleteAsync(id, tenantId, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new NodeDeletedEvent(tenantId, id), ct);
    }

    public async Task DeleteEdgeAsync(EdgeId id, CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId!;

        _ = await unitOfWork.GraphEdges.GetByIdAsync(id, tenantId, ct)
            ?? throw new KeyNotFoundException($"Edge {id} not found");

        await unitOfWork.GraphEdges.DeleteAsync(id, tenantId, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new EdgeDeletedEvent(tenantId, id), ct);
    }

    private static GraphNodeDto MapToDto(GraphNode node) => new()
    {
        Id = node.Id.Value,
        Type = node.Type,
        Properties = node.Properties,
        CreatedAt = node.CreatedAt
    };

    private static GraphEdgeDto MapToDto(GraphEdge edge) => new()
    {
        Id = edge.Id.Value,
        Type = edge.Type,
        SourceId = edge.SourceId.Value,
        TargetId = edge.TargetId.Value,
        Properties = edge.Properties,
        CreatedAt = edge.CreatedAt
    };
}
