using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class LeaveRequestRepository(INeo4jContext context) : ILeaveRequestRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<LeaveRequest?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeaveRequest'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'LeaveRequest'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(l => l.EmployeeId == employeeId);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(l => l.Status == status);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(l => l.StartDate >= startDate && l.EndDate <= endDate);
    }

    public async Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(leaveRequest);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'LeaveRequest',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = leaveRequest.Id.ToString(),
                tenantId = leaveRequest.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = leaveRequest.CreatedAt.ToString("O"),
                updatedAt = leaveRequest.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(leaveRequest);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeaveRequest'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = leaveRequest.Id.ToString(),
                tenantId = leaveRequest.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeaveRequest'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static Dictionary<string, object> SerializeProperties(LeaveRequest leaveRequest) => new()
    {
        ["EmployeeId"] = leaveRequest.EmployeeId.ToString(),
        ["LeaveType"] = leaveRequest.LeaveType.ToString(),
        ["StartDate"] = leaveRequest.StartDate.ToString("O"),
        ["EndDate"] = leaveRequest.EndDate.ToString("O"),
        ["TotalDays"] = leaveRequest.TotalDays.ToString(),
        ["Reason"] = leaveRequest.Reason ?? string.Empty,
        ["Status"] = leaveRequest.Status.ToString(),
        ["ApprovedById"] = leaveRequest.ApprovedById?.ToString() ?? string.Empty,
        ["ApprovedAt"] = leaveRequest.ApprovedAt?.ToString("O") ?? string.Empty,
        ["RejectionReason"] = leaveRequest.RejectionReason ?? string.Empty
    };

    private static LeaveRequest MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new LeaveRequest
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            EmployeeId = Guid.Parse(GetProperty(properties, "EmployeeId")),
            LeaveType = Enum.Parse<LeaveType>(GetProperty(properties, "LeaveType")),
            StartDate = DateTime.Parse(GetProperty(properties, "StartDate")),
            EndDate = DateTime.Parse(GetProperty(properties, "EndDate")),
            TotalDays = int.Parse(GetProperty(properties, "TotalDays")),
            Reason = GetPropertyOrNull(properties, "Reason"),
            Status = Enum.Parse<LeaveStatus>(GetProperty(properties, "Status")),
            ApprovedById = Guid.TryParse(GetPropertyOrNull(properties, "ApprovedById"), out var approvedById) ? approvedById : null,
            ApprovedAt = DateTime.TryParse(GetPropertyOrNull(properties, "ApprovedAt"), out var approvedAt) ? approvedAt : null,
            RejectionReason = GetPropertyOrNull(properties, "RejectionReason"),
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
