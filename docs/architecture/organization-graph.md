# Organization Graph

## The Heart of OrgSphere

The Organization Graph is the central data structure. It models the entire organization as a network of interconnected entities.

Every feature in OrgSphere integrates with this graph. If a feature doesn't naturally integrate, we redesign it until it does.

---

## Graph Model

### Nodes (Entities)

```
┌─────────────────────────────────────────────────────────┐
│                    Organization Graph                    │
│                                                         │
│   ┌──────────┐     ┌──────────┐     ┌──────────┐      │
│   │ Company  │────▶│  Region  │────▶│  Office  │      │
│   └──────────┘     └──────────┘     └──────────┘      │
│        │                                                 │
│        ▼                                                 │
│   ┌──────────┐     ┌──────────┐     ┌──────────┐      │
│   │Department│────▶│   Team   │────▶│Employee  │      │
│   └──────────┘     └──────────┘     └──────────┘      │
│        │                  │                │            │
│        ▼                  ▼                ▼            │
│   ┌──────────┐     ┌──────────┐     ┌──────────┐      │
│   │ Project  │     │ Workflow │     │Approval  │      │
│   └──────────┘     └──────────┘     └──────────┘      │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Node Types

| Node | Description | Key Properties |
|------|-------------|----------------|
| Company | Top-level entity | name, domain, settings |
| Region | Geographic region | name, timezone, locale |
| Office | Physical location | address, capacity, facilities |
| Department | Major organizational unit | name, budget, head |
| Team | Working group | name, focus, lead |
| Employee | Person | name, role, skills, status |
| Contractor | External worker | name, vendor, contract |
| Vendor | External organization | name, contact, services |
| Project | Initiative | name, goals, timeline |
| Committee | Ad-hoc group | name, purpose, members |
| Workflow | Process definition | name, steps, triggers |
| Approval | Request instance | type, requester, status |
| Communication | Message | channel, content, targets |
| Document | File | name, type, location |

### Edges (Relationships)

| Edge | Source → Target | Description |
|------|-----------------|-------------|
| CONTAINS | Company → Department | Structural containment |
| CONTAINS | Department → Team | Team within department |
| MEMBER_OF | Employee → Team | Team membership |
| REPORTS_TO | Employee → Employee | Reporting hierarchy |
| MANAGES | Employee → Department | Department head |
| LEADS | Employee → Team | Team lead |
| WORKS_ON | Employee → Project | Project assignment |
| APPROVES | Employee → Approval | Approval authority |
| LOCATED_AT | Employee → Office | Work location |
| HAS_SKILL | Employee → Skill | Competency |
| OWNS | Employee → Asset | Asset assignment |
| COMMUNICATES | Employee → Communication | Message participation |

---

## Graph Operations

### Traversal

Find all employees reporting to a manager:

```
MATCH (manager:Employee {id: $managerId})-[:REPORTS_TO*]->(report:Employee)
RETURN report
```

Find all team members:

```
MATCH (team:Team {id: $teamId})<-[:MEMBER_OF]-(employee:Employee)
RETURN employee
```

### Aggregation

Count employees per department:

```
MATCH (dept:Department)<-[:CONTAINS]-(emp:Employee)
RETURN dept.name, count(emp)
```

### Pattern Matching

Find overloaded managers (more than 8 direct reports):

```
MATCH (mgr:Employee)-[:REPORTS_TO]->(manager:Employee)
WITH manager, count(mgr) as reports
WHERE reports > 8
RETURN manager, reports
```

---

## Graph Integrity

### Constraints

- Every Employee must belong to exactly one Department
- Every Employee must have exactly one direct Manager (except CEO)
- Every Department must have exactly one Head
- Every Team must have exactly one Lead
- No circular reporting chains

### Validation Rules

- Employee cannot report to themselves
- Manager must be in same or parent department
- Team members must belong to the team's department
- Approval chains must be acyclic

---

## Graph Evolution

### Temporal Graph

The graph tracks changes over time:

- When an employee moves departments
- When a team is restructured
- When reporting lines change

Every edge has:

- `validFrom`: when the relationship started
- `validTo`: when the relationship ended (null if current)
- `changedBy`: who made the change
- `changeReason`: why the change was made

### Versioning

- Snapshot versions for point-in-time queries
- Diff versions for change tracking
- Branch versions for simulation

---

## Graph Analytics

### Metrics

- **Depth**: Longest reporting chain
- **Breadth**: Number of direct reports per manager
- **Span of Control**: Average reports per manager
- **Connectivity**: How interconnected teams are
- **Centralization**: How dependent the org is on key people

### Insights

- Identify overloaded managers
- Detect organizational bottlenecks
- Find collaboration gaps
- Suggest restructuring opportunities

---

## Integration Points

Every module queries the graph:

| Module | Graph Usage |
|--------|-------------|
| Leave Management | Check team availability |
| Approvals | Route to correct approver |
| Communication | Target messages by relationship |
| Analytics | Derive metrics from structure |
| AI Assistant | Reason about relationships |
| Permissions | Determine access based on hierarchy |
| Recruitment | Identify hiring needs by team |
| Onboarding | Assign buddy, schedule introductions |
