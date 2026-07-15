using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class ApprovalChain
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public ApprovalType ApprovalType { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int StepCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
