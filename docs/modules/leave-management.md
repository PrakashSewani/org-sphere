# Leave Management

## Leave Policies

### Policy Configuration

- Leave types (vacation, sick, personal, etc.)
- Accrual rules
- Carryover rules
- Waiting periods
- Blackout dates

### Leave Types

| Type | Description | Accrual |
|------|-------------|---------|
| Vacation | Paid time off | Monthly/annual |
| Sick | Health-related absence | Monthly/annual |
| Personal | Personal matters | Monthly/annual |
| Parental | Parental leave | One-time |
| Bereavement | Family loss | One-time |
| Unpaid | Unpaid leave | N/A |

---

## Leave Requests

### Request Flow

```
1. Employee selects leave type
2. Employee selects dates
3. Employee adds reason
4. System checks balance
5. System checks policy
6. System routes to approver
7. Approvers reviews
8. System updates balance
9. System notifies team
```

### Request Details

- Leave type
- Start date
- End date
- Duration (days/hours)
- Reason
- Attachments (if required)

---

## Approval Workflow

### Graph-Aware Routing

- Route to direct manager
- Skip to department head for extended leave
- Route to HR for policy exceptions
- Route to Finance for unpaid leave

### Approval Rules

- Manager approval required
- HR approval for >5 days
- VP approval for >10 days
- CEO approval for >20 days

### Delegation

- Manager can delegate approval
- Automatic delegation on leave
- Escalation on timeout

---

## Balance Management

### Accrual

- Monthly accrual
- Annual lump sum
- Custom accrual rules
- Carryover rules

### Balance Tracking

- Current balance
- Pending requests
- Used this year
- Projected balance

### Adjustments

- Manual adjustments
- Policy corrections
- Retroactive changes

---

## Calendar Integration

### Team Calendar

- View team availability
- See who's out
- Plan around absences
- Color-coded by type

### Personal Calendar

- My leave history
- Upcoming leave
- Pending requests
- Balance overview

### Export

- Google Calendar
- Outlook Calendar
- ICS format

---

## Graph Integration

### Relationships

```
Employee
  ├── REQUESTS → Leave Request
  ├── APPROVED_BY → Manager
  ├── AFFECTS → Team Availability
  └── BALANCE → Leave Balance
```

### Graph Queries

- Who's out this week?
- Team availability on date
- Manager's team coverage
- Department capacity

---

## Notifications

### Events

- Request submitted
- Request approved
- Request rejected
- Balance updated
- Upcoming leave reminder

### Channels

- In-app notification
- Email notification
- Calendar update

---

## Analytics

### Metrics

- Leave utilization
- Absence patterns
- Approval times
- Coverage gaps

### Insights

- Team availability trends
- Seasonal patterns
- Policy effectiveness
- Compliance metrics

---

## Policies

### Policy Types

- Accrual policies
- Carryover policies
- Blackout dates
- Waiting periods
- Documentation requirements

### Configuration

- Per leave type
- Per department
- Per employment type
- Per tenure
