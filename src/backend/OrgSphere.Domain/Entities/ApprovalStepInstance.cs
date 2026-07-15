using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class ApprovalStepInstance
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public Guid RequestId { get; set; }
    public Guid StepId { get; set; }
    public int StepOrder { get; set; }
    public Guid? ApproverId { get; set; }
    public Guid? DelegatedToId { get; set; }
    public ApprovalStepStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
