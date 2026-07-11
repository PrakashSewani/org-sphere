using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Domain.Entities;

public abstract class BaseEntity
{
    public TenantId TenantId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Tenant : BaseEntity
{
    public TenantId Id { get; set; } = TenantId.New();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantPlan Plan { get; set; } = TenantPlan.Free;
    public bool IsActive { get; set; } = true;
    
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<GraphNode> Nodes { get; set; } = new List<GraphNode>();
    public ICollection<GraphEdge> Edges { get; set; } = new List<GraphEdge>();
}

public class User : BaseEntity
{
    public UserId Id { get; set; } = UserId.New();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    public Tenant Tenant { get; set; } = null!;
}

public class GraphNode : BaseEntity
{
    public NodeId Id { get; set; } = NodeId.New();
    public NodeType Type { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    
    public Tenant Tenant { get; set; } = null!;
    public ICollection<GraphEdge> OutgoingEdges { get; set; } = new List<GraphEdge>();
    public ICollection<GraphEdge> IncomingEdges { get; set; } = new List<GraphEdge>();
}

public class GraphEdge : BaseEntity
{
    public EdgeId Id { get; set; } = EdgeId.New();
    public EdgeType Type { get; set; }
    public NodeId SourceId { get; set; } = null!;
    public NodeId TargetId { get; set; } = null!;
    public Dictionary<string, object> Properties { get; set; } = new();
    
    public Tenant Tenant { get; set; } = null!;
    public GraphNode Source { get; set; } = null!;
    public GraphNode Target { get; set; } = null!;
}
