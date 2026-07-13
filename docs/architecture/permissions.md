# Permissions

## Authorization Model

OrgSphere uses a layered authorization model combining Role-Based Access Control (RBAC) with graph-derived permissions.

---

## Permission Layers

```
┌─────────────────────────────────────────────────┐
│              Permission Evaluation               │
│                                                 │
│  ┌─────────────────────────────────────────┐   │
│  │  Layer 1: Global Platform Permissions    │   │
│  │  (Platform Admin, Super User)            │   │
│  └─────────────────────────────────────────┘   │
│                    ▼                            │
│  ┌─────────────────────────────────────────┐   │
│  │  Layer 2: Tenant-Level Roles             │   │
│  │  (HR Admin, Manager, Employee)           │   │
│  └─────────────────────────────────────────┘   │
│                    ▼                            │
│  ┌─────────────────────────────────────────┐   │
│  │  Layer 3: Graph-Based Permissions        │   │
│  │  (Department head, Team lead, etc.)      │   │
│  └─────────────────────────────────────────┘   │
│                    ▼                            │
│  ┌─────────────────────────────────────────┐   │
│  │  Layer 4: Resource-Level Permissions     │   │
│  │  (Owner, collaborator, viewer)           │   │
│  └─────────────────────────────────────────┘   │
└─────────────────────────────────────────────────┘
```

---

## Roles

### Platform Roles

| Role | Description |
|------|-------------|
| Platform Admin | Full system access across tenants |
| Support Admin | Tenant support access |

### Tenant Roles

| Role | Description |
|------|-------------|
| HR Admin | Full HR operations |
| Department Head | Department management |
| Manager | Team management |
| Team Lead | Team coordination |
| Employee | Self-service operations |
| Contractor | Limited access |

---

## Permissions

### Permission Format

```
module:action:scope
```

Examples:

- `employee:read:own`
- `employee:write:department`
- `leave:approve:team`
- `analytics:read:company`

### Scopes

| Scope | Description |
|-------|-------------|
| own | Own resources only |
| team | Team members |
| department | Department resources |
| company | All company resources |
| global | Cross-tenant (platform only) |

---

## Graph-Based Permissions

### Hierarchy-Based Access

Managers automatically get permissions for their reports:

```
Manager → Employee A (direct report)
Manager → Employee B (direct report)
         → Employee C (report to B)
```

Manager can:

- Read own direct reports
- Approve direct report requests
- View team analytics

### Department-Based Access

Department heads get permissions for their department:

- Read all department members
- Approve department requests
- View department analytics
- Manage department structure

### Inherited Permissions

Permissions cascade down the hierarchy:

```
CEO → VP Engineering → Director → Manager → Employee
 │        │              │          │
 │        │              │          └─ Can approve team requests
 │        │              └─ Can approve department requests
 │        └─ Can approve VP-level requests
 └─ Can approve everything
```

---

## Permission Evaluation

### Decision Flow

```
1. Is user platform admin? → ALLOW
2. Is user tenant admin? → ALLOW (tenant scope)
3. Does user have required role? → Check role permissions
4. Does graph position grant access? → Check graph permissions
5. Does resource ownership grant access? → Check resource permissions
6. Default → DENY
```

### Caching

- Permissions cached per session
- Cache invalidated on role/position change
- Graph changes trigger cache refresh

---

## Delegation

### Temporary Delegation

Managers can delegate approval authority:

- Specify delegate person
- Set time range
- Limit scope (specific request types)
- Automatic revocation on expiry

### Emergency Delegation

Automatic delegation when:

- Manager is on leave
- Manager is unavailable
- Escalation timeout reached

---

## Audit Trail

All permission checks are logged:

- User ID
- Action attempted
- Resource accessed
- Decision (allow/deny)
- Reason (role, graph position, etc.)
- Timestamp

---

## API Authorization

### Request Flow

```
1. Extract tenant context from JWT
2. Extract user roles and permissions
3. Identify resource and action
4. Evaluate permission layers
5. Allow or deny
6. Log decision
```

### Middleware

Authorization middleware applies to:

- All API endpoints
- SignalR hub connections
- Background jobs
