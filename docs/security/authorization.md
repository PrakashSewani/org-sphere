# Authorization

## Authorization Model

Layered authorization combining RBAC with graph-derived permissions.

---

## Role-Based Access Control (RBAC)

### Platform Roles

| Role | Description |
|------|-------------|
| Platform Admin | Full system access |
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

### Role Hierarchy

```
Platform Admin
  └── Support Admin

HR Admin
  └── Department Head
        └── Manager
              └── Team Lead
                    └── Employee
                          └── Contractor
```

---

## Permission Format

```
module:action:scope
```

### Modules

- employee
- department
- team
- leave
- attendance
- approval
- communication
- analytics
- settings
- ai

### Actions

- read
- write
- delete
- approve
- export
- admin

### Scopes

- own
- team
- department
- company
- global

---

## Permission Matrix

### Employee Module

| Role | read:own | read:team | read:dept | read:company | write:own | write:team |
|------|----------|-----------|-----------|--------------|-----------|------------|
| Employee | ✓ | ✗ | ✗ | ✗ | ✓ | ✗ |
| Team Lead | ✓ | ✓ | ✗ | ✗ | ✓ | ✓ |
| Manager | ✓ | ✓ | ✓ | ✗ | ✓ | ✓ |
| Dept Head | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| HR Admin | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

### Leave Module

| Role | read:own | request | approve:team | approve:dept | admin |
|------|----------|---------|--------------|--------------|-------|
| Employee | ✓ | ✓ | ✗ | ✗ | ✗ |
| Team Lead | ✓ | ✓ | ✓ | ✗ | ✗ |
| Manager | ✓ | ✓ | ✓ | ✗ | ✗ |
| Dept Head | ✓ | ✓ | ✓ | ✓ | ✗ |
| HR Admin | ✓ | ✓ | ✓ | ✓ | ✓ |

---

## Graph-Based Permissions

### Hierarchy-Based Access

Managers automatically get permissions for their reports:

```
Manager → Employee A (direct report)
         → Employee B (direct report)
              → Employee C (report to B)
```

Manager can:
- Read direct reports
- Approve direct report requests
- View team analytics

### Department-Based Access

Department heads get department-wide access:

- Read all department members
- Approve department requests
- View department analytics
- Manage department structure

### Inheritance

Permissions cascade down the hierarchy:

```
CEO → VP → Director → Manager → Employee
 │      │           │          │
 │      │           │          └─ Team level
 │      │           └─ Department level
 │      └─ Division level
 └─ Company level
```

---

## Permission Evaluation

### Decision Flow

```
1. Is platform admin? → ALLOW
2. Is tenant admin? → ALLOW (tenant scope)
3. Has required role? → Check role permissions
4. Graph position grants access? → Check graph permissions
5. Resource ownership grants access? → Check resource permissions
6. Default → DENY
```

### Caching

- Permissions cached per session
- Cache invalidated on role change
- Graph changes trigger refresh
- TTL: 5 minutes

---

## Delegation

### Manual Delegation

- Manager specifies delegate
- Scope limitation
- Time limitation
- Automatic revocation

### Automatic Delegation

- Manager on leave
- Manager unavailable
- Escalation timeout

---

## Row-Level Security

### Tenant Isolation

- All queries filtered by tenant
- Cross-tenant access impossible
- Database-level enforcement

### Data Scoping

- Employee sees own data
- Manager sees team data
- Dept head sees department data
- HR sees all data

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

Authorization middleware:

- All API endpoints
- SignalR hub connections
- Background jobs
