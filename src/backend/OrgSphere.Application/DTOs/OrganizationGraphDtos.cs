using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.DTOs;

public record CompanyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Industry { get; init; } = string.Empty;
    public int EmployeeCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record RegionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Timezone { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record OfficeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record DepartmentDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Budget { get; init; }
    public string CostCenter { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record TeamDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string FocusArea { get; init; } = string.Empty;
    public int MaxSize { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record OrgEmployeeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateCompanyRequest(string Name, string Industry, int EmployeeCount);
public record UpdateCompanyRequest(string Name, string Industry, int EmployeeCount);

public record CreateRegionRequest(string Name, string Timezone);
public record UpdateRegionRequest(string Name, string Timezone);

public record CreateOfficeRequest(string Name, string City, string Country, string Address);
public record UpdateOfficeRequest(string Name, string City, string Country, string Address);

public record CreateDepartmentRequest(string Name, decimal Budget, string CostCenter);
public record UpdateDepartmentRequest(string Name, decimal Budget, string CostCenter);

public record CreateTeamRequest(string Name, string FocusArea, int MaxSize);
public record UpdateTeamRequest(string Name, string FocusArea, int MaxSize);

public record CreateOrgEmployeeRequest(string Name, string Email, string JobTitle);
public record UpdateOrgEmployeeRequest(string Name, string Email, string JobTitle);
