# Test Workflow Skill

Load this skill when writing or running tests for OrgSphere.

---

## When to Use

- Writing unit tests for services
- Writing integration tests for APIs
- Writing end-to-end tests for workflows
- Running test suites
- Debugging test failures

---

## Tech Stack

- **Test Framework**: xUnit
- **Mocking**: Moq
- **Integration Tests**: WebApplicationFactory
- **Assertions**: xUnit assertions + FluentAssertions (optional)

---

## Testing Strategy

### Test Pyramid

```
        ┌─────────┐
        │   E2E   │  Few, slow, high confidence
        ├─────────┤
        │Integration│  Moderate, test API boundaries
        ├─────────┤
        │  Unit   │  Many, fast, focused
        └─────────┘
```

### Coverage Targets

- Unit tests: 80%+
- Integration tests: Critical paths
- E2E tests: User journeys

---

## Unit Tests

### Service Tests

```csharp
// tests/OrgSphere.Tests/Services/LeaveServiceTests.cs
using Moq;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using Xunit;

namespace OrgSphere.Tests.Services;

public class LeaveServiceTests
{
    private readonly Mock<ILeaveRepository> _repository = new();
    private readonly Mock<IGraphService> _graphService = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly LeaveService _sut;

    public LeaveServiceTests()
    {
        _tenantContext.Setup(t => t.TenantId).Returns(new TenantId(Guid.NewGuid()));
        _sut = new LeaveService(
            _repository.Object,
            _graphService.Object,
            _eventBus.Object,
            _tenantContext.Object);
    }

    [Fact]
    public async Task CreateRequest_ShouldCreateLeaveRequest()
    {
        // Arrange
        var command = new CreateLeaveRequestCommand(
            EmployeeId: Guid.NewGuid(),
            Type: LeaveType.Vacation,
            StartDate: new DateTime(2025, 2, 1),
            EndDate: new DateTime(2025, 2, 5),
            Reason: "Holiday");

        _graphService.Setup(g => g.GetNodeAsync(
                It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GraphNode { Id = new NodeId(command.EmployeeId) });

        _repository.Setup(r => r.CreateAsync(
                It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest r, CancellationToken _) => r);

        // Act
        var result = await _sut.CreateRequestAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Pending, result.Status);
        _eventBus.Verify(e => e.PublishAsync(
            It.IsAny<LeaveRequestedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateRequest_ShouldThrowWhenEmployeeNotFound()
    {
        // Arrange
        var command = new CreateLeaveRequestCommand(
            EmployeeId: Guid.NewGuid(),
            Type: LeaveType.Vacation,
            StartDate: new DateTime(2025, 2, 1),
            EndDate: new DateTime(2025, 2, 5),
            Reason: null);

        _graphService.Setup(g => g.GetNodeAsync(
                It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphNode?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateRequestAsync(command));
    }

    [Fact]
    public async Task ApproveRequest_ShouldUpdateStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approvedBy = Guid.NewGuid();
        var existing = new LeaveRequest
        {
            Id = requestId,
            Status = LeaveStatus.Pending,
            EmployeeId = Guid.NewGuid()
        };

        _repository.Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repository.Setup(r => r.UpdateAsync(
                It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeaveRequest r, CancellationToken _) => r);

        // Act
        var result = await _sut.ApproveRequestAsync(requestId, approvedBy);

        // Assert
        Assert.Equal(LeaveStatus.Approved, result.Status);
        Assert.Equal(approvedBy, result.ApprovedBy);
    }
}
```

### Graph Service Tests

```csharp
// tests/OrgSphere.Tests/Services/GraphServiceTests.cs
using Moq;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using Xunit;

namespace OrgSphere.Tests.Services;

public class GraphServiceTests
{
    private readonly Mock<IGraphRepository> _graphRepo = new();
    private readonly Mock<ITenantContext> _tenantContext = new();
    private readonly GraphService _sut;

    public GraphServiceTests()
    {
        _tenantContext.Setup(t => t.TenantId).Returns(new TenantId(Guid.NewGuid()));
        _sut = new GraphService(_graphRepo.Object, _tenantContext.Object);
    }

    [Fact]
    public async Task CreateNode_ShouldEnforceTenantContext()
    {
        // Arrange
        var props = new Dictionary<string, object> { ["Name"] = "John Doe" };

        // Act
        var node = await _sut.CreateNodeAsync(NodeType.Employee, props);

        // Assert
        Assert.Equal(_tenantContext.Object.TenantId, node.TenantId);
    }

    [Fact]
    public async Task Traverse_ShouldOnlyReturnSameTenantNodes()
    {
        // Arrange
        var managerId = new NodeId(Guid.NewGuid());

        // Act
        var results = await _sut.GetDirectReportsAsync(managerId.Value);

        // Assert
        foreach (var node in results)
        {
            Assert.Equal(_tenantContext.Object.TenantId, node.TenantId);
        }
    }
}
```

---

## Integration Tests

### API Tests with WebApplicationFactory

```csharp
// tests/OrgSphere.Tests/Integration/LeaveApiTests.cs
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OrgSphere.Application.DTOs;
using Xunit;

namespace OrgSphere.Tests.Integration;

public class LeaveApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LeaveApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        // Add auth header for authenticated endpoints
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", TestAuthHelper.GetToken());
    }

    [Fact]
    public async Task PostLeaveRequest_ShouldReturn201()
    {
        // Arrange
        var command = new CreateLeaveRequestCommand(
            EmployeeId: Guid.NewGuid(),
            Type: LeaveType.Vacation,
            StartDate: new DateTime(2025, 2, 1),
            EndDate: new DateTime(2025, 2, 5),
            Reason: "Holiday");

        // Act
        var response = await _client.PostAsJsonAsync("/api/leave/requests", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LeaveRequestDto>();
        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Pending, result!.Status);
    }

    [Fact]
    public async Task PostLeaveRequest_ShouldReturn401WithoutAuth()
    {
        // Arrange
        var unauthClient = new HttpClient();
        unauthClient.BaseAddress = _client.BaseAddress;

        // Act
        var response = await unauthClient.PostAsJsonAsync(
            "/api/leave/requests",
            new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequests_ShouldReturnList()
    {
        // Act
        var response = await _client.GetAsync("/api/leave/requests");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotNull(results);
    }
}
```

### Database/Repository Tests (Neo4j)

```csharp
// tests/OrgSphere.Tests/Integration/LeaveRepositoryTests.cs
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Repositories;
using Xunit;

namespace OrgSphere.Tests.Integration;

[Collection("Neo4j")]
public class LeaveRepositoryTests : IDisposable
{
    private readonly Neo4jContext _context;
    private readonly LeaveRepository _repository;
    private readonly TenantId _tenantId = new(Guid.NewGuid());

    public LeaveRepositoryTests()
    {
        _context = new Neo4jContext(/* test connection */);
        _repository = new LeaveRepository(_context);
    }

    [Fact]
    public async Task Create_ShouldPersistAndRetrieve()
    {
        // Arrange
        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            EmployeeId = Guid.NewGuid(),
            Type = LeaveType.Vacation,
            StartDate = new DateTime(2025, 2, 1),
            EndDate = new DateTime(2025, 2, 5),
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.CreateAsync(entity);
        var found = await _repository.GetByIdAsync(entity.Id, _tenantId);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(entity.EmployeeId, found!.EmployeeId);
    }

    [Fact]
    public async Task GetById_ShouldIsolateByTenant()
    {
        // Arrange
        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            EmployeeId = Guid.NewGuid(),
            Type = LeaveType.Sick,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(entity);

        // Act - query with different tenant
        var otherTenant = new TenantId(Guid.NewGuid());
        var found = await _repository.GetByIdAsync(entity.Id, otherTenant);

        // Assert
        Assert.Null(found);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Test Utilities

### Mock Helpers

```csharp
// tests/OrgSphere.Tests/TestHelpers/MockHelpers.cs
using Moq;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;

namespace OrgSphere.Tests.TestHelpers;

public static class MockHelpers
{
    public static Mock<IEventBus> CreateMockEventBus()
    {
        var mock = new Mock<IEventBus>();
        mock.Setup(e => e.PublishAsync(
                It.IsAny<IDomainEvent>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    public static Mock<ITenantContext> CreateMockTenantContext(Guid? tenantId = null)
    {
        var mock = new Mock<ITenantContext>();
        mock.Setup(t => t.TenantId).Returns(new TenantId(tenantId ?? Guid.NewGuid()));
        return mock;
    }
}
```

### Test Auth Helper

```csharp
// tests/OrgSphere.Tests/TestHelpers/TestAuthHelper.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OrgSphere.Tests.TestHelpers;

public static class TestAuthHelper
{
    public static string GetToken(string role = "admin", Guid? tenantId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key-for-testing-only"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("tenantId", (tenantId ?? Guid.NewGuid()).ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: "test",
            audience: "test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## Running Tests

### Commands

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests only
dotnet test --filter "Category=Integration"

# Run specific test class
dotnet test --filter "FullyQualifiedName~LeaveServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~LeaveServiceTests.CreateRequest_ShouldCreateLeaveRequest"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run in watch mode
dotnet watch test
```

### CI/CD Integration

```yaml
# .github/workflows/test.yml
name: Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      neo4j:
        image: neo4j:5
        ports:
          - 7687:7687
        env:
          NEO4J_AUTH: neo4j/testpassword
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --collectCoverage=true
      - run: dotnet format --verify-no-changes
```

---

## Test Checklist

Before merging:

- [ ] All tests pass (`dotnet test`)
- [ ] Coverage meets target (80%+ for unit tests)
- [ ] No test isolation issues
- [ ] Mocks are cleaned up (verify no state leakage)
- [ ] Test data is properly managed
- [ ] Integration tests use WebApplicationFactory
- [ ] Tests are readable and maintainable
