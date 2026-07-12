using FluentAssertions;
using Moq;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Tests.Fixtures;

namespace OrgSphere.Tests.Services;

public class GraphServiceTests : IDisposable
{
    private readonly MockUnitOfWorkFixture _fixture = new();
    private readonly MockTenantContextFixture _tenantContext = new();
    private readonly Mock<IEventBus> _eventBus;
    private readonly GraphService _sut;

    public GraphServiceTests()
    {
        _eventBus = MockEventBusFixture.Create();
        _tenantContext.SetTenantContext(_fixture.TestTenantId, _fixture.TestUserId, UserRole.Admin);
        _sut = new GraphService(_fixture.UnitOfWork.Object, _tenantContext, _eventBus.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task CreateNodeAsync_ShouldCreateNodeAndPublishEvent()
    {
        var node = _fixture.CreateTestNode();
        _fixture.GraphNodeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.CreateNodeAsync(NodeType.Department, new Dictionary<string, object> { ["Name"] = "Engineering" });

        result.Should().NotBeNull();
        result.Type.Should().Be(NodeType.Department);
        _fixture.GraphNodeRepository.Verify(r => r.CreateAsync(It.IsAny<GraphNode>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNodeAsync_ShouldReturnNode_WhenExists()
    {
        var node = _fixture.CreateTestNode();
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        var result = await _sut.GetNodeAsync(node.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(node.Id.Value);
        result.Type.Should().Be(NodeType.Department);
    }

    [Fact]
    public async Task GetNodeAsync_ShouldReturnNull_WhenNotExists()
    {
        var nodeId = NodeId.New();
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(nodeId, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSphere.Domain.Entities.GraphNode?)null);

        var result = await _sut.GetNodeAsync(nodeId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateEdgeAsync_ShouldCreateEdge_WhenNodesExist()
    {
        var sourceNode = _fixture.CreateTestNode(NodeType.Employee, "Alice");
        var targetNode = _fixture.CreateTestNode(NodeType.Employee, "Bob");
        var edge = _fixture.CreateTestEdge(sourceNode.Id, targetNode.Id);

        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(sourceNode.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceNode);
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(targetNode.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetNode);
        _fixture.GraphEdgeRepository.Setup(r => r.CreateAsync(It.IsAny<GraphEdge>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(edge);

        var result = await _sut.CreateEdgeAsync(EdgeType.ReportsTo, sourceNode.Id, targetNode.Id);

        result.Should().NotBeNull();
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<EdgeCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEdgeAsync_ShouldThrow_WhenSourceNodeNotFound()
    {
        var sourceId = NodeId.New();
        var targetId = NodeId.New();

        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(sourceId, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSphere.Domain.Entities.GraphNode?)null);

        var act = () => _sut.CreateEdgeAsync(EdgeType.ReportsTo, sourceId, targetId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Source node {sourceId}*");
    }

    [Fact]
    public async Task CreateEdgeAsync_ShouldThrow_WhenTargetNodeNotFound()
    {
        var sourceId = NodeId.New();
        var targetId = NodeId.New();
        var sourceNode = _fixture.CreateTestNode();

        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(sourceId, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceNode);
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(targetId, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSphere.Domain.Entities.GraphNode?)null);

        var act = () => _sut.CreateEdgeAsync(EdgeType.ReportsTo, sourceId, targetId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*Target node {targetId}*");
    }

    [Fact]
    public async Task DeleteNodeAsync_ShouldDeleteNodeAndPublishEvent()
    {
        var node = _fixture.CreateTestNode();
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(node);

        await _sut.DeleteNodeAsync(node.Id);

        _fixture.GraphNodeRepository.Verify(r => r.DeleteAsync(node.Id, _fixture.TestTenantId, It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<NodeDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteNodeAsync_ShouldThrow_WhenNotFound()
    {
        var nodeId = NodeId.New();
        _fixture.GraphNodeRepository.Setup(r => r.GetByIdAsync(nodeId, _fixture.TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSphere.Domain.Entities.GraphNode?)null);

        var act = () => _sut.DeleteNodeAsync(nodeId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllNodesAsync_ShouldReturnAllNodes()
    {
        var nodes = new List<OrgSphere.Domain.Entities.GraphNode>
        {
            _fixture.CreateTestNode(NodeType.Department, "Engineering"),
            _fixture.CreateTestNode(NodeType.Department, "Product")
        };
        _fixture.GraphNodeRepository.Setup(r => r.GetAllAsync(_fixture.TestTenantId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodes);

        var result = await _sut.GetAllNodesAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllNodesAsync_ShouldFilterByType()
    {
        var nodes = new List<OrgSphere.Domain.Entities.GraphNode>
        {
            _fixture.CreateTestNode(NodeType.Department, "Engineering")
        };
        _fixture.GraphNodeRepository.Setup(r => r.GetAllAsync(_fixture.TestTenantId, NodeType.Department, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nodes);

        var result = await _sut.GetAllNodesAsync(NodeType.Department);

        result.Should().HaveCount(1);
        result[0].Type.Should().Be(NodeType.Department);
    }
}
