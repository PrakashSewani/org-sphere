using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Auth;

public class RefreshTokenStore(INeo4jContext context, ILogger<RefreshTokenStore> logger) : IRefreshTokenStore
{
    public async Task SaveAsync(string token, UserId userId, TenantId tenantId, DateTime expiresAt, CancellationToken ct = default)
    {
        await using var session = context.AsyncSession();

        await session.RunAsync(
            """
            CREATE (rt:RefreshToken {
                Token: $token,
                UserId: $userId,
                TenantId: $tenantId,
                ExpiresAt: $expiresAt,
                IsRevoked: false,
                CreatedAt: datetime()
            })
            """,
            new Dictionary<string, object?>
            {
                ["token"] = token,
                ["userId"] = userId.ToString(),
                ["tenantId"] = tenantId.ToString(),
                ["expiresAt"] = expiresAt.ToString("O")
            });
    }

    public async Task<(UserId UserId, TenantId TenantId)?> GetAsync(string token, CancellationToken ct = default)
    {
        await using var session = context.AsyncSession();

        var cursor = await session.RunAsync(
            """
            MATCH (rt:RefreshToken {Token: $token})
            WHERE rt.IsRevoked = false AND rt.ExpiresAt > datetime()
            RETURN rt.UserId AS UserId, rt.TenantId AS TenantId
            """,
            new Dictionary<string, object?> { ["token"] = token });

        var records = await cursor.ToListAsync(ct);
        if (records.Count == 0)
            return null;

        var record = records[0];
        return (new UserId(Guid.Parse(record["UserId"].As<string>())),
                new TenantId(Guid.Parse(record["TenantId"].As<string>())));
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        await using var session = context.AsyncSession();

        await session.RunAsync(
            """
            MATCH (rt:RefreshToken {Token: $token})
            SET rt.IsRevoked = true
            """,
            new Dictionary<string, object?> { ["token"] = token });

        logger.LogInformation("Refresh token revoked");
    }

    public async Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        await using var session = context.AsyncSession();

        await session.RunAsync(
            """
            MATCH (rt:RefreshToken)
            WHERE rt.ExpiresAt < datetime() OR rt.IsRevoked = true
            DELETE rt
            """);
    }
}
