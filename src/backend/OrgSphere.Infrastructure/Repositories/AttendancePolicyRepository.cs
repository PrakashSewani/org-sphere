using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class AttendancePolicyRepository(INeo4jContext context) : IAttendancePolicyRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<AttendancePolicy?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'AttendancePolicy'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<AttendancePolicy>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'AttendancePolicy'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<AttendancePolicy?> GetActiveAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.FirstOrDefault(p => p.IsActive);
    }

    public async Task<AttendancePolicy> CreateAsync(AttendancePolicy attendancePolicy, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(attendancePolicy);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'AttendancePolicy',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = attendancePolicy.Id.ToString(),
                tenantId = attendancePolicy.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = attendancePolicy.CreatedAt.ToString("O"),
                updatedAt = attendancePolicy.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(AttendancePolicy attendancePolicy, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(attendancePolicy);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'AttendancePolicy'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = attendancePolicy.Id.ToString(),
                tenantId = attendancePolicy.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'AttendancePolicy'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static Dictionary<string, object> SerializeProperties(AttendancePolicy attendancePolicy) => new()
    {
        ["Name"] = attendancePolicy.Name,
        ["WorkStartTime"] = attendancePolicy.WorkStartTime.ToString("O"),
        ["WorkEndTime"] = attendancePolicy.WorkEndTime.ToString("O"),
        ["GracePeriodMinutes"] = attendancePolicy.GracePeriodMinutes.ToString(),
        ["RequireCheckIn"] = attendancePolicy.RequireCheckIn.ToString(),
        ["RequireCheckOut"] = attendancePolicy.RequireCheckOut.ToString(),
        ["AllowRemoteCheckIn"] = attendancePolicy.AllowRemoteCheckIn.ToString(),
        ["IsActive"] = attendancePolicy.IsActive.ToString()
    };

    private static AttendancePolicy MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new AttendancePolicy
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            Name = GetProperty(properties, "Name"),
            WorkStartTime = TimeOnly.Parse(GetProperty(properties, "WorkStartTime")),
            WorkEndTime = TimeOnly.Parse(GetProperty(properties, "WorkEndTime")),
            GracePeriodMinutes = int.Parse(GetProperty(properties, "GracePeriodMinutes")),
            RequireCheckIn = bool.Parse(GetProperty(properties, "RequireCheckIn")),
            RequireCheckOut = bool.Parse(GetProperty(properties, "RequireCheckOut")),
            AllowRemoteCheckIn = bool.Parse(GetProperty(properties, "AllowRemoteCheckIn")),
            IsActive = bool.Parse(GetProperty(properties, "IsActive")),
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>()),
            UpdatedAt = DateTime.Parse(props["UpdatedAt"].As<string>())
        };
    }

    private static string GetProperty(Dictionary<string, object> properties, string key)
    {
        return properties.TryGetValue(key, out var value) ? value.ToString()! : string.Empty;
    }
}
