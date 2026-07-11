using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly INeo4jContext _context;

    public TenantRepository(INeo4jContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (t:Tenant {Id: $id}) RETURN t",
            new { id = id.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (t:Tenant {Slug: $slug}) RETURN t",
            new { slug });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (t:Tenant {
                Id: $id,
                Name: $name,
                Slug: $slug,
                Plan: $plan,
                IsActive: $isActive,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN t",
            new
            {
                id = tenant.Id.Value.ToString(),
                name = tenant.Name,
                slug = tenant.Slug,
                plan = tenant.Plan.ToString(),
                isActive = tenant.IsActive,
                createdAt = tenant.CreatedAt.ToString("O"),
                updatedAt = tenant.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (t:Tenant {Id: $id})
              SET t.Name = $name,
                  t.Slug = $slug,
                  t.Plan = $plan,
                  t.IsActive = $isActive,
                  t.UpdatedAt = $updatedAt",
            new
            {
                id = tenant.Id.Value.ToString(),
                name = tenant.Name,
                slug = tenant.Slug,
                plan = tenant.Plan.ToString(),
                isActive = tenant.IsActive,
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(TenantId id, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (t:Tenant {Id: $id}) DETACH DELETE t",
            new { id = id.Value.ToString() });
    }

    private static Tenant MapToEntity(IRecord record)
    {
        var node = record["t"].As<INode>();
        var props = node.Properties;
        return new Tenant
        {
            Id = new TenantId(Guid.Parse(props["Id"].As<string>())),
            Name = props["Name"].As<string>(),
            Slug = props["Slug"].As<string>(),
            Plan = Enum.Parse<TenantPlan>(props["Plan"].As<string>()),
            IsActive = props["IsActive"].As<bool>(),
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>()),
            UpdatedAt = DateTime.Parse(props["UpdatedAt"].As<string>())
        };
    }
}
