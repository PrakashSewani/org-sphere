namespace OrgSphere.Infrastructure.Migrations.Migrations;

public class CreateConstraints : IMigration
{
    public string Id => "001";
    public string Name => "CreateConstraints";

    public string[] Up() =>
    [
        "CREATE CONSTRAINT tenant_id_unique IF NOT EXISTS FOR (t:Tenant) REQUIRE t.Id IS UNIQUE",
        "CREATE CONSTRAINT user_id_unique IF NOT EXISTS FOR (u:User) REQUIRE u.Id IS UNIQUE",
        "CREATE CONSTRAINT graphnode_id_unique IF NOT EXISTS FOR (n:GraphNode) REQUIRE n.Id IS UNIQUE",
        "CREATE CONSTRAINT graphedge_id_unique IF NOT EXISTS FOR (e:GraphEdge) REQUIRE e.Id IS UNIQUE",
        "CREATE CONSTRAINT refreshtoken_token_unique IF NOT EXISTS FOR (rt:RefreshToken) REQUIRE rt.Token IS UNIQUE"
    ];
}
