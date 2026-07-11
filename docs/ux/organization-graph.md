# Organization Graph UX

## Graph Interaction Design

The Organization Graph is the primary interface. It should feel like Figma, Miro, or React Flow.

---

## Canvas

### Infinite Canvas

- Pan by dragging
- Zoom with scroll/pinch
- Minimap for navigation
- Grid/snap options

### Viewport Controls

- Fit to screen
- Zoom to selection
- Reset view
- Save view states

---

## Node Interactions

### Selection

- Click to select
- Shift+click for multi-select
- Drag to select area
- Ctrl+A for all

### Manipulation

- Drag to move
- Double-click to edit
- Right-click for context menu
- Delete key to remove

### Connection

- Drag from port to create edge
- Drop on target to connect
- Click to select edge
- Delete to remove edge

---

## Visual Design

### Node Styles

```
┌──────────────────┐
│  Department      │  ← Header (color-coded)
│  ──────────────  │
│  👤 Alice (Head) │  ← Key info
│  👥 12 members   │
│  📍 New York     │
└──────────────────┘
```

### Color Coding

| Entity | Color |
|--------|-------|
| Company | Blue |
| Department | Green |
| Team | Purple |
| Employee | Gray |
| Project | Orange |
| Office | Teal |

### State Indicators

- Active: Solid border
- Inactive: Dashed border
- Selected: Highlighted border
- Hover: Subtle highlight

---

## Interactions

### Zoom Levels

| Level | View |
|-------|------|
| 100% | Full organization |
| 75% | Department level |
| 50% | Team level |
| 25% | Individual level |

### Context Menu

Right-click on entity:

```
┌─────────────────────┐
│ View Details        │
│ Edit                │
│ Add Report          │
│ Move to Team        │
│ Simulate Change     │
│ ─────────────────── │
│ Copy                │
│ Export              │
│ Delete              │
└─────────────────────┘
```

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `⌘ + Z` | Undo |
| `⌘ + Shift + Z` | Redo |
| `⌘ + C` | Copy |
| `⌘ + V` | Paste |
| `⌘ + A` | Select all |
| `Delete` | Delete selected |
| `⌘ + +` | Zoom in |
| `⌘ + -` | Zoom out |
| `⌘ + 0` | Fit to screen |

---

## Sidebar

### Entity Details

When an entity is selected, show details in sidebar:

```
┌─────────────────────────────┐
│  ← Back                     │
│                             │
│  Engineering Department     │
│  ─────────────────────────  │
│                             │
│  👤 Head: Alice Johnson     │
│  👥 Members: 12             │
│  📍 Office: New York        │
│  💰 Budget: $2.5M           │
│                             │
│  ─────────────────────────  │
│                             │
│  Teams:                     │
│  ├── Frontend (5)           │
│  ├── Backend (4)            │
│  └── DevOps (3)             │
│                             │
│  ─────────────────────────  │
│                             │
│  Actions:                   │
│  [Edit] [Add Team] [More]  │
└─────────────────────────────┘
```

---

## Toolbar

### Top Toolbar

```
┌─────────────────────────────────────────────────────┐
│ ← → │ 🔍 Search │ 📊 View │ ➕ Add │ ⚙️ Settings │
└─────────────────────────────────────────────────────┘
```

### View Options

- Full organization
- Department view
- Team view
- Reporting view
- Matrix view
- Timeline view

---

## Search

### In-Graph Search

- Type to search
- Highlight matching nodes
- Filter by type
- Filter by property

### Results

- Click to focus
- Double-click to select
- Enter to open details

---

## Simulation Mode

### Toggle

- Switch to simulation mode
- Make changes
- See impact
- Apply or discard

### Visual Diff

- Added entities (green)
- Removed entities (red)
- Modified entities (yellow)
- Unchanged entities (gray)

---

## Collaboration

### Real-Time

- See other users' cursors
- Live updates
- Conflict resolution
- User presence indicators

### Comments

- Comment on any entity
- Thread discussions
- @mention users
- Resolve comments

---

## Performance

### Optimization

- Virtual rendering
- Level of detail
- Lazy loading
- Caching

### Indicators

- Loading spinner
- Progress bar
- Node count
- Zoom level
