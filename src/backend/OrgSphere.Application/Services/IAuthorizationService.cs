using OrgSphere.Domain.Enums;

namespace OrgSphere.Application.Services;

public interface IAuthorizationService
{
    bool HasPermission(UserRole role, string module, string action, string scope);
    bool HasRole(UserRole userRole, UserRole requiredRole);
    IReadOnlyList<string> GetPermissions(UserRole role);
}
