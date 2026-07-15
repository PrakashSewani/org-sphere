using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class AttendancePolicy
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public TimeOnly WorkStartTime { get; set; }
    public TimeOnly WorkEndTime { get; set; }
    public int GracePeriodMinutes { get; set; }
    public bool RequireCheckIn { get; set; }
    public bool RequireCheckOut { get; set; }
    public bool AllowRemoteCheckIn { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
