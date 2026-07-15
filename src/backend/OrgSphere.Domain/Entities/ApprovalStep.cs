using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class ApprovalStep
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public Guid ChainId { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ApproverType { get; set; }
    public Guid? SpecificApproverId { get; set; }
    public string? Condition { get; set; }
    public int TimeoutHours { get; set; }
    public bool IsRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
