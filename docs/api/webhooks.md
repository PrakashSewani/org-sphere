# Webhooks

## Webhook System

HTTP callbacks for real-time event notifications to external systems.

---

## Webhook Configuration

### Create Webhook

```json
{
  "url": "https://external-system.com/webhook",
  "events": [
    "employee.created",
    "employee.updated",
    "leave.requested",
    "leave.approved"
  ],
  "secret": "whsec_...",
  "active": true
}
```

### Webhook Properties

| Property | Type | Description |
|----------|------|-------------|
| id | string | Unique identifier |
| url | string | Target URL |
| events | array | Event types to subscribe |
| secret | string | Signing secret |
| active | boolean | Enabled/disabled |
| createdAt | datetime | Creation time |
| updatedAt | datetime | Last update time |

---

## Event Delivery

### Payload Format

```json
{
  "id": "evt-123",
  "type": "employee.created",
  "tenantId": "tenant-456",
  "timestamp": "2025-01-15T10:30:00Z",
  "data": {
    "employeeId": "emp-789",
    "name": "John Doe",
    "department": "Engineering"
  }
}
```

### Headers

```
Content-Type: application/json
X-Webhook-ID: wh-012
X-Webhook-Signature: sha256=...
X-Webhook-Timestamp: 1705312200
X-Tenant-ID: tenant-456
```

---

## Signature Verification

### Signing

```csharp
using System.Security.Cryptography;
using System.Text;

var payload = JsonSerializer.Serialize(data);
var keyBytes = Encoding.UTF8.GetBytes(secret);
using var hmac = new HMACSHA256(keyBytes);
var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
var signature = Convert.ToHexString(hash).ToLowerInvariant();
```

### Verification

```csharp
using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
var computed = Convert.ToHexString(
    hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
var isValid = CryptographicOperations.FixedTimeEquals(
    Encoding.UTF8.GetBytes(computed),
    Encoding.UTF8.GetBytes(receivedSignature));
```

---

## Delivery Guarantees

### At-Least-Once

- Retry on failure
- Exponential backoff
- Maximum retry attempts
- Dead letter queue

### Retry Schedule

| Attempt | Delay |
|---------|-------|
| 1 | Immediate |
| 2 | 1 minute |
| 3 | 5 minutes |
| 4 | 30 minutes |
| 5 | 2 hours |
| 6 | 8 hours |
| 7 | 24 hours |

### Timeout

- Connection timeout: 10 seconds
- Response timeout: 30 seconds

---

## Webhook Logs

### Log Entry

```json
{
  "id": "log-012",
  "webhookId": "wh-012",
  "event": "employee.created",
  "status": "delivered",
  "statusCode": 200,
  "request": {
    "headers": { ... },
    "body": { ... }
  },
  "response": {
    "headers": { ... },
    "body": { ... }
  },
  "duration": 245,
  "timestamp": "2025-01-15T10:30:00Z"
}
```

### Log Status

| Status | Description |
|--------|-------------|
| delivered | Successfully delivered |
| failed | Delivery failed |
| pending | Awaiting delivery |
| retrying | Retry in progress |

---

## Event Types

### Employee Events

- employee.created
- employee.updated
- employee.deleted

### Department Events

- department.created
- department.updated
- department.deleted

### Team Events

- team.created
- team.updated
- team.deleted
- team.member.added
- team.member.removed

### Leave Events

- leave.requested
- leave.approved
- leave.rejected
- leave.cancelled

### Attendance Events

- attendance.checked.in
- attendance.checked.out

### Approval Events

- approval.requested
- approval.approved
- approval.rejected
- approval.escalated

---

## Management API

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /webhooks | List webhooks |
| GET | /webhooks/:id | Get webhook |
| POST | /webhooks | Create webhook |
| PUT | /webhooks/:id | Update webhook |
| DELETE | /webhooks/:id | Delete webhook |
| POST | /webhooks/:id/test | Test webhook |
| GET | /webhooks/:id/logs | Get delivery logs |

### Test Webhook

```json
POST /webhooks/wh-012/test
{
  "event": "employee.created",
  "data": {
    "employeeId": "test-001",
    "name": "Test Employee"
  }
}
```

---

## Security

### IP Whitelisting

- Configure allowed IPs
- Dynamic IP ranges
- IP validation on delivery

### Authentication

- Bearer token
- Basic auth
- Custom headers
- Signature verification

### Rate Limiting

- Per-tenant limits
- Per-webhook limits
- Burst allowance
- Throttling

---

## Monitoring

### Metrics

- Delivery success rate
- Average delivery time
- Error rates
- Retry rates

### Alerts

- Delivery failures
- High error rates
- Latency spikes
- Rate limit warnings

---

## Best Practices

### Endpoint Configuration

- Use HTTPS
- Accept POST requests
- Return 2xx quickly
- Process asynchronously
- Handle duplicates

### Error Handling

- Idempotent processing
- Deduplication
- Graceful degradation
- Logging

### Security

- Verify signatures
- Validate timestamps
- Use secrets
- Rotate secrets
