namespace OrgSphere.Infrastructure.Migrations.Migrations;

public class CreateIndexes : IMigration
{
    public string Id => "002";
    public string Name => "CreateIndexes";

    public string[] Up() =>
    [
        "CREATE INDEX user_email_index IF NOT EXISTS FOR (u:User) ON (u.Email)",
        "CREATE INDEX user_tenant_index IF NOT EXISTS FOR (u:User) ON (u.TenantId)",
        "CREATE INDEX graphnode_type_index IF NOT EXISTS FOR (n:GraphNode) ON (n.Type)",
        "CREATE INDEX graphnode_tenant_index IF NOT EXISTS FOR (n:GraphNode) ON (n.TenantId)",
        "CREATE INDEX graphedge_type_index IF NOT EXISTS FOR (e:GraphEdge) ON (e.Type)",
        "CREATE INDEX graphedge_tenant_index IF NOT EXISTS FOR (e:GraphEdge) ON (e.TenantId)",
        "CREATE INDEX tenant_slug_index IF NOT EXISTS FOR (t:Tenant) ON (t.Slug)"
    ];
}
