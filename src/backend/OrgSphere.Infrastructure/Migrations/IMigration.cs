namespace OrgSphere.Infrastructure.Migrations;

public interface IMigration
{
    string Id { get; }
    string Name { get; }
    string[] Up();
}
