using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.DTOs;

public record EmployeeDto
{
    public Guid Id { get; init; }
    public string EmployeeId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid DepartmentId { get; init; }
    public Guid? TeamId { get; init; }
    public Guid? ManagerId { get; init; }
    public Guid? OfficeId { get; init; }
    public EmploymentType EmploymentType { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public EmployeeStatus Status { get; init; }
    public string? Bio { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public List<string> Skills { get; init; } = [];
    public EmployeePreferencesDto Preferences { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record EmployeePreferencesDto
{
    public bool EmailNotifications { get; init; } = true;
    public bool PushNotifications { get; init; } = true;
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
}

public record CreateEmployeeRequest(
    string EmployeeId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Title,
    Guid DepartmentId,
    Guid? TeamId,
    Guid? ManagerId,
    Guid? OfficeId,
    EmploymentType EmploymentType,
    DateTime StartDate);

public record UpdateEmployeeRequest(
    string? Phone,
    string Title,
    Guid DepartmentId,
    Guid? TeamId,
    Guid? ManagerId,
    Guid? OfficeId,
    EmploymentType EmploymentType,
    DateTime? EndDate,
    EmployeeStatus Status);

public record UpdateProfileRequest(
    string? Bio,
    string? Phone,
    List<string>? Skills);

public record UpdatePreferencesRequest(
    bool? EmailNotifications,
    bool? PushNotifications,
    string? ContactEmail,
    string? ContactPhone);

public record EmployeeDocumentDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}
