using OrgSphere.Domain.Enums;

namespace OrgSphere.Domain.Permissions;

public static class RolePermissions
{
    private static readonly Dictionary<UserRole, HashSet<Permission>> _roleMap = new()
    {
        [UserRole.Owner] = [.. AllPermissions()],
        [UserRole.Admin] = [.. AllPermissions()],
        [UserRole.HR] =
        [
            Permission.Parse("employee:read:company"),
            Permission.Parse("employee:write:company"),
            Permission.Parse("employee:delete:company"),
            Permission.Parse("leave:read:company"),
            Permission.Parse("leave:request:company"),
            Permission.Parse("leave:approve:company"),
            Permission.Parse("attendance:read:company"),
            Permission.Parse("attendance:write:company"),
            Permission.Parse("approval:read:company"),
            Permission.Parse("approval:approve:company"),
            Permission.Parse("analytics:read:company"),
            Permission.Parse("settings:read:company"),
            Permission.Parse("settings:write:company"),
        ],
        [UserRole.Manager] =
        [
            Permission.Parse("employee:read:team"),
            Permission.Parse("employee:write:team"),
            Permission.Parse("leave:read:team"),
            Permission.Parse("leave:request:own"),
            Permission.Parse("leave:approve:team"),
            Permission.Parse("attendance:read:team"),
            Permission.Parse("attendance:write:team"),
            Permission.Parse("approval:read:team"),
            Permission.Parse("approval:approve:team"),
            Permission.Parse("analytics:read:team"),
        ],
        [UserRole.Employee] =
        [
            Permission.Parse("employee:read:own"),
            Permission.Parse("employee:write:own"),
            Permission.Parse("leave:read:own"),
            Permission.Parse("leave:request:own"),
            Permission.Parse("attendance:read:own"),
            Permission.Parse("attendance:write:own"),
            Permission.Parse("approval:read:own"),
        ],
        [UserRole.Contractor] =
        [
            Permission.Parse("employee:read:own"),
            Permission.Parse("employee:write:own"),
            Permission.Parse("attendance:read:own"),
            Permission.Parse("attendance:write:own"),
        ],
    };

    public static IReadOnlySet<Permission> GetPermissionsForRole(UserRole role)
    {
        return _roleMap.TryGetValue(role, out var permissions) ? permissions : [];
    }

    public static bool RoleHasPermission(UserRole role, string module, string action, string scope)
    {
        var required = new Permission(module, action, scope);
        return GetPermissionsForRole(role).Contains(required);
    }

    private static IEnumerable<Permission> AllPermissions()
    {
        string[] modules = ["employee", "department", "team", "leave", "attendance", "approval", "communication", "analytics", "settings", "ai"];
        string[] actions = ["read", "write", "delete", "approve", "export", "admin"];
        string[] scopes = ["own", "team", "department", "company", "global"];

        foreach (var module in modules)
            foreach (var action in actions)
                foreach (var scope in scopes)
                    yield return new Permission(module, action, scope);
    }
}
