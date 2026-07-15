using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class ApprovalStepRepository(INeo4jContext context) : IApprovalStepRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<ApprovalStep?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalStep'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<ApprovalStep>> GetByChainIdAsync(Guid chainId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(s => s.ChainId == chainId);
    }

    public async Task<ApprovalStep> CreateAsync(ApprovalStep step, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(step);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'ApprovalStep',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = step.Id.ToString(),
                tenantId = step.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = step.CreatedAt.ToString("O"),
                updatedAt = step.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(ApprovalStep step, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(step);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalStep'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = step.Id.ToString(),
                tenantId = step.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalStep'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    public async Task DeleteByChainIdAsync(Guid chainId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var steps = await GetByChainIdAsync(chainId, tenantId, cancellationToken);
        foreach (var step in steps)
        {
            await DeleteAsync(step.Id, tenantId, cancellationToken);
        }
    }

    private async Task<IEnumerable<ApprovalStep>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'ApprovalStep'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    private static Dictionary<string, object> SerializeProperties(ApprovalStep step) => new()
    {
        ["ChainId"] = step.ChainId.ToString(),
        ["StepOrder"] = step.StepOrder.ToString(),
        ["Name"] = step.Name,
        ["ApproverType"] = step.ApproverType ?? string.Empty,
        ["SpecificApproverId"] = step.SpecificApproverId?.ToString() ?? string.Empty,
        ["Condition"] = step.Condition ?? string.Empty,
        ["TimeoutHours"] = step.TimeoutHours.ToString(),
        ["IsRequired"] = step.IsRequired.ToString()
    };

    private static ApprovalStep MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new ApprovalStep
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            ChainId = Guid.Parse(GetProperty(properties, "ChainId")),
            StepOrder = int.Parse(GetProperty(properties, "StepOrder")),
            Name = GetProperty(properties, "Name"),
            ApproverType = GetPropertyOrNull(properties, "ApproverType"),
            SpecificApproverId = Guid.TryParse(GetPropertyOrNull(properties, "SpecificApproverId"), out var approverId) ? approverId : null,
            Condition = GetPropertyOrNull(properties, "Condition"),
            TimeoutHours = int.Parse(GetProperty(properties, "TimeoutHours")),
            IsRequired = bool.Parse(GetProperty(properties, "IsRequired")),
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
