# OrgSphere - Agent Instructions

You are working on **OrgSphere**, a graph-native Organization Operating Platform (OrgOS).

---

## Session Protocol

Every session MUST follow this sequence:

```
1. Read this file (AGENT.md) - ALWAYS
2. Read TODO.md - Find next unchecked item (or user specifies)
3. Detect user intent from their message
4. Auto-load relevant skill from skills/ folder
5. Check relevant docs/ before implementing
6. Execute the TODO item
7. Update documentation if needed
8. Run tests if code changed
9. Prompt user: "Module complete. Please test [what was built]."
10. Wait for user confirmation
11. Check off the item in TODO.md
12. End session
```

---

## Intent Detection & Auto-Skill Loading

### Detection Rules

| User Message Contains | Auto-Load Skill |
|----------------------|-----------------|
| "module", "feature", "implement", "build", "create", "add" | `skills/module-dev/SKILL.md` |
| "graph", "node", "edge", "relationship", "entity", "traverse" | `skills/org-graph/SKILL.md` |
| "test", "spec", "coverage", "assert", "mock" | `skills/test-workflow/SKILL.md` |
| "doc", "adr", "readme", "document", "changelog" | `skills/docs-workflow/SKILL.md` |
| "bug", "fix", "error", "issue", "broken" | Read AGENT.md + grep for related code |
| "review", "feedback", "improve", "refactor" | Read AGENT.md + design-principles.md |
| "deploy", "setup", "config" | Read AGENT.md + architecture docs |

### Skill Conflict Resolution

If multiple skills match:
1. Use the most specific match
2. If ambiguous, ask user for clarification
3. Default to module-dev for implementation tasks

---

## Agent Types

### Explore Agent

**Purpose**: Search codebase, find files, understand structure.

**When to Use**: Finding files, searching code, understanding structure, answering questions.

**Behavior**: Read AGENT.md → Search docs/ → Report findings concisely → Reference file paths

---

### Build Agent

**Purpose**: Implement features, create new code.

**When to Use**: Creating modules, implementing features, writing services, creating APIs.

**Auto-Loads**: `skills/module-dev/SKILL.md`

**Behavior**: Read module docs → Follow patterns → Update docs → Run tests

---

### Review Agent

**Purpose**: Review code, check quality, suggest improvements.

**When to Use**: Code review, quality checks, security review, performance review.

**Auto-Loads**: `skills/docs-workflow/SKILL.md` + design principles

**Behavior**: Check multi-tenant isolation → Verify graph integration → Ensure docs updates

---

### Docs Agent

**Purpose**: Update documentation, create ADRs.

**When to Use**: After architectural decisions, after feature implementation, when requirements change.

**Auto-Loads**: `skills/docs-workflow/SKILL.md`

**Behavior**: Follow docs-workflow → Use ADR template → Keep docs current → Link related docs

---

### Test Agent

**Purpose**: Write tests, run test suites, debug failures.

**When to Use**: Writing unit/integration/E2E tests, debugging failures, running test suites.

**Auto-Loads**: `skills/test-workflow/SKILL.md`

**Behavior**: Follow test-workflow → Ensure test isolation → Mock dependencies → Maintain coverage

---

## TODO Workflow

### How It Works

1. **New Session**: Agent reads AGENT.md + TODO.md
2. **Pick Item**: User specifies or agent picks next unchecked item
3. **Execute**: Agent completes the TODO item
4. **Prompt Test**: Agent says "Module complete. Please test [X]."
5. **User Tests**: User verifies the feature works
6. **Check Off**: Agent marks item as complete in TODO.md
7. **End Session**: Session ends, ready for next item

### Agent Responsibilities

- Read TODO.md at session start
- Complete ONE item per session
- Follow relevant skill patterns
- Update docs after implementation
- Prompt user to test before finishing
- Check off item after user confirms

### User Responsibilities

- Test the completed module/feature
- Confirm it works as expected
- Start new session for next item
- Specify which item to work on (or let agent pick)

### When Item Is Too Large

If a TODO item is too complex for one session:
1. Agent splits it into sub-tasks
2. Agent completes what's possible
3. Agent documents remaining work
4. Agent updates TODO.md with sub-items
5. Session ends, continues in next session

---

## Required Reading

| Document | When to Read |
|----------|--------------|
| [Product Vision](docs/vision/product-vision.md) | Always |
| [Design Principles](docs/vision/design-principles.md) | Always |
| [Terminology](docs/vision/terminology.md) | When unsure about terms |
| [Organization Graph](docs/architecture/organization-graph.md) | Before graph-related work |
| [System Design](docs/architecture/system-design.md) | Before architectural changes |
| [Module Docs](docs/modules/) | Before working on a specific module |
| [ADR Decisions](docs/decisions/) | Before making architectural decisions |

---

## Core Rules

### 1. Organization Graph First

Every feature must answer: **"How does this interact with the Organization Graph?"**

If it doesn't integrate with the graph, reconsider the design.

### 2. Documentation is Part of the Product

- Update docs when you change features
- Create ADRs for architectural decisions
- Keep module docs current
- Never let documentation become stale

### 3. Follow Design Principles

- Simplicity over cleverness
- Configuration over code
- Modularity over monolith
- Extensibility over rigidity

### 4. Code Conventions

- C# / .NET 10 for backend
- React for frontend
- REST for all CRUD operations
- SignalR for real-time graph updates
- Events for inter-service communication

### 5. Multi-Tenant Always

- Every query must include tenant context
- Never expose cross-tenant data
- Validate tenant access on every operation
- Log tenant context in all events

---

## Feature Development Workflow

### Step 1: Understand the Requirement

- Read the relevant module doc
- Understand how it integrates with the graph
- Identify affected entities and relationships

### Step 2: Design the Solution

- Check for existing patterns in codebase
- Follow established abstractions
- Consider edge cases
- Plan for multi-tenancy

### Step 3: Implement

- Write clean, documented code
- Follow existing code style
- Add tests for new functionality
- Handle errors gracefully

### Step 4: Update Documentation

- Update module doc if behavior changed
- Update API doc if endpoints changed
- Create ADR if architectural decision made
- Update relevant diagrams

### Step 5: Verify

- Run tests
- Run type checks
- Run linter
- Verify multi-tenant isolation

---

## Graph Integration Patterns

### Creating Entities

```csharp
await graphService.CreateNodeAsync(
    NodeType.Employee,
    new Dictionary<string, object>
    {
        ["Name"] = name,
        ["Email"] = email,
        ["Department"] = department
    });
```

### Creating Relationships

```csharp
await graphService.CreateEdgeAsync(
    EdgeType.REPORTS_TO,
    sourceId: employeeId,
    targetId: managerId);
```

### Querying

```csharp
var reports = await graphService.GetDirectReportsAsync(managerId);
```

---

## Skill Registry

| Skill | Path | Triggers |
|-------|------|----------|
| Organization Graph | `skills/org-graph/SKILL.md` | graph, node, edge, relationship, entity, traverse |
| Module Development | `skills/module-dev/SKILL.md` | module, feature, implement, build, create, service |
| Documentation | `skills/docs-workflow/SKILL.md` | doc, adr, readme, document, changelog |
| Testing | `skills/test-workflow/SKILL.md` | test, spec, coverage, assert, mock, unit, integration |

### Skill Loading Rules

1. **Always load AGENT.md** - No exceptions
2. **Load one primary skill** - Based on intent detection
3. **Load secondary docs** - If task is complex
4. **Never skip skill loading** - Even for simple tasks

---

## Task Routing

### Task Classification

| Task | Agent | Skill | Priority |
|------|-------|-------|----------|
| Find a file | Explore | - | Low |
| Understand code | Explore | - | Low |
| Create new module | Build | module-dev | High |
| Add feature to module | Build | module-dev | High |
| Work with graph | Build | org-graph | High |
| Fix a bug | Build | - | Medium |
| Write tests | Test | test-workflow | Medium |
| Review PR | Review | - | Medium |
| Update docs | Docs | docs-workflow | Medium |
| Architecture decision | Docs + Build | docs-workflow | High |

### Multi-Agent Tasks

1. **Feature Implementation**: Build (module-dev) → Test (test-workflow) → Docs (docs-workflow) → Review
2. **Bug Fix**: Explore → Build → Test (test-workflow) → Docs
3. **Refactor**: Explore → Review → Build (module-dev) → Test (test-workflow) → Docs
4. **Architecture Change**: Docs (docs-workflow) → Build → Test → Review

---

## Behavior Rules

### Always

- Read AGENT.md first (every session)
- Auto-load skill based on intent
- Check docs before implementing
- Follow existing patterns
- Validate tenant isolation
- Update documentation
- Run tests before completion
- Handle errors explicitly

### Never

- Skip skill loading
- Ignore detected intent
- Skip documentation updates
- Ignore multi-tenant isolation
- Hardcode values
- Skip tests
- Commit without running checks
- Make architectural decisions without ADR

---

## Error Handling

### When Uncertain

1. Read the relevant docs
2. Search for existing patterns
3. Ask for clarification
4. Document your decision

### When Blocked

1. Document the blocker
2. Check for alternatives
3. Ask for help
4. Don't commit broken code

### When Errors Occur

1. Stop immediately
2. Document the error
3. Fix with proper tests
4. Update docs to prevent recurrence

---

## Quality Gates

### Before Task Completion

- [ ] Relevant skill was loaded
- [ ] Code follows existing patterns
- [ ] Multi-tenant isolation verified
- [ ] Tests written and passing
- [ ] Documentation updated
- [ ] No lint errors
- [ ] No type errors
- [ ] Error handling implemented

### Before Merge

- [ ] All tests passing
- [ ] Code reviewed
- [ ] Documentation complete
- [ ] No security issues
- [ ] Performance acceptable
- [ ] Multi-tenant isolation verified

---

## Example Session Flows

### Example 1: "Create a leave management module"

```
1. READ AGENT.md ✓
2. DETECT intent: "create module" → module-dev skill
3. LOAD skills/module-dev/SKILL.md ✓
4. CHECK docs/modules/leave-management.md ✓
5. IMPLEMENT leave module following skill patterns
6. UPDATE docs if needed
7. WRITE tests following test-workflow
8. RUN tests
9. REPORT completion
```

### Example 2: "Add team membership relationship to graph"

```
1. READ AGENT.md ✓
2. DETECT intent: "relationship, graph" → org-graph skill
3. LOAD skills/org-graph/SKILL.md ✓
4. CHECK docs/architecture/organization-graph.md ✓
5. IMPLEMENT using graph patterns from skill
6. UPDATE docs if needed
7. WRITE tests
8. RUN tests
9. REPORT completion
```

### Example 3: "Fix bug in leave approval flow"

```
1. READ AGENT.md ✓
2. DETECT intent: "fix bug" → no specific skill, read module doc
3. GREP for leave approval code
4. READ docs/modules/leave-management.md ✓
5. DIAGNOSE the issue
6. FIX with tests
7. UPDATE docs if behavior changed
8. RUN tests
9. REPORT completion
```

### Example 4: "Write tests for employee service"

```
1. READ AGENT.md ✓
2. DETECT intent: "write tests" → test-workflow skill
3. LOAD skills/test-workflow/SKILL.md ✓
4. READ employee service code
5. WRITE unit tests following skill patterns
6. WRITE integration tests if needed
7. RUN tests
8. REPORT coverage
```

---

> **Remember**: You are building a product, not just writing code. Every decision affects the entire system. Think like a CTO.
