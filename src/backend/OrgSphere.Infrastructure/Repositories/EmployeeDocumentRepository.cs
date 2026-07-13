using System.Text.Json;
using Neo4j.Driver;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Persistence;

namespace OrgSphere.Infrastructure.Repositories;

public class EmployeeDocumentRepository(INeo4jContext context) : IEmployeeDocumentRepository
{
    private readonly INeo4jContext _context = context;

    public async Task<EmployeeDocument?> GetByIdAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'EmployeeDocument'}) RETURN n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        var record = records.FirstOrDefault();
        return record is null ? null : MapToEntity(record);
    }

    public async Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"MATCH (n:GraphNode {TenantId: $tenantId, Type: 'EmployeeDocument'})
              WHERE n.Properties CONTAINS $employeeId
              RETURN n",
            new { tenantId = tenantId.Value.ToString(), employeeId = employeeId.ToString() });

        var records = await result.ToListAsync(cancellationToken);
        return [.. records.Select(MapToEntity).Where(d => d.EmployeeId == employeeId)];
    }

    public async Task<EmployeeDocument> CreateAsync(EmployeeDocument document, CancellationToken cancellationToken = default)
    {
        var properties = new Dictionary<string, object>
        {
            ["EmployeeId"] = document.EmployeeId.ToString(),
            ["FileName"] = document.FileName,
            ["ContentType"] = document.ContentType,
            ["FileSize"] = document.FileSize.ToString(),
            ["StoragePath"] = document.StoragePath,
            ["Description"] = document.Description ?? string.Empty
        };

        await using var session = _context.AsyncSession();
        var result = await session.RunAsync(
            @"CREATE (n:GraphNode {
                Id: $id,
                TenantId: $tenantId,
                Type: 'EmployeeDocument',
                Properties: $properties,
                CreatedAt: $createdAt,
                UpdatedAt: $updatedAt
            })
            RETURN n",
            new
            {
                id = document.Id.ToString(),
                tenantId = document.TenantId.Value.ToString(),
                properties = JsonSerializer.Serialize(properties),
                createdAt = document.CreatedAt.ToString("O"),
                updatedAt = document.CreatedAt.ToString("O")
            });

        var records = await result.ToListAsync(cancellationToken);
        return MapToEntity(records.First());
    }

    public async Task DeleteAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        await using var session = _context.AsyncSession();
        await session.RunAsync(
            "MATCH (n:GraphNode {Id: $id, TenantId: $tenantId, Type: 'EmployeeDocument'}) DETACH DELETE n",
            new { id = id.ToString(), tenantId = tenantId.Value.ToString() });
    }

    private static EmployeeDocument MapToEntity(IRecord record)
    {
        var node = record["n"].As<INode>();
        var props = node.Properties;
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(props["Properties"].As<string>()) ?? [];

        return new EmployeeDocument
        {
            Id = Guid.Parse(props["Id"].As<string>()),
            TenantId = new TenantId(Guid.Parse(props["TenantId"].As<string>())),
            EmployeeId = Guid.Parse(properties["EmployeeId"].ToString()!),
            FileName = properties["FileName"].ToString()!,
            ContentType = properties["ContentType"].ToString()!,
            FileSize = long.Parse(properties["FileSize"].ToString()!),
            StoragePath = properties["StoragePath"].ToString()!,
            Description = properties.TryGetValue("Description", out var desc) ? desc.ToString() : null,
            CreatedAt = DateTime.Parse(props["CreatedAt"].As<string>())
        };
    }
}
