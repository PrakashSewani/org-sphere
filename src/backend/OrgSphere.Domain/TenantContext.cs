using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain;

public class TenantContext : ITenantContext
{
    public TenantId? TenantId { get; set; }
    public UserId? UserId { get; set; }
    public UserRole? Role { get; set; }
    public bool IsAuthenticated => TenantId is not null && UserId is not null;
}
