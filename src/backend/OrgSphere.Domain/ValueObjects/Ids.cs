namespace OrgSphere.Domain.ValueObjects;

public record TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public record UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public record NodeId(Guid Value)
{
    public static NodeId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public record EdgeId(Guid Value)
{
    public static EdgeId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
