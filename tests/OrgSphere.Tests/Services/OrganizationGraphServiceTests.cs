using FluentAssertions;
using Moq;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Tests.Fixtures;

namespace OrgSphere.Tests.Services;

public class OrganizationGraphServiceTests : IDisposable
{
    private readonly MockUnitOfWorkFixture _fixture = new();
    private readonly MockTenantContextFixture _tenantContext = new();
    private readonly Mock<IEventBus> _eventBus;
    private readonly OrganizationGraphService _sut;

    public OrganizationGraphServiceTests()
    {
        _eventBus = MockEventBusFixture.Create();
        _tenantContext.SetTenantContext(_fixture.TestTenantId, _fixture.TestUserId, UserRole.Admin);
        _sut = new OrganizationGraphService(_fixture.UnitOfWork.Object, _tenantContext, _eventBus.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region Company

    [Fact]
    public async Task CreateCompanyAsync_ShouldCreateNodeAndPublishEvent()
    {
        var node = _fixture.CreateTestNode(NodeType.Company, "Acme Corp");
        _fixture.GraphNodeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.CreateCompanyAsync(new CreateCompanyRequest("Acme Corp", "Tech", 100));

        result.Should().NotBeNull();
        result.Name.Should().Be("Acme Corp");
        _fixture.GraphNodeRepository.Verify(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCompanyAsync_ShouldReturnCompany_WhenExists()
    {
        var node = _fixture.CreateTestNode(NodeType.Company, "Acme Corp");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.GetCompanyAsync(node.Id.Value);

        result.Should().NotBeNull();
        result!.Id.Should().Be(node.Id.Value);
        result.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetCompanyAsync_ShouldReturnNull_WhenNotExists()
    {
        var id = Guid.NewGuid();
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(It.IsAny<NodeId>(), _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphNode?)null);

        var result = await _sut.GetCompanyAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCompaniesAsync_ShouldReturnAllCompanies()
    {
        var nodes = new List<GraphNode>
        {
            _fixture.CreateTestNode(NodeType.Company, "Acme Corp"),
            _fixture.CreateTestNode(NodeType.Company, "Globex")
        };
        _fixture.GraphNodeRepository.Setup(r => r.GetAllAsync(_fixture.TestTenantId, NodeType.Company, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodes);

        var result = await _sut.GetAllCompaniesAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateCompanyAsync_ShouldUpdatePropertiesAndPublishEvent()
    {
        var node = _fixture.CreateTestNode(NodeType.Company, "Acme Corp");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.UpdateCompanyAsync(node.Id.Value, new UpdateCompanyRequest("Acme Inc", "Finance", 200));

        result.Name.Should().Be("Acme Inc");
        result.Industry.Should().Be("Finance");
        result.EmployeeCount.Should().Be(200);
        _fixture.GraphNodeRepository.Verify(r => r.UpdateAsync(node, It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCompanyAsync_ShouldThrow_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(It.IsAny<NodeId>(), _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphNode?)null);

        var act = () => _sut.UpdateCompanyAsync(id, new UpdateCompanyRequest("X", "Y", 0));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteCompanyAsync_ShouldDeleteNodeAndPublishEvent()
    {
        var node = _fixture.CreateTestNode(NodeType.Company, "Acme Corp");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        await _sut.DeleteCompanyAsync(node.Id.Value);

        _fixture.GraphNodeRepository.Verify(r => r.DeleteAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCompanyAsync_ShouldThrow_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(It.IsAny<NodeId>(), _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphNode?)null);

        var act = () => _sut.DeleteCompanyAsync(id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Department

    [Fact]
    public async Task CreateDepartmentAsync_ShouldCreateNodeWithProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Department, "Engineering");
        _fixture.GraphNodeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.CreateDepartmentAsync(new CreateDepartmentRequest("Engineering", 500000, "CC-001"));

        result.Should().NotBeNull();
        result.Name.Should().Be("Engineering");
        _fixture.GraphNodeRepository.Verify(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDepartmentAsync_ShouldReturnDepartment_WhenExists()
    {
        var node = _fixture.CreateTestNode(NodeType.Department, "Engineering");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.GetDepartmentAsync(node.Id.Value);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ShouldReturnFilteredNodes()
    {
        var nodes = new List<GraphNode> { _fixture.CreateTestNode(NodeType.Department, "Engineering") };
        _fixture.GraphNodeRepository.Setup(r => r.GetAllAsync(_fixture.TestTenantId, NodeType.Department, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodes);

        var result = await _sut.GetAllDepartmentsAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Engineering");
    }

    [Fact]
    public async Task UpdateDepartmentAsync_ShouldUpdateProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Department, "Engineering");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.UpdateDepartmentAsync(node.Id.Value, new UpdateDepartmentRequest("Software", 750000, "CC-002"));

        result.Name.Should().Be("Software");
        result.Budget.Should().Be(750000);
        result.CostCenter.Should().Be("CC-002");
    }

    [Fact]
    public async Task DeleteDepartmentAsync_ShouldDeleteAndPublishEvent()
    {
        var node = _fixture.CreateTestNode(NodeType.Department, "Engineering");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        await _sut.DeleteDepartmentAsync(node.Id.Value);

        _fixture.GraphNodeRepository.Verify(r => r.DeleteAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Team

    [Fact]
    public async Task CreateTeamAsync_ShouldCreateNodeWithProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Team, "Backend Team");
        _fixture.GraphNodeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.CreateTeamAsync(new CreateTeamRequest("Backend Team", "API", 10));

        result.Should().NotBeNull();
        result.Name.Should().Be("Backend Team");
    }

    [Fact]
    public async Task GetTeamAsync_ShouldReturnTeam_WhenExists()
    {
        var node = _fixture.CreateTestNode(NodeType.Team, "Backend Team");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.GetTeamAsync(node.Id.Value);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Backend Team");
    }

    [Fact]
    public async Task UpdateTeamAsync_ShouldUpdateProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Team, "Backend Team");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.UpdateTeamAsync(node.Id.Value, new UpdateTeamRequest("Platform Team", "Infrastructure", 8));

        result.Name.Should().Be("Platform Team");
        result.FocusArea.Should().Be("Infrastructure");
        result.MaxSize.Should().Be(8);
    }

    [Fact]
    public async Task DeleteTeamAsync_ShouldDeleteAndPublishEvent()
    {
        var node = _fixture.CreateTestNode(NodeType.Team, "Backend Team");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        await _sut.DeleteTeamAsync(node.Id.Value);

        _fixture.GraphNodeRepository.Verify(r => r.DeleteAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Employee

    [Fact]
    public async Task CreateEmployeeAsync_ShouldCreateNodeWithProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        _fixture.GraphNodeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.CreateEmployeeAsync(new CreateOrgEmployeeRequest("Alice", "alice@acme.com", "Engineer"));

        result.Should().NotBeNull();
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetEmployeeAsync_ShouldReturnEmployee_WhenExists()
    {
        var node = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.GetEmployeeAsync(node.Id.Value);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ShouldUpdateProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.UpdateEmployeeAsync(node.Id.Value, new UpdateOrgEmployeeRequest("Alice Smith", "alice@acme.com", "Senior Engineer"));

        result.Name.Should().Be("Alice Smith");
        result.JobTitle.Should().Be("Senior Engineer");
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldDeleteAndPublishEvent()
    {
        var node = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        await _sut.DeleteEmployeeAsync(node.Id.Value);

        _fixture.GraphNodeRepository.Verify(r => r.DeleteAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Region

    [Fact]
    public async Task CreateRegionAsync_ShouldCreateNodeWithProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Region, "EMEA");
        _fixture.GraphNodeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.CreateRegionAsync(new CreateRegionRequest("EMEA", "Europe/London"));

        result.Should().NotBeNull();
        result.Name.Should().Be("EMEA");
    }

    [Fact]
    public async Task UpdateRegionAsync_ShouldUpdateProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Region, "EMEA");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.UpdateRegionAsync(node.Id.Value, new UpdateRegionRequest("APAC", "Asia/Tokyo"));

        result.Name.Should().Be("APAC");
        result.Timezone.Should().Be("Asia/Tokyo");
    }

    #endregion

    #region Office

    [Fact]
    public async Task CreateOfficeAsync_ShouldCreateNodeWithProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Office, "NYC Office");
        _fixture.GraphNodeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.CreateOfficeAsync(new CreateOfficeRequest("NYC Office", "New York", "USA", "123 Broadway"));

        result.Should().NotBeNull();
        result.Name.Should().Be("NYC Office");
    }

    [Fact]
    public async Task UpdateOfficeAsync_ShouldUpdateProperties()
    {
        var node = _fixture.CreateTestNode(NodeType.Office, "NYC Office");
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.UpdateOfficeAsync(node.Id.Value, new UpdateOfficeRequest("SF Office", "San Francisco", "USA", "456 Market St"));

        result.Name.Should().Be("SF Office");
        result.City.Should().Be("San Francisco");
    }

    #endregion

    #region Edges

    [Fact]
    public async Task CreateEdgeAsync_ShouldCreateEdge_WhenNodesExist()
    {
        var sourceNode = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        var targetNode = _fixture.CreateTestNode(NodeType.Team, "Backend");
        var edge = _fixture.CreateTestEdge(sourceNode.Id, targetNode.Id, EdgeType.MemberOf);

        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(sourceNode.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceNode);
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(targetNode.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetNode);
        _fixture.GraphEdgeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphEdge>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(edge);

        var result = await _sut.CreateEdgeAsync(EdgeType.MemberOf, sourceNode.Id.Value, targetNode.Id.Value);

        result.Should().NotBeNull();
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<EdgeCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEdgeAsync_ShouldThrow_WhenSourceNotFound()
    {
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(It.IsAny<NodeId>(), _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphNode?)null);

        var act = () => _sut.CreateEdgeAsync(EdgeType.MemberOf, sourceId, targetId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Source node {sourceId}*");
    }

    [Fact]
    public async Task CreateEdgeAsync_ShouldThrow_WhenTargetNotFound()
    {
        var sourceNode = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        var targetId = Guid.NewGuid();

        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(sourceNode.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceNode);
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(It.Is<NodeId>(n => n != sourceNode.Id), _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphNode?)null);

        var act = () => _sut.CreateEdgeAsync(EdgeType.MemberOf, sourceNode.Id.Value, targetId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteEdgeAsync_ShouldDeleteAndPublishEvent()
    {
        var sourceNode = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        var targetNode = _fixture.CreateTestNode(NodeType.Team, "Backend");
        var edge = _fixture.CreateTestEdge(sourceNode.Id, targetNode.Id, EdgeType.MemberOf);

        _fixture.GraphEdgeRepository.Setup(r => r.GetByIdAsync(edge.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(edge);

        await _sut.DeleteEdgeAsync(edge.Id.Value);

        _fixture.GraphEdgeRepository.Verify(r => r.DeleteAsync(edge.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<EdgeDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEdgeAsync_ShouldThrow_WhenNotFound()
    {
        var edgeId = Guid.NewGuid();
        _fixture.GraphEdgeRepository.Setup(r => r.GetByIdAsync(It.IsAny<EdgeId>(), _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphEdge?)null);

        var act = () => _sut.DeleteEdgeAsync(edgeId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetEdgesAsync_ShouldReturnOutgoingAndIncomingEdges()
    {
        var nodeId = NodeId.New();
        var sourceEdge = _fixture.CreateTestEdge(nodeId, NodeId.New(), EdgeType.Manages);
        var targetEdge = _fixture.CreateTestEdge(NodeId.New(), nodeId, EdgeType.ReportsTo);

        _fixture.GraphEdgeRepository.Setup(r => r.GetBySourceAsync(nodeId, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([sourceEdge]);
        _fixture.GraphEdgeRepository.Setup(r => r.GetByTargetAsync(nodeId, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([targetEdge]);

        var result = await _sut.GetEdgesAsync(nodeId.Value);

        result.Should().HaveCount(2);
    }

    #endregion
}
