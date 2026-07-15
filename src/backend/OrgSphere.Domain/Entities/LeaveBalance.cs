using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class LeaveBalance
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public int Year { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int PendingDays { get; set; }
    public int CarriedForwardDays { get; set; }
    public int RemainingDays => TotalDays + CarriedForwardDays - UsedDays - PendingDays;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
