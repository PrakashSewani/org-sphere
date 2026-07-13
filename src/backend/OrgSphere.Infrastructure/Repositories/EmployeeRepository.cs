using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class EmployeeRepository(INeo4jContext context) : IEmployeeRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<Employee?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'Employee'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<Employee?> GetByEmailAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"MATCH (n:GraphNode {TenantId: $tenantId, Type: 'Employee'})
              WHERE n.Properties CONTAINS $email
              RETURN n",
            new { tenantId = tenantId.Value.ToString(), email = email });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        if (record is null) return null;

        var employee = MapToEntity(record);
        return employee.Email.Equals(email, StringComparison.OrdinalIgnoreCase) ? employee : null;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'Employee'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(e => e.DepartmentId == departmentId);
    }

    public async Task<IEnumerable<Employee>> GetByManagerAsync(Guid managerId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(e => e.ManagerId == managerId);
    }

    public async Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(e => e.Status == status);
    }

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(employee);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'Employee',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = employee.Id.ToString(),
                tenantId = employee.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = employee.CreatedAt.ToString("O"),
                updatedAt = employee.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(employee);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'Employee'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = employee.Id.ToString(),
                tenantId = employee.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'Employee'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static Dictionary<string, object> SerializeProperties(Employee employee) => new()
    {
        ["EmployeeId"] = employee.EmployeeId,
        ["FirstName"] = employee.FirstName,
        ["LastName"] = employee.LastName,
        ["Email"] = employee.Email,
        ["Phone"] = employee.Phone ?? string.Empty,
        ["Title"] = employee.Title,
        ["DepartmentId"] = employee.DepartmentId.ToString(),
        ["TeamId"] = employee.TeamId?.ToString() ?? string.Empty,
        ["ManagerId"] = employee.ManagerId?.ToString() ?? string.Empty,
        ["OfficeId"] = employee.OfficeId?.ToString() ?? string.Empty,
        ["EmploymentType"] = employee.EmploymentType.ToString(),
        ["StartDate"] = employee.StartDate.ToString("O"),
        ["EndDate"] = employee.EndDate?.ToString("O") ?? string.Empty,
        ["Status"] = employee.Status.ToString(),
        ["Bio"] = employee.Bio ?? string.Empty,
        ["ProfilePictureUrl"] = employee.ProfilePictureUrl ?? string.Empty,
        ["Skills"] = JsonSerializer.Serialize(employee.Skills),
        ["Preferences"] = JsonSerializer.Serialize(employee.Preferences)
    };

    private static Employee MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new Employee
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            EmployeeId = GetProperty(properties, "EmployeeId"),
            FirstName = GetProperty(properties, "FirstName"),
            LastName = GetProperty(properties, "LastName"),
            Email = GetProperty(properties, "Email"),
            Phone = GetPropertyOrNull(properties, "Phone"),
            Title = GetProperty(properties, "Title"),
            DepartmentId = Guid.Parse(GetProperty(properties, "DepartmentId")),
            TeamId = Guid.TryParse(GetPropertyOrNull(properties, "TeamId"), out var teamId) ? teamId : null,
            ManagerId = Guid.TryParse(GetPropertyOrNull(properties, "ManagerId"), out var managerId) ? managerId : null,
            OfficeId = Guid.TryParse(GetPropertyOrNull(properties, "OfficeId"), out var officeId) ? officeId : null,
            EmploymentType = Enum.Parse<EmploymentType>(GetProperty(properties, "EmploymentType")),
            StartDate = DateTime.Parse(GetProperty(properties, "StartDate")),
            EndDate = DateTime.TryParse(GetPropertyOrNull(properties, "EndDate"), out var endDate) ? endDate : null,
            Status = Enum.Parse<EmployeeStatus>(GetProperty(properties, "Status")),
            Bio = GetPropertyOrNull(properties, "Bio"),
            ProfilePictureUrl = GetPropertyOrNull(properties, "ProfilePictureUrl"),
            Skills = JsonSerializer.Deserialize<List<string>>(GetProperty(properties, "Skills")) ?? [],
            Preferences = JsonSerializer.Deserialize<EmployeePreferences>(GetProperty(properties, "Preferences")) ?? new(),
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>()),
            UpdatedAt = DateTime.Parse(props["UpdatedAt"].As<string>())
        };
    }

    private static string GetProperty(Dictionary<string, object> properties, string key)
    {
        return properties.TryGetValue(key, out var value) ? value.ToString()! : string.Empty;
    }

    private static string? GetPropertyOrNull(Dictionary<string, object> properties, string key)
    {
        return properties.TryGetValue(key, out var value) ? value.ToString() : null;
    }
}
