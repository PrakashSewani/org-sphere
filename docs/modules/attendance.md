# Attendance

## Multi-Mode Attendance Tracking

OrgSphere supports flexible attendance tracking for office, remote, and hybrid work environments.

---

## Attendance Modes

### Office Attendance

- Check-in/check-out at office
- QR code scanning
- Biometric verification
- GPS verification

### Remote Work

- Self-declaration
- Activity tracking
- Online status
- Timezone-aware logging

### Hybrid Schedule

- Configurable hybrid policies
- Required office days
- Flexible days
- Schedule management

---

## Tracking Methods

### GPS

- Location verification
- Geofencing
- Location history
- Privacy controls

### QR Code

- Office-specific QR codes
- Scan to check-in
- Time-limited codes
- Anti-fraud measures

### Biometric

- Fingerprint
- Face recognition
- Integration with biometric devices
- Secure storage

### Manual

- Self check-in/check-out
- Manager approval
- Exception handling

---

## Time Tracking

### Clock In/Out

- One-click check-in
- Automatic check-out
- Break tracking
- Overtime calculation

### Timesheet

- Daily timesheet
- Weekly timesheet
- Project-based time
- Approval workflow

### Schedule

- Work schedule definition
- Shift management
- Holiday calendar
- Time-off integration

---

## Policies

### Policy Configuration

- Work hours per day
- Work days per week
- Grace period
- Overtime rules
- Break requirements

### Flexibility

- Flexible hours
- Core hours
- Compressed workweek
- Results-only work environment

---

## Graph Integration

### Relationships

```
Employee
  ├── ATTENDS → Office
  ├── SCHEDULED_FOR → Schedule
  ├── FOLLOWING → Policy
  └── TRACKED_BY → Method
```

### Graph Queries

- Who is in the office today?
- Which teams are remote?
- Office capacity utilization
- Attendance patterns by team

---

## Analytics

### Attendance Metrics

- Attendance rate
- Punctuality rate
- Overtime hours
- Remote work frequency

### Insights

- Office utilization trends
- Team attendance patterns
- Productivity correlations
- Compliance metrics

---

## Compliance

### Labor Law Compliance

- Working hours tracking
- Rest period enforcement
- Overtime regulations
- Record keeping

### Audit Trail

- Complete check-in/out history
- Modification history
- Approval history
- Exception history

---

## Integrations

### Calendar Systems

- Google Calendar
- Outlook Calendar
- Apple Calendar

### Time Tracking Tools

- Toggl
- Harvest
- Clockify

### Access Control

- Door access systems
- Badge systems
- Visitor management
