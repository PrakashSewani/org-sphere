using Moq;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Tests.Fixtures;

public class MockUnitOfWorkFixture
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public Mock<ITenantRepository> TenantRepository { get; } = new();
    public Mock<IUserRepository> UserRepository { get; } = new();
    public Mock<IGraphNodeRepository> GraphNodeRepository { get; } = new();
    public Mock<IGraphEdgeRepository> GraphEdgeRepository { get; } = new();

    public TenantId TestTenantId { get; } = TenantId.New();
    public UserId TestUserId { get; } = UserId.New();

    public MockUnitOfWorkFixture()
    {
        UnitOfWork.Setup(u => u.Tenants).Returns(TenantRepository.Object);
        UnitOfWork.Setup(u => u.Users).Returns(UserRepository.Object);
        UnitOfWork.Setup(u => u.GraphNodes).Returns(GraphNodeRepository.Object);
        UnitOfWork.Setup(u => u.GraphEdges).Returns(GraphEdgeRepository.Object);
        UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    public Tenant CreateTestTenant(string name = "Test Corp", string slug = "test-corp")
    {
        return new Tenant
        {
            Id = TestTenantId,
            Name = name,
            Slug = slug,
            Plan = TenantPlan.Starter,
            IsActive = true
        };
    }

    public User CreateTestUser(string email = "test@test.com", UserRole role = UserRole.Employee)
    {
        return new User
        {
            Id = TestUserId,
            TenantId = TestTenantId,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "Test",
            LastName = "User",
            Role = role,
            IsActive = true
        };
    }

    public GraphNode CreateTestNode(NodeType type = NodeType.Department, string name = "Engineering")
    {
        return new GraphNode
        {
            Id = NodeId.New(),
            TenantId = TestTenantId,
            Type = type,
            Properties = new Dictionary<string, object> { ["Name"] = name }
        };
    }

    public GraphEdge CreateTestEdge(NodeId sourceId, NodeId targetId, EdgeType type = EdgeType.MemberOf)
    {
        return new GraphEdge
        {
            Id = EdgeId.New(),
            TenantId = TestTenantId,
            Type = type,
            SourceId = sourceId,
            TargetId = targetId,
            Properties = []
        };
    }
}
