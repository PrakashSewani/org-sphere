using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class ApprovalRequest
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public ApprovalType Type { get; set; }
    public Guid RequesterId { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Metadata { get; set; }
    public ApprovalStatus Status { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public Guid? FinalApproverId { get; set; }
    public DateTime? FinalApprovedAt { get; set; }
    public string? FinalRejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
