using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Seed.Seeds;

public class GraphSeed(INeo4jContext context, ILogger<GraphSeed> logger)
{
    public async Task SeedAsync(TenantId tenantId, CancellationToken ct = default)
    {
        await using var session = context.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (n:GraphNode {TenantId: $tenantId, Type: 'Company'})
            RETURN n.Id AS Id LIMIT 1
            """,
            new Dictionary<string, object?> { ["tenantId"] = tenantId.ToString() });

        var records = await cursor.ToListAsync(ct);
        if (records.Count > 0)
        {
            logger.LogInformation("Graph seed data already exists, skipping");
            return;
        }

        var companyId = NodeId.New();
        var engineeringId = NodeId.New();
        var productId = NodeId.New();
        var hrId = NodeId.New();
        var backendTeamId = NodeId.New();
        var frontendTeamId = NodeId.New();
        var productTeamId = NodeId.New();
        var hrTeamId = NodeId.New();

        var aliceId = NodeId.New();
        var bobId = NodeId.New();
        var carolId = NodeId.New();
        var daveId = NodeId.New();
        var eveId = NodeId.New();
        var frankId = NodeId.New();
        var graceId = NodeId.New();
        var heidiId = NodeId.New();
        var ivanId = NodeId.New();

        await CreateNode(session, companyId, NodeType.Company, tenantId,
            new Dictionary<string, object> { { "Name", "Acme Corp" } });

        await CreateNode(session, engineeringId, NodeType.Department, tenantId,
            new Dictionary<string, object> { { "Name", "Engineering" } });
        await CreateNode(session, productId, NodeType.Department, tenantId,
            new Dictionary<string, object> { { "Name", "Product" } });
        await CreateNode(session, hrId, NodeType.Department, tenantId,
            new Dictionary<string, object> { { "Name", "HR" } });

        await CreateNode(session, backendTeamId, NodeType.Team, tenantId,
            new Dictionary<string, object> { { "Name", "Backend Team" } });
        await CreateNode(session, frontendTeamId, NodeType.Team, tenantId,
            new Dictionary<string, object> { { "Name", "Frontend Team" } });
        await CreateNode(session, productTeamId, NodeType.Team, tenantId,
            new Dictionary<string, object> { { "Name", "Product Team" } });
        await CreateNode(session, hrTeamId, NodeType.Team, tenantId,
            new Dictionary<string, object> { { "Name", "HR Team" } });

        await CreateNode(session, aliceId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Alice Admin" }, { "Email", "admin@acme.com" } });
        await CreateNode(session, bobId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Bob Smith" }, { "Email", "bob@acme.com" } });
        await CreateNode(session, carolId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Carol Jones" }, { "Email", "carol@acme.com" } });
        await CreateNode(session, daveId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Dave Wilson" }, { "Email", "dave@acme.com" } });
        await CreateNode(session, eveId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Eve Brown" }, { "Email", "eve@acme.com" } });
        await CreateNode(session, frankId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Frank Davis" }, { "Email", "frank@acme.com" } });
        await CreateNode(session, graceId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Grace Miller" }, { "Email", "grace@acme.com" } });
        await CreateNode(session, heidiId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Heidi Taylor" }, { "Email", "heidi@acme.com" } });
        await CreateNode(session, ivanId, NodeType.Employee, tenantId,
            new Dictionary<string, object> { { "Name", "Ivan Anderson" }, { "Email", "ivan@acme.com" } });

        await CreateEdge(session, EdgeId.New(), EdgeType.Manages, companyId, engineeringId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.Manages, companyId, productId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.Manages, companyId, hrId, tenantId);

        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, aliceId, backendTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, bobId, backendTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, carolId, backendTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, daveId, frontendTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, eveId, frontendTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, frankId, productTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, graceId, productTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, heidiId, hrTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.MemberOf, ivanId, hrTeamId, tenantId);

        await CreateEdge(session, EdgeId.New(), EdgeType.ReportsTo, bobId, aliceId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.ReportsTo, carolId, aliceId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.ReportsTo, eveId, daveId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.ReportsTo, graceId, frankId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.ReportsTo, ivanId, heidiId, tenantId);

        await CreateEdge(session, EdgeId.New(), EdgeType.Manages, aliceId, backendTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.Manages, daveId, frontendTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.Manages, frankId, productTeamId, tenantId);
        await CreateEdge(session, EdgeId.New(), EdgeType.Manages, heidiId, hrTeamId, tenantId);

        var tenantIdValue = tenantId.ToString();
        logger.LogInformation("Seeded organization graph for tenant {TenantId}", tenantIdValue);
    }

    private static async Task CreateNode(IAsyncSession session, NodeId id, NodeType type, TenantId tenantId,
        Dictionary<string, object> properties)
    {
        var propsJson = System.Text.Json.JsonSerializer.Serialize(properties);
        await session.RunAsync(
            """
            CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: $type,
                Properties: $properties,
                CreatedAt: datetime(),
                UpdatedAt: datetime()
            })
            """,
            new Dictionary<string, object?>
            {
                ["id"] = id.ToString(),
                ["tenantId"] = tenantId.ToString(),
                ["type"] = type.ToString(),
                ["properties"] = propsJson
            });
    }

    private static async Task CreateEdge(IAsyncSession session, EdgeId id, EdgeType type,
        NodeId sourceId, NodeId targetId, TenantId tenantId)
    {
        await session.RunAsync(
            """
            MATCH (s:GraphNode {Id: $sourceId, TenantId: $tenantId})
            MATCH (t:GraphNode {Id: $targetId, TenantId: $tenantId})
            CREATE (e:GraphEdge {
                Id: $id,
                TenantId: $tenantId,
                Type: $type,
                SourceId: $sourceId,
                TargetId: $targetId,
                Properties: '{}',
                CreatedAt: datetime(),
                UpdatedAt: datetime()
            })
            CREATE (e)-[:EDGE]->(s)
            CREATE (e)-[:EDGE]->(t)
            """,
            new Dictionary<string, object?>
            {
                ["id"] = id.ToString(),
                ["tenantId"] = tenantId.ToString(),
                ["type"] = type.ToString(),
                ["sourceId"] = sourceId.ToString(),
                ["targetId"] = targetId.ToString()
            });
    }
}
