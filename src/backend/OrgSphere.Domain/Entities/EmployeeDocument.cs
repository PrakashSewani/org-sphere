using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public class EmployeeDocument
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
