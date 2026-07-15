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
- [x] **TODO-002**: Set up Docker for Neo4j and Redis databases
- [x] **TODO-003**: Set up development environment (dotnet format, pre-commit hooks)
- [x] **TODO-004**: Set up CI/CD pipeline (GitHub Actions: lint, test, build)

### Database

- [x] **TODO-005**: Set up graph database (Neo4j) with Docker (docker-compose.yml)
- [x] **TODO-006**: Set up Redis for caching and refresh token storage
- [x] **TODO-007**: Create database migration system
- [x] **TODO-008**: Create seed data system for development

### Core Services

- [x] **TODO-009**: Build multi-tenant middleware (tenant context injection, isolation)
- [x] **TODO-010**: Build core graph service (createNode, createEdge, traverse, query)
- [x] **TODO-011**: Build event bus service (publish, subscribe, event store)
- [x] **TODO-012**: Build authentication service (JWT, login, register, refresh)
- [x] **TODO-013**: Build authorization service (RBAC, permission checks)

### Testing Foundation

- [x] **TODO-014**: Set up test framework (xUnit, test utilities, fixtures)
- [x] **TODO-015**: Write tests for graph service
- [x] **TODO-016**: Write tests for auth service
- [x] **TODO-017**: Write tests for multi-tenant isolation

---

## Phase 2: Core Modules

### Organization Graph

- [x] **TODO-018**: Build Organization Graph schema (Company, Region, Office, Department, Team, Employee nodes)
- [x] **TODO-019**: Build Organization Graph API (CRUD for all entity types)
- [x] **TODO-020**: Build SignalR hub for real-time graph updates
- [x] **TODO-021**: Write tests for Organization Graph

### Employee Management

- [x] **TODO-022**: Build Employee module (types, service, repository)
- [x] **TODO-023**: Build Employee API (REST endpoints)
- [x] **TODO-024**: Build Employee self-service (profile update, document upload)
- [x] **TODO-025**: Write tests for Employee module

### Leave Management

- [x] **TODO-026**: Build Leave module (types, service, repository)
- [x] **TODO-027**: Build Leave policies configuration
- [x] **TODO-028**: Build Leave request workflow (submit, approve, reject)
- [x] **TODO-029**: Build Leave balance tracking
- [x] **TODO-030**: Write tests for Leave module

### Attendance

- [x] **TODO-031**: Build Attendance module (types, service, repository)
- [x] **TODO-032**: Build check-in/check-out functionality
- [x] **TODO-033**: Build attendance policies and schedules
- [x] **TODO-034**: Write tests for Attendance module

### Approval Engine

- [ ] **TODO-035**: Build Approval module (types, service, repository)
- [ ] **TODO-036**: Build approval chain configuration
- [ ] **TODO-037**: Build graph-aware approval routing
- [ ] **TODO-038**: Build delegation and escalation
- [ ] **TODO-039**: Write tests for Approval module

---

## Phase 3: Organization Designer

### Visual Graph

- [ ] **TODO-040**: Set up React app with graph visualization library (React Flow)
- [ ] **TODO-041**: Build infinite canvas with zoom/pan
- [ ] **TODO-042**: Build node rendering (Department, Team, Employee nodes)
- [ ] **TODO-043**: Build edge rendering (reporting lines, membership)
- [ ] **TODO-044**: Build node selection and multi-select
- [ ] **TODO-045**: Build drag-and-drop node repositioning

### Graph Interactions

- [ ] **TODO-046**: Build create entity modal (add department, team, employee)
- [ ] **TODO-047**: Build edit entity sidebar (view/edit properties)
- [ ] **TODO-048**: Build connect entities (drag to create reporting line)
- [ ] **TODO-049**: Build delete entity confirmation
- [ ] **TODO-050**: Build context menu (right-click actions)

### Graph Views

- [ ] **TODO-051**: Build full organization view
- [ ] **TODO-052**: Build department-focused view
- [ ] **TODO-053**: Build team-focused view
- [ ] **TODO-054**: Build minimap for navigation

### Graph Features

- [ ] **TODO-055**: Build search within graph (highlight matching nodes)
- [ ] **TODO-056**: Build filter by entity type
- [ ] **TODO-057**: Build simulation mode (what-if scenarios)
- [ ] **TODO-058**: Build graph validation (circular reference prevention)

---

## Phase 4: UI & UX

### App Shell

- [ ] **TODO-059**: Build app shell (sidebar, topbar, main content)
- [ ] **TODO-060**: Build routing (React Router)
- [ ] **TODO-061**: Build authentication flow (login, register, forgot password)
- [ ] **TODO-062**: Build protected routes

### Dashboard

- [ ] **TODO-063**: Build executive dashboard (headcount, growth, key metrics)
- [ ] **TODO-064**: Build HR dashboard (pending actions, leave, attendance)
- [ ] **TODO-065**: Build manager dashboard (team size, approvals, availability)
- [ ] **TODO-066**: Build employee dashboard (profile, leave balance, requests)

### Employee UI

- [ ] **TODO-067**: Build employee list view (table, filters, search)
- [ ] **TODO-068**: Build employee detail view (profile, timeline, skills)
- [ ] **TODO-069**: Build employee create/edit form

### Leave UI

- [ ] **TODO-070**: Build leave request form
- [ ] **TODO-071**: Build leave calendar view
- [ ] **TODO-072**: Build leave requests list (pending, approved, rejected)
- [ ] **TODO-073**: Build leave policy configuration UI

### Approval UI

- [ ] **TODO-074**: Build pending approvals view
- [ ] **TODO-075**: Build approval detail view
- [ ] **TODO-076**: Build approval history view

---

## Phase 5: Features

### Communication

- [ ] **TODO-077**: Build announcement system (create, publish, target)
- [ ] **TODO-078**: Build notification center (in-app notifications)
- [ ] **TODO-079**: Build notification preferences

### Analytics

- [ ] **TODO-080**: Build analytics service (metrics calculation)
- [ ] **TODO-081**: Build analytics dashboards (charts, graphs)
- [ ] **TODO-082**: Build custom report builder
- [ ] **TODO-083**: Build report export (PDF, CSV)

### Search

- [ ] **TODO-084**: Build search service (full-text, fuzzy)
- [ ] **TODO-085**: Build search UI (global search bar, results)
- [ ] **TODO-086**: Build relationship-aware search

### AI Assistant

- [ ] **TODO-087**: Build AI service (natural language query processing)
- [ ] **TODO-088**: Build AI chat interface
- [ ] **TODO-089**: Build AI insights dashboard widget

---

## Phase 6: Business Features

### Billing (Separate Service — OrgSphere.Billing repo)

> **Note**: Billing is a separate microservice. These items are for the **core app only** — integrating with the license validator. The Stripe integration lives in the `OrgSphere.Billing` repo.

- [ ] **TODO-090**: Build `ILicenseValidator` interface in OrgSphere.Domain
- [ ] **TODO-091**: Build offline license key validator (RSA signature verification) for self-hosted
- [ ] **TODO-092**: Build JWT-based license validator for SaaS deployment
- [ ] **TODO-093**: Build license status middleware (check license on tenant init)
- [ ] **TODO-094**: Build tier enforcement (max members, feature gating by tier)

### Admin

- [ ] **TODO-095**: Build tenant admin dashboard
- [ ] **TODO-096**: Build user management (invite, deactivate, roles)
- [ ] **TODO-097**: Build settings management (company, leave, attendance, approvals)

---

## Phase 7: Polish & Launch

### Testing

- [ ] **TODO-098**: Write integration tests for all modules
- [ ] **TODO-099**: Write E2E tests for critical user journeys
- [ ] **TODO-100**: Perform security audit
- [ ] **TODO-101**: Perform performance testing

### Documentation

- [ ] **TODO-102**: Write API documentation (OpenAPI/Swagger)
- [ ] **TODO-103**: Write user guide
- [ ] **TODO-104**: Write deployment guide
- [ ] **TODO-105**: Write self-hosted installation guide

### Deployment

- [ ] **TODO-106**: Set up production infrastructure (Docker, Kubernetes)
- [ ] **TODO-107**: Set up monitoring and alerting
- [ ] **TODO-108**: Set up backup and recovery
- [ ] **TODO-109**: Deploy to production (Cloud SaaS)

### Launch Prep

- [ ] **TODO-110**: Create landing page
- [ ] **TODO-111**: Set up analytics tracking (product analytics)
- [ ] **TODO-112**: Create onboarding flow for new tenants
- [ ] **TODO-113**: Beta testing with real users

### Cleanup

- [ ] **TODO-114**: Remove Swagger (Swashbuckle), Swagger CORS policy, and dev-only Swagger middleware from production builds

---

## Progress

| Phase | Total | Completed | Status |
|-------|-------|-----------|--------|
| Phase 1: Foundation | 17 | 17 | Complete |
| Phase 2: Core Modules | 22 | 13 | In Progress |
| Phase 3: Org Designer | 19 | 0 | Not Started |
| Phase 4: UI & UX | 18 | 0 | Not Started |
| Phase 5: Features | 13 | 0 | Not Started |
| Phase 6: Business | 8 | 0 | Not Started |
| Phase 7: Polish | 17 | 0 | Not Started |
| **Total** | **114** | **30** | **26%** |

---

## Notes

### Gaps Fixed (2026-07-13)

- **TODO-001**: Added `Dockerfile` (multi-stage .NET 10 build) + `.dockerignore` + app service in `docker-compose.yml`
- **TODO-003**: Initialized `.husky/pre-commit` with `dotnet format` hook
- **TODO-004**: Created `.github/workflows/ci.yml` (build, test, format check on push/PR)

### Dependency Order

- Phase 1 must complete before Phase 2
- Phase 2 must complete before Phase 3
- Phase 3 can parallel with Phase 4
- Phase 5 depends on Phase 2
- Phase 6 can start after Phase 2
- Phase 7 is last

---

## Docker & Deployment

- [ ] **TODO-090**: Dockerize backend API (add app service back to docker-compose, multi-stage Dockerfile)

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
