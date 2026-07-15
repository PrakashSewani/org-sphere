using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class LeavePolicy
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; }
    public int DefaultDaysPerYear { get; set; }
    public bool CarryForward { get; set; }
    public int MaxCarryForwardDays { get; set; }
    public bool RequireApproval { get; set; }
    public int MinServiceDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
