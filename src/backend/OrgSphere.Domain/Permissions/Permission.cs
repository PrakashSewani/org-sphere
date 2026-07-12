namespace OrgSphere.Domain.Permissions;

public record Permission(string Module, string Action, string Scope)
{
    public override string ToString() => $"{Module}:{Action}:{Scope}";

    public static Permission Parse(string permissionString)
    {
        var parts = permissionString.Split(':');
        if (parts.Length != 3)
            throw new FormatException($"Invalid permission format: {permissionString}");
        return new Permission(parts[0], parts[1], parts[2]);
    }
}
