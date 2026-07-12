using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Seed.Seeds;

public class TenantSeed(INeo4jContext context, ILogger<TenantSeed> logger)
{
    public async Task<TenantId?> SeedAsync(CancellationToken ct = default)
    {
        await using var session = context.AsyncSession();

        var cursor = await session.RunAsync(
            "MATCH (t:Tenant {Slug: $slug}) RETURN t.Id AS Id",
            new Dictionary<string, object?> { ["slug"] = "acme-corp" });

        var records = await cursor.ToListAsync(ct);
        if (records.Count > 0)
        {
            return new TenantId(Guid.Parse(records[0]["Id"].As<string>()));
        }

        var tenantId = TenantId.New();

        await session.RunAsync(
            """
            CREATE (t:Tenant {
                Id: $id,
                Name: $name,
                Slug: $slug,
                Plan: $plan,
                IsActive: true,
                CreatedAt: datetime(),
                UpdatedAt: datetime()
            })
            """,
            new Dictionary<string, object?>
            {
                ["id"] = tenantId.ToString(),
                ["name"] = "Acme Corp",
                ["slug"] = "acme-corp",
                ["plan"] = TenantPlan.Starter.ToString()
            });

        var tenantIdValue = tenantId.ToString();
        logger.LogInformation("Seeded tenant: Acme Corp ({Id})", tenantIdValue);
        return tenantId;
    }
}
