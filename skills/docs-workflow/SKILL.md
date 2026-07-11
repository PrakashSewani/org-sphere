# Documentation Workflow Skill

Load this skill when updating or creating documentation for OrgSphere.

---

## When to Use

- After implementing a feature
- After making architectural decisions
- After changing module behavior
- When creating new modules
- When fixing bugs that affect documentation

---

## Documentation Rules

### 1. Documentation Evolves with Code

Never let documentation become stale. If the code changes, update the docs.

### 2. Every Architectural Decision Gets an ADR

Create an ADR when:
- Choosing a technology
- Changing architecture
- Making a significant design decision
- Changing established patterns

### 3. Module Docs Stay Current

Update module docs when:
- Adding new features
- Changing behavior
- Modifying APIs
- Updating permissions

### 4. Requirements Update When Scope Changes

Update business requirements when:
- Adding new modules
- Changing features
- Modifying constraints
- Changing priorities

---

## Documentation Structure

### Vision Docs

- **product-vision.md**: Why OrgSphere exists
- **product-goals.md**: What OrgSphere achieves
- **design-principles.md**: How we build
- **terminology.md**: Shared vocabulary

### Architecture Docs

- **overview.md**: High-level architecture
- **organization-graph.md**: The central abstraction
- **system-design.md**: Technical design
- **tenancy.md**: Multi-tenant isolation
- **permissions.md**: Authorization model
- **workflows.md**: Workflow engine
- **billing.md**: Stripe integration

### Module Docs

- **[module-name].md**: Module-specific documentation

### Business Docs

- **business-requirements.md**: Core requirements
- **personas.md**: User archetypes
- **pricing.md**: Pricing strategy

---

## ADR Template

```markdown
# ADR-XXX: [Decision Title]

## Status

[Proposed | Accepted | Deprecated | Superseded by ADR-YYY]

## Date

YYYY-MM-DD

## Context

[What is the issue that we're seeing that motivates this decision?]

## Decision

[What is the change that we're proposing and/or doing?]

## Consequences

### Positive

- [Benefit 1]
- [Benefit 2]

### Negative

- [Drawback 1]
- [Drawback 2]

### Neutral

- [Neutral consideration 1]

## Alternatives Considered

1. **[Alternative 1]**: [Why not chosen]
2. **[Alternative 2]**: [Why not chosen]

## References

- [Link to related docs]
```

---

## Module Doc Template

```markdown
# [Module Name]

## Overview

[What this module does and why it exists]

## Features

### [Feature 1]

[Description]

### [Feature 2]

[Description]

## Graph Integration

### Relationships

[How this module integrates with the Organization Graph]

### Graph Queries

[Common graph queries used by this module]

## Data Model

### [Entity]

[Entity properties and relationships]

## API

### Endpoints

[API endpoints for this module]

### Events

[Events published by this module]

## Configuration

[Configuration options for this module]

## Permissions

[Permission definitions for this module]

## Analytics

[Metrics and insights from this module]
```

---

## Update Workflow

### After Implementing a Feature

1. Check if module doc needs update
2. Check if API doc needs update
3. Check if architecture doc needs update
4. Update relevant documentation

### After Architectural Decision

1. Create ADR
2. Update architecture docs
3. Update relevant module docs
4. Update README if needed

### After Bug Fix

1. Check if docs had incorrect information
2. Update docs if they were wrong
3. Add notes about the fix if relevant

---

## Documentation Checklist

Before considering a feature complete:

- [ ] Module doc updated (if applicable)
- [ ] API doc updated (if endpoints changed)
- [ ] ADR created (if architectural decision)
- [ ] README updated (if major change)
- [ ] Examples updated (if behavior changed)
- [ ] Diagrams updated (if structure changed)

---

## Writing Style

### Be Clear

- Use simple language
- Avoid jargon when possible
- Define terms when first used

### Be Complete

- Include all necessary information
- Provide examples
- Link to related docs

### Be Current

- Update when changes happen
- Remove outdated information
- Mark deprecated features

### Be Consistent

- Use consistent formatting
- Follow naming conventions
- Use the terminology from terminology.md

---

## Common Documentation Tasks

### Adding a New Module

1. Create module doc from template
2. Add to docs/README.md
3. Update architecture overview if needed
4. Update business requirements if needed

### Changing Module Behavior

1. Update module doc
2. Update API doc if endpoints changed
3. Create ADR if architectural decision
4. Update examples if needed

### Adding New Entity

1. Update organization-graph.md
2. Update module doc
3. Update terminology.md if needed
4. Add to relevant diagrams
