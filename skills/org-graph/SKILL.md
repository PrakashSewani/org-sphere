# Organization Graph Skill

Load this skill when working with the Organization Graph, creating entities, relationships, or graph queries.

---

## When to Use

- Creating new entity types (nodes)
- Creating new relationship types (edges)
- Writing graph traversal queries
- Modifying graph structure
- Working with graph analytics
- Debugging graph issues

---

## Tech Stack

- **Graph DB**: Neo4j 5.x with Cypher query language
- **Driver**: `Neo4j.Driver` NuGet package
- **Repository Pattern**: `GraphNodeRepository`, `GraphEdgeRepository`
- **Events**: Domain events published via `IEventBus`

---

## Graph Concepts

### Nodes

Every entity in the system is a node:

- Company
- Region
- Office
- Department
- Team
- Employee
- Project
- Workflow
- Approval

### Edges

Every relationship is an edge:

- REPORTS_TO
- MEMBER_OF
- MANAGES
- CONTAINS
- WORKS_ON
- APPROVES
- LOCATED_AT
- HAS_SKILL

---

## Creating Nodes

### Using the Service Layer

```csharp
// Via OrganizationGraphService (preferred)
public async Task<CompanyDto> CreateCompanyAsync(
    CreateCompanyRequest request,
    CancellationToken ct = default)
{
    var props = new Dictionary<string, object>
    {
        ["Name"] = request.Name,
        ["Industry"] = request.Industry,
        ["EmployeeCount"] = request.EmployeeCount
    };
    var node = await CreateTypedNodeAsync(NodeType.Company, props, ct);
    return MapToCompanyDto(node);
}
```

### Direct Neo4j Cypher (for complex operations)

```csharp
// In repository for custom queries
public async Task<GraphNode> CreateNodeAsync(
    GraphNode node,
    CancellationToken ct = default)
{
    var query = @"
        CREATE (n:Node {
            id: $id,
            tenantId: $tenantId,
            type: $type,
            createdAt: $createdAt,
            updatedAt: $updatedAt
        })
        SET n += $properties
        RETURN n";

    var result = await _session.RunAsync(query, new
    {
        id = node.Id.Value.ToString(),
        tenantId = node.TenantId.Value.ToString(),
        type = node.Type.ToString(),
        createdAt = node.CreatedAt,
        updatedAt = node.UpdatedAt,
        properties = node.Properties
    });

    return node;
}
```

### REST API Call

```http
POST /api/organizationgraph/companies
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Acme Corp",
  "industry": "Technology",
  "employeeCount": 500
}
```

---

## Creating Edges

### Using the Service Layer

```csharp
// Create reporting relationship
public async Task<GraphEdgeDto> CreateEdgeAsync(
    EdgeType type,
    Guid sourceId,
    Guid targetId,
    CancellationToken ct = default)
{
    var tenantId = _tenantContext.TenantId!;
    var source = await _unitOfWork.GraphNodes.GetByIdAsync(
        new NodeId(sourceId), tenantId, ct)
        ?? throw new KeyNotFoundException($"Source node {sourceId} not found");
    var target = await _unitOfWork.GraphNodes.GetByIdAsync(
        new NodeId(targetId), tenantId, ct)
        ?? throw new KeyNotFoundException($"Target node {targetId} not found");

    var edge = new GraphEdge
    {
        TenantId = tenantId,
        Type = type,
        SourceId = source.Id,
        TargetId = target.Id,
        Properties = []
    };

    var created = await _unitOfWork.GraphEdges.CreateAsync(edge, ct);
    await _unitOfWork.SaveChangesAsync(ct);
    await _eventBus.PublishAsync(
        new EdgeCreatedEvent(tenantId, created.Id, type, source.Id, target.Id), ct);
    return MapToEdgeDto(created);
}
```

### REST API Call

```http
POST /api/organizationgraph/edges
Content-Type: application/json
Authorization: Bearer {token}

{
  "type": "REPORTS_TO",
  "sourceId": "emp-001-guid",
  "targetId": "mgr-001-guid"
}
```

---

## Graph Traversal Queries

### Find All Reports (Recursive Cypher)

```csharp
// In repository
public async Task<IReadOnlyList<GraphNode>> FindAllReportsAsync(
    Guid managerId,
    TenantId tenantId,
    CancellationToken ct = default)
{
    var query = @"
        MATCH (manager:Node {id: $managerId, tenantId: $tenantId})-[:REPORTS_TO*]->(report:Node)
        WHERE report.type = 'Employee'
        RETURN report";

    var result = await _session.RunAsync(query, new
    {
        managerId = managerId.ToString(),
        tenantId = tenantId.Value.ToString()
    });

    return await result.ToListAsync(ct);
}
```

### Find Direct Reports

```csharp
public async Task<IReadOnlyList<GraphNode>> FindDirectReportsAsync(
    Guid managerId,
    TenantId tenantId,
    CancellationToken ct = default)
{
    var query = @"
        MATCH (report:Node)-[:REPORTS_TO]->(manager:Node {id: $managerId, tenantId: $tenantId})
        WHERE report.type = 'Employee'
        RETURN report";

    var result = await _session.RunAsync(query, new
    {
        managerId = managerId.ToString(),
        tenantId = tenantId.Value.ToString()
    });

    return await result.ToListAsync(ct);
}
```

### Find Team Members

```csharp
public async Task<IReadOnlyList<GraphNode>> FindTeamMembersAsync(
    Guid teamId,
    TenantId tenantId,
    CancellationToken ct = default)
{
    var query = @"
        MATCH (member:Node)-[:MEMBER_OF]->(team:Node {id: $teamId, tenantId: $tenantId})
        WHERE member.type = 'Employee'
        RETURN member";

    var result = await _session.RunAsync(query, new
    {
        teamId = teamId.ToString(),
        tenantId = tenantId.Value.ToString()
    });

    return await result.ToListAsync(ct);
}
```

### Find Department Hierarchy

```csharp
public async Task<object> FindDepartmentHierarchyAsync(
    Guid departmentId,
    TenantId tenantId,
    CancellationToken ct = default)
{
    var query = @"
        MATCH (dept:Node {id: $deptId, tenantId: $tenantId, type: 'Department'})
        OPTIONAL MATCH (dept)<-[:CONTAINS]-(team:Node)
        OPTIONAL MATCH (team)<-[:MEMBER_OF]-(emp:Node)
        RETURN dept, collect(DISTINCT team) as teams, collect(DISTINCT emp) as members";

    var result = await _session.RunAsync(query, new
    {
        deptId = departmentId.ToString(),
        tenantId = tenantId.Value.ToString()
    });

    return await result.SingleAsync(ct);
}
```

---

## Validation Rules

### Circular Reference Prevention

```csharp
public async Task<bool> WouldCreateCycleAsync(
    Guid sourceId,
    Guid targetId,
    EdgeType relationshipType,
    TenantId tenantId,
    CancellationToken ct = default)
{
    var query = @"
        MATCH path = (source:Node {id: $sourceId, tenantId: $tenantId})
                     -[:{relationshipType}*]->(target:Node {id: $targetId})
        RETURN count(path) > 0 as hasCycle";

    var result = await _session.RunAsync(query, new
    {
        sourceId = sourceId.ToString(),
        targetId = targetId.ToString(),
        tenantId = tenantId.Value.ToString()
    });

    var record = await result.SingleAsync(ct);
    return record["hasCycle"].As<bool>();
}
```

### Tenant Isolation

```csharp
// Every query MUST include tenantId filter
// BAD:  MATCH (n:Node {id: $id})
// GOOD: MATCH (n:Node {id: $id, tenantId: $tenantId})
```

---

## Graph Analytics

### Span of Control

```csharp
public async Task<List<(string ManagerName, int ReportCount)>> CalculateSpanOfControlAsync(
    TenantId tenantId,
    CancellationToken ct = default)
{
    var query = @"
        MATCH (mgr:Node {tenantId: $tenantId, type: 'Employee'})<-[:REPORTS_TO]-(report:Node)
        WHERE report.type = 'Employee'
        WITH mgr, count(report) as reports
        RETURN mgr.properties.Name as name, reports
        ORDER BY reports DESC";

    var result = await _session.RunAsync(query, new
    {
        tenantId = tenantId.Value.ToString()
    });

    return await result.ToListAsync(r => (
        r["name"].As<string>(),
        r["reports"].As<int>()), ct);
}
```

### Organization Depth

```csharp
public async Task<int> CalculateOrgDepthAsync(
    TenantId tenantId,
    CancellationToken ct = default)
{
    var query = @"
        MATCH path = (ceo:Node {tenantId: $tenantId, type: 'Employee'})
                     -[:REPORTS_TO*]->(emp:Node)
        WHERE NOT (emp)-[:REPORTS_TO]->()
        RETURN length(path) as depth
        ORDER BY depth DESC
        LIMIT 1";

    var result = await _session.RunAsync(query, new
    {
        tenantId = tenantId.Value.ToString()
    });

    var record = await result.FirstOrDefaultAsync(ct);
    return record?["depth"].As<int>() ?? 0;
}
```

---

## SignalR Real-Time Updates

When a graph mutation occurs, the `GraphEventRelay` automatically broadcasts to connected clients:

```csharp
// Client connects to /hubs/graph
// Receives events like:
{
  "eventType": "Node.Created",
  "eventId": "...",
  "occurredAt": "2025-01-15T10:30:00Z",
  "data": { "nodeType": "Employee", "nodeId": "..." }
}
```

---

## Common Pitfalls

### Don't

- Forget `tenantId` in Cypher queries (security violation)
- Create edges without validating both nodes exist
- Allow circular references in REPORTS_TO chains
- Skip event publishing after mutations
- Hardcode node types as strings (use `NodeType` enum)

### Do

- Always validate tenant context via `ITenantContext`
- Publish domain events for all graph mutations
- Use parameterized Cypher queries (never string concatenation)
- Handle errors explicitly with proper HTTP status codes
- Log graph operations for audit trail
