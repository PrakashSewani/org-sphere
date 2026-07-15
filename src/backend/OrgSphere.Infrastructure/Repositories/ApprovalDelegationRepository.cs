using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class ApprovalDelegationRepository(INeo4jContext context) : IApprovalDelegationRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<ApprovalDelegation?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalDelegation'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<ApprovalDelegation>> GetByDelegatorAsync(Guid delegatorId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(d => d.DelegatorId == delegatorId);
    }

    public async Task<ApprovalDelegation?> GetActiveByDelegatorAsync(Guid delegatorId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.FirstOrDefault(d => d.DelegatorId == delegatorId && d.IsActive);
    }

    public async Task<ApprovalDelegation> CreateAsync(ApprovalDelegation delegation, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(delegation);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'ApprovalDelegation',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = delegation.Id.ToString(),
                tenantId = delegation.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = delegation.CreatedAt.ToString("O"),
                updatedAt = delegation.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(ApprovalDelegation delegation, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(delegation);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalDelegation'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = delegation.Id.ToString(),
                tenantId = delegation.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalDelegation'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private async Task<IEnumerable<ApprovalDelegation>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'ApprovalDelegation'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    private static Dictionary<string, object> SerializeProperties(ApprovalDelegation delegation) => new()
    {
        ["DelegatorId"] = delegation.DelegatorId.ToString(),
        ["DelegateId"] = delegation.DelegateId.ToString(),
        ["Scope"] = delegation.Scope ?? string.Empty,
        ["StartDate"] = delegation.StartDate?.ToString("O") ?? string.Empty,
        ["EndDate"] = delegation.EndDate?.ToString("O") ?? string.Empty,
        ["IsActive"] = delegation.IsActive.ToString()
    };

    private static ApprovalDelegation MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new ApprovalDelegation
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            DelegatorId = Guid.Parse(GetProperty(properties, "DelegatorId")),
            DelegateId = Guid.Parse(GetProperty(properties, "DelegateId")),
            Scope = GetPropertyOrNull(properties, "Scope"),
            StartDate = DateTime.TryParse(GetPropertyOrNull(properties, "StartDate"), out var startDate) ? startDate : null,
            EndDate = DateTime.TryParse(GetPropertyOrNull(properties, "EndDate"), out var endDate) ? endDate : null,
            IsActive = bool.Parse(GetProperty(properties, "IsActive")),
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
