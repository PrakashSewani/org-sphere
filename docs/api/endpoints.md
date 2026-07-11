# API Endpoints

## API Design

RESTful API with GraphQL support for flexible queries.

---

## Base URL

```
https://api.orgsphere.com/v1
```

## Authentication

```
Authorization: Bearer <token>
```

---

## Graph API

### Nodes

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /graph/nodes | List all nodes |
| GET | /graph/nodes/:id | Get node by ID |
| POST | /graph/nodes | Create node |
| PUT | /graph/nodes/:id | Update node |
| DELETE | /graph/nodes/:id | Delete node |

### Edges

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /graph/edges | List all edges |
| GET | /graph/edges/:id | Get edge by ID |
| POST | /graph/edges | Create edge |
| PUT | /graph/edges/:id | Update edge |
| DELETE | /graph/edges/:id | Delete edge |

### Traversal

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /graph/traverse | Graph traversal query |
| POST | /graph/search | Graph search |

---

## Employee API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /employees | List employees |
| GET | /employees/:id | Get employee |
| POST | /employees | Create employee |
| PUT | /employees/:id | Update employee |
| DELETE | /employees/:id | Delete employee |
| GET | /employees/:id/team | Get employee team |
| GET | /employees/:id/reports | Get direct reports |
| GET | /employees/:id/history | Get employment history |

---

## Department API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /departments | List departments |
| GET | /departments/:id | Get department |
| POST | /departments | Create department |
| PUT | /departments/:id | Update department |
| DELETE | /departments/:id | Delete department |
| GET | /departments/:id/teams | Get department teams |
| GET | /departments/:id/employees | Get department employees |

---

## Team API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /teams | List teams |
| GET | /teams/:id | Get team |
| POST | /teams | Create team |
| PUT | /teams/:id | Update team |
| DELETE | /teams/:id | Delete team |
| GET | /teams/:id/members | Get team members |
| POST | /teams/:id/members | Add team member |
| DELETE | /teams/:id/members/:empId | Remove team member |

---

## Leave API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /leave/policies | List leave policies |
| GET | /leave/balances | Get leave balances |
| POST | /leave/requests | Create leave request |
| GET | /leave/requests | List leave requests |
| PUT | /leave/requests/:id | Update leave request |
| PUT | /leave/requests/:id/approve | Approve request |
| PUT | /leave/requests/:id/reject | Reject request |

---

## Attendance API

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /attendance/check-in | Check in |
| POST | /attendance/check-out | Check out |
| GET | /attendance/records | Get attendance records |
| GET | /attendance/summary | Get attendance summary |
| PUT | /attendance/records/:id | Update record |

---

## Approval API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /approvals | List approvals |
| GET | /approvals/:id | Get approval |
| POST | /approvals | Create approval |
| PUT | /approvals/:id/approve | Approve |
| PUT | /approvals/:id/reject | Reject |
| PUT | /approvals/:id/escalate | Escalate |

---

## Communication API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /messages | List messages |
| GET | /messages/:id | Get message |
| POST | /messages | Send message |
| PUT | /messages/:id/read | Mark as read |
| GET | /channels | List channels |
| POST | /channels | Create channel |

---

## Analytics API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /analytics/dashboard | Get dashboard data |
| GET | /analytics/reports | List reports |
| GET | /analytics/reports/:id | Get report |
| POST | /analytics/reports | Create report |
| GET | /analytics/metrics | Get metrics |

---

## AI API

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /ai/query | Natural language query |
| GET | /ai/insights | Get AI insights |
| GET | /ai/recommendations | Get recommendations |
| POST | /ai/simulate | Simulate change |

---

## Settings API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /settings | Get tenant settings |
| PUT | /settings | Update settings |
| GET | /settings/leave | Get leave settings |
| PUT | /settings/leave | Update leave settings |
| GET | /settings/approvals | Get approval settings |
| PUT | /settings/approvals | Update approval settings |

---

## Common Query Parameters

### Pagination

```
?page=1&limit=20
```

### Filtering

```
?department=engineering&status=active
```

### Sorting

```
?sort=name&order=asc
```

### Search

```
?search=alice
```

---

## Response Format

### Success

```json
{
  "success": true,
  "data": { ... },
  "meta": {
    "page": 1,
    "limit": 20,
    "total": 100
  }
}
```

### Error

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input",
    "details": { ... }
  }
}
```

---

## Rate Limiting

- Free tier: 100 requests/minute
- Starter: 1,000 requests/minute
- Professional: 10,000 requests/minute
- Enterprise: Unlimited

---

## Versioning

- API version in URL: `/v1/`
- Breaking changes require new version
- Deprecation notice 6 months before removal
