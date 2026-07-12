using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Permissions;

namespace OrgSphere.Application.Services;

public class AuthorizationService : IAuthorizationService
{
    public bool HasPermission(UserRole role, string module, string action, string scope)
    {
        return RolePermissions.RoleHasPermission(role, module, action, scope);
    }

    public bool HasRole(UserRole userRole, UserRole requiredRole)
    {
        return userRole <= requiredRole;
    }

    public IReadOnlyList<string> GetPermissions(UserRole role)
    {
        return [.. RolePermissions.GetPermissionsForRole(role)
            .Select(p => p.ToString())
            .OrderBy(p => p)];
    }
}
