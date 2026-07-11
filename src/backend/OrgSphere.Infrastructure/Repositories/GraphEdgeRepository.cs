using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class GraphEdgeRepository(INeo4jContext context) : IGraphEdgeRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<GraphEdge?> GetByIdAsync(EdgeId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (e:GraphEdge {Id: $id, TenantId: $tenantId}) RETURN e",
            new { id = id.Value.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<GraphEdge>> GetAllAsync(TenantId tenantId, EdgeType? type = null, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var query = "MATCH (e:GraphEdge {TenantId: $tenantId})";
        var parameters = new Dictionary<string, object> { { "tenantId", tenantId.Value.ToString() } };

        if (type.HasValue)
        {
            query += " WHERE e.Type = $type";
            parameters.Add("type", type.Value.ToString());
        }

        query += " RETURN e";

        var result = await session.RunAsync(query, parameters);
        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(x => MapToEntity(x))];
    }

    public async Task<GraphEdge> CreateAsync(GraphEdge edge, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"MATCH (source:GraphNode {Id: $sourceId})
              MATCH (target:GraphNode {Id: $targetId})
              CREATE (e:GraphEdge {
                Id: $id,
                TenantId: $tenantId,
                Type: $type,
                SourceId: $sourceId,
                TargetId: $targetId,
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
              })
              CREATE (source)-[:EDGE]->(e)
              CREATE (e)-[:EDGE]->(target)
              RETURN e",
            new
            {
                id = edge.Id.Value.ToString(),
                tenantId = edge.TenantId.Value.ToString(),
                type = edge.Type.ToString(),
                sourceId = edge.SourceId.Value.ToString(),
                targetId = edge.TargetId.Value.ToString(),
                properties = System.Text.Json.JsonSerializer.Serialize(edge.Properties),
                createdAt = edge.CreatedAt.ToString("O"),
                updatedAt = edge.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(GraphEdge edge, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (e:GraphEdge {Id: $id, TenantId: $tenantId})
              SET e.Type = $type,
                  e.Properties = $properties,
                  e.UpdatedAt = $updatedAt",
            new
            {
                id = edge.Id.Value.ToString(),
                tenantId = edge.TenantId.Value.ToString(),
                type = edge.Type.ToString(),
                properties = System.Text.Json.JsonSerializer.Serialize(edge.Properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(EdgeId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (e:GraphEdge {Id: $id, TenantId: $tenantId}) DETACH DELETE e",
            new { id = id.Value.ToString(), tenantId = tenantId.Value.ToString() });
    }

    public async Task<IEnumerable<GraphEdge>> GetBySourceAsync(NodeId sourceId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (e:GraphEdge {SourceId: $sourceId, TenantId: $tenantId}) RETURN e",
            new { sourceId = sourceId.Value.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(x => MapToEntity(x))];
    }

    public async Task<IEnumerable<GraphEdge>> GetByTargetAsync(NodeId targetId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (e:GraphEdge {TargetId: $targetId, TenantId: $tenantId}) RETURN e",
            new { targetId = targetId.Value.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(x => MapToEntity(x))];
    }

    private static GraphEdge MapToEntity(IRecord record, string edgeKey = "e")
    {
        var node = record[edgeKey].As<INode>();
        var props = node.Properties;
        return new GraphEdge
        {
            Id = new EdgeId(Guid.Parse(props["Id"].As<string>())),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            Type = Enum.Parse<EdgeType>(props["Type"].As<string>()),
            SourceId = new NodeId(Guid.Parse(props["SourceId"].As<string>())),
            TargetId = new NodeId(Guid.Parse(props["TargetId"].As<string>())),
            Properties = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [],
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>()),
            UpdatedAt = DateTime.Parse(props["UpdatedAt"].As<string>())
        };
    }
}
