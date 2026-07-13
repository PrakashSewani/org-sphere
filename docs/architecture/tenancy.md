# Tenancy

## Multi-Tenant Architecture

OrgSphere is designed as a commercial SaaS platform with strict tenant isolation.

Each customer receives:

```
company.orgsphere.com
```

---

## Tenant Model

### Isolation Levels

| Resource | Isolation |
|----------|-----------|
| Data | Schema or database per tenant |
| Users | Tenant-scoped authentication |
| Configuration | Tenant-specific settings |
| Workflows | Tenant-specific approval chains |
| Branding | Tenant-specific UI customization |
| Permissions | Tenant-specific RBAC |
| Analytics | Tenant-scoped insights |
| AI | Tenant-scoped reasoning |

---

## Tenant Structure

```
┌─────────────────────────────────────────────────┐
│                  OrgSphere Platform              │
│                                                 │
│  ┌─────────────┐  ┌─────────────┐             │
│  │  Tenant A   │  │  Tenant B   │             │
│  │             │  │             │             │
│  │  Graph      │  │  Graph      │             │
│  │  Users      │  │  Users      │             │
│  │  Config     │  │  Config     │             │
│  │  Workflows  │  │  Workflows  │             │
│  │  Data       │  │  Data       │             │
│  └─────────────┘  └─────────────┘             │
│                                                 │
│  ┌─────────────┐  ┌─────────────┐             │
│  │  Tenant C   │  │  Tenant D   │             │
│  │  ...        │  │  ...        │             │
│  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────┘
```

---

## Tenant Lifecycle

### Onboarding

1. Create tenant record
2. Initialize graph with root node (Company)
3. Set up default configuration
4. Create admin user
5. Configure branding
6. Set up authentication provider

### Operations

- User management
- Configuration changes
- Data imports
- Integration setup
- Support access

### Offboarding

1. Export tenant data
2. Archive tenant graph
3. Remove tenant data
4. Release resources

---

## Data Isolation Strategies

### Option 1: Schema per Tenant

Each tenant gets a dedicated database schema.

**Pros:**
- Strong isolation
- Easy backup/restore per tenant
- Simple data deletion

**Cons:**
- More database connections
- Schema migration complexity
- Higher resource usage

### Option 2: Database per Tenant

Each tenant gets a dedicated database.

**Pros:**
- Maximum isolation
- Independent scaling
- Complete separation

**Cons:**
- Highest resource cost
- Connection management complexity
- Backup complexity

### Option 3: Shared Schema with Tenant ID

All tenants share tables with tenant_id column.

**Pros:**
- Efficient resource usage
- Simple schema management
- Easy cross-tenant analytics

**Cons:**
- Weaker isolation (application-enforced)
- More complex queries
- Harder to isolate failures

---

## Recommendation

**Schema per tenant** for most customers.

**Database per tenant** for enterprise customers requiring maximum isolation.

**Shared schema** for free tier or demo environments.

---

## Tenant Context

Every request carries tenant context:

```csharp
// OrgSphere.Domain/TenantContext.cs
public class TenantContext : ITenantContext
{
    public TenantId? TenantId { get; set; }
    public UserId? UserId { get; set; }
    public UserRole? Role { get; set; }
}
```

This context propagates through:

- API requests
- Event messages
- Background jobs
- Cache keys
- Log entries

---

## Cross-Tenant Operations

### Platform Admin

Platform administrators can:

- View tenant health metrics
- Access tenant configuration
- Perform support operations
- Generate cross-tenant analytics

### Data Residency

Support data residency requirements:

- Tenant data in specific regions
- Compliance with local regulations
- Data sovereignty guarantees

---

## Tenant Configuration

Each tenant can configure:

- Company information
- Branding (logo, colors)
- Authentication provider
- Leave policies
- Approval chains
- Notification rules
- Integration settings
- Feature flags

---

## Scaling Considerations

### Horizontal Scaling

- Stateless services scale horizontally
- Tenant routing via load balancer
- Session affinity not required

### Vertical Scaling

- Graph database may need vertical scaling
- Query optimization per tenant
- Read replicas for heavy tenants

### Tenant Isolation

- Resource quotas per tenant
- Rate limiting per tenant
- Background job prioritization
