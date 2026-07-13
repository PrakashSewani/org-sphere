using FluentAssertions;
using Moq;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Events;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Tests.Fixtures;

namespace OrgSphere.Tests.Services;

public class EmployeeServiceTests : IDisposable
{
    private readonly Mock<IEmployeeRepository> _repository = new();
    private readonly Mock<IEmployeeDocumentRepository> _documentRepository = new();
    private readonly Mock<IEventBus> _eventBus;
    private readonly MockTenantContextFixture _tenantContext = new();
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _eventBus = MockEventBusFixture.Create();
        _tenantContext.SetTenantContext(TenantId.New(), UserId.New(), UserRole.Admin);
        _sut = new EmployeeService(_repository.Object, _documentRepository.Object, _eventBus.Object, _tenantContext);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #region Create

    [Fact]
    public async Task CreateAsync_ShouldCreateEmployeeAndPublishEvent()
    {
        var request = new CreateEmployeeRequest(
            EmployeeId: "EMP-001",
            FirstName: "Alice",
            LastName: "Smith",
            Email: "alice@acme.com",
            Phone: "+1234567890",
            Title: "Engineer",
            DepartmentId: Guid.NewGuid(),
            TeamId: null,
            ManagerId: null,
            OfficeId: null,
            EmploymentType: EmploymentType.FullTime,
            StartDate: new DateTime(2025, 1, 15));

        _repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        _repository.Setup(r => r.CreateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee e, CancellationToken _) => e);

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("alice@acme.com");
        result.Status.Should().Be(EmployeeStatus.Active);
        _repository.Verify(r => r.CreateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<EmployeeCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowWhenEmailAlreadyExists()
    {
        var request = new CreateEmployeeRequest(
            EmployeeId: "EMP-001",
            FirstName: "Alice",
            LastName: "Smith",
            Email: "alice@acme.com",
            Phone: null,
            Title: "Engineer",
            DepartmentId: Guid.NewGuid(),
            TeamId: null,
            ManagerId: null,
            OfficeId: null,
            EmploymentType: EmploymentType.FullTime,
            StartDate: DateTime.UtcNow);

        var existing = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            Email = "alice@acme.com"
        };

        _repository.Setup(r => r.GetByEmailAsync("alice@acme.com", It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*alice@acme.com*");
    }

    #endregion

    #region Read

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployee_WhenExists()
    {
        var employee = CreateTestEmployee();
        _repository.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.GetByIdAsync(employee.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(employee.Id);
        result.FirstName.Should().Be(employee.FirstName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnEmployee_WhenExists()
    {
        var employee = CreateTestEmployee();
        _repository.Setup(r => r.GetByEmailAsync(employee.Email, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.GetByEmailAsync(employee.Email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(employee.Email);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEmployees()
    {
        var employees = new List<Employee>
        {
            CreateTestEmployee("Alice"),
            CreateTestEmployee("Bob")
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDepartmentAsync_ShouldReturnFilteredEmployees()
    {
        var departmentId = Guid.NewGuid();
        var employees = new List<Employee>
        {
            CreateTestEmployee(departmentId: departmentId)
        };
        _repository.Setup(r => r.GetByDepartmentAsync(departmentId, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        var result = await _sut.GetByDepartmentAsync(departmentId);

        result.Should().HaveCount(1);
        result[0].DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public async Task GetByManagerAsync_ShouldReturnFilteredEmployees()
    {
        var managerId = Guid.NewGuid();
        var employees = new List<Employee>
        {
            CreateTestEmployee(managerId: managerId)
        };
        _repository.Setup(r => r.GetByManagerAsync(managerId, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        var result = await _sut.GetByManagerAsync(managerId);

        result.Should().HaveCount(1);
        result[0].ManagerId.Should().Be(managerId);
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEmployeeAndPublishEvent()
    {
        var employee = CreateTestEmployee();
        _repository.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateEmployeeRequest(
            Phone: "+9876543210",
            Title: "Senior Engineer",
            DepartmentId: employee.DepartmentId,
            TeamId: employee.TeamId,
            ManagerId: employee.ManagerId,
            OfficeId: employee.OfficeId,
            EmploymentType: EmploymentType.FullTime,
            EndDate: null,
            Status: EmployeeStatus.Active);

        var result = await _sut.UpdateAsync(employee.Id, request);

        result.Should().NotBeNull();
        result.Phone.Should().Be("+9876543210");
        result.Title.Should().Be("Senior Engineer");
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<EmployeeUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var request = new UpdateEmployeeRequest(
            Phone: null, Title: "Engineer", DepartmentId: Guid.NewGuid(),
            TeamId: null, ManagerId: null, OfficeId: null,
            EmploymentType: EmploymentType.FullTime, EndDate: null,
            Status: EmployeeStatus.Active);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEmployeeAndPublishEvent()
    {
        var employee = CreateTestEmployee();
        _repository.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _repository.Setup(r => r.DeleteAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(employee.Id);

        _repository.Verify(r => r.DeleteAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<EmployeeDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Self-Service

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateBioAndSkills()
    {
        var employee = CreateTestEmployee();
        _repository.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateProfileRequest(
            Bio: "Software engineer with 5 years experience",
            Phone: "+1111111111",
            Skills: ["C#", "React", "Neo4j"]);

        var result = await _sut.UpdateProfileAsync(employee.Id, request);

        result.Should().NotBeNull();
        result.Bio.Should().Be("Software engineer with 5 years experience");
        result.Phone.Should().Be("+1111111111");
        result.Skills.Should().BeEquivalentTo(["C#", "React", "Neo4j"]);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(e => e.PublishAsync(It.IsAny<EmployeeUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldThrow_WhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var act = () => _sut.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileRequest("Bio", null, null));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdatePreferencesAsync_ShouldUpdateNotificationSettings()
    {
        var employee = CreateTestEmployee();
        _repository.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdatePreferencesRequest(
            EmailNotifications: false,
            PushNotifications: true,
            ContactEmail: "personal@email.com",
            ContactPhone: null);

        var result = await _sut.UpdatePreferencesAsync(employee.Id, request);

        result.Should().NotBeNull();
        result.Preferences.EmailNotifications.Should().BeFalse();
        result.Preferences.PushNotifications.Should().BeTrue();
        result.Preferences.ContactEmail.Should().Be("personal@email.com");
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePreferencesAsync_ShouldThrow_WhenNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var act = () => _sut.UpdatePreferencesAsync(Guid.NewGuid(), new UpdatePreferencesRequest(null, null, null, null));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnEmployeeDocuments()
    {
        var employee = CreateTestEmployee();
        var documents = new List<EmployeeDocument>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId!,
                EmployeeId = employee.Id,
                FileName = "resume.pdf",
                ContentType = "application/pdf",
                FileSize = 1024,
                StoragePath = "/uploads/resume.pdf",
                CreatedAt = DateTime.UtcNow
            }
        };

        _documentRepository.Setup(r => r.GetByEmployeeAsync(employee.Id, It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        var result = await _sut.GetDocumentsAsync(employee.Id);

        result.Should().HaveCount(1);
        result[0].FileName.Should().Be("resume.pdf");
    }

    #endregion

    private Employee CreateTestEmployee(
        string firstName = "Alice",
        Guid? departmentId = null,
        Guid? managerId = null)
    {
        return new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId!,
            EmployeeId = "EMP-001",
            FirstName = firstName,
            LastName = "Smith",
            Email = $"{firstName.ToLower()}@acme.com",
            Phone = "+1234567890",
            Title = "Engineer",
            DepartmentId = departmentId ?? Guid.NewGuid(),
            TeamId = null,
            ManagerId = managerId,
            OfficeId = null,
            EmploymentType = EmploymentType.FullTime,
            StartDate = new DateTime(2025, 1, 15),
            Status = EmployeeStatus.Active,
            Skills = ["C#", "React"],
            Preferences = new EmployeePreferences(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
