using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task DeleteAsync(TenantId id, CancellationToken cancellationToken = default);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserId id, TenantId tenantId, CancellationToken cancellationToken = default);
}

public interface IGraphNodeRepository
{
    Task<GraphNode?> GetByIdAsync(NodeId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GraphNode>> GetAllAsync(TenantId tenantId, NodeType? type = null, CancellationToken cancellationToken = default);
    Task<GraphNode> CreateAsync(GraphNode node, CancellationToken cancellationToken = default);
    Task UpdateAsync(GraphNode node, CancellationToken cancellationToken = default);
    Task DeleteAsync(NodeId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GraphNode>> TraverseAsync(NodeId startNodeId, EdgeType edgeType, CancellationToken cancellationToken = default);
}

public interface IGraphEdgeRepository
{
    Task<GraphEdge?> GetByIdAsync(EdgeId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GraphEdge>> GetAllAsync(TenantId tenantId, EdgeType? type = null, CancellationToken cancellationToken = default);
    Task<GraphEdge> CreateAsync(GraphEdge edge, CancellationToken cancellationToken = default);
    Task UpdateAsync(GraphEdge edge, CancellationToken cancellationToken = default);
    Task DeleteAsync(EdgeId id, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GraphEdge>> GetBySourceAsync(NodeId sourceId, TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GraphEdge>> GetByTargetAsync(NodeId targetId, TenantId tenantId, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable
{
    ITenantRepository Tenants { get; }
    IUserRepository Users { get; }
    IGraphNodeRepository GraphNodes { get; }
    IGraphEdgeRepository GraphEdges { get; }
    IEmployeeRepository Employees { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
