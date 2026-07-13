using OrgSphere.Domain.Interfaces;
using OrgSphere.Infrastructure.Persistence;
using OrgSphere.Infrastructure.Repositories;

namespace OrgSphere.Infrastructure;

public class UnitOfWork(INeo4jContext context) : IUnitOfWork
{
    private readonly INeo4jContext _context = context;
    private ITenantRepository? _tenants;
    private IUserRepository? _users;
    private IGraphNodeRepository? _graphNodes;
    private IGraphEdgeRepository? _graphEdges;
    private IEmployeeRepository? _employees;

    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IGraphNodeRepository GraphNodes => _graphNodes ??= new GraphNodeRepository(_context);
    public IGraphEdgeRepository GraphEdges => _graphEdges ??= new GraphEdgeRepository(_context);
    public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Neo4j doesn't have traditional transactions like RDBMS
        // Changes are committed as they happen
        return await Task.FromResult(1);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
