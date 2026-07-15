using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class LeaveBalanceRepository(INeo4jContext context) : ILeaveBalanceRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<LeaveBalance?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeaveBalance'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<LeaveBalance?> GetByEmployeeAndTypeAsync(Guid employeeId, LeaveType leaveType, int year, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetByEmployeeAsync(employeeId, year, tenantId, cancellationToken);
        return all.FirstOrDefault(b => b.LeaveType == leaveType);
    }

    public async Task<IEnumerable<LeaveBalance>> GetByEmployeeAsync(Guid employeeId, int year, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'LeaveBalance'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return records.Select(MapToEntity).Where(b => b.EmployeeId == employeeId && b.Year == year);
    }

    public async Task<LeaveBalance> CreateAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(leaveBalance);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'LeaveBalance',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = leaveBalance.Id.ToString(),
                tenantId = leaveBalance.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = leaveBalance.CreatedAt.ToString("O"),
                updatedAt = leaveBalance.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(LeaveBalance leaveBalance, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(leaveBalance);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeaveBalance'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = leaveBalance.Id.ToString(),
                tenantId = leaveBalance.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    private static Dictionary<string, object> SerializeProperties(LeaveBalance leaveBalance) => new()
    {
        ["EmployeeId"] = leaveBalance.EmployeeId.ToString(),
        ["LeaveType"] = leaveBalance.LeaveType.ToString(),
        ["Year"] = leaveBalance.Year.ToString(),
        ["TotalDays"] = leaveBalance.TotalDays.ToString(),
        ["UsedDays"] = leaveBalance.UsedDays.ToString(),
        ["PendingDays"] = leaveBalance.PendingDays.ToString(),
        ["CarriedForwardDays"] = leaveBalance.CarriedForwardDays.ToString()
    };

    private static LeaveBalance MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new LeaveBalance
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            EmployeeId = Guid.Parse(GetProperty(properties, "EmployeeId")),
            LeaveType = Enum.Parse<LeaveType>(GetProperty(properties, "LeaveType")),
            Year = int.Parse(GetProperty(properties, "Year")),
            TotalDays = int.Parse(GetProperty(properties, "TotalDays")),
            UsedDays = int.Parse(GetProperty(properties, "UsedDays")),
            PendingDays = int.Parse(GetProperty(properties, "PendingDays")),
            CarriedForwardDays = int.Parse(GetProperty(properties, "CarriedForwardDays")),
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>()),
            UpdatedAt = DateTime.Parse(props["UpdatedAt"].As<string>())
        };
    }

    private static string GetProperty(Dictionary<string, object> properties, string key)
    {
        return properties.TryGetValue(key, out var value) ? value.ToString()! : string.Empty;
    }
}
