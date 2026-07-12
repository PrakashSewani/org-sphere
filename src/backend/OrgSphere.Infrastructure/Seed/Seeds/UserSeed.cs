using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Seed.Seeds;

public class UserSeed(INeo4jContext context, ILogger<UserSeed> logger)
{
    public async Task SeedAsync(TenantId tenantId, CancellationToken ct = default)
    {
        await using var session = context.AsyncSession();

        var cursor = await session.RunAsync(
            "MATCH (u:User {Email: $email}) RETURN u.Id AS Id",
            new Dictionary<string, object?> { ["email"] = "admin@acme.com" });

        var records = await cursor.ToListAsync(ct);
        if (records.Count > 0)
        {
            logger.LogInformation("Admin user already exists, skipping user seed");
            return;
        }

        var users = new[]
        {
            (UserId.New(), "admin@acme.com", "Alice", "Admin", UserRole.Admin),
            (UserId.New(), "bob@acme.com", "Bob", "Smith", UserRole.Manager),
            (UserId.New(), "carol@acme.com", "Carol", "Jones", UserRole.Employee),
            (UserId.New(), "dave@acme.com", "Dave", "Wilson", UserRole.Manager),
            (UserId.New(), "eve@acme.com", "Eve", "Brown", UserRole.Employee),
            (UserId.New(), "frank@acme.com", "Frank", "Davis", UserRole.Manager),
            (UserId.New(), "grace@acme.com", "Grace", "Miller", UserRole.Employee),
            (UserId.New(), "heidi@acme.com", "Heidi", "Taylor", UserRole.HR),
            (UserId.New(), "ivan@acme.com", "Ivan", "Anderson", UserRole.Employee),
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        foreach (var (id, email, firstName, lastName, role) in users)
        {
            await session.RunAsync(
                """
                CREATE (u:User {
                    Id: $id,
                    TenantId: $tenantId,
                    Email: $email,
                    PasswordHash: $passwordHash,
                    FirstName: $firstName,
                    LastName: $lastName,
                    Role: $role,
                    IsActive: true,
                    CreatedAt: datetime(),
                    UpdatedAt: datetime()
                })
                """,
                new Dictionary<string, object?>
                {
                    ["id"] = id.ToString(),
                    ["tenantId"] = tenantId.ToString(),
                    ["email"] = email,
                    ["passwordHash"] = passwordHash,
                    ["firstName"] = firstName,
                    ["lastName"] = lastName,
                    ["role"] = role.ToString()
                });
        }

        var tenantIdValue = tenantId.ToString();
        logger.LogInformation("Seeded {Count} users for tenant {TenantId}", users.Length, tenantIdValue);
    }
}
