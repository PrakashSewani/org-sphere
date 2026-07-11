# Accessibility

## Accessibility Standards

OrgSphere follows WCAG 2.1 Level AA standards.

---

## Core Principles

### Perceivable

- Text alternatives for non-text content
- Captions for multimedia
- Content adaptable to different presentations
- Sufficient color contrast

### Operable

- Keyboard accessible
- Enough time to read content
- No seizure-inducing content
- Navigable and findable

### Understandable

- Readable and understandable
- Predictable operation
- Input assistance

### Robust

- Compatible with assistive technologies

---

## Keyboard Navigation

### Tab Order

- Logical tab order
- Visible focus indicators
- Skip navigation links
- Focus trapping in modals

### Shortcuts

| Shortcut | Action |
|----------|--------|
| Tab | Move to next element |
| Shift+Tab | Move to previous element |
| Enter | Activate button/link |
| Space | Toggle checkbox |
| Arrow keys | Navigate within groups |
| Escape | Close modal/menu |

---

## Screen Reader Support

### ARIA Labels

```html
<button aria-label="Add employee">+</button>
<nav aria-label="Main navigation">...</nav>
<div role="alert">Error message</div>
```

### Live Regions

```html
<div aria-live="polite">Status update</div>
<div aria-live="assertive">Error message</div>
```

### Landmarks

```html
<header role="banner">...</header>
<nav role="navigation">...</nav>
<main role="main">...</main>
<footer role="contentinfo">...</footer>
```

---

## Color & Contrast

### Contrast Ratios

- Normal text: 4.5:1 minimum
- Large text: 3:1 minimum
- UI components: 3:1 minimum

### Color Usage

- Never use color alone to convey information
- Always provide text/icon alternatives
- Support high contrast mode
- Color-blind friendly palettes

### Focus Indicators

- Visible focus ring
- High contrast focus ring
- Consistent focus styling

---

## Forms

### Labels

- Every input has a label
- Labels are associated with inputs
- Required fields marked
- Error messages linked

### Validation

- Inline validation
- Clear error messages
- Error summary at top
- Focus management on error

### Instructions

- Help text provided
- Format examples
- Required field indicators
- Character limits

---

## Charts & Visualizations

### Text Alternatives

- Data tables for charts
- Chart descriptions
- Summary statistics
- Trend descriptions

### Keyboard Navigation

- Navigate data points
- Select data series
- Zoom and pan
- Export data

---

## Multimedia

### Video

- Captions
- Audio descriptions
- Transcript
- Playback controls

### Audio

- Transcript
- Volume controls
- No auto-play

---

## Responsive Design

### Mobile

- Touch targets (44x44px minimum)
- Pinch to zoom
- Orientation support
- No horizontal scrolling

### Desktop

- Resizable up to 200%
- No loss of content
- Reflow at zoom levels
- Readable at all sizes

---

## Testing

### Automated Testing

- axe-core integration
- Lighthouse audits
- CI/CD accessibility checks

### Manual Testing

- Keyboard-only navigation
- Screen reader testing
- Color contrast verification
- Mobile accessibility

### User Testing

- Users with disabilities
- Assistive technology users
- Feedback incorporation

---

## Components

### Button

```html
<button 
  aria-label="Delete employee"
  aria-describedby="delete-help"
>
  Delete
</button>
<span id="delete-help">This action cannot be undone</span>
```

### Modal

```html
<div role="dialog" aria-labelledby="modal-title">
  <h2 id="modal-title">Confirm Delete</h2>
  <p>Are you sure?</p>
  <button>Cancel</button>
  <button>Delete</button>
</div>
```

### Table

```html
<table aria-label="Employee list">
  <thead>
    <tr>
      <th scope="col">Name</th>
      <th scope="col">Role</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Alice</td>
      <td>Manager</td>
    </tr>
  </tbody>
</table>
```
