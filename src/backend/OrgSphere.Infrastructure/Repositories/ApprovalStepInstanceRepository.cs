using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class ApprovalStepInstanceRepository(INeo4jContext context) : IApprovalStepInstanceRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<ApprovalStepInstance?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalStepInstance'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<ApprovalStepInstance>> GetByRequestIdAsync(Guid requestId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(s => s.RequestId == requestId);
    }

    public async Task<IEnumerable<ApprovalStepInstance>> GetPendingByApproverAsync(Guid approverId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(s => s.ApproverId == approverId && s.Status == ApprovalStepStatus.Pending);
    }

    public async Task<ApprovalStepInstance?> GetPendingByRequestAndOrderAsync(Guid requestId, int stepOrder, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.FirstOrDefault(s => s.RequestId == requestId && s.StepOrder == stepOrder && s.Status == ApprovalStepStatus.Pending);
    }

    public async Task<ApprovalStepInstance> CreateAsync(ApprovalStepInstance stepInstance, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(stepInstance);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'ApprovalStepInstance',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = stepInstance.Id.ToString(),
                tenantId = stepInstance.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = stepInstance.CreatedAt.ToString("O"),
                updatedAt = stepInstance.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(ApprovalStepInstance stepInstance, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(stepInstance);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalStepInstance'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = stepInstance.Id.ToString(),
                tenantId = stepInstance.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    private async Task<IEnumerable<ApprovalStepInstance>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'ApprovalStepInstance'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    private static Dictionary<string, object> SerializeProperties(ApprovalStepInstance step) => new()
    {
        ["RequestId"] = step.RequestId.ToString(),
        ["StepId"] = step.StepId.ToString(),
        ["StepOrder"] = step.StepOrder.ToString(),
        ["ApproverId"] = step.ApproverId?.ToString() ?? string.Empty,
        ["DelegatedToId"] = step.DelegatedToId?.ToString() ?? string.Empty,
        ["Status"] = step.Status.ToString(),
        ["Comments"] = step.Comments ?? string.Empty,
        ["RespondedAt"] = step.RespondedAt?.ToString("O") ?? string.Empty,
        ["EscalatedAt"] = step.EscalatedAt?.ToString("O") ?? string.Empty
    };

    private static ApprovalStepInstance MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new ApprovalStepInstance
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            RequestId = Guid.Parse(GetProperty(properties, "RequestId")),
            StepId = Guid.Parse(GetProperty(properties, "StepId")),
            StepOrder = int.Parse(GetProperty(properties, "StepOrder")),
            ApproverId = Guid.TryParse(GetPropertyOrNull(properties, "ApproverId"), out var approverId) ? approverId : null,
            DelegatedToId = Guid.TryParse(GetPropertyOrNull(properties, "DelegatedToId"), out var delegatedToId) ? delegatedToId : null,
            Status = Enum.Parse<ApprovalStepStatus>(GetProperty(properties, "Status")),
            Comments = GetPropertyOrNull(properties, "Comments"),
            RespondedAt = DateTime.TryParse(GetPropertyOrNull(properties, "RespondedAt"), out var respondedAt) ? respondedAt : null,
            EscalatedAt = DateTime.TryParse(GetPropertyOrNull(properties, "EscalatedAt"), out var escalatedAt) ? escalatedAt : null,
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
