using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.DTOs;

public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public TenantPlan Plan { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record GraphNodeDto
{
    public Guid Id { get; init; }
    public NodeType Type { get; init; }
    public Dictionary<string, object> Properties { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public record GraphEdgeDto
{
    public Guid Id { get; init; }
    public EdgeType Type { get; init; }
    public Guid SourceId { get; init; }
    public Guid TargetId { get; init; }
    public Dictionary<string, object> Properties { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public record PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
