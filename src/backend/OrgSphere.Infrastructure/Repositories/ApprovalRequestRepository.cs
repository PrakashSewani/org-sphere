using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class ApprovalRequestRepository(INeo4jContext context) : IApprovalRequestRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<ApprovalRequest?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalRequest'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<ApprovalRequest>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {TenantId: $tenantId, Type: 'ApprovalRequest'}) RETURN n",
            new { tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity)];
    }

    public async Task<IEnumerable<ApprovalRequest>> GetByRequesterAsync(Guid requesterId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(a => a.RequesterId == requesterId);
    }

    public async Task<IEnumerable<ApprovalRequest>> GetByApproverAsync(Guid approverId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(a => a.FinalApproverId == approverId);
    }

    public async Task<IEnumerable<ApprovalRequest>> GetByStatusAsync(ApprovalStatus status, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(a => a.Status == status);
    }

    public async Task<IEnumerable<ApprovalRequest>> GetByTypeAsync(ApprovalType type, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(tenantId, cancellationToken);
        return all.Where(a => a.Type == type);
    }

    public async Task<ApprovalRequest> CreateAsync(ApprovalRequest approvalRequest, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(approvalRequest);

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'ApprovalRequest',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = approvalRequest.Id.ToString(),
                tenantId = approvalRequest.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = approvalRequest.CreatedAt.ToString("O"),
                updatedAt = approvalRequest.UpdatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task UpdateAsync(ApprovalRequest approvalRequest, CancellationToken cancellationToken = default)
    {
        var properties = SerializeProperties(approvalRequest);

        await using var session = _context.AsyncSession();
        await session.RunAsync(
            @"MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalRequest'})
              SET n.Properties = $properties,
                  n.UpdatedAt = $updatedAt",
            new
            {
                id = approvalRequest.Id.ToString(),
                tenantId = approvalRequest.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                updatedAt = DateTime.UtcNow.ToString("O")
            });
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'ApprovalRequest'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static Dictionary<string, object> SerializeProperties(ApprovalRequest request) => new()
    {
        ["Type"] = request.Type.ToString(),
        ["RequesterId"] = request.RequesterId.ToString(),
        ["RelatedEntityId"] = request.RelatedEntityId?.ToString() ?? string.Empty,
        ["Title"] = request.Title ?? string.Empty,
        ["Description"] = request.Description ?? string.Empty,
        ["Metadata"] = request.Metadata ?? string.Empty,
        ["Status"] = request.Status.ToString(),
        ["CurrentStep"] = request.CurrentStep.ToString(),
        ["TotalSteps"] = request.TotalSteps.ToString(),
        ["FinalApproverId"] = request.FinalApproverId?.ToString() ?? string.Empty,
        ["FinalApprovedAt"] = request.FinalApprovedAt?.ToString("O") ?? string.Empty,
        ["FinalRejectionReason"] = request.FinalRejectionReason ?? string.Empty
    };

    private static ApprovalRequest MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new ApprovalRequest
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            Type = Enum.Parse<ApprovalType>(GetProperty(properties, "Type")),
            RequesterId = Guid.Parse(GetProperty(properties, "RequesterId")),
            RelatedEntityId = Guid.TryParse(GetPropertyOrNull(properties, "RelatedEntityId"), out var relatedId) ? relatedId : null,
            Title = GetPropertyOrNull(properties, "Title"),
            Description = GetPropertyOrNull(properties, "Description"),
            Metadata = GetPropertyOrNull(properties, "Metadata"),
            Status = Enum.Parse<ApprovalStatus>(GetProperty(properties, "Status")),
            CurrentStep = int.Parse(GetProperty(properties, "CurrentStep")),
            TotalSteps = int.Parse(GetProperty(properties, "TotalSteps")),
            FinalApproverId = Guid.TryParse(GetPropertyOrNull(properties, "FinalApproverId"), out var approverId) ? approverId : null,
            FinalApprovedAt = DateTime.TryParse(GetPropertyOrNull(properties, "FinalApprovedAt"), out var approvedAt) ? approvedAt : null,
            FinalRejectionReason = GetPropertyOrNull(properties, "FinalRejectionReason"),
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
