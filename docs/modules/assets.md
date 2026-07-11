# Asset Management

## Company Asset Tracking

Track company assets assigned to employees and manage the asset lifecycle.

---

## Asset Types

### Hardware

- Laptops
- Monitors
- Keyboards
- Mice
- Headsets
- Mobile devices
- Badges

### Software

- Licenses
- Subscriptions
- Tools
- Applications

### Equipment

- Office furniture
- Equipment
- Tools
- Vehicles

---

## Asset Lifecycle

### Procurement

- Purchase request
- Approval
- Procurement
- Receiving
- Asset tagging

### Assignment

- Assign to employee
- Assignment tracking
- Condition notes
- Photos

### Maintenance

- Scheduled maintenance
- Repair requests
- Warranty tracking
- Insurance

### Retirement

- Return from employee
- Condition assessment
- Disposal
- Data wiping

---

## Asset Records

### Asset Properties

- Asset ID
- Name
- Type
- Serial number
- Purchase date
- Purchase cost
- Warranty expiry
- Current condition
- Assigned to

### Asset History

- Assignment history
- Condition changes
- Maintenance history
- Location changes

---

## Graph Integration

### Relationships

```
Asset
  ├── ASSIGNED_TO → Employee
  ├── OWNS → Department
  ├── LOCATED_AT → Office
  ├── MAINTAINED_BY → Vendor
  └── RELATED_TO → Project
```

### Graph Queries

- Assets per employee
- Assets per department
- Unassigned assets
- Asset utilization

---

## Compliance

### Security

- Asset inventory
- Data wiping requirements
- Return tracking
- Audit trail

### Financial

- Depreciation tracking
- Insurance claims
- Budget impact
- Cost allocation

---

## Analytics

### Metrics

- Asset utilization
- Cost per employee
- Maintenance costs
- Return rate

### Insights

- Procurement optimization
- Replacement planning
- Cost reduction opportunities
- Compliance gaps
