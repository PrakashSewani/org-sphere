using FluentAssertions;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Permissions;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Tests.Fixtures;

namespace OrgSphere.Tests.Services;

public class TenantIsolationTests
{
    private readonly MockTenantContextFixture _tenantContextA = new();
    private readonly MockTenantContextFixture _tenantContextB = new();
    private readonly AuthorizationService _authorizationService = new();

    public TenantIsolationTests()
    {
        var tenantA = TenantId.New();
        var tenantB = TenantId.New();
        _tenantContextA.SetTenantContext(tenantA, UserId.New(), UserRole.Admin);
        _tenantContextB.SetTenantContext(tenantB, UserId.New(), UserRole.Employee);
    }

    [Fact]
    public void TenantContext_ShouldBeIsolated_BetweenDifferentTenants()
    {
        _tenantContextA.TenantId.Should().NotBe(_tenantContextB.TenantId);
    }

    [Fact]
    public void TenantContext_ShouldTrackAuthentication()
    {
        var authenticated = new MockTenantContextFixture();
        authenticated.SetTenantContext(TenantId.New(), UserId.New(), UserRole.Employee);
        authenticated.IsAuthenticated.Should().BeTrue();

        var unauthenticated = new MockTenantContextFixture();
        unauthenticated.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void TenantContext_ShouldTrackUserRole()
    {
        var context = new MockTenantContextFixture();
        context.SetTenantContext(TenantId.New(), UserId.New(), UserRole.Manager);
        context.Role.Should().Be(UserRole.Manager);
    }

    [Fact]
    public void AuthorizationService_ShouldGrantPermission_ForAdminRole()
    {
        _authorizationService.HasPermission(UserRole.Admin, "employee", "read", "company").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.Admin, "employee", "write", "company").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.Admin, "leave", "approve", "company").Should().BeTrue();
    }

    [Fact]
    public void AuthorizationService_ShouldDenyPermission_ForEmployeeOnTeamScope()
    {
        _authorizationService.HasPermission(UserRole.Employee, "employee", "read", "team").Should().BeFalse();
        _authorizationService.HasPermission(UserRole.Employee, "leave", "approve", "team").Should().BeFalse();
    }

    [Fact]
    public void AuthorizationService_ShouldGrantPermission_ForEmployeeOnOwnScope()
    {
        _authorizationService.HasPermission(UserRole.Employee, "employee", "read", "own").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.Employee, "employee", "write", "own").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.Employee, "leave", "request", "own").Should().BeTrue();
    }

    [Fact]
    public void AuthorizationService_ShouldGrantPermission_ForManagerOnTeamScope()
    {
        _authorizationService.HasPermission(UserRole.Manager, "employee", "read", "team").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.Manager, "leave", "approve", "team").Should().BeTrue();
    }

    [Fact]
    public void AuthorizationService_ShouldDenyPermission_ForManagerOnCompanyScope()
    {
        _authorizationService.HasPermission(UserRole.Manager, "employee", "read", "company").Should().BeFalse();
        _authorizationService.HasPermission(UserRole.Manager, "settings", "write", "company").Should().BeFalse();
    }

    [Fact]
    public void AuthorizationService_ShouldGrantHR_AllCompanyPermissions()
    {
        _authorizationService.HasPermission(UserRole.HR, "employee", "read", "company").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.HR, "employee", "write", "company").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.HR, "leave", "approve", "company").Should().BeTrue();
        _authorizationService.HasPermission(UserRole.HR, "settings", "write", "company").Should().BeTrue();
    }

    [Fact]
    public void AuthorizationService_ShouldDenyContractor_OnWritePermissions()
    {
        _authorizationService.HasPermission(UserRole.Contractor, "employee", "write", "team").Should().BeFalse();
        _authorizationService.HasPermission(UserRole.Contractor, "leave", "request", "own").Should().BeFalse();
    }

    [Fact]
    public void AuthorizationService_ShouldReturnCorrectPermissions_ForEachRole()
    {
        var adminPerms = _authorizationService.GetPermissions(UserRole.Admin);
        var employeePerms = _authorizationService.GetPermissions(UserRole.Employee);

        adminPerms.Count.Should().BeGreaterThan(employeePerms.Count);
        employeePerms.Should().OnlyContain(p => p.Contains("own"));
    }

    [Fact]
    public void RoleHierarchy_AdminShouldHaveMorePermissionsThanManager()
    {
        var adminPerms = _authorizationService.GetPermissions(UserRole.Admin);
        var managerPerms = _authorizationService.GetPermissions(UserRole.Manager);

        adminPerms.Count.Should().BeGreaterThan(managerPerms.Count);
    }

    [Fact]
    public void RoleHierarchy_ManagerShouldHaveMorePermissionsThanEmployee()
    {
        var managerPerms = _authorizationService.GetPermissions(UserRole.Manager);
        var employeePerms = _authorizationService.GetPermissions(UserRole.Employee);

        managerPerms.Count.Should().BeGreaterThan(employeePerms.Count);
    }

    [Fact]
    public void TenantId_ShouldBeUsedInView_Model()
    {
        var tenantId = TenantId.New();
        var context = new MockTenantContextFixture();
        context.SetTenantContext(tenantId, UserId.New(), UserRole.Employee);

        context.TenantId!.Value.Should().Be(tenantId.Value);
    }
}
