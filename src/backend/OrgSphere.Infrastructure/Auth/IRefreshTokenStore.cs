using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Infrastructure.Auth;

public interface IRefreshTokenStore
{
    Task SaveAsync(string token, UserId userId, TenantId tenantId, DateTime expiresAt, CancellationToken ct = default);
    Task<(UserId UserId, TenantId TenantId)?> GetAsync(string token, CancellationToken ct = default);
    Task RevokeAsync(string token, CancellationToken ct = default);
    Task DeleteExpiredAsync(CancellationToken ct = default);
}
