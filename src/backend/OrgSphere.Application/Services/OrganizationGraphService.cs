using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public class OrganizationGraphService(
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IEventBus eventBus) : IOrganizationGraphService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly IEventBus _eventBus = eventBus;

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request, CancellationToken ct = default)
    {
        var props = new Dictionary<string, object>
        {
            ["Name"] = request.Name,
            ["Industry"] = request.Industry,
            ["EmployeeCount"] = request.EmployeeCount
        };
        var node = await CreateTypedNodeAsync(NodeType.Company, props, ct);
        return MapToCompanyDto(node);
    }

    public async Task<CompanyDto?> GetCompanyAsync(Guid id, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct);
        return node is null ? null : MapToCompanyDto(node);
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllCompaniesAsync(CancellationToken ct = default)
    {
        var nodes = await _unitOfWork.GraphNodes.GetAllAsync(_tenantContext.TenantId!, NodeType.Company, ct);
        return [.. nodes.Select(MapToCompanyDto)];
    }

    public async Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct) ?? throw new KeyNotFoundException($"Company {id} not found");
        node.Properties["Name"] = request.Name;
        node.Properties["Industry"] = request.Industry;
        node.Properties["EmployeeCount"] = request.EmployeeCount;
        node.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.GraphNodes.UpdateAsync(node, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeUpdatedEvent(_tenantContext.TenantId!, node.Id), ct);
        return MapToCompanyDto(node);
    }

    public async Task DeleteCompanyAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteNodeAsync(id, ct);
    }

    public async Task<RegionDto> CreateRegionAsync(CreateRegionRequest request, CancellationToken ct = default)
    {
        var props = new Dictionary<string, object>
        {
            ["Name"] = request.Name,
            ["Timezone"] = request.Timezone
        };
        var node = await CreateTypedNodeAsync(NodeType.Region, props, ct);
        return MapToRegionDto(node);
    }

    public async Task<RegionDto?> GetRegionAsync(Guid id, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct);
        return node is null ? null : MapToRegionDto(node);
    }

    public async Task<IReadOnlyList<RegionDto>> GetAllRegionsAsync(CancellationToken ct = default)
    {
        var nodes = await _unitOfWork.GraphNodes.GetAllAsync(_tenantContext.TenantId!, NodeType.Region, ct);
        return [.. nodes.Select(MapToRegionDto)];
    }

    public async Task<RegionDto> UpdateRegionAsync(Guid id, UpdateRegionRequest request, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct) ?? throw new KeyNotFoundException($"Region {id} not found");
        node.Properties["Name"] = request.Name;
        node.Properties["Timezone"] = request.Timezone;
        node.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.GraphNodes.UpdateAsync(node, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeUpdatedEvent(_tenantContext.TenantId!, node.Id), ct);
        return MapToRegionDto(node);
    }

    public async Task DeleteRegionAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteNodeAsync(id, ct);
    }

    public async Task<OfficeDto> CreateOfficeAsync(CreateOfficeRequest request, CancellationToken ct = default)
    {
        var props = new Dictionary<string, object>
        {
            ["Name"] = request.Name,
            ["City"] = request.City,
            ["Country"] = request.Country,
            ["Address"] = request.Address
        };
        var node = await CreateTypedNodeAsync(NodeType.Office, props, ct);
        return MapToOfficeDto(node);
    }

    public async Task<OfficeDto?> GetOfficeAsync(Guid id, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct);
        return node is null ? null : MapToOfficeDto(node);
    }

    public async Task<IReadOnlyList<OfficeDto>> GetAllOfficesAsync(CancellationToken ct = default)
    {
        var nodes = await _unitOfWork.GraphNodes.GetAllAsync(_tenantContext.TenantId!, NodeType.Office, ct);
        return [.. nodes.Select(MapToOfficeDto)];
    }

    public async Task<OfficeDto> UpdateOfficeAsync(Guid id, UpdateOfficeRequest request, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct) ?? throw new KeyNotFoundException($"Office {id} not found");
        node.Properties["Name"] = request.Name;
        node.Properties["City"] = request.City;
        node.Properties["Country"] = request.Country;
        node.Properties["Address"] = request.Address;
        node.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.GraphNodes.UpdateAsync(node, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeUpdatedEvent(_tenantContext.TenantId!, node.Id), ct);
        return MapToOfficeDto(node);
    }

    public async Task DeleteOfficeAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteNodeAsync(id, ct);
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request, CancellationToken ct = default)
    {
        var props = new Dictionary<string, object>
        {
            ["Name"] = request.Name,
            ["Budget"] = request.Budget,
            ["CostCenter"] = request.CostCenter
        };
        var node = await CreateTypedNodeAsync(NodeType.Department, props, ct);
        return MapToDepartmentDto(node);
    }

    public async Task<DepartmentDto?> GetDepartmentAsync(Guid id, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct);
        return node is null ? null : MapToDepartmentDto(node);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken ct = default)
    {
        var nodes = await _unitOfWork.GraphNodes.GetAllAsync(_tenantContext.TenantId!, NodeType.Department, ct);
        return [.. nodes.Select(MapToDepartmentDto)];
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(Guid id, UpdateDepartmentRequest request, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct) ?? throw new KeyNotFoundException($"Department {id} not found");
        node.Properties["Name"] = request.Name;
        node.Properties["Budget"] = request.Budget;
        node.Properties["CostCenter"] = request.CostCenter;
        node.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.GraphNodes.UpdateAsync(node, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeUpdatedEvent(_tenantContext.TenantId!, node.Id), ct);
        return MapToDepartmentDto(node);
    }

    public async Task DeleteDepartmentAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteNodeAsync(id, ct);
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, CancellationToken ct = default)
    {
        var props = new Dictionary<string, object>
        {
            ["Name"] = request.Name,
            ["FocusArea"] = request.FocusArea,
            ["MaxSize"] = request.MaxSize
        };
        var node = await CreateTypedNodeAsync(NodeType.Team, props, ct);
        return MapToTeamDto(node);
    }

    public async Task<TeamDto?> GetTeamAsync(Guid id, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct);
        return node is null ? null : MapToTeamDto(node);
    }

    public async Task<IReadOnlyList<TeamDto>> GetAllTeamsAsync(CancellationToken ct = default)
    {
        var nodes = await _unitOfWork.GraphNodes.GetAllAsync(_tenantContext.TenantId!, NodeType.Team, ct);
        return [.. nodes.Select(MapToTeamDto)];
    }

    public async Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamRequest request, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct) ?? throw new KeyNotFoundException($"Team {id} not found");
        node.Properties["Name"] = request.Name;
        node.Properties["FocusArea"] = request.FocusArea;
        node.Properties["MaxSize"] = request.MaxSize;
        node.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.GraphNodes.UpdateAsync(node, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeUpdatedEvent(_tenantContext.TenantId!, node.Id), ct);
        return MapToTeamDto(node);
    }

    public async Task DeleteTeamAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteNodeAsync(id, ct);
    }

    public async Task<OrgEmployeeDto> CreateEmployeeAsync(CreateOrgEmployeeRequest request, CancellationToken ct = default)
    {
        var props = new Dictionary<string, object>
        {
            ["Name"] = request.Name,
            ["Email"] = request.Email,
            ["JobTitle"] = request.JobTitle
        };
        var node = await CreateTypedNodeAsync(NodeType.Employee, props, ct);
        return MapToEmployeeDto(node);
    }

    public async Task<OrgEmployeeDto?> GetEmployeeAsync(Guid id, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct);
        return node is null ? null : MapToEmployeeDto(node);
    }

    public async Task<IReadOnlyList<OrgEmployeeDto>> GetAllEmployeesAsync(CancellationToken ct = default)
    {
        var nodes = await _unitOfWork.GraphNodes.GetAllAsync(_tenantContext.TenantId!, NodeType.Employee, ct);
        return [.. nodes.Select(MapToEmployeeDto)];
    }

    public async Task<OrgEmployeeDto> UpdateEmployeeAsync(Guid id, UpdateOrgEmployeeRequest request, CancellationToken ct = default)
    {
        var node = await GetNodeAsync(id, ct) ?? throw new KeyNotFoundException($"Employee {id} not found");
        node.Properties["Name"] = request.Name;
        node.Properties["Email"] = request.Email;
        node.Properties["JobTitle"] = request.JobTitle;
        node.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.GraphNodes.UpdateAsync(node, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeUpdatedEvent(_tenantContext.TenantId!, node.Id), ct);
        return MapToEmployeeDto(node);
    }

    public async Task DeleteEmployeeAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteNodeAsync(id, ct);
    }

    public async Task<IReadOnlyList<GraphEdgeDto>> GetEdgesAsync(Guid nodeId, CancellationToken ct = default)
    {
        var nid = new NodeId(nodeId);
        var outgoing = await _unitOfWork.GraphEdges.GetBySourceAsync(nid, _tenantContext.TenantId!, ct);
        var incoming = await _unitOfWork.GraphEdges.GetByTargetAsync(nid, _tenantContext.TenantId!, ct);
        return [.. outgoing.Concat(incoming).Select(MapToEdgeDto)];
    }

    public async Task<GraphEdgeDto> CreateEdgeAsync(EdgeType type, Guid sourceId, Guid targetId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var source = await _unitOfWork.GraphNodes.GetByIdAsync(new NodeId(sourceId), tenantId, ct)
            ?? throw new KeyNotFoundException($"Source node {sourceId} not found");
        var target = await _unitOfWork.GraphNodes.GetByIdAsync(new NodeId(targetId), tenantId, ct)
            ?? throw new KeyNotFoundException($"Target node {targetId} not found");

        var edge = new GraphEdge
        {
            TenantId = tenantId,
            Type = type,
            SourceId = source.Id,
            TargetId = target.Id,
            Properties = []
        };

        var created = await _unitOfWork.GraphEdges.CreateAsync(edge, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new EdgeCreatedEvent(tenantId, created.Id, type, source.Id, target.Id), ct);
        return MapToEdgeDto(created);
    }

    public async Task DeleteEdgeAsync(Guid edgeId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId!;
        var eid = new EdgeId(edgeId);
        _ = await _unitOfWork.GraphEdges.GetByIdAsync(eid, tenantId, ct)
            ?? throw new KeyNotFoundException($"Edge {edgeId} not found");
        await _unitOfWork.GraphEdges.DeleteAsync(eid, tenantId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new EdgeDeletedEvent(tenantId, eid), ct);
    }

    private async Task<GraphNode> CreateTypedNodeAsync(NodeType type, Dictionary<string, object> properties, CancellationToken ct)
    {
        var node = new GraphNode
        {
            TenantId = _tenantContext.TenantId!,
            Type = type,
            Properties = properties
        };
        var created = await _unitOfWork.GraphNodes.CreateAsync(node, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeCreatedEvent(_tenantContext.TenantId!, created.Id, type), ct);
        return created;
    }

    private async Task<GraphNode?> GetNodeAsync(Guid id, CancellationToken ct)
    {
        return await _unitOfWork.GraphNodes.GetByIdAsync(new NodeId(id), _tenantContext.TenantId!, ct);
    }

    private async Task DeleteNodeAsync(Guid id, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId!;
        var nodeId = new NodeId(id);
        _ = await _unitOfWork.GraphNodes.GetByIdAsync(nodeId, tenantId, ct)
            ?? throw new KeyNotFoundException($"Node {id} not found");
        await _unitOfWork.GraphNodes.DeleteAsync(nodeId, tenantId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _eventBus.PublishAsync(new NodeDeletedEvent(tenantId, nodeId), ct);
    }

    private static CompanyDto MapToCompanyDto(GraphNode node) => new()
    {
        Id = node.Id.Value,
        Name = node.Properties.TryGetValue("Name", out var n) ? n.ToString()! : string.Empty,
        Industry = node.Properties.TryGetValue("Industry", out var i) ? i.ToString()! : string.Empty,
        EmployeeCount = node.Properties.TryGetValue("EmployeeCount", out var ec) ? Convert.ToInt32(ec) : 0,
        CreatedAt = node.CreatedAt,
        UpdatedAt = node.UpdatedAt
    };

    private static RegionDto MapToRegionDto(GraphNode node) => new()
    {
        Id = node.Id.Value,
        Name = node.Properties.TryGetValue("Name", out var n) ? n.ToString()! : string.Empty,
        Timezone = node.Properties.TryGetValue("Timezone", out var tz) ? tz.ToString()! : string.Empty,
        CreatedAt = node.CreatedAt,
        UpdatedAt = node.UpdatedAt
    };

    private static OfficeDto MapToOfficeDto(GraphNode node) => new()
    {
        Id = node.Id.Value,
        Name = node.Properties.TryGetValue("Name", out var n) ? n.ToString()! : string.Empty,
        City = node.Properties.TryGetValue("City", out var c) ? c.ToString()! : string.Empty,
        Country = node.Properties.TryGetValue("Country", out var co) ? co.ToString()! : string.Empty,
        Address = node.Properties.TryGetValue("Address", out var a) ? a.ToString()! : string.Empty,
        CreatedAt = node.CreatedAt,
        UpdatedAt = node.UpdatedAt
    };

    private static DepartmentDto MapToDepartmentDto(GraphNode node) => new()
    {
        Id = node.Id.Value,
        Name = node.Properties.TryGetValue("Name", out var n) ? n.ToString()! : string.Empty,
        Budget = node.Properties.TryGetValue("Budget", out var b) ? Convert.ToDecimal(b) : 0,
        CostCenter = node.Properties.TryGetValue("CostCenter", out var cc) ? cc.ToString()! : string.Empty,
        CreatedAt = node.CreatedAt,
        UpdatedAt = node.UpdatedAt
    };

    private static TeamDto MapToTeamDto(GraphNode node) => new()
    {
        Id = node.Id.Value,
        Name = node.Properties.TryGetValue("Name", out var n) ? n.ToString()! : string.Empty,
        FocusArea = node.Properties.TryGetValue("FocusArea", out var fa) ? fa.ToString()! : string.Empty,
        MaxSize = node.Properties.TryGetValue("MaxSize", out var ms) ? Convert.ToInt32(ms) : 0,
        CreatedAt = node.CreatedAt,
        UpdatedAt = node.UpdatedAt
    };

    private static OrgEmployeeDto MapToEmployeeDto(GraphNode node) => new()
    {
        Id = node.Id.Value,
        Name = node.Properties.TryGetValue("Name", out var n) ? n.ToString()! : string.Empty,
        Email = node.Properties.TryGetValue("Email", out var e) ? e.ToString()! : string.Empty,
        JobTitle = node.Properties.TryGetValue("JobTitle", out var jt) ? jt.ToString()! : string.Empty,
        CreatedAt = node.CreatedAt,
        UpdatedAt = node.UpdatedAt
    };

    private static GraphEdgeDto MapToEdgeDto(GraphEdge edge) => new()
    {
        Id = edge.Id.Value,
        Type = edge.Type,
        SourceId = edge.SourceId.Value,
        TargetId = edge.TargetId.Value,
        Properties = edge.Properties,
        CreatedAt = edge.CreatedAt
    };
}
