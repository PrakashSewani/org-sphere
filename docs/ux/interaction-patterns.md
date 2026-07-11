# Interaction Patterns

## UI Patterns

Common interaction patterns used throughout OrgSphere.

---

## Form Patterns

### Single-Page Form

For simple forms with few fields:

```
┌─────────────────────────────┐
│  Form Title                 │
│  ─────────────────────────  │
│                             │
│  Field 1 [________]         │
│  Field 2 [________]         │
│  Field 3 [________]         │
│                             │
│  ─────────────────────────  │
│  [Cancel]           [Save]  │
└─────────────────────────────┘
```

### Multi-Step Form

For complex forms:

```
┌─────────────────────────────┐
│  Step 1 ── Step 2 ── Step 3 │
│  ─────────────────────────  │
│                             │
│  Current step content       │
│                             │
│  ─────────────────────────  │
│  [Back]           [Next]    │
└─────────────────────────────┘
```

### Modal Form

For quick actions:

```
┌─────────────────────────────┐
│  Modal Title           [X]  │
│  ─────────────────────────  │
│                             │
│  Form content               │
│                             │
│  ─────────────────────────  │
│  [Cancel]           [Save]  │
└─────────────────────────────┘
```

---

## List Patterns

### Simple List

```
┌─────────────────────────────┐
│  Item 1              [>]   │
│  Item 2              [>]   │
│  Item 3              [>]   │
└─────────────────────────────┘
```

### List with Actions

```
┌─────────────────────────────┐
│  Item 1        [Edit] [⋮]  │
│  Item 2        [Edit] [⋮]  │
│  Item 3        [Edit] [⋮]  │
└─────────────────────────────┘
```

### Grouped List

```
┌─────────────────────────────┐
│  Group A                   │
│  ├── Item 1        [⋮]    │
│  └── Item 2        [⋮]    │
│  Group B                   │
│  ├── Item 3        [⋮]    │
│  └── Item 4        [⋮]    │
└─────────────────────────────┘
```

---

## Table Patterns

### Simple Table

```
┌─────────────────────────────┐
│  Name      │ Role    │ Dept │
│  ─────────────────────────  │
│  Alice     │ Manager │ Eng  │
│  Bob       │ Lead    │ Eng  │
│  Carol     │ Dev     │ Eng  │
└─────────────────────────────┘
```

### Sortable Table

```
┌─────────────────────────────┐
│  Name ↕     │ Role ↕       │
│  ─────────────────────────  │
│  Alice ↑    │ Manager      │
│  Bob        │ Lead         │
│  Carol ↓    │ Dev          │
└─────────────────────────────┘
```

### Actionable Table

```
┌─────────────────────────────┐
│  Name     │ Status │ Actions│
│  ─────────────────────────  │
│  Alice    │ Active │ [⋮]   │
│  Bob      │ Active │ [⋮]   │
│  Carol    │ Inactive│ [⋮]  │
└─────────────────────────────┘
```

---

## Card Patterns

### Metric Card

```
┌──────────────────┐
│  Title           │
│  ████████████ 92%│
│  +5.2% from last │
└──────────────────┘
```

### Action Card

```
┌──────────────────┐
│  Icon            │
│  Title           │
│  Description     │
│  [Action Button] │
└──────────────────┘
```

### Entity Card

```
┌──────────────────┐
│  Avatar          │
│  Name            │
│  Role            │
│  Department      │
│  [View Profile]  │
└──────────────────┘
```

---

## Feedback Patterns

### Toast Notification

```
┌─────────────────────────────┐
│  ✓ Success message     [X] │
└─────────────────────────────┘
```

### Alert Banner

```
┌─────────────────────────────┐
│  ⚠ Warning message    [X]  │
└─────────────────────────────┘
```

### Inline Validation

```
┌─────────────────────────────┐
│  Email [invalid@email  ]    │
│        ↑ Please enter valid │
└─────────────────────────────┘
```

---

## Navigation Patterns

### Tabs

```
┌─────────────────────────────┐
│  Tab 1 │ Tab 2 │ Tab 3     │
│  ─────────────────────────  │
│  Content for selected tab   │
└─────────────────────────────┘
```

### Accordion

```
┌─────────────────────────────┐
│  Section 1            [▼]  │
│  ─────────────────────────  │
│  Content                    │
├─────────────────────────────┤
│  Section 2            [▶]  │
└─────────────────────────────┘
```

### Breadcrumbs

```
Home > Department > Team > Employee
```

---

## Data Patterns

### Empty State

```
┌─────────────────────────────┐
│                             │
│        📭 No items          │
│                             │
│  Create your first item     │
│  [Create Button]            │
│                             │
└─────────────────────────────┘
```

### Loading State

```
┌─────────────────────────────┐
│                             │
│        ⏳ Loading...        │
│                             │
└─────────────────────────────┘
```

### Error State

```
┌─────────────────────────────┐
│                             │
│        ❌ Error occurred    │
│                             │
│  [Retry Button]             │
│                             │
└─────────────────────────────┘
```
