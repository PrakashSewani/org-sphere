namespace OrgSphere.Infrastructure.Migrations.Migrations;

public class CreateFullTextIndexes : IMigration
{
    public string Id => "004";
    public string Name => "CreateFullTextIndexes";

    public string[] Up() =>
    [
        "CREATE FULLTEXT INDEX graphnode_properties_ft IF NOT EXISTS FOR (n:GraphNode) ON EACH [n.Properties]"
    ];
}
