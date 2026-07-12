namespace OrgSphere.Infrastructure.Migrations.Migrations;

public class CreateTenantCompositeIndexes : IMigration
{
    public string Id => "003";
    public string Name => "CreateTenantCompositeIndexes";

    public string[] Up() =>
    [
        "CREATE INDEX user_tenant_email IF NOT EXISTS FOR (u:User) ON (u.TenantId, u.Email)",
        "CREATE INDEX graphnode_tenant_type IF NOT EXISTS FOR (n:GraphNode) ON (n.TenantId, n.Type)",
        "CREATE INDEX graphedge_tenant_type IF NOT EXISTS FOR (e:GraphEdge) ON (e.TenantId, e.Type)",
        "CREATE INDEX graphedge_source IF NOT EXISTS FOR (e:GraphEdge) ON (e.SourceId)",
        "CREATE INDEX graphedge_target IF NOT EXISTS FOR (e:GraphEdge) ON (e.TargetId)"
    ];
}
