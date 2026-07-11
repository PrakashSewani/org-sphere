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

### Template

```typescript
interface CreateNodeInput {
  type: string;          // Node type (e.g., 'Employee')
  tenantId: string;      // Required for multi-tenancy
  properties: Record<string, any>;
  metadata?: Record<string, any>;
}

async function createNode(input: CreateNodeInput): Promise<Node> {
  // Validate tenant context
  validateTenantContext(input.tenantId);

  // Validate node type
  validateNodeType(input.type);

  // Create node
  const node = await graphDB.createNode({
    ...input,
    createdAt: new Date(),
    updatedAt: new Date()
  });

  // Publish event
  await eventBus.publish({
    type: `${input.type.toLowerCase()}.created`,
    tenantId: input.tenantId,
    data: { nodeId: node.id, ...input.properties }
  });

  return node;
}
```

### Examples

```typescript
// Create Employee
await createNode({
  type: 'Employee',
  tenantId: 'tenant-123',
  properties: {
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@company.com',
    title: 'Software Engineer',
    employeeId: 'EMP-001'
  }
});

// Create Department
await createNode({
  type: 'Department',
  tenantId: 'tenant-123',
  properties: {
    name: 'Engineering',
    description: 'Product engineering team'
  }
});
```

---

## Creating Edges

### Template

```typescript
interface CreateEdgeInput {
  type: string;          // Edge type (e.g., 'REPORTS_TO')
  sourceId: string;      // Source node ID
  targetId: string;      // Target node ID
  tenantId: string;      // Required for multi-tenancy
  properties?: Record<string, any>;
}

async function createEdge(input: CreateEdgeInput): Promise<Edge> {
  // Validate tenant context
  validateTenantContext(input.tenantId);

  // Validate nodes exist and belong to tenant
  await validateNodesBelongToTenant(
    input.sourceId,
    input.targetId,
    input.tenantId
  );

  // Validate relationship rules
  await validateRelationshipRules(input);

  // Create edge
  const edge = await graphDB.createEdge({
    ...input,
    createdAt: new Date()
  });

  // Publish event
  await eventBus.publish({
    type: `relationship.created`,
    tenantId: input.tenantId,
    data: {
      edgeType: input.type,
      sourceId: input.sourceId,
      targetId: input.targetId
    }
  });

  return edge;
}
```

### Examples

```typescript
// Create reporting relationship
await createEdge({
  type: 'REPORTS_TO',
  sourceId: 'emp-001',
  targetId: 'emp-002',
  tenantId: 'tenant-123'
});

// Create team membership
await createEdge({
  type: 'MEMBER_OF',
  sourceId: 'emp-001',
  targetId: 'team-001',
  tenantId: 'tenant-123',
  properties: {
    role: 'member',
    joinedAt: new Date()
  }
});
```

---

## Graph Traversal Queries

### Find All Reports (Recursive)

```typescript
async function findAllReports(
  managerId: string,
  tenantId: string
): Promise<Node[]> {
  return graphDB.traverse({
    start: managerId,
    relationship: 'REPORTS_TO',
    direction: 'INCOMING',
    depth: 'recursive',  // Unlimited depth
    tenantId
  });
}
```

### Find Direct Reports

```typescript
async function findDirectReports(
  managerId: string,
  tenantId: string
): Promise<Node[]> {
  return graphDB.traverse({
    start: managerId,
    relationship: 'REPORTS_TO',
    direction: 'INCOMING',
    depth: 1,  // Only direct reports
    tenantId
  });
}
```

### Find Team Members

```typescript
async function findTeamMembers(
  teamId: string,
  tenantId: string
): Promise<Node[]> {
  return graphDB.traverse({
    start: teamId,
    relationship: 'MEMBER_OF',
    direction: 'INCOMING',
    depth: 1,
    tenantId
  });
}
```

### Find Department Hierarchy

```typescript
async function findDepartmentHierarchy(
  departmentId: string,
  tenantId: string
): Promise<GraphResult> {
  return graphDB.query({
    query: `
      MATCH (dept:Department {id: $deptId})<-[:CONTAINS]-(team:Team)
      MATCH (team)<-[:MEMBER_OF]-(emp:Employee)
      RETURN dept, team, collect(emp) as members
    `,
    params: { deptId: departmentId },
    tenantId
  });
}
```

---

## Validation Rules

### Circular Reference Prevention

```typescript
async function wouldCreateCycle(
  sourceId: string,
  targetId: string,
  relationshipType: string,
  tenantId: string
): Promise<boolean> {
  // Check if target is already an ancestor of source
  const ancestors = await graphDB.traverse({
    start: sourceId,
    relationship: relationshipType,
    direction: 'OUTGOING',
    tenantId
  });

  return ancestors.some(node => node.id === targetId);
}
```

### Tenant Isolation

```typescript
async function validateNodesBelongToTenant(
  nodeIds: string[],
  tenantId: string
): Promise<void> {
  const nodes = await graphDB.getNodes(nodeIds);

  for (const node of nodes) {
    if (node.tenantId !== tenantId) {
      throw new Error('Access denied: node belongs to different tenant');
    }
  }
}
```

---

## Graph Analytics

### Span of Control

```typescript
async function calculateSpanOfControl(
  tenantId: string
): Promise<Metric[]> {
  return graphDB.query({
    query: `
      MATCH (mgr:Employee)<-[:REPORTS_TO]-(report:Employee)
      WITH mgr, count(report) as reports
      RETURN mgr.name, reports
      ORDER BY reports DESC
    `,
    tenantId
  });
}
```

### Organization Depth

```typescript
async function calculateOrgDepth(
  tenantId: string
): Promise<number> {
  const result = await graphDB.query({
    query: `
      MATCH path = (ceo:Employee)-[:REPORTS_TO*]->(emp:Employee)
      WHERE NOT (emp)-[:REPORTS_TO]->()
      RETURN length(path) as depth
      ORDER BY depth DESC
      LIMIT 1
    `,
    tenantId
  });

  return result[0]?.depth ?? 0;
}
```

---

## Common Pitfalls

### Don't

- Forget tenantId in queries
- Create edges without validating nodes
- Allow circular references
- Skip event publishing
- Hardcode node types

### Do

- Always validate tenant context
- Publish events for all changes
- Use parameterized queries
- Handle errors explicitly
- Log graph operations
