using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class LeavePolicyRepository(INeo4jContext context) : ILeavePolicyRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<LeavePolicy?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeavePolicy'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<LeavePolicy>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'LeavePolicy'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<LeavePolicy?> GetByLeaveTypeAsync(LeaveType leaveType, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.FirstOrDefault(p => p.LeaveType == leaveType);
    }

    public async Task<LeavePolicy> CreateAsync(LeavePolicy leavePolicy, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(leavePolicy);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'LeavePolicy',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = leavePolicy.Id.ToString(),
                tenantId = leavePolicy.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = leavePolicy.CreatedAt.ToString("O"),
                updatedAt = leavePolicy.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(LeavePolicy leavePolicy, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(leavePolicy);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeavePolicy'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = leavePolicy.Id.ToString(),
                tenantId = leavePolicy.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'LeavePolicy'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static Dictionary<string, object> SerializeProperties(LeavePolicy leavePolicy) => new()
    {
        ["Name"] = leavePolicy.Name,
        ["LeaveType"] = leavePolicy.LeaveType.ToString(),
        ["DefaultDaysPerYear"] = leavePolicy.DefaultDaysPerYear.ToString(),
        ["CarryForward"] = leavePolicy.CarryForward.ToString(),
        ["MaxCarryForwardDays"] = leavePolicy.MaxCarryForwardDays.ToString(),
        ["RequireApproval"] = leavePolicy.RequireApproval.ToString(),
        ["MinServiceDays"] = leavePolicy.MinServiceDays.ToString(),
        ["IsActive"] = leavePolicy.IsActive.ToString()
    };

    private static LeavePolicy MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new LeavePolicy
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            Name = GetProperty(properties, "Name"),
            LeaveType = Enum.Parse<LeaveType>(GetProperty(properties, "LeaveType")),
            DefaultDaysPerYear = int.Parse(GetProperty(properties, "DefaultDaysPerYear")),
            CarryForward = bool.Parse(GetProperty(properties, "CarryForward")),
            MaxCarryForwardDays = int.Parse(GetProperty(properties, "MaxCarryForwardDays")),
            RequireApproval = bool.Parse(GetProperty(properties, "RequireApproval")),
            MinServiceDays = int.Parse(GetProperty(properties, "MinServiceDays")),
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
