using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class ApprovalChainRepository(INeo4jContext context) : IApprovalChainRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<ApprovalChain?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalChain'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<ApprovalChain>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'ApprovalChain'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<ApprovalChain?> GetByTypeAsync(ApprovalType type, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.FirstOrDefault(c => c.ApprovalType == type);
    }

    public async Task<ApprovalChain> CreateAsync(ApprovalChain chain, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(chain);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'ApprovalChain',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = chain.Id.ToString(),
                tenantId = chain.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = chain.CreatedAt.ToString("O"),
                updatedAt = chain.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(ApprovalChain chain, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(chain);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalChain'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = chain.Id.ToString(),
                tenantId = chain.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalChain'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static Dictionary<string, object> SerializeProperties(ApprovalChain chain) => new()
    {
        ["Name"] = chain.Name,
        ["ApprovalType"] = chain.ApprovalType.ToString(),
        ["Description"] = chain.Description ?? string.Empty,
        ["IsActive"] = chain.IsActive.ToString(),
        ["StepCount"] = chain.StepCount.ToString()
    };

    private static ApprovalChain MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new ApprovalChain
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            Name = GetProperty(properties, "Name"),
            ApprovalType = Enum.Parse<ApprovalType>(GetProperty(properties, "ApprovalType")),
            Description = GetPropertyOrNull(properties, "Description"),
            IsActive = bool.Parse(GetProperty(properties, "IsActive")),
            StepCount = int.Parse(GetProperty(properties, "StepCount")),
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
