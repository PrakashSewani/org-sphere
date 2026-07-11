using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class GraphNodeRepository(INeo4jContext context) : IGraphNodeRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<GraphNode?> GetByIdAsync(NodeId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId}) RETURN n",
            new { id = id.Value.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<GraphNode>> GetAllAsync(TenantId tenantId, NodeType? type = null, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var query = "MATCH (n:GraphNode {TenantId: $tenantId})";
        var parameters = new Dictionary<string, object> { { "tenantId", tenantId.Value.ToString() } };

        if (type.HasValue)
        {
            query += " WHERE n.Type = $type";
            parameters.Add("type", type.Value.ToString());
        }

        query += " RETURN n";

        var result = await session.RunAsync(query, parameters);
        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(x => MapToEntity(x))];
    }

    public async Task<GraphNode> CreateAsync(GraphNode node, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: $type,
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = node.Id.Value.ToString(),
                tenantId = node.TenantId.Value.ToString(),
                type = node.Type.ToString(),
                properties = System.Text.Json.JsonSerializer.Serialize(node.Properties),
                createdAt = node.CreatedAt.ToString("O"),
                updatedAt = node.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(GraphNode node, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId})
              SET n.Type = $type,
                  n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = node.Id.Value.ToString(),
                tenantId = node.TenantId.Value.ToString(),
                type = node.Type.ToString(),
                properties = System.Text.Json.JsonSerializer.Serialize(node.Properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(NodeId id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId}) DETACH DELETE n",
            new { id = id.Value.ToString(), tenantId = tenantId.Value.ToString() });
    }

    public async Task<IEnumerable<GraphNode>> TraverseAsync(NodeId startNodeId, EdgeType edgeType, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"MATCH (start:GraphNode {Id: $startNodeId})-[:CONNECTS_TO]->(target:GraphNode)
              RETURN target",
            new { startNodeId = startNodeId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(r => MapToEntity(r, "target"))];
    }

    private static GraphNode MapToEntity(IRecord record, string nodeKey = "n")
    {
        var node = record[nodeKey].As<INode>();
        var props = node.Properties;
        return new GraphNode
        {
            Id = new NodeId(Guid.Parse(props["Id"].As<string>())),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            Type = Enum.Parse<NodeType>(props["Type"].As<string>()),
            Properties = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [],
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>()),
            UpdatedAt = DateTime.Parse(props["UpdatedAt"].As<string>())
        };
    }
}
