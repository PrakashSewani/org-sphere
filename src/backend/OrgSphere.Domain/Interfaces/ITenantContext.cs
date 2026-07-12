using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface ITenantContext
{
    TenantId? TenantId { get; }
    UserId? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
