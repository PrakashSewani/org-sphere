using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class ApprovalDelegation
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public Guid DelegatorId { get; set; }
    public Guid DelegateId { get; set; }
    public string? Scope { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
