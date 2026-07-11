# Project Structure

## Overview

OrgSphere uses a monorepo structure with a .NET backend and React frontend.

```
org-sphere/
├── src/
│   ├── backend/
│   │   ├── OrgSphere.Domain/           # Core domain entities and interfaces
│   │   ├── OrgSphere.Application/      # CQRS commands, queries, validators
│   │   ├── OrgSphere.Infrastructure/   # Neo4j repositories, external services
│   │   └── OrgSphere.API/              # REST API controllers and middleware
│   └── frontend/                       # React web application
├── docs/                               # Product and architecture documentation
├── skills/                             # Agent skill definitions
├── docker-compose.yml                  # Local development services
└── OrgSphere.sln                       # .NET solution file
```

## Architecture Layers

### Domain Layer (`OrgSphere.Domain`)

**Purpose**: Core business logic, entities, and interfaces.

- No dependencies on other projects
- Contains entity definitions
- Contains repository interfaces
- Contains domain enums and value objects

### Application Layer (`OrgSphere.Application`)

**Purpose**: Application logic, CQRS handlers, and validators.

- Depends on Domain only
- Contains command and query handlers
- Contains FluentValidation validators
- Contains DTOs and mapping profiles
- Uses DispatchR.Mediator for CQRS

### Infrastructure Layer (`OrgSphere.Infrastructure`)

**Purpose**: External service implementations.

- Depends on Domain and Application
- Contains Neo4j context and repositories
- Contains Unit of Work implementation
- Contains dependency injection extensions

### API Layer (`OrgSphere.API`)

**Purpose**: HTTP API and middleware.

- Depends on Application and Infrastructure
- Contains REST controllers
- Contains exception handling middleware
- Contains Swagger/OpenAPI configuration
- Contains Docker and deployment configuration

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- Node.js 20+ (for frontend)

### Development Setup

1. Start databases:
   ```bash
   docker-compose up -d
   ```

2. Run API:
   ```bash
   cd src/backend/OrgSphere.API
   dotnet run
   ```

3. Access Swagger:
   ```
   http://localhost:5001/swagger
   ```

## Design Patterns

- **Clean Architecture**: Unidirectional dependencies
- **CQRS**: Separate read/write operations
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Mediator Pattern**: Decoupled handlers via DispatchR
