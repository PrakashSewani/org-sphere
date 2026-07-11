# Approvals

## Approval Engine

Configurable approval workflows that route requests through the Organization Graph.

---

## Approval Types

### Leave Requests

- Manager approval
- HR approval (extended leave)
- VP approval (long-term leave)

### Expense Reports

- Manager approval
- Finance approval
- VP approval (large amounts)

### Equipment Requests

- Manager approval
- IT approval
- Procurement approval

### Access Requests

- Manager approval
- IT approval
- Security approval

### Promotion Requests

- Manager recommendation
- HR approval
- VP approval
- Executive approval

---

## Workflow Configuration

### Approval Chain Definition

```
Request Type → Step 1 → Step 2 → ... → Complete
```

### Step Configuration

| Property | Description |
|----------|-------------|
| Approver | Who approves (role, person, graph position) |
| Condition | When this step applies |
| Timeout | How long before escalation |
| Delegation | Who can approve in their place |

### Condition Types

- Amount threshold
- Request type
- Department
- Employee level
- Duration
- Custom rules

---

## Graph-Aware Routing

### Hierarchy-Based

- Route to direct manager
- Route to department head
- Route to functional leader

### Role-Based

- Route to HR for policy
- Route to Finance for budget
- Route to IT for technical

### Custom Rules

- Route based on amount
- Route based on department
- Route based on request type
- Route based on history

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
- System-configured rules

---

## Escalation

### Timeout Escalation

- Configurable timeout per step
- Automatic escalation to next approver
- Notification to original approver
- Audit trail

### Condition Escalation

- Amount exceeds threshold
- Policy exception detected
- Compliance issue
- Risk assessment

---

## State Machine

```
┌─────────┐
│ Pending │
└────┬────┘
     │ submit
     ▼
┌─────────┐
│ In Review│
└────┬────┘
     │
     ├──── approve ────▶ ┌──────────┐
     │                    │ Approved │
     ├──── reject ──────▶ └──────────┘
     │                    ┌──────────┐
     ├──── escalate ────▶ │Escalated │
     │                    └──────────┘
     │
     └──── cancel ─────▶ ┌──────────┐
                          │ Cancelled │
                          └──────────┘
```

---

## Graph Integration

### Relationships

```
Approval Request
  ├── SUBMITTED_BY → Employee
  ├── APPROVED_BY → Approver
  ├── TYPE → Request Type
  └── AFFECTS → Department Budget
```

### Graph Queries

- Pending approvals for manager
- Approval history for employee
- Average approval time
- Bottleneck identification

---

## Audit Trail

### Logged Information

- Request details
- Approval decisions
- Timestamps
- Comments
- Delegation records
- Escalation records

### Compliance

- Complete history
- Tamper-proof logging
- Export capability
- Retention policies

---

## Analytics

### Metrics

- Average approval time
- Approval rate
- Rejection rate
- Escalation rate

### Insights

- Bottleneck identification
- Approval pattern analysis
- Policy effectiveness
- Compliance metrics
