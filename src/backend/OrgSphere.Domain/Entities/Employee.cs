using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? OfficeId { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public List<string> Skills { get; set; } = [];
    public EmployeePreferences Preferences { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EmployeePreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}
