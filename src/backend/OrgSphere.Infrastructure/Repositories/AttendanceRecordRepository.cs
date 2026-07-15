using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class AttendanceRecordRepository(INeo4jContext context) : IAttendanceRecordRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<AttendanceRecord?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'AttendanceRecord'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<AttendanceRecord?> GetByEmployeeAndDateAsync(Guid employeeId, DateTime date, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetByEmployeeAsync(employeeId, tenantId, cancellationToken);
        return all.FirstOrDefault(r => r.Date.Date == date.Date);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'AttendanceRecord'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<IEnumerable<AttendanceRecord>> GetByEmployeeAsync(Guid employeeId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(r => r.EmployeeId == employeeId);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(r => r.Date.Date >= startDate.Date && r.Date.Date <= endDate.Date);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetByEmployeeAndDateRangeAsync(Guid employeeId, DateTime startDate, DateTime endDate, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(r => r.EmployeeId == employeeId && r.Date.Date >= startDate.Date && r.Date.Date <= endDate.Date);
    }

    public async Task<AttendanceRecord> CreateAsync(AttendanceRecord attendanceRecord, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(attendanceRecord);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'AttendanceRecord',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = attendanceRecord.Id.ToString(),
                tenantId = attendanceRecord.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = attendanceRecord.CreatedAt.ToString("O"),
                updatedAt = attendanceRecord.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(AttendanceRecord attendanceRecord, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(attendanceRecord);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'AttendanceRecord'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = attendanceRecord.Id.ToString(),
                tenantId = attendanceRecord.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    private static Dictionary<string, object> SerializeProperties(AttendanceRecord attendanceRecord) => new()
    {
        ["EmployeeId"] = attendanceRecord.EmployeeId.ToString(),
        ["Date"] = attendanceRecord.Date.ToString("O"),
        ["CheckInTime"] = attendanceRecord.CheckInTime?.ToString("O") ?? string.Empty,
        ["CheckOutTime"] = attendanceRecord.CheckOutTime?.ToString("O") ?? string.Empty,
        ["Status"] = attendanceRecord.Status.ToString(),
        ["Notes"] = attendanceRecord.Notes ?? string.Empty
    };

    private static AttendanceRecord MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new AttendanceRecord
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            EmployeeId = Guid.Parse(GetProperty(properties, "EmployeeId")),
            Date = DateTime.Parse(GetProperty(properties, "Date")),
            CheckInTime = DateTime.TryParse(GetPropertyOrNull(properties, "CheckInTime"), out var checkInTime) ? checkInTime : null,
            CheckOutTime = DateTime.TryParse(GetPropertyOrNull(properties, "CheckOutTime"), out var checkOutTime) ? checkOutTime : null,
            Status = Enum.Parse<AttendanceStatus>(GetProperty(properties, "Status")),
            Notes = GetPropertyOrNull(properties, "Notes"),
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
