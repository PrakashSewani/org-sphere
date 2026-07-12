using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrgSphere.Domain.ValueObjects;
using StackExchange.Redis;

namespace OrgSphere.Infrastructure.Auth;

public class RedisRefreshTokenStore(IConnectionMultiplexer redis, ILogger<RedisRefreshTokenStore> logger) : IRefreshTokenStore
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly ILogger<RedisRefreshTokenStore> _logger = logger;

    private const string _keyPrefix = "refresh_token:";

    public async Task SaveAsync(string token, UserId userId, TenantId tenantId, DateTime expiresAt, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{_keyPrefix}{token}";
        var ttl = expiresAt - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
            ttl = TimeSpan.FromMinutes(1);

        var data = new RefreshTokenData(userId.Value, tenantId.Value);
        var json = JsonSerializer.Serialize(data);

        await db.StringSetAsync(key, json, ttl);
        _logger.LogDebug("Refresh token saved to Redis with TTL {Ttl}", ttl);
    }

    public async Task<(UserId UserId, TenantId TenantId)?> GetAsync(string token, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{_keyPrefix}{token}";

        var json = await db.StringGetAsync(key);
        if (json.IsNullOrEmpty)
            return null;

        var data = JsonSerializer.Deserialize<RefreshTokenData>((string)json!);
        if (data is null)
            return null;

        return (new UserId(data.UserId), new TenantId(data.TenantId));
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{_keyPrefix}{token}";
        await db.KeyDeleteAsync(key);
        _logger.LogDebug("Refresh token revoked from Redis");
    }

    public Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        // Redis handles expiry via TTL — no-op
        return Task.CompletedTask;
    }

    private record RefreshTokenData(Guid UserId, Guid TenantId);
}
