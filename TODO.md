# OrgSphere - Project TODO

> **Instructions for Agent**: Read this file at session start. Pick ONE unchecked item. Complete it. Prompt user to test. Check it off. End session. Next session picks next item.

> **Instructions for User**: After agent completes a module, test it. If working, start a new session and tell agent which TODO item to work on next.

---

## How This Works

1. Start new session
2. Agent reads AGENT.md + this file
3. Agent picks next unchecked item (or user specifies)
4. Agent completes the item
5. Agent prompts: "Module complete. Please test [module/feature]."
6. User tests, confirms working
7. Agent checks off the item
8. Session ends
9. New session starts for next item

---

## Phase 1: Foundation

### Project Setup

- [x] **TODO-001**: Initialize project structure (.NET solution, Clean Architecture, Docker, gitignore)
- [x] **TODO-002**: Set up Docker for Neo4j and PostgreSQL databases
- [x] **TODO-003**: Set up development environment (dotnet format, pre-commit hooks)
- [x] **TODO-004**: Set up CI/CD pipeline (GitHub Actions: lint, test, build)

### Database

- [x] **TODO-005**: Set up graph database (Neo4j) with Docker (docker-compose.yml)
- [x] **TODO-006**: Set up relational database (PostgreSQL) with Docker
- [ ] **TODO-007**: Create database migration system
- [ ] **TODO-008**: Create seed data system for development

### Core Services

- [ ] **TODO-009**: Build multi-tenant middleware (tenant context injection, isolation)
- [ ] **TODO-010**: Build core graph service (createNode, createEdge, traverse, query)
- [ ] **TODO-011**: Build event bus service (publish, subscribe, event store)
- [ ] **TODO-012**: Build authentication service (JWT, login, register, refresh)
- [ ] **TODO-013**: Build authorization service (RBAC, permission checks)

### Testing Foundation

- [ ] **TODO-014**: Set up test framework (xUnit, test utilities, fixtures)
- [ ] **TODO-015**: Write tests for graph service
- [ ] **TODO-016**: Write tests for auth service
- [ ] **TODO-017**: Write tests for multi-tenant isolation

---

## Phase 2: Core Modules

### Organization Graph

- [ ] **TODO-017**: Build Organization Graph schema (Company, Region, Office, Department, Team, Employee nodes)
- [ ] **TODO-018**: Build Organization Graph API (CRUD for all entity types)
- [ ] **TODO-019**: Build Organization Graph resolvers (GraphQL)
- [ ] **TODO-020**: Write tests for Organization Graph

### Employee Management

- [ ] **TODO-021**: Build Employee module (types, service, repository)
- [ ] **TODO-022**: Build Employee API (REST + GraphQL endpoints)
- [ ] **TODO-023**: Build Employee self-service (profile update, document upload)
- [ ] **TODO-024**: Write tests for Employee module

### Leave Management

- [ ] **TODO-025**: Build Leave module (types, service, repository)
- [ ] **TODO-026**: Build Leave policies configuration
- [ ] **TODO-027**: Build Leave request workflow (submit, approve, reject)
- [ ] **TODO-028**: Build Leave balance tracking
- [ ] **TODO-029**: Write tests for Leave module

### Attendance

- [ ] **TODO-030**: Build Attendance module (types, service, repository)
- [ ] **TODO-031**: Build check-in/check-out functionality
- [ ] **TODO-032**: Build attendance policies and schedules
- [ ] **TODO-033**: Write tests for Attendance module

### Approval Engine

- [ ] **TODO-034**: Build Approval module (types, service, repository)
- [ ] **TODO-035**: Build approval chain configuration
- [ ] **TODO-036**: Build graph-aware approval routing
- [ ] **TODO-037**: Build delegation and escalation
- [ ] **TODO-038**: Write tests for Approval module

---

## Phase 3: Organization Designer

### Visual Graph

- [ ] **TODO-039**: Set up React app with graph visualization library (React Flow)
- [ ] **TODO-040**: Build infinite canvas with zoom/pan
- [ ] **TODO-041**: Build node rendering (Department, Team, Employee nodes)
- [ ] **TODO-042**: Build edge rendering (reporting lines, membership)
- [ ] **TODO-043**: Build node selection and multi-select
- [ ] **TODO-044**: Build drag-and-drop node repositioning

### Graph Interactions

- [ ] **TODO-045**: Build create entity modal (add department, team, employee)
- [ ] **TODO-046**: Build edit entity sidebar (view/edit properties)
- [ ] **TODO-047**: Build connect entities (drag to create reporting line)
- [ ] **TODO-048**: Build delete entity confirmation
- [ ] **TODO-049**: Build context menu (right-click actions)

### Graph Views

- [ ] **TODO-050**: Build full organization view
- [ ] **TODO-051**: Build department-focused view
- [ ] **TODO-052**: Build team-focused view
- [ ] **TODO-053**: Build minimap for navigation

### Graph Features

- [ ] **TODO-054**: Build search within graph (highlight matching nodes)
- [ ] **TODO-055**: Build filter by entity type
- [ ] **TODO-056**: Build simulation mode (what-if scenarios)
- [ ] **TODO-057**: Build graph validation (circular reference prevention)

---

## Phase 4: UI & UX

### App Shell

- [ ] **TODO-058**: Build app shell (sidebar, topbar, main content)
- [ ] **TODO-059**: Build routing (React Router)
- [ ] **TODO-060**: Build authentication flow (login, register, forgot password)
- [ ] **TODO-061**: Build protected routes

### Dashboard

- [ ] **TODO-062**: Build executive dashboard (headcount, growth, key metrics)
- [ ] **TODO-063**: Build HR dashboard (pending actions, leave, attendance)
- [ ] **TODO-064**: Build manager dashboard (team size, approvals, availability)
- [ ] **TODO-065**: Build employee dashboard (profile, leave balance, requests)

### Employee UI

- [ ] **TODO-066**: Build employee list view (table, filters, search)
- [ ] **TODO-067**: Build employee detail view (profile, timeline, skills)
- [ ] **TODO-068**: Build employee create/edit form

### Leave UI

- [ ] **TODO-069**: Build leave request form
- [ ] **TODO-070**: Build leave calendar view
- [ ] **TODO-071**: Build leave requests list (pending, approved, rejected)
- [ ] **TODO-072**: Build leave policy configuration UI

### Approval UI

- [ ] **TODO-073**: Build pending approvals view
- [ ] **TODO-074**: Build approval detail view
- [ ] **TODO-075**: Build approval history view

---

## Phase 5: Features

### Communication

- [ ] **TODO-076**: Build announcement system (create, publish, target)
- [ ] **TODO-077**: Build notification center (in-app notifications)
- [ ] **TODO-078**: Build notification preferences

### Analytics

- [ ] **TODO-079**: Build analytics service (metrics calculation)
- [ ] **TODO-080**: Build analytics dashboards (charts, graphs)
- [ ] **TODO-081**: Build custom report builder
- [ ] **TODO-082**: Build report export (PDF, CSV)

### Search

- [ ] **TODO-083**: Build search service (full-text, fuzzy)
- [ ] **TODO-084**: Build search UI (global search bar, results)
- [ ] **TODO-085**: Build relationship-aware search

### AI Assistant

- [ ] **TODO-086**: Build AI service (natural language query processing)
- [ ] **TODO-087**: Build AI chat interface
- [ ] **TODO-088**: Build AI insights dashboard widget

---

## Phase 6: Business Features

### Billing

- [ ] **TODO-089**: Set up Stripe integration (products, prices, subscriptions)
- [ ] **TODO-090**: Build checkout flow (plan selection, payment)
- [ ] **TODO-091**: Build subscription management (upgrade, downgrade, cancel)
- [ ] **TODO-092**: Build usage reporting (member count → Stripe)
- [ ] **TODO-093**: Build PPP regional pricing
- [ ] **TODO-094**: Build billing portal (invoices, payment methods)

### Self-Hosted License

- [ ] **TODO-095**: Build license key generation
- [ ] **TODO-096**: Build license validation system
- [ ] **TODO-097**: Build license purchase flow

### Admin

- [ ] **TODO-098**: Build tenant admin dashboard
- [ ] **TODO-099**: Build user management (invite, deactivate, roles)
- [ ] **TODO-100**: Build settings management (company, leave, attendance, approvals)

---

## Phase 7: Polish & Launch

### Testing

- [ ] **TODO-101**: Write integration tests for all modules
- [ ] **TODO-102**: Write E2E tests for critical user journeys
- [ ] **TODO-103**: Perform security audit
- [ ] **TODO-104**: Perform performance testing

### Documentation

- [ ] **TODO-105**: Write API documentation (OpenAPI/Swagger)
- [ ] **TODO-106**: Write user guide
- [ ] **TODO-107**: Write deployment guide
- [ ] **TODO-108**: Write self-hosted installation guide

### Deployment

- [ ] **TODO-109**: Set up production infrastructure (Docker, Kubernetes)
- [ ] **TODO-110**: Set up monitoring and alerting
- [ ] **TODO-111**: Set up backup and recovery
- [ ] **TODO-112**: Deploy to production (Cloud SaaS)

### Launch Prep

- [ ] **TODO-113**: Create landing page
- [ ] **TODO-114**: Set up analytics tracking (product analytics)
- [ ] **TODO-115**: Create onboarding flow for new tenants
- [ ] **TODO-116**: Beta testing with real users

---

## Progress

| Phase | Total | Completed | Status |
|-------|-------|-----------|--------|
| Phase 1: Foundation | 16 | 4 | In Progress |
| Phase 2: Core Modules | 22 | 0 | Not Started |
| Phase 3: Org Designer | 19 | 0 | Not Started |
| Phase 4: UI & UX | 18 | 0 | Not Started |
| Phase 5: Features | 14 | 0 | Not Started |
| Phase 6: Business | 13 | 0 | Not Started |
| Phase 7: Polish | 16 | 0 | Not Started |
| **Total** | **118** | **4** | **3%** |

---

## Notes

### Dependency Order

- Phase 1 must complete before Phase 2
- Phase 2 must complete before Phase 3
- Phase 3 can parallel with Phase 4
- Phase 5 depends on Phase 2
- Phase 6 can start after Phase 2
- Phase 7 is last

### Session Guidelines

- Each TODO item = one agent session
- If item is too large, agent should split it
- Agent must prompt user to test before checking off
- User must confirm working before next session

### Blocked Items

If an item is blocked:
1. Document the blocker
2. Skip to next unblocked item
3. Return to blocked item when unblocked
