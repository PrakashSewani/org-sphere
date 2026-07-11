# Workflows

## Workflow Engine

OrgSphere includes a configurable workflow engine for approvals, automations, and business processes.

---

## Workflow Concepts

### Workflow Definition

A workflow is a sequence of steps that process a request:

```
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│ Request │───▶│ Step 1  │───▶│ Step 2  │───▶│ Complete│
│ Created │    │ Review  │    │ Approve │    │         │
└─────────┘    └─────────┘    └─────────┘    └─────────┘
                   │              │
                   ▼              ▼
                ┌─────────┐   ┌─────────┐
                │ Reject  │   │ Escalate│
                └─────────┘   └─────────┘
```

### Workflow Types

| Type | Description |
|------|-------------|
| Approval | Multi-step approval chain |
| Onboarding | New employee setup process |
| Offboarding | Employee exit process |
| Transfer | Employee movement between teams |
| Custom | User-defined processes |

---

## Approval Chains

### Configuration

Each request type can have a configurable approval chain:

```
Leave Request → Manager → HR (if >5 days) → Complete
Expense Request → Manager → Finance → Complete
Promotion → Manager → HR → VP → Complete
```

### Approval Rules

- **Sequential**: Each approver must approve before next
- **Parallel**: Multiple approvers simultaneously
- **Conditional**: Route based on request properties
- **Quorum**: Require N of M approvals

### Graph-Aware Routing

Approval chains use the Organization Graph:

- Route to direct manager
- Skip to department head for large amounts
- Route to HR for policy exceptions
- Route to Finance for budget items

---

## Delegation

### Manual Delegation

Managers can delegate authority:

```
Manager A → Delegates to Manager B
Scope: Leave approvals only
Duration: July 1-15, 2025
```

### Automatic Delegation

System can auto-delegate when:

- Manager is on approved leave
- Manager is marked unavailable
- Escalation timeout reached

---

## Escalation

### Timeout Escalation

If an approver doesn't respond within configured time:

```
Step 1: Manager (24 hours)
  ↓ timeout
Step 2: Department Head (24 hours)
  ↓ timeout
Step 3: HR Admin (final)
```

### Condition Escalation

Automatic escalation based on conditions:

- Amount exceeds threshold
- Request type requires higher authority
- Policy exception detected

---

## Workflow State Machine

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

## Workflow Events

### Events Published

| Event | When |
|-------|------|
| workflow.created | Request submitted |
| workflow.approved | Step approved |
| workflow.rejected | Step rejected |
| workflow.escalated | Step escalated |
| workflow.completed | Workflow finished |
| workflow.cancelled | Request cancelled |

### Event Consumers

- Notification Service: Send notifications
- Analytics Service: Track metrics
- AI Service: Learn patterns
- Audit Service: Log activity

---

## Configuration

### Admin Interface

Administrators can:

- Create workflow definitions
- Configure approval chains
- Set escalation rules
- Define delegation policies
- Map request types to workflows

### Per-Tenant Configuration

Each tenant has:

- Custom workflow definitions
- Custom approval chains
- Custom escalation rules
- Custom delegation policies

---

## Workflow Templates

### Predefined Templates

| Template | Steps |
|----------|-------|
| Leave Request | Manager → HR (if needed) |
| Expense Approval | Manager → Finance |
| Promotion | Manager → HR → VP |
| Equipment Request | Manager → IT |
| Access Request | Manager → IT → Security |

### Custom Templates

Tenants can create custom templates for:

- Internal processes
- Compliance requirements
- Industry-specific workflows
