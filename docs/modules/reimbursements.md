# Reimbursements

## Expense Management

Streamlined expense reporting and reimbursement integrated with the Organization Graph.

---

## Expense Types

### Categories

- Travel
- Meals
- Transportation
- Office supplies
- Training
- Equipment
- Software
- Other

### Receipt Requirements

- Digital receipt upload
- OCR receipt scanning
- Receipt matching
- Missing receipt affidavit

---

## Expense Reports

### Report Creation

- Add expenses
- Categorize expenses
- Attach receipts
- Add notes

### Report Submission

- Manager approval
- Finance review
- Policy compliance check
- Receipt verification

### Report Status

```
Draft → Submitted → Under Review → Approved → Reimbursed
                ↓
           Rejected (with reason)
```

---

## Approval Workflow

### Graph-Aware Routing

- Route to direct manager
- Route to finance for large amounts
- Route to VP for executive expenses
- Route to HR for policy exceptions

### Approval Rules

- Amount thresholds
- Category rules
- Policy compliance
- Receipt requirements

---

## Policy Configuration

### Expense Policies

- Category limits
- Daily limits
- Monthly limits
- Annual limits
- Approval thresholds

### Compliance Rules

- Receipt requirements
- Documentation requirements
- Time limits
- Policy exceptions

---

## Graph Integration

### Relationships

```
Expense Report
  ├── SUBMITTED_BY → Employee
  ├── APPROVED_BY → Approver
  ├── FOR → Department/Project
  └── CATEGORY → Expense Type
```

### Graph Queries

- Expense by department
- Expense by category
- Approval turnaround time
- Policy compliance rates

---

## Reimbursement

### Payment Methods

- Direct deposit
- Check
- Corporate card reconciliation

### Payment Processing

- Batch processing
- Individual processing
- International payments
- Tax implications

---

## Analytics

### Metrics

- Average reimbursement time
- Expense by category
- Policy compliance rate
- Budget utilization

### Insights

- Spending patterns
- Policy effectiveness
- Cost optimization opportunities
- Compliance issues
